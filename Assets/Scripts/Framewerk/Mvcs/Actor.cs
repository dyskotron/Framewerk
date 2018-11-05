using Framewerk.Core;
using Framewerk.Events;

namespace Framewerk.Mvcs
{
    /// <summary>
    /// Base class for Models & Services with basic Injections
    /// </summary>
    public class Actor
    {
        [Inject] protected IEventDispatcher _eventDispatcher;

        /// <summary>
        /// As actors (Mainly Models and Services) should not listen to events but just dispatch them,
        /// this only way how they need to interact with dispatcher
        /// </summary>
        /// <param name="e"></param>
        protected void DispatchEvent<T>(T e) where T : AbstractEvent
        {
            _eventDispatcher.DispatchEvent(e);
        }
    }
}