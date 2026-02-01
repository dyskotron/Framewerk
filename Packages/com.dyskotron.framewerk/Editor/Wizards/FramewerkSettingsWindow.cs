using UnityEditor;
using UnityEngine;

namespace Framewerk.Editor.Wizards
{
    public class FramewerkSettingsWindow : EditorWindow
    {
        private const string PREFS_UI_TYPE_KEY = "FramewerkWizard_UiTypeKey";
        private const string PREFS_UI_TYPE_KEY_IS_PREFIX = "FramewerkWizard_UiTypeKeyIsPrefix";
        private const string PREFS_POPUP_TYPE_KEY = "FramewerkWizard_PopupTypeKey";

        private string uiTypeKey = "UI";
        private bool uiTypeKeyIsPrefix = false;
        private string popupTypeKey = "Popups";

        [MenuItem("Framewerk/Settings")]
        public static void ShowWindow()
        {
            var window = GetWindow<FramewerkSettingsWindow>("Framewerk Settings");
            window.minSize = new Vector2(400, 200);
            window.Show();
        }

        private void OnEnable()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            uiTypeKey = EditorPrefs.GetString(PREFS_UI_TYPE_KEY, "UI");
            uiTypeKeyIsPrefix = EditorPrefs.GetBool(PREFS_UI_TYPE_KEY_IS_PREFIX, false);
            popupTypeKey = EditorPrefs.GetString(PREFS_POPUP_TYPE_KEY, "Popups");
        }

        private void SaveSettings()
        {
            EditorPrefs.SetString(PREFS_UI_TYPE_KEY, uiTypeKey);
            EditorPrefs.SetBool(PREFS_UI_TYPE_KEY_IS_PREFIX, uiTypeKeyIsPrefix);
            EditorPrefs.SetString(PREFS_POPUP_TYPE_KEY, popupTypeKey);
        }

        private void ResetToDefaults()
        {
            uiTypeKey = "UI";
            uiTypeKeyIsPrefix = false;
            popupTypeKey = "Popups";
            SaveSettings();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Framewerk Manager Settings", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "These settings configure how addressable paths are built for UI and Popup components.",
                MessageType.Info);

            GUILayout.Space(10);

            EditorGUILayout.LabelField("UI Manager Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            uiTypeKey = EditorGUILayout.TextField("UI TypeKey", uiTypeKey);
            uiTypeKeyIsPrefix = EditorGUILayout.Toggle("TypeKey is Prefix", uiTypeKeyIsPrefix);

            EditorGUI.indentLevel--;
            GUILayout.Space(10);

            EditorGUILayout.LabelField("Popup Manager Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            popupTypeKey = EditorGUILayout.TextField("Popup TypeKey", popupTypeKey);

            EditorGUI.indentLevel--;
            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Reset to Defaults", GUILayout.Width(150)))
            {
                ResetToDefaults();
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Save", GUILayout.Width(100)))
            {
                SaveSettings();
                Close();
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
        }

        private void OnDisable()
        {
            // Auto-save when window is closed
            SaveSettings();
        }
    }
}
