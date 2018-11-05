using Framewerk.Core;
using Framewerk.Mvcs;

namespace Framewerk.Managers.Popup
{
    public interface IPopupMediator : IMediator
    {
        void HandleBackButton();
        void Close();
    }

    public abstract class PopupMediator<T> : Mediator<T>, IPopupMediator where T : View
    {
        [Inject] protected IPopUpManager PopupManager;

        protected override void Init()
        {
            EnableHidingByCanvas();
            
            base.Init();
        }

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