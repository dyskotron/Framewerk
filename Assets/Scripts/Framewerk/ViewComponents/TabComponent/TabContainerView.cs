using Framewerk.Mvcs;
using UnityEngine;

namespace Framewerk.ViewComponents.TabComponent
{
    public class TabContainerView : View
    {
        public RectTransform ContentsParent;
        public RectTransform TabTogglesParent;
        public GameObject[] TabsContentPrefabs;

    }
}