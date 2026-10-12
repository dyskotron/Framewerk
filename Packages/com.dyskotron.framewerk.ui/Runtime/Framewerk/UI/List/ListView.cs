using Framewerk.UI.Components;
using strange.extensions.mediation.impl;
using UnityEngine;

namespace Framewerk.UI.List
{
    public class ListView : View
    {
        [Header("List")]
        public RectTransform ContentsParent;
        public GameObject ItemPrefab;
        public GameObject EmptyContent;

        [Tooltip("What user input changes selection. Code can override it on the mediator at runtime.")]
        public SelectionTrigger SelectionTrigger = SelectionTrigger.Click;
        
        //public VirtualListScroller ListScroller;
        //public DragElement DragElement;
        //public bool UnselectAllOnDrag = false;
    }
}