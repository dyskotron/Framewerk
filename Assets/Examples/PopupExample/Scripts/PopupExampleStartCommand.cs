using Framewerk.Popups;
using strange.extensions.command.impl;
using UnityEngine;

namespace Framewerk.Examples.PopupExample
{
    public class PopupExampleStartCommand : Command
    {
        [Inject] public IPopupManager PopupManager { get; set; }

        public override async void Execute()
        {
            // Show the basic popup on start using async method for DI support
            await PopupManager.InstantiatePopupAsync<BasicPopupView>(new[]
            {
                new PopupButtonSetting 
                { 
                    optionText = "Test", 
                    closesPopup = false,
                    clickHandler = () => Debug.Log("[PopupExample] Test button clicked!")
                },
                PopupButtonSetting.CloseButton
            });
        }
    }
}
