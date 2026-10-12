using Framewerk.UI.List;
using TMPro;

namespace Framewerk.Examples.ListExample
{
    public class ContactListItemMediator : ListItemMediator<ContactListItemView, ContactData>
    {
        public override void SetData(ContactData dataProvider, int index)
        {
            base.SetData(dataProvider, index);

            View.Label.text = $"{dataProvider.Name} - {dataProvider.Phone}";
        }

        public override void SetHovered(bool hovered)
        {
            base.SetHovered(hovered);

            View.Label.fontStyle = hovered ? FontStyles.Bold : FontStyles.Normal;
        }
    }
}
