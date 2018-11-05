namespace Framewerk.ViewComponents.TabComponent
{
    public interface IPageTabContainerComponent : ITabContainerComponent
    {
        void OnPageSwitch(int pageIndex, bool value);
    }
}