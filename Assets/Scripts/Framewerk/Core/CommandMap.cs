using System;
using System.Collections.Generic;
using Framewerk.Events;
using Framewerk.Mvcs;
using UnityEngine;

namespace Framewerk.Core
{
    public interface ICommandMap
    {
        void MapCommand<TEvent, TCommand>() where TEvent : AbstractEvent where TCommand : ICommand;
        void UnmapCommand<TEvent>() where TEvent : AbstractEvent;
    }

    //TODO: detain/release mechanism from robotlegs
    public class CommandMap : ICommandMap
    {
        private readonly Dictionary<Type, Type> _GHeventToCommandMap;

        private readonly IInjector _injector;
        private readonly IEventDispatcher _observer;

        public CommandMap(IInjector injector, IEventDispatcher observer)
        {
            _eventToCommandMap = new Dictionary<Type, Type>();
            _injector = injector;
            _observer = observer;
        }

        public void MapCommand<TEvent, TCommand>() where TEvent : AbstractEvent where TCommand : ICommand
        {
            //Debug.LogWarning("<color=\"green\">"  + this + "MapCommand : " + typeof(TEvent) + "</color>");
            _eventToCommandMap[typeof(TEvent)] = typeof(TCommand);
            _observer.AddListener<TEvent>(EventHandler<TEvent>);
        }

        public void UnmapCommand<TEvent>() where TEvent : AbstractEvent
        {
            if (!_eventToCommandMap.ContainsKey(typeof(TEvent)))
            {
                Debug.LogWarningFormat("<color=\"aqua\">"  + this + "UNMAP : THERE IS NO COMMAND MAPPING FOR {0}</color>", typeof(TEvent));
                return;
            }
            _eventToCommandMap.Remove(typeof(TEvent));
            _observer.RemoveListener<TEvent>(EventHandler<TEvent>);
        }

        private void EventHandler<TEvent>(AbstractEvent e)
        {
            //TODO event type could be - forced(has to be processed), queued(until there will be right mapping for it), weak(or so - is processed when there is processor)

            var commandType = _eventToCommandMap[e.GetType()];
            var command = (ICommand) Activator.CreateInstance(commandType);

            if (command == null)
            {
                Debug.LogWarningFormat("<color=\"aqua\">"  + this + " : MAPPED CLASS FOR {0} IS NOT {1}</color>", e.GetType(), typeof(ICommand));
                return;
            }

            _injector.MapValue(e, e.GetType());
            _injector.InjectInto(command);
            _injector.Unmap(e);

            //Debug.LogWarning("<color=\"aqua\">"  + this + " Execute: " + command + "</color>");

            command.Execute();
        }
    }
}