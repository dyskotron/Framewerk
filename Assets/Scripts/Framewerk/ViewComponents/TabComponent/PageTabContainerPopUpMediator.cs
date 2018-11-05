using Framewerk.Core;
using Framewerk.Managers.Popup;

namespace Framewerk.ViewComponents.TabComponent
{
    public abstract class PageTabContainerPopUpMediator<T> : PageTabContainerMediator<T>, IPopupMediator where T : PageTabContainerView
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

        public override void PreviousPage()
        {
            if (PageIndex <= 0)
                Close();
            else
                PageIndex--;
        }
    }
}