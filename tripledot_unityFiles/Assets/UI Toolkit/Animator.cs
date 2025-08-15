using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_UIELEMENTS
using UnityEngine.UIElements.Experimental;
#endif
using DG.Tweening; // For DOTween animations
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// IdleAnimator provides idle animations (scale, rotation, position, opacity) for UI elements.
/// Supports both UGUI (RectTransform) and UI Toolkit (VisualElement) targets.
/// </summary>
public class IdleAnimator : MonoBehaviour
{
    // -------------------- General Settings --------------------
    [Header("General Settings")]
    public bool isUIElements = false; // False = UGUI, True = UI Toolkit
    public bool playOnEnable = true;  // Start animation automatically when enabled
    public Ease easeType = Ease.InOutSine; // DOTween easing type
    public float duration = 1f; // Time for one "half" cycle of animation

    // -------------------- Scale Animation Settings --------------------
    [Header("Scale Settings")]
    public bool animateScale = true;
    [Min(0f)] public float minScale = 1f; // Start scale
    [Min(0f)] public float maxScale = 1.05f; // End scale

    // -------------------- Opacity Animation Settings --------------------
    [Header("Opacity Settings")]
    public bool animateOpacity = false;
    [Range(0f, 1f)] public float minOpacity = 0.8f;
    [Range(0f, 1f)] public float maxOpacity = 1f;

    // -------------------- Rotation Animation Settings --------------------
    [Header("Rotation Settings")]
    public bool animateRotation = false;
    public float minRotation = -5f; // degrees
    public float maxRotation = 5f;  // degrees

    // -------------------- Position Animation Settings --------------------
    [Header("Position Settings")]
    public bool animatePosition = false;
    public Vector3 minPosition = Vector3.zero;
    public Vector3 maxPosition = new Vector3(0f, 10f, 0f);

    // -------------------- UGUI Target --------------------
    [Header("Target Override (UGUI)")]
    public RectTransform targetUGUI;

#if UNITY_UIELEMENTS
    // -------------------- UI Toolkit Targets --------------------
    [Header("UI Toolkit Targets")]
    [Tooltip("Assign the UXML asset here to enable dropdown selection of element names")]
    public VisualTreeAsset visualTreeAsset;

    [Tooltip("List of element names to animate. Select from dropdown.")]
    [SerializeField] private string[] uiToolkitElementNames;
    private VisualElement[] targetElements;

    [SerializeField, HideInInspector] private int elementCount = 0;
#endif

    private RectTransform rectTransform; // UGUI reference
    private CanvasGroup canvasGroup;     // For opacity in UGUI
    private bool isAnimating = false;    // Track if animation is active

    // -------------------- Initialization --------------------
    void Awake()
    {
        if (!isUIElements)
        {
            // UGUI setup
            rectTransform = targetUGUI != null ? targetUGUI : GetComponent<RectTransform>();
            if (animateOpacity)
            {
                // Ensure CanvasGroup exists for opacity animation
                canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            }
        }
        else
        {
#if UNITY_UIELEMENTS
            var uiDoc = GetComponent<UIDocument>();
            if (uiDoc != null)
            {
                if (uiToolkitElementNames != null && uiToolkitElementNames.Length > 0)
                {
                    // Fetch VisualElements by name from UIDocument
                    targetElements = new VisualElement[uiToolkitElementNames.Length];
                    for (int i = 0; i < uiToolkitElementNames.Length; i++)
                    {
                        targetElements[i] = uiDoc.rootVisualElement.Q(uiToolkitElementNames[i]);
                    }
                }
                else
                {
                    // If no names specified, animate the root
                    targetElements = new VisualElement[] { uiDoc.rootVisualElement };
                }

                if (targetElements == null || targetElements.Length == 0)
                {
                    Debug.LogWarning("IdleAnimator: No UI Toolkit elements found to animate.");
                }
            }
            else
            {
                Debug.LogError("IdleAnimator: No UIDocument found on this GameObject.");
            }
#endif
        }
    }

    void OnEnable()
    {
        if (playOnEnable) StartIdle();
    }

    void OnDisable()
    {
        StopIdle();
    }

    // -------------------- Public Methods --------------------
    /// <summary>
    /// Starts the idle animation for the assigned target(s)
    /// </summary>
    public void StartIdle()
    {
        if (isAnimating) return;

        if (!isUIElements && rectTransform != null && rectTransform.gameObject.activeInHierarchy)
        {
            AnimateUGUI();
            isAnimating = true;
        }
#if UNITY_UIELEMENTS
        else if (isUIElements && targetElements != null)
        {
            AnimateUIElements();
            isAnimating = true;
        }
#endif
    }

    /// <summary>
    /// Stops all active animations
    /// </summary>
    public void StopIdle()
    {
        DOTween.Kill(this); // Stop UGUI tweens
#if UNITY_UIELEMENTS
        StopAllCoroutines(); // Stop UI Toolkit coroutines
#endif
        isAnimating = false;
    }

