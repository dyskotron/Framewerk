using System.Collections.Generic;

namespace Framewerk.Examples.ScreenFsmExample
{
    public class MainMenuModel
    {
        public List<MenuItemData> GetMenuItems()
        {
            return new List<MenuItemData>
            {
                new MenuItemData { Title = "About", TargetScreen = ScreenType.About },
                new MenuItemData { Title = "Play Game", TargetScreen = ScreenType.Game },
                new MenuItemData { Title = "Settings", TargetScreen = ScreenType.Settings },
                new MenuItemData { Title = "Leaderboards", TargetScreen = ScreenType.Leaderboards }
            };
        }
    }
}
