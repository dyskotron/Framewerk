using System.Collections.Generic;
using Framewerk.UI.List;
using FramewerkDemo.MainMenu.Controller;
using FramewerkDemo.MainMenu.Model;

namespace FramewerkDemo.MainMenu
{
    public class MenuPanelMediator : ListMediator<MenuView, MenuDataProvider>
    {
        [Inject] public IMenuModel MenuModel { get; set; }
        [Inject] public MenuItemSelectedSignal MenuItemSelectedSignal { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();

            SetData(MenuModel.GetMenuData());
        }

        protected override void ListItemClicked(int index, MenuDataProvider dataProvider)
        {
            MenuItemSelectedSignal.Dispatch(dataProvider.ItemIdId);
        }
    }
}
