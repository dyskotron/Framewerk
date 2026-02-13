using System.Collections.Generic;
using Framewerk.UI.List;
using UnityEngine;

namespace Framewerk.Examples.ListExample
{
    public class ContactListMediator : ListMediator<ContactListView, ContactData>
    {
        [Inject] public ContactListDataSignal ContactListDataSignal { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();
            ContactListDataSignal.AddListener(OnDataReceived);
        }

        public override void OnRemove()
        {
            ContactListDataSignal.RemoveListener(OnDataReceived);
            base.OnRemove();
        }

        private void OnDataReceived(List<ContactData> data)
        {
            SetData(data);
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
