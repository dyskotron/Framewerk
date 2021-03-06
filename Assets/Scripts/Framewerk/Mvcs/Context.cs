﻿using Framewerk.Core;
using Framewerk.Events;
using Framewerk.ViewUtils;
using UnityEngine;

namespace Framewerk.Mvcs
{
    public abstract class Context
    {
        protected IInjector Injector;
        protected ICommandMap CommandMap;
        protected IEventDispatcher EventDispatcher;
        protected GameObject ScriptsGO;

        protected Context(BaseViewSettings viewSettings)
        {
            Injector = new Injector();
            EventDispatcher = new EventDispatcher();
            CommandMap = new CommandMap(Injector, EventDispatcher);

            Injector.MapValue(Injector);
            Injector.MapValue(EventDispatcher);
            Injector.MapValue(viewSettings);

            Startup();
        }

        protected void Close()
        {
            Injector.Destroy();
            Exit();
        }

        protected virtual void Startup()
        {
            Injector.Init();
            EventDispatcher.DispatchEvent(new FramewerkStartEvent());
        }

        protected abstract void Exit();
    }
}