using Framewerk.Core;
using Framewerk.Mvcs;
using FramewerkDemo.MainMenu.Controller;
using FramewerkDemo.MainMenu.Model;

namespace FramewerkDemo.Examples
{
    public class TopMenuMediator : Mediator<TopMenuView>
    {
        [Inject] private IMenuModel _menuModel;
        
        protected override void Init()
        {
            base.Init();
            
            AttachButtonListener(View.CloseButton, CloseButtonClickedHandler);
        }

        private void CloseButtonClickedHandler()
        {
            DispatchEvent(new ShowMenuEvent());
        }

        public void SetExampleId(ExampleId exampleId)
        {
            View.TitleText.text = _menuModel.GetMenuItemById(exampleId).Label;
        }
    }
}