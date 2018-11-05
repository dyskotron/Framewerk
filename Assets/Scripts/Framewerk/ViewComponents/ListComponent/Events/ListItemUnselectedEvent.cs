using Framewerk.Events;

namespace Framewerk.ViewComponents.ListComponent
{
    public class ListItemUnselectedEvent : AbstractEvent
    {
        public int ItemIndex { get; set; }
        
        public ListItemUnselectedEvent(int index)
        {
            ItemIndex = index;
        }
    }
}