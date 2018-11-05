using Framewerk.Managers;

namespace Framewerk.ViewComponents.TabComponent
{
    public interface ITabContainerInitData
    {
        int TabIndex { get; }
    }

    public class TabContainerInitData : ViewInitData, ITabContainerInitData
    {
        public int TabIndex { get; private set; }

        public TabContainerInitData(int tabIndex)
        {
            TabIndex = tabIndex;
        }
    }
}