using UnityEngine;

namespace Framewerk
{
    /// <summary>
    /// Project-level configuration for overriding default UI templates.
    /// Create one via Assets > Create > Framewerk > Skin Config.
    /// Place at Assets/Settings/Framewerk/SkinConfig.asset for auto-discovery.
    /// </summary>
    [CreateAssetMenu(fileName = "SkinConfig", menuName = "Framewerk/Skin Config")]
    public class SkinConfig : ScriptableObject
    {
        [Header("Skin Info")]
        [Tooltip("Name of this skin")]
        public string SkinName;
        
        [Tooltip("Description of this skin")]
        [TextArea(2, 4)]
        public string Description;

        [Header("Template Overrides")]
        [Tooltip("Override for Popup component template")]
        public GameObject PopupTemplate;
        
        [Tooltip("Override for List component template")]
        public GameObject ListTemplate;
        
        [Tooltip("Override for ListItem component template")]
        public GameObject ListItemTemplate;
        
        [Tooltip("Override for Screen/Panel component template")]
        public GameObject ScreenTemplate;
        
        [Tooltip("Override for View component template")]
        public GameObject ViewTemplate;

        /// <summary>
        /// Gets the template prefab for the specified component type.
        /// </summary>
        /// <param name="componentType">Integer value of ComponentType enum</param>
        /// <returns>The override template or null if not set</returns>
        public GameObject GetTemplate(int componentType)
        {
            // ComponentType enum: Screen=0, Popup=1, List=2, ListItem=3, View=4
            switch (componentType)
            {
                case 0: return ScreenTemplate;
                case 1: return PopupTemplate;
                case 2: return ListTemplate;
                case 3: return ListItemTemplate;
                case 4: return ViewTemplate;
                default: return null;
            }
        }

        /// <summary>
        /// Checks if this skin has an override for the specified component type.
        /// </summary>
        /// <param name="componentType">Integer value of ComponentType enum</param>
        /// <returns>True if an override is set</returns>
        public bool HasTemplate(int componentType)
        {
            return GetTemplate(componentType) != null;
        }
    }
}