    /// <summary>
    /// Restarts the idle animation
    /// </summary>
    public void RestartIdle()
    {
        StopIdle();
        StartIdle();
    }

    // -------------------- UGUI Animation --------------------
    void AnimateUGUI()
    {
        if (animateScale)
            rectTransform.DOScale(maxScale, duration).From(minScale).SetEase(easeType).SetLoops(-1, LoopType.Yoyo).SetId(this);

        if (animateRotation)
            rectTransform.DOLocalRotate(new Vector3(0f, 0f, maxRotation), duration)
                .From(new Vector3(0f, 0f, minRotation))
                .SetEase(easeType)
                .SetLoops(-1, LoopType.Yoyo)
                .SetId(this);

        if (animatePosition)
            rectTransform.DOLocalMove(maxPosition, duration).From(minPosition)
                .SetEase(easeType)
                .SetLoops(-1, LoopType.Yoyo)
                .SetId(this);

        if (animateOpacity && canvasGroup != null)
            canvasGroup.DOFade(minOpacity, duration)
                .SetEase(easeType)
                .SetLoops(-1, LoopType.Yoyo)
                .SetId(this);
    }

#if UNITY_UIELEMENTS
    // -------------------- UI Toolkit Animation --------------------
    void AnimateUIElements()
    {
        StopAllCoroutines(); // Prevent multiple coroutines per element
        foreach (var element in targetElements)
        {
            if (element == null) continue;
            StartCoroutine(AnimateElementCoroutine(element, duration));
        }
    }

    System.Collections.IEnumerator AnimateElementCoroutine(VisualElement element, float duration)
    {
        float time = 0f;
        bool forward = true;

        // Cache original values
        Vector3 originalScale = element.transform.scale;
        Vector3 originalPos = element.transform.position;
        Quaternion originalRot = element.transform.rotation;

        while (true)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            // Scale
            if (animateScale)
            {
                float startScale = forward ? minScale : maxScale;
                float endScale = forward ? maxScale : minScale;
                element.transform.scale = originalScale * Mathf.Lerp(startScale, endScale, easedT);
            }

            // Position
            if (animatePosition)
            {
                Vector3 startPos = forward ? originalPos + minPosition : originalPos + maxPosition;
                Vector3 endPos = forward ? originalPos + maxPosition : originalPos + minPosition;
                element.transform.position = Vector3.Lerp(startPos, endPos, easedT);
            }

            // Rotation
            if (animateRotation)
            {
                float startRot = forward ? minRotation : maxRotation;
                float endRot = forward ? maxRotation : minRotation;
                element.transform.rotation = originalRot * Quaternion.Euler(0f, 0f, Mathf.Lerp(startRot, endRot, easedT));
            }

            // Opacity
            if (animateOpacity)
            {
                float startOpacity = forward ? minOpacity : maxOpacity;
                float endOpacity = forward ? maxOpacity : minOpacity;
                element.style.opacity = Mathf.Lerp(startOpacity, endOpacity, easedT);
            }

            if (t >= 1f)
            {
                time = 0f;
                forward = !forward; // Reverse direction
            }
            yield return null;
        }
    }
#endif

