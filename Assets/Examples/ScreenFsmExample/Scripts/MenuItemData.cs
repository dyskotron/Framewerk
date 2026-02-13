using Framewerk.UI.List;

namespace Framewerk.Examples.ScreenFsmExample
{
    public class MenuItemData : IListItemDataProvider
    {
        public string Title { get; set; }
        public ScreenType TargetScreen { get; set; }
    }

    public enum ScreenType
    {
        About,
        Game,
        Settings,
        Leaderboards
    }
}
