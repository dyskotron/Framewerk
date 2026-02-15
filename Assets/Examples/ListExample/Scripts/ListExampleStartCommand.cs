using Framewerk.Managers;
using strange.extensions.command.impl;

namespace Framewerk.Examples.ListExample
{
    public class ListExampleStartCommand : Command
    {
        [Inject] public IUiManager UiManager { get; set; }

        public override async void Execute()
        {
            await UiManager.InstantiateViewAsync<ContactListView>();
        }
    }
}
