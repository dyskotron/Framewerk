using UnityEditor;
using UnityEngine;
using Framewerk;

namespace Framewerk.Editor.Wizards
{
    /// <summary>
    /// Resolves template paths for the Component Scaffold Wizard.
    /// Checks for project template set overrides first, falls back to framework defaults.
    /// </summary>
    public static class TemplateSetResolver
    {
        private const string DEFAULT_TEMPLATE_PATH = "Packages/com.dyskotron.framewerk.editor/Editor/Wizards/Templates/";
        private const string CONVENTIONAL_TEMPLATE_SET_PATH = "Assets/Settings/Framewerk/TemplateSetConfig.asset";
        private const string ACTIVE_TEMPLATE_SET_PREF_KEY = "Framewerk_ActiveTemplateSetGUID";

        private static TemplateSetConfig _cachedTemplateSet;
        private static bool _cacheInitialized;

        /// <summary>
        /// Gets the template path for a component type.
        /// Checks template set override first, falls back to framework default.
        /// </summary>
        public static string GetTemplatePath(ComponentType componentType)
        {
            var templateSet = GetActiveTemplateSet();
            
            if (templateSet != null && templateSet.HasTemplate((int)componentType))
            {
                var template = templateSet.GetTemplate((int)componentType);
                string path = AssetDatabase.GetAssetPath(template);
                if (!string.IsNullOrEmpty(path))
                {
                    return path;
                }
            }

            return GetDefaultTemplatePath(componentType);
        }

        /// <summary>
        /// Gets the active project template set, if any.
        /// First checks EditorPrefs for explicitly selected template set,
        /// then falls back to conventional path, then scans project.
        /// </summary>
        public static TemplateSetConfig GetActiveTemplateSet()
        {
            if (_cacheInitialized)
            {
                return _cachedTemplateSet;
            }

            _cacheInitialized = true;
            
            // First, check if there's an explicitly selected template set in EditorPrefs
            string activeGuid = EditorPrefs.GetString(ACTIVE_TEMPLATE_SET_PREF_KEY, "");
            if (!string.IsNullOrEmpty(activeGuid))
            {
                string path = AssetDatabase.GUIDToAssetPath(activeGuid);
                if (!string.IsNullOrEmpty(path))
                {
                    _cachedTemplateSet = AssetDatabase.LoadAssetAtPath<TemplateSetConfig>(path);
                    if (_cachedTemplateSet != null)
                    {
                        return _cachedTemplateSet;
                    }
                }
                // GUID was invalid or asset deleted - clear the pref
                EditorPrefs.DeleteKey(ACTIVE_TEMPLATE_SET_PREF_KEY);
            }

            // Check conventional path as fallback
            _cachedTemplateSet = AssetDatabase.LoadAssetAtPath<TemplateSetConfig>(CONVENTIONAL_TEMPLATE_SET_PATH);
            if (_cachedTemplateSet != null)
            {
                // Store this as the active template set
                string guid = AssetDatabase.AssetPathToGUID(CONVENTIONAL_TEMPLATE_SET_PATH);
                EditorPrefs.SetString(ACTIVE_TEMPLATE_SET_PREF_KEY, guid);
                return _cachedTemplateSet;
            }

            // Scan for any TemplateSetConfig in Assets
            string[] guids = AssetDatabase.FindAssets("t:TemplateSetConfig", new[] { "Assets" });
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _cachedTemplateSet = AssetDatabase.LoadAssetAtPath<TemplateSetConfig>(path);
                
                if (_cachedTemplateSet != null)
                {
                    // Store first found as active
                    EditorPrefs.SetString(ACTIVE_TEMPLATE_SET_PREF_KEY, guids[0]);
                }
                
                if (guids.Length > 1)
                {
                    Debug.LogWarning($"[TemplateSetResolver] Multiple TemplateSetConfig assets found. Using: {path}\nSelect active template set in Framewerk > Settings.");
                }
            }

            return _cachedTemplateSet;
        }

        /// <summary>
        /// Returns true if the project has a TemplateSetConfig.
        /// </summary>
        public static bool HasProjectTemplateSet()
        {
            return GetActiveTemplateSet() != null;
        }

        /// <summary>
        /// Clears the cached template set reference.
        /// Called by TemplateSetConfigPostprocessor when assets change,
        /// or when active template set selection changes in settings window.
        /// </summary>
        public static void ClearCache()
        {
            _cachedTemplateSet = null;
            _cacheInitialized = false;
        }

        /// <summary>
        /// Gets the framework default template path for a component type.
        /// </summary>
        public static string GetDefaultTemplatePath(ComponentType componentType)
        {
            switch (componentType)
            {
                case ComponentType.Popup:
                    return DEFAULT_TEMPLATE_PATH + "PopupTemplate.prefab";
                case ComponentType.List:
                    return DEFAULT_TEMPLATE_PATH + "ListTemplate.prefab";
                case ComponentType.ListItem:
                    return DEFAULT_TEMPLATE_PATH + "ListItemTemplate.prefab";
                case ComponentType.VerticalTabs:
                    return DEFAULT_TEMPLATE_PATH + "VerticalTabContainerTemplate.prefab";
                case ComponentType.HorizontalTabs:
                    return DEFAULT_TEMPLATE_PATH + "HorizontalTabContainerTemplate.prefab";
                case ComponentType.ViewStack:
                    return DEFAULT_TEMPLATE_PATH + "ViewStackTemplate.prefab";
                case ComponentType.View:
                    return DEFAULT_TEMPLATE_PATH + "ViewTemplate.prefab";
                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets the framework default template path for a tab item (used alongside tab containers).
        /// </summary>
        public static string GetDefaultTabItemTemplatePath(ComponentType componentType)
        {
            switch (componentType)
            {
                case ComponentType.VerticalTabs:
                    return DEFAULT_TEMPLATE_PATH + "VerticalTabTemplate.prefab";
                case ComponentType.HorizontalTabs:
                    return DEFAULT_TEMPLATE_PATH + "HorizontalTabTemplate.prefab";
                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets the template path for button prefabs (used by popups).
        /// </summary>
        public static string GetButtonTemplatePath()
        {
            // TODO: Support template set override for buttons
            return DEFAULT_TEMPLATE_PATH + "ButtonTemplate.prefab";
        }
    }
}
