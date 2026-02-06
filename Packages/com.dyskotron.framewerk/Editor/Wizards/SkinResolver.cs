using UnityEditor;
using UnityEngine;
using Framewerk;

namespace Framewerk.Editor.Wizards
{
    /// <summary>
    /// Resolves template paths for the Component Scaffold Wizard.
    /// Checks for project skin overrides first, falls back to framework defaults.
    /// </summary>
    public static class SkinResolver
    {
        private const string DEFAULT_TEMPLATE_PATH = "Packages/com.dyskotron.framewerk/Editor/Wizards/Templates/";
        private const string CONVENTIONAL_SKIN_PATH = "Assets/Settings/Framewerk/SkinConfig.asset";

        private static SkinConfig _cachedSkin;
        private static bool _cacheInitialized;

        /// <summary>
        /// Gets the template path for a component type.
        /// Checks skin override first, falls back to framework default.
        /// </summary>
        public static string GetTemplatePath(ComponentType componentType)
        {
            var skin = GetActiveSkin();
            
            if (skin != null && skin.HasTemplate((int)componentType))
            {
                var template = skin.GetTemplate((int)componentType);
                string path = AssetDatabase.GetAssetPath(template);
                if (!string.IsNullOrEmpty(path))
                {
                    return path;
                }
            }

            return GetDefaultTemplatePath(componentType);
        }

        /// <summary>
        /// Gets the active project skin, if any.
        /// Checks conventional path first, then scans project.
        /// </summary>
        public static SkinConfig GetActiveSkin()
        {
            if (_cacheInitialized)
            {
                return _cachedSkin;
            }

            _cacheInitialized = true;

            // Check conventional path first
            _cachedSkin = AssetDatabase.LoadAssetAtPath<SkinConfig>(CONVENTIONAL_SKIN_PATH);
            if (_cachedSkin != null)
            {
                return _cachedSkin;
            }

            // Scan for any SkinConfig in Assets
            string[] guids = AssetDatabase.FindAssets("t:SkinConfig", new[] { "Assets" });
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _cachedSkin = AssetDatabase.LoadAssetAtPath<SkinConfig>(path);
                
                if (guids.Length > 1)
                {
                    Debug.LogWarning($"[SkinResolver] Multiple SkinConfig assets found. Using: {path}");
                }
            }

            return _cachedSkin;
        }

        /// <summary>
        /// Returns true if the project has a SkinConfig.
        /// </summary>
        public static bool HasProjectSkin()
        {
            return GetActiveSkin() != null;
        }

        /// <summary>
        /// Clears the cached skin reference.
        /// Called by SkinConfigPostprocessor when assets change.
        /// </summary>
        internal static void ClearCache()
        {
            _cachedSkin = null;
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
                case ComponentType.Screen:
                    return DEFAULT_TEMPLATE_PATH + "PanelTemplate.prefab";
                case ComponentType.View:
                    return DEFAULT_TEMPLATE_PATH + "ViewTemplate.prefab";
                default:
                    return null;
            }
        }
    }
}
