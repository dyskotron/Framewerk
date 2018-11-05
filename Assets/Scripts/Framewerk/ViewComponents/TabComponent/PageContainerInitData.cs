namespace Framewerk.ViewComponents.TabComponent
{
    public interface IPageTabContainerInitData : ITabContainerInitData
    {
        int PageIndex { get; }
    }

    public class PageTabContainerInitData : TabContainerInitData, IPageTabContainerInitData
    {
        public int PageIndex { get; private set; }

        public PageTabContainerInitData(int pageIndex, int tabIndex) : base(tabIndex)
        {
            PageIndex = pageIndex;
        }
    }
}