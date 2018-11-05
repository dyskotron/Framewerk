using Framewerk.Events;

namespace Framewerk.ViewComponents.ListComponent
{
    public class ListItemSelectedEvent : AbstractEvent
    {
        public int ItemIndex { get; set; }
        
        public ListItemSelectedEvent(int index)
        {
            ItemIndex = index;
        }
    }
}