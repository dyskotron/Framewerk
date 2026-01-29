namespace Framewerk.Popups.OkCancelWindow
{
    public class OkCancelWindowMediator : PopupMediator<OkCancelWindowView>
    {
        [Inject] public string Message { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();
            
            Init(View);
        }

        protected override void Init(PopupView popupView)
        {
            base.Init(popupView);

            View.MessageText.text = Message;
        }        
    }
}