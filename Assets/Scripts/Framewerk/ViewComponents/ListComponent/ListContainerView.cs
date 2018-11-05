using Framewerk.Mvcs;
using Framewerk.Mvcs.UIElements;
using UnityEngine;
using UnityEngine.UI;

namespace Framewerk.ViewComponents.ListComponent
{
    public class ListContainerView : View
    {
        [Header("List Container")]
        public RectTransform ContentsParent;
        public GameObject ItemPrefab;
        public GameObject EmptyContent;
        
        public VirtualListScroller ListScroller;
        public DragElement DragElement;
        public bool UnselectAllOnDrag = false;
    }
}