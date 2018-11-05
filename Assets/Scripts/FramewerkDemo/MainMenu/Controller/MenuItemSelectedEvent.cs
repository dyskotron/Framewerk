using Framewerk.Events;
using FramewerkDemo.MainMenu.Model;

namespace FramewerkDemo.MainMenu
{
    public class MenuItemSelectedEvent : AbstractEvent
    {
        public ExampleId ExampleId { get; private set; }

        public MenuItemSelectedEvent(ExampleId exampleId)
        {
            ExampleId = exampleId;
        }
    }
}