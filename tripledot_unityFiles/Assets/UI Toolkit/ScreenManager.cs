using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

/// <summary>
/// Maps buttons to UI screens and handles switching between screens.
/// Designed for UI Toolkit using TemplateContainers for each screen.
/// </summary>

[System.Serializable]
public class ButtonMapping
{
    public string buttonName;    // The name of the button to listen for
    public string targetScreen;  // The screen this button will show when clicked
}

[RequireComponent(typeof(UIDocument))]
public class ScreenManager : MonoBehaviour
{
    [Header("Start Screen")]
    public string startScreen; // Name of the screen to show first

    [Header("Button Mappings")]
    public List<ButtonMapping> buttonMappings = new List<ButtonMapping>(); // Maps buttons to target screens

    private UIDocument uiDocument;
    private VisualElement root;
    private Dictionary<string, TemplateContainer> screens = new Dictionary<string, TemplateContainer>();

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        // Schedule initialization to ensure TemplateContainers are fully loaded
        root.schedule.Execute(() =>
        {
            InitializeScreens();
            InitializeButtons();
        }).ExecuteLater(1);
    }

    /// <summary>
    /// Finds all TemplateContainers with class "screen" and initializes them.
    /// Deactivates all except the start screen.
    /// </summary>
    private void InitializeScreens()
    {
        screens.Clear();

        var screenElements = root.Query<TemplateContainer>(className: "screen").ToList();

        if (screenElements.Count == 0)
        {
            Debug.LogWarning("ScreenManager: No screens found in the UI.");
            return;
        }

        foreach (var screen in screenElements)
        {
            if (string.IsNullOrEmpty(screen.name))
            {
                Debug.LogWarning("ScreenManager: Found a screen without a name. Assign a unique name in UXML.");
                continue;
            }

            screens[screen.name] = screen;

            // Disable interactions initially
            screen.RemoveFromClassList("active");
            SetPickingModeRecursive(screen, PickingMode.Ignore);

            Debug.Log($"ScreenManager: Detected screen '{screen.name}'");
        }

        // Activate start screen
        if (!string.IsNullOrEmpty(startScreen) && screens.ContainsKey(startScreen))
            ShowScreen(startScreen);
        else
        {
            startScreen = screens.Keys.First();
            ShowScreen(startScreen);
        }
    }

    /// <summary>
    /// Connects button clicks to their target screens.
    /// </summary>
    private void InitializeButtons()
    {
        foreach (var mapping in buttonMappings)
        {
            if (string.IsNullOrEmpty(mapping.buttonName) || string.IsNullOrEmpty(mapping.targetScreen))
                continue;

            Button button = null;

            // Search all screens for the button
            foreach (var screen in screens.Values)
            {
                button = screen.Q<Button>(mapping.buttonName);
                if (button != null) break;
            }

            if (button != null)
            {
                string target = mapping.targetScreen;
                button.clicked += () => ShowScreen(target);
                Debug.Log($"ScreenManager: Attached button '{mapping.buttonName}' to target screen '{target}'");
            }
            else
            {
                Debug.LogWarning($"ScreenManager: Button '{mapping.buttonName}' not found in any screen.");
            }
        }
    }

    /// <summary>
    /// Switches to the specified screen, deactivating all others.
    /// </summary>
    public void ShowScreen(string screenName)
    {
        if (!screens.ContainsKey(screenName))
        {
            Debug.LogWarning($"ScreenManager: Screen '{screenName}' not found!");
            return;
        }

        foreach (var kvp in screens)
        {
            SetScreenActive(kvp.Value, kvp.Key == screenName);
        }

        Debug.Log($"ScreenManager: Switched to screen '{screenName}'");
    }

    /// <summary>
    /// Activates or deactivates a screen and sets picking mode recursively for all children.
    /// </summary>
    private void SetScreenActive(TemplateContainer screen, bool active)
    {
        if (active)
        {
            screen.AddToClassList("active");
            SetPickingModeRecursive(screen, PickingMode.Position);
        }
        else
        {
            screen.RemoveFromClassList("active");
            SetPickingModeRecursive(screen, PickingMode.Ignore);
        }
    }

    /// <summary>
    /// Recursively sets PickingMode for an element and its children.
    /// PickingMode.Ignore = ignores pointer events
    /// PickingMode.Position = allows pointer interactions
    /// </summary>
    private void SetPickingModeRecursive(VisualElement element, PickingMode mode)
    {
        element.pickingMode = mode;
        foreach (var child in element.Children())
        {
            SetPickingModeRecursive(child, mode);
        }
    }
}
