using Framewerk.ViewComponents.ListComponent;
using FramewerkDemo.MainMenu.Model;

namespace FramewerkDemo.MainMenu
{
    public class MenuItemMediator : ListItemMediator<MenuItemView, MenuDataProvider>
    {
        public override void SetData(MenuDataProvider dataProvider, int index)
        {
            base.SetData(dataProvider, index);
            View.SetIndex(index + 1);
            View.SetLabel(dataProvider.ItemIdId.ToString());
        }
    }
}