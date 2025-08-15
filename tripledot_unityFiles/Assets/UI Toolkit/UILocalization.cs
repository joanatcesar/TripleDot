using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

/// <summary>
/// Manages the Settings popup UI and applies localization to all text elements.
/// Supports runtime language switching.
/// </summary>
public class SettingsPopupUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument; // Reference to the UI Toolkit document

    // Example localization dictionary
    // Key = placeholder text in UXML, Value = localized text to display
    private Dictionary<string, string> localization = new Dictionary<string, string>()
    {
        { "LOCALIZE_Sound", "Sound" },
        { "LOCALIZE_Music", "Music" },
        { "LOCALIZE_Vibration", "Vibration" },
        { "LOCALIZE_Notification", "Notification" },
        { "LOCALIZE_Language", "Language" },
        { "LOCALIZE_Settings", "Settings" },
        { "LOCALIZE_T&C", "T&C" },
        { "LOCALIZE_Privacy", "Privacy" },
        { "LOCALIZE_Support", "Support" },
        { "LOCALIZE_Ver_1.0.0", "Version 1.0.0" }
    };

    private void OnEnable()
    {
        var root = uiDocument.rootVisualElement;

        // Apply localization to all Label and Button elements in the root
        ApplyLocalization(root);
    }

    /// <summary>
    /// Goes through all Label and Button elements and replaces their text using the localization dictionary.
    /// </summary>
    private void ApplyLocalization(VisualElement root)
    {
        // Apply localization to Labels
        var labels = root.Query<Label>().ToList();
        foreach (var label in labels)
        {
            if (localization.TryGetValue(label.text, out string localizedText))
            {
                label.text = localizedText;
            }
        }

        // Apply localization to Buttons
        var buttons = root.Query<Button>().ToList();
        foreach (var button in buttons)
        {
            if (localization.TryGetValue(button.text, out string localizedText))
            {
                button.text = localizedText;
            }
        }
    }

    /// <summary>
    /// Allows runtime language switching by replacing the localization dictionary.
    /// </summary>
    /// <param name="newLocalization">New dictionary of key-value pairs for localization</param>
    public void SetLanguage(Dictionary<string, string> newLocalization)
    {
        localization = newLocalization;
        ApplyLocalization(uiDocument.rootVisualElement);
    }
}
