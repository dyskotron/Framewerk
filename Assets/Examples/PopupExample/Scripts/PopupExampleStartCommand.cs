using Framewerk.Popups;
using Plugins.Framewerk;
using strange.extensions.command.impl;
using UnityEngine;

namespace Framewerk.Examples.PopupExample
{
    public class PopupExampleStartCommand : Command
    {
        [Inject] public ViewConfig ViewConfig { get; set; }
        [Inject] public IPopupManager PopupManager { get; set; }

        public override void Execute()
        {
            // Show the basic popup on start using Promise-based method for DI support
            PopupManager.InstantiatePopupAsync<BasicPopupView>(new[]
            {
                new PopupButtonSetting
                {
                    optionText = "Test",
                    closesPopup = false,
                    clickHandler = () => Debug.Log("[PopupExample] Test button clicked!")
                },
                new PopupButtonSetting
                {
                    optionText = "Close",
                    closesPopup = true
                }
            });
        }
    }
}
