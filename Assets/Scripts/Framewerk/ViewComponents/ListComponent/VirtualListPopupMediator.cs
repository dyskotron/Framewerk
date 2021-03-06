﻿using Framewerk.Core;
using Framewerk.Managers.Popup;

namespace Framewerk.ViewComponents.ListComponent
{
    public abstract class VirtualListPopupMediator<TView, TMediator, TData> : VirtualListMediator<TView, TMediator, TData>, IPopupMediator where TView : ListContainerView
                                                                                                                where TMediator : class, IListItemMediator<TData>, new()
                                                                                                                where TData : class, IListItemDataProvider
    {
        [Inject] protected IPopUpManager PopupManager;

        public virtual void HandleBackButton()
        {
            Close();
        }

        public virtual void Close()
        {
            PopupManager.ClosePopUp(this);
        }
    }
}