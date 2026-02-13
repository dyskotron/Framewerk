using System.Collections.Generic;
using Framewerk.Managers;
using strange.extensions.command.impl;

namespace Framewerk.Examples.ListExample
{
    public class ListExampleStartCommand : Command
    {
        [Inject] public IUiManager UiManager { get; set; }
        [Inject] public ContactListDataSignal ContactListDataSignal { get; set; }

        public override async void Execute()
        {
            // Create sample contact data
            var contacts = new List<ContactData>
            {
                new ContactData { Name = "Alice Johnson", Phone = "+1 555-0101" },
                new ContactData { Name = "Bob Smith", Phone = "+1 555-0102" },
                new ContactData { Name = "Carol Davis", Phone = "+1 555-0103" },
                new ContactData { Name = "David Wilson", Phone = "+1 555-0104" },
                new ContactData { Name = "Eve Brown", Phone = "+1 555-0105" }
            };

            // Instantiate the contact list view
            await UiManager.InstantiateViewAsync<ContactListView>();
            
            // Send data to the list via signal
            ContactListDataSignal.Dispatch(contacts);
        }
    }
}
