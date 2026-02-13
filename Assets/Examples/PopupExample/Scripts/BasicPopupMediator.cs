using Framewerk.Popups;

namespace Framewerk.Examples.PopupExample
{
    public class BasicPopupMediator : PopupMediator<BasicPopupView>
    {
        public override void OnRegister()
        {
            base.OnRegister();
            Init(View);
        }
    }
}
