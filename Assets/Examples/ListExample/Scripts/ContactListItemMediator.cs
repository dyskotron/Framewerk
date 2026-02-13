using Framewerk.UI.List;

namespace Framewerk.Examples.ListExample
{
    public class ContactListItemMediator : ListItemMediator<ContactListItemView, ContactData>
    {
        public override void SetData(ContactData dataProvider, int index)
        {
            base.SetData(dataProvider, index);
            
            View.Label.text = $"{dataProvider.Name} - {dataProvider.Phone}";
        }
    }
}
