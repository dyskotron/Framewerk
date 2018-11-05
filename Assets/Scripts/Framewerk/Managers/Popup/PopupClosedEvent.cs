using System;
using Framewerk.Events;

namespace Framewerk.Managers.Popup
{
    public class PopupClosedEvent : AbstractEvent
    {
        public Type PopupType {get; private set; }

        public PopupClosedEvent(Type type)
        {
            PopupType = type;
        }
    }
}