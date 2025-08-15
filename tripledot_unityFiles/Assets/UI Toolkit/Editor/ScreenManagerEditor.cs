using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Custom Inspector for ScreenManager.
/// Provides dropdowns to select start screen, buttons, and target screens in the editor.
/// Automatically queries UI Toolkit screens and buttons for easy mapping.
/// </summary>
[CustomEditor(typeof(ScreenManager))]
public class ScreenManagerEditor : Editor
{
    private List<string> screenNames = new List<string>(); // List of screen names found in UIDocument
    private Dictionary<string, List<string>> screenButtons = new Dictionary<string, List<string>>(); // Buttons grouped by screen

    /// <summary>
    /// Queries the UIDocument for all screens and their buttons, and populates
    /// screenNames and screenButtons dictionaries for the inspector dropdowns.
    /// </summary>
    private void RefreshScreenAndButtonNames()
    {
        screenNames.Clear();
        screenButtons.Clear();

        var manager = (ScreenManager)target;
        var doc = manager.GetComponent<UIDocument>();
        if (doc == null || doc.rootVisualElement == null)
            return;

        // Find all TemplateContainers with class "screen"
        var screens = doc.rootVisualElement.Query<TemplateContainer>()
                                          .Where(t => t.ClassListContains("screen"))
                                          .ToList();

        foreach (var screen in screens)
        {
            string screenName = screen.name;
            if (string.IsNullOrEmpty(screenName))
                continue; // Skip screens without a name

            screenNames.Add(screenName);

            // Collect all buttons inside this screen
            var buttons = screen.Query<Button>().ToList();
            screenButtons[screenName] = buttons.ConvertAll(b => b.name);
        }
    }

    /// <summary>
    /// Draws the custom inspector GUI for the ScreenManager.
    /// Includes Start Screen dropdown and Button Mappings editor.
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Refresh screen names and button lists each frame to reflect UXML changes
        RefreshScreenAndButtonNames();

        var manager = (ScreenManager)target;

        // -------------------- Start Screen Dropdown --------------------
        int startIndex = Mathf.Max(0, screenNames.IndexOf(manager.startScreen));
        int selectedStart = EditorGUILayout.Popup("Start Screen", startIndex, screenNames.ToArray());
        if (screenNames.Count > 0)
            manager.startScreen = screenNames[selectedStart];

        // -------------------- Button Mappings --------------------
        var mappingsProp = serializedObject.FindProperty("buttonMappings");

        for (int i = 0; i < mappingsProp.arraySize; i++)
        {
            var mapping = mappingsProp.GetArrayElementAtIndex(i);
            var btnNameProp = mapping.FindPropertyRelative("buttonName");
            var targetScreenProp = mapping.FindPropertyRelative("targetScreen");

            EditorGUILayout.BeginVertical("box");

            // Target screen dropdown
            int targetIndex = Mathf.Max(0, screenNames.IndexOf(targetScreenProp.stringValue));
            int selectedTarget = EditorGUILayout.Popup("Target Screen", targetIndex, screenNames.ToArray());
            if (screenNames.Count > 0)
                targetScreenProp.stringValue = screenNames[selectedTarget];

            // Button dropdown: buttons from all screens excluding the target screen
            List<string> buttonList = screenButtons.Where(kvp => kvp.Key != targetScreenProp.stringValue)
                                                   .SelectMany(kvp => kvp.Value)
                                                   .Distinct()
                                                   .ToList();

            int btnIndex = Mathf.Max(0, buttonList.IndexOf(btnNameProp.stringValue));
            int selectedBtn = EditorGUILayout.Popup("Button Name", btnIndex, buttonList.ToArray());
            if (buttonList.Count > 0)
                btnNameProp.stringValue = buttonList[selectedBtn];

            // Remove mapping button
            if (GUILayout.Button("Remove Mapping"))
                mappingsProp.DeleteArrayElementAtIndex(i);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        // Add new mapping button
        if (GUILayout.Button("Add Mapping"))
            mappingsProp.InsertArrayElementAtIndex(mappingsProp.arraySize);

        serializedObject.ApplyModifiedProperties();
    }
}
