using UnityEditor;
using UnityEngine;

namespace Framewerk.Editor.Wizards
{
    /// <summary>
    /// [DEPRECATED] This settings window is no longer needed.
    /// Address configuration is now handled via ViewConfig (ContextPrefixSO)
    /// and AddressBuilder (hardcoded TypeKeys).
    /// 
    /// This file is kept for migration reference only. Delete after confirming all
    /// projects have migrated to the new address system.
    /// </summary>
    [System.Obsolete("Use ViewConfig.ContextPrefixSO instead. TypeKeys are now hardcoded in AddressBuilder. CustomPrefix is passed at runtime.")]
    public class FramewerkSettingsWindow : EditorWindow
    {
        [MenuItem("Framewerk/Settings (Deprecated)")]
        public static void ShowWindow()
        {
            var window = GetWindow<FramewerkSettingsWindow>("Framewerk Settings");
            window.minSize = new Vector2(400, 280);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("⚠️ DEPRECATED", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This settings window is no longer needed.\n\n" +
                "Address configuration is now handled via:\n" +
                "• ViewConfig.ContextPrefixSO - ScriptableObject reference for context prefix\n" +
                "• CustomPrefix - Passed at runtime via method arguments\n" +
                "• AddressBuilder.TypeKeys - Hardcoded constants (Popup, List, List.ListItem)\n\n" +
                "The wizard now reads ContextPrefixSO from your selected Bootstrap.",
                MessageType.Warning);

            GUILayout.Space(10);

            EditorGUILayout.LabelField("New Address Format:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("[ContextPrefix]/[CustomPrefix]/UI/[TypeKey]/[ClassName]", EditorStyles.miniLabel);
            
            GUILayout.Space(10);
            
            EditorGUILayout.LabelField("Source of each part:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("  • ContextPrefix: ViewConfig.ContextPrefixSO.Prefix", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("  • CustomPrefix: Passed at runtime (optional)", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("  • TypeKey: Derived from component type", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("  • ClassName: Derived from C# class name", EditorStyles.miniLabel);
            
            GUILayout.Space(10);
            
            EditorGUILayout.LabelField("TypeKey Values (hardcoded):", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("  • View: (empty)", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("  • Popup: \"Popup\"", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("  • List: \"List\"", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("  • ListItem: \"List.ListItem\"", EditorStyles.miniLabel);

            GUILayout.Space(20);

            if (GUILayout.Button("Close", GUILayout.Width(100)))
            {
                Close();
            }
        }
    }
}
