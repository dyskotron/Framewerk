using Framewerk.Popups;
using strange.extensions.command.impl;

namespace FramewerkDemo.ListPopupDemo
{
    public class ShowItemPopupCommand : Command
    {
        [Inject] public IPopupManager PopupManager { get; set; }
        [Inject] public ItemData ItemData { get; set; }

        public override async void Execute()
        {
            var injects = new object[] { ItemData };
            await PopupManager.InstantiatePopupAsync<ItemPopupView>(injects);
        }
    }
}
