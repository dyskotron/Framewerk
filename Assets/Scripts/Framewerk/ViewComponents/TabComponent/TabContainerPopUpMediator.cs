using Framewerk.Core;
using Framewerk.Managers.Popup;

namespace Framewerk.ViewComponents.TabComponent
{
    public abstract class TabContainerPopUpMediator<T> : TabContainerMediator<T>, IPopupMediator where T : TabContainerView
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