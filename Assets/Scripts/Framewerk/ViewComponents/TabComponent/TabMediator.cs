using Framewerk.Mvcs;
using UnityEngine;

namespace Framewerk.ViewComponents.TabComponent
{
    public interface ITabMediator : IMediator
    {
        int TabIndex { get; set; }
        void SetParent(RectTransform parent, bool worldPositionStays);
        void SetSiblingIndex(int tabIndex);
    }

    public class TabMediator<T> : Mediator<T>, ITabMediator where T : ITabView
    {
        public int TabIndex { get; set; }

        protected override void Init()
        {
            EnableHidingByCanvas();
            base.Init();
        }

        //TODO: page/tab scroll to tab component temp solution, remove!!
        public void SetParent(RectTransform parent, bool worldPositionStays)
        {
            UIgo.transform.SetParent(parent,worldPositionStays);
        }

        public void SetSiblingIndex(int tabIndex)
        {
            UIgo.transform.SetSiblingIndex(tabIndex);
        }
        //
    }
}