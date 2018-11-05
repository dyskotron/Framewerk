using Framewerk.Events;

namespace Framewerk.ViewComponents.ListComponent
{
    public class ListItemClickedInternalEvent : AbstractEvent
    {
        public int ItemIndex { get; set; }
        
        public ListItemClickedInternalEvent(int index)
        {
            ItemIndex = index;
        }
    }
}