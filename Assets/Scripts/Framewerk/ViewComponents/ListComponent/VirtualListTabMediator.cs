using Framewerk.ViewComponents.TabComponent;
using UnityEngine;

namespace Framewerk.ViewComponents.ListComponent
{   
    /// <inheritdoc cref="ListContainerTabMediator{TView,TMediator,TData}" />
    public class VirtualListTabMediator<TView, TMediator, TData> : VirtualListMediator<TView, TMediator, TData>, ITabMediator where TView : ListContainerView, ITabView
        where TMediator : class, IListItemMediator<TData>, new()
        where TData : class, IListItemDataProvider
    {
        public int TabIndex { get; set; }

        protected override void Init()
        {
            EnableHidingByCanvas();
            base.Init();
        }

        //TODO temp scrol to tab solution remove!
        public void SetParent(RectTransform parent, bool worldPositionStays)
        {
            UIgo.transform.SetParent(parent, worldPositionStays);
        }

        public void SetSiblingIndex(int tabIndex)
        {
            UIgo.transform.SetSiblingIndex(tabIndex);
        }
    }
}