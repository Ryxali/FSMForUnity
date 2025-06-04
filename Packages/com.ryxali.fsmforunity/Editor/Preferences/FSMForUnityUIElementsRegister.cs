using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
// Register a SettingsProvider using UIElements for the drawing framework:
internal static class FSMForUnityUIElementsRegister
{
    [SettingsProvider]
    public static SettingsProvider CreateMyCustomSettingsProvider()
    {
        // First parameter is the path in the Settings window.
        // Second parameter is the scope of this setting: it only appears in the Settings window for the Project scope.
        var provider = new SettingsProvider("Preferences/FSMForUnity", SettingsScope.User)
        {
            label = "FSM For Unity",
            // activateHandler is called when the user clicks on the Settings item in the Settings window.
            activateHandler = (searchContext, rootElement) =>
            {
                //var settings = FSMForUnitySettings.GetSerializedSettings();

                // rootElement is a VisualElement. If you add any children to it, the OnGUI function
                // isn't called because the SettingsProvider uses the UIElements drawing framework.
                //var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/settings_ui.uss");
                //rootElement.styleSheets.Add(styleSheet);
                var title = new Label()
                {
                    text = "Custom UI Elements"
                };
                title.AddToClassList("title");
                rootElement.Add(title);

                var properties = new VisualElement()
                {
                    style =
                    {
                        flexDirection = FlexDirection.Column
                    }
                };
                properties.AddToClassList("property-list");
                rootElement.Add(properties);
                //properties.Add(CreateColorField("Background Color", FSMForUnityPreferences.gridView_backgroundColor));
                //properties.Add(CreateColorField("Foreground Color", FSMForUnityPreferences.gridView_gridColor));
                properties.Add(CreateToggleField("Show Editor Machines", FSMForUnityPreferences.settings_showEditorMachines));
                //properties.Add(new PropertyField(settings.FindProperty("m_SomeString")));
                //properties.Add(new PropertyField(settings.FindProperty("m_Number")));

                //rootElement.Bind(settings);
            },

            // Populate the search keywords to enable smart search filtering and label highlighting:
            keywords = new HashSet<string>(new[] { "FSM", "Background Color", "Foreground Color" })
        };

        return provider;
    }

    private static Toggle CreateToggleField(string title, EditorPrefToggle prefToggle)
    {
        var field = new Toggle(title);
        field.value = prefToggle.value;
        prefToggle.onChanged += v => field.SetValueWithoutNotify(v);
        field.RegisterValueChangedCallback(cb => prefToggle.value = cb.newValue);
        return field;
    }

    private static ColorField CreateColorField(string title, EditorPrefColor prefColor)
    {
        var field = new ColorField(title);
        field.value = prefColor.value;
        field.RegisterValueChangedCallback(cb => prefColor.value = cb.newValue);
        return field;
    }
}