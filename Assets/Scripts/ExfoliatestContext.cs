using Framewerk;
using Framewerk.StrangeCore;
using Plugins.Framewerk;
using strange.extensions.context.impl;

namespace SomeSpace
{
    public class ExfoliatestContext : FramewerkMVCSContext
    {
        private readonly ViewConfig _viewConfig;

        public ExfoliatestContext(ContextView view, ViewConfig viewConfig) : base(view, true)
        {
            _viewConfig = viewConfig;
        }

        protected override void mapBindings()
        {
            base.mapBindings();

            // Framewerk core
            injectionBinder.Bind<ViewConfig>().ToValue(_viewConfig);

            // COMMANDS
            commandBinder.Bind<ContextStartSignal>().To<ExfoliatestStartCommand>();
        }
    }
}
