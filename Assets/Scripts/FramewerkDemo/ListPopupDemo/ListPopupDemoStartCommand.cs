using Framewerk.Managers;
using Framewerk.Popups;
using Plugins.Framewerk;
using strange.extensions.command.impl;

namespace FramewerkDemo.ListPopupDemo
{
    public class ListPopupDemoStartCommand : Command
    {
        [Inject] public IPopupManager PopupManager { get; set; }
        [Inject] public IUiManager UiManager { get; set; }
        [Inject] public ViewConfig ViewConfig { get; set; }

        public override async void Execute()
        {
            PopupManager.Init("ListPopupDemo", ViewConfig.Popups);

            // Load and instantiate the ItemList at startup
            await UiManager.InstantiateViewAsync("ListPopupDemo/ItemList", ViewConfig.UiDefault);
        }
    }
}
