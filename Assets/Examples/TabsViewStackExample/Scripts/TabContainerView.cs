using Framewerk.UI.List;
using Framewerk.UI.ViewStack;
using UnityEngine;

namespace Framewerk.Examples.TabsViewStackExample
{
    public class TabContainerView : ListView
    {
        [Header("Tab Container")]
        public ViewStackView ContentStack;
        
        [Header("Content Area")]
        [Tooltip("Parent transform where the ViewStack will be created if ContentStack is not assigned")]
        public RectTransform ContentArea;
    }
}
