using Framewerk.UI.List;

namespace Framewerk.Examples.ScreenFsmExample
{
    public class MainMenuItemMediator : ListItemMediator<MainMenuItemView, MenuItemData>
    {
        public override void SetData(MenuItemData dataProvider, int index)
        {
            base.SetData(dataProvider, index);
            
            View.Label.text = dataProvider.Title;
        }

    }
}
