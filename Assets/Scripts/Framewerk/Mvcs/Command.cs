using Framewerk.Core;
using Framewerk.Events;

namespace Framewerk.Mvcs
{
    public interface ICommand
    {
        void Execute();
    }

    public abstract class Command : ICommand
    {
        //_eventDispatcher should be private but because of some limitations of C# reflection
        // injector can't see private properties of extended classes
        [Inject] protected IEventDispatcher _eventDispatcher;

        public abstract void Execute();

        /// <summary>
        /// As Commands should not listen to events but just dispatch them,
        /// this only way how they need to interact with dispatcher
        /// </summary>
        /// <param name="e"></param>
        protected void DispatchEvent<T>(T e) where T : AbstractEvent
        {
            _eventDispatcher.DispatchEvent(e);
        }
    }
}