using Framewerk.Events;

namespace Framewerk.ViewComponents.ListComponent
{
    public class ListItemClickedEvent : AbstractEvent
    {
        public int ItemIndex { get; set; }
        
        public ListItemClickedEvent(int index)
        {
            ItemIndex = index;
        }
    }
}