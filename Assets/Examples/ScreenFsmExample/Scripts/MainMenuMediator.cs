using Framewerk.UI.List;

namespace Framewerk.Examples.ScreenFsmExample
{
    public class MainMenuMediator : ListMediator<MainMenuView, MenuItemData>
    {
        [Inject] public NavigateToScreenSignal NavigateToScreenSignal { get; set; }
        [Inject] public MainMenuModel MenuModel { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();
            SetData(MenuModel.GetMenuItems());
        }

        protected override void ListItemSelected(int index, MenuItemData dataProvider)
        {
            base.ListItemSelected(index, dataProvider);
            
            if (dataProvider != null)
            {
                NavigateToScreenSignal.Dispatch(dataProvider.TargetScreen);
            }
        }
    }
}
