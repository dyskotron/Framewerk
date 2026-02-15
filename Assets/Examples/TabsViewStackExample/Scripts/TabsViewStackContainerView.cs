using strange.extensions.mediation.impl;
using UnityEngine;

namespace Framewerk.Examples.TabsViewStackExample
{
    /// <summary>
    /// Container view for the TabsViewStack example.
    /// Holds references to the layout areas where child views will be instantiated.
    /// This is just the shell - the mediator spawns the actual ViewGroup.
    /// </summary>
    public class TabsViewStackContainerView : View
    {
        [Header("Content Areas")]
        [Tooltip("Parent transform for the tab container view")]
        public Transform TabsParent;

        [Tooltip("Parent transform for the view stack view")]
        public Transform ContentParent;
    }
}
