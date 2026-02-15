using Framewerk.UI.List;
using UnityEngine;

namespace Framewerk.Examples.ListExample
{
    public class ContactListMediator : ListMediator<ContactListView, ContactData>
    {
        [Inject] public ContactDataProvider ContactDataProvider { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();
            SetData(ContactDataProvider.GetContactData());
        }

        protected override void ListItemClicked(int index, ContactData dataProvider)
        {
            base.ListItemClicked(index, dataProvider);
            
            if (dataProvider != null)
            {
                Debug.Log($"[ListExample] Clicked: {dataProvider.Name}");
            }
        }
    }
}
