namespace Framewerk.ViewComponents.TabComponent
{
    public interface ITabContainerComponent
    {
        void OnTabSwitch(int tabIndex, bool value);
        void OnTabCreated(int index, ITabMediator tab);
        void Destroy();
    }
}