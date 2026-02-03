using Framewerk.Managers;
using Plugins.Framewerk;
using strange.extensions.command.impl;

namespace FramewerkDemo.ListPopupDemo
{
    public class ListPopupDemoStartCommand : Command
    {
        [Inject] public IUiManager UiManager { get; set; }
        [Inject] public ViewConfig ViewConfig { get; set; }

        public override async void Execute()
        {
            // PopupManager is now auto-configured via ViewConfig injection
            // ViewConfig.ContextPrefixSO should reference a ContextPrefix SO with Prefix="ListPopupDemo"

            // Load and instantiate the ItemList at startup
            // For Lists, we build the address explicitly since UiManager's default TypeKey is empty
            // The addressable asset should be at: ListPopupDemo/UI/List/ItemList
            string contextPrefix = ViewConfig.ContextPrefixSO?.Prefix;
            string listAddress = AddressBuilder.BuildAddress(
                contextPrefix, 
                null,  // no custom prefix
                AddressBuilder.TypeKeys.List, 
                "ItemList");
            await UiManager.InstantiateViewAsync(listAddress, ViewConfig.UiDefault);
        }
    }
}
