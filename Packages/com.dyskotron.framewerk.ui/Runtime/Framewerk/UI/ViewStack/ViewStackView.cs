using strange.extensions.mediation.impl;
using UnityEngine;

namespace Framewerk.UI.ViewStack
{
    /// <summary>
    /// View component for ViewStack.
    /// Just holds reference to Content container where children live.
    /// </summary>
    public class ViewStackView : View
    {
        [Header("ViewStack")]
        [Tooltip("Container for stacked children. Children are just GameObjects in hierarchy.")]
        public RectTransform Content;
    }
}
