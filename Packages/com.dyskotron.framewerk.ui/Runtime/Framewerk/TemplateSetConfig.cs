using UnityEngine;

namespace Framewerk
{
    /// <summary>
    /// Project-level configuration for overriding default UI templates.
    /// Create one via Assets > Create > Framewerk > Template Set.
    /// Place at Assets/Settings/Framewerk/TemplateSetConfig.asset for auto-discovery.
    /// </summary>
    [CreateAssetMenu(fileName = "TemplateSetConfig", menuName = "Framewerk/Template Set")]
    public class TemplateSetConfig : ScriptableObject
    {
        [Header("Template Set Info")]
        [Tooltip("Name of this template set")]
        public string TemplateSetName;

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

        [Header("Tab Templates")]
        [Tooltip("Override for Vertical Tab Container template")]
        public GameObject VerticalTabContainerTemplate;
        
        [Tooltip("Override for Vertical Tab Item template")]
        public GameObject VerticalTabTemplate;
        
        [Tooltip("Override for Horizontal Tab Container template")]
        public GameObject HorizontalTabContainerTemplate;
        
        [Tooltip("Override for Horizontal Tab Item template")]
        public GameObject HorizontalTabTemplate;

        [Header("ViewStack Templates")]
        [Tooltip("Override for ViewStack component template")]
        public GameObject ViewStackTemplate;

        /// <summary>
        /// Gets the template prefab for the specified component type.
        /// </summary>
        /// <param name="componentType">Integer value of ComponentType enum</param>
        /// <returns>The override template or null if not set</returns>
        public GameObject GetTemplate(int componentType)
        {
            // ComponentType enum: Popup=0, List=1, ListItem=2, View=3, VerticalTabs=4, HorizontalTabs=5, ViewStack=6
            switch (componentType)
            {
                case 0: return PopupTemplate;
                case 1: return ListTemplate;
                case 2: return ListItemTemplate;
                case 3: return ViewTemplate;
                case 4: return VerticalTabContainerTemplate;
                case 5: return HorizontalTabContainerTemplate;
                case 6: return ViewStackTemplate;
                default: return null;
            }
        }

        /// <summary>
        /// Gets the tab item template for a tab container type.
        /// </summary>
        /// <param name="componentType">Integer value of ComponentType enum (VerticalTabs or HorizontalTabs)</param>
        /// <returns>The tab item template or null if not set</returns>
        public GameObject GetTabItemTemplate(int componentType)
        {
            switch (componentType)
            {
                case 4: return VerticalTabTemplate;
                case 5: return HorizontalTabTemplate;
                default: return null;
            }
        }

        /// <summary>
        /// Checks if this template set has an override for the specified component type.
        /// </summary>
        /// <param name="componentType">Integer value of ComponentType enum</param>
        /// <returns>True if an override is set</returns>
        public bool HasTemplate(int componentType)
        {
            return GetTemplate(componentType) != null;
        }
    }
}