#if UNITY_EDITOR
    // -------------------- Custom Editor --------------------
    [CustomEditor(typeof(IdleAnimator))]
    public class IdleAnimatorEditor : Editor
    {
        // Serialized properties
        SerializedProperty isUIElementsProp, playOnEnableProp, easeTypeProp, durationProp;
        SerializedProperty animateScaleProp, minScaleProp, maxScaleProp;
        SerializedProperty animateOpacityProp, minOpacityProp, maxOpacityProp;
        SerializedProperty animateRotationProp, minRotationProp, maxRotationProp;
        SerializedProperty animatePositionProp, minPositionProp, maxPositionProp;
        SerializedProperty targetUGUIProp;
        SerializedProperty visualTreeAssetProp, uiToolkitElementNamesProp, elementCountProp;

        bool previousIsUIElements = false; // Track changes

        void OnEnable()
        {
            // Fetch all serialized properties
            isUIElementsProp = serializedObject.FindProperty("isUIElements");
            playOnEnableProp = serializedObject.FindProperty("playOnEnable");
            easeTypeProp = serializedObject.FindProperty("easeType");
            durationProp = serializedObject.FindProperty("duration");

            animateScaleProp = serializedObject.FindProperty("animateScale");
            minScaleProp = serializedObject.FindProperty("minScale");
            maxScaleProp = serializedObject.FindProperty("maxScale");

            animateOpacityProp = serializedObject.FindProperty("animateOpacity");
            minOpacityProp = serializedObject.FindProperty("minOpacity");
            maxOpacityProp = serializedObject.FindProperty("maxOpacity");

            animateRotationProp = serializedObject.FindProperty("animateRotation");
            minRotationProp = serializedObject.FindProperty("minRotation");
            maxRotationProp = serializedObject.FindProperty("maxRotation");

            animatePositionProp = serializedObject.FindProperty("animatePosition");
            minPositionProp = serializedObject.FindProperty("minPosition");
            maxPositionProp = serializedObject.FindProperty("maxPosition");

            targetUGUIProp = serializedObject.FindProperty("targetUGUI");

            visualTreeAssetProp = serializedObject.FindProperty("visualTreeAsset");
            uiToolkitElementNamesProp = serializedObject.FindProperty("uiToolkitElementNames");
            elementCountProp = serializedObject.FindProperty("elementCount");

            previousIsUIElements = isUIElementsProp.boolValue;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(isUIElementsProp);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                bool currentIsUIElements = isUIElementsProp.boolValue;
                if (currentIsUIElements != previousIsUIElements)
                {
                    previousIsUIElements = currentIsUIElements;

                    // Adjust array sizes for UI Toolkit elements
                    if (currentIsUIElements)
                    {
                        elementCountProp.intValue = Mathf.Max(0, elementCountProp.intValue);
                        if (uiToolkitElementNamesProp.arraySize != elementCountProp.intValue)
                            uiToolkitElementNamesProp.arraySize = elementCountProp.intValue;
                    }
                    else
                    {
                        elementCountProp.intValue = 0;
                        uiToolkitElementNamesProp.arraySize = 0;
                    }

                    serializedObject.Update();
                }
            }

            // Draw remaining fields
            EditorGUILayout.PropertyField(playOnEnableProp);
            EditorGUILayout.PropertyField(easeTypeProp);
            EditorGUILayout.PropertyField(durationProp);

            EditorGUILayout.PropertyField(animateScaleProp);
            if (animateScaleProp.boolValue)
            {
                EditorGUILayout.PropertyField(minScaleProp);
                EditorGUILayout.PropertyField(maxScaleProp);
            }

            EditorGUILayout.PropertyField(animateOpacityProp);
            if (animateOpacityProp.boolValue)
            {
                EditorGUILayout.PropertyField(minOpacityProp);
                EditorGUILayout.PropertyField(maxOpacityProp);
            }

            EditorGUILayout.PropertyField(animateRotationProp);
            if (animateRotationProp.boolValue)
            {
                EditorGUILayout.PropertyField(minRotationProp);
                EditorGUILayout.PropertyField(maxRotationProp);
            }

            EditorGUILayout.PropertyField(animatePositionProp);
            if (animatePositionProp.boolValue)
            {
                EditorGUILayout.PropertyField(minPositionProp);
                EditorGUILayout.PropertyField(maxPositionProp);
            }

            // Target selection based on mode
            if (!isUIElementsProp.boolValue)
            {
                EditorGUILayout.PropertyField(targetUGUIProp);
            }
            else
            {
                if (visualTreeAssetProp != null)
                {
                    EditorGUILayout.PropertyField(visualTreeAssetProp);

                    if (visualTreeAssetProp.objectReferenceValue != null)
                    {
                        VisualTreeAsset vta = visualTreeAssetProp.objectReferenceValue as VisualTreeAsset;
                        var names = GetAllElementNames(vta);

                        elementCountProp.intValue = EditorGUILayout.IntField("Number of Elements", elementCountProp.intValue);
                        elementCountProp.intValue = Mathf.Max(0, elementCountProp.intValue);

                        if (uiToolkitElementNamesProp.arraySize != elementCountProp.intValue)
                            uiToolkitElementNamesProp.arraySize = elementCountProp.intValue;

                        // Dropdown selection for each element
                        for (int i = 0; i < elementCountProp.intValue; i++)
                        {
                            string currentName = uiToolkitElementNamesProp.GetArrayElementAtIndex(i).stringValue;
                            int index = names.IndexOf(currentName);
                            if (index == -1) index = 0;

                            index = EditorGUILayout.Popup($"Element {i}", index, names.ToArray());
                            uiToolkitElementNamesProp.GetArrayElementAtIndex(i).stringValue = names[index];
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Assign a VisualTreeAsset to enable element selection.", MessageType.Info);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("SerializedProperty visualTreeAssetProp is null.", MessageType.Error);
                }
            }

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
                EditorUtility.SetDirty(target);

            if (GUILayout.Button("Restart Idle Animation"))
                ((IdleAnimator)target).RestartIdle();
        }

        // -------------------- Helper Methods --------------------
        private System.Collections.Generic.List<string> GetAllElementNames(VisualTreeAsset vta)
        {
            var names = new System.Collections.Generic.List<string>();
            VisualElement root = vta.CloneTree();
            if (root != null) AddNamesRecursive(root, names);

            if (names.Count == 0) names.Add("<No named elements>");
            return names;
        }

        private void AddNamesRecursive(VisualElement element, System.Collections.Generic.List<string> names)
        {
            if (!string.IsNullOrEmpty(element.name)) names.Add(element.name);
            foreach (var child in element.Children()) AddNamesRecursive(child, names);
        }
    }
#endif
}
