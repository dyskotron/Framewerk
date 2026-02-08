using Framewerk;
using Framewerk.StrangeCore;
using FramewerkDemo.ListPopupDemo.Signals;
using Plugins.Framewerk;
using strange.extensions.context.impl;

namespace FramewerkDemo.ListPopupDemo
{
    public class ListPopupDemoContext : FramewerkMVCSContext
    {
        private readonly ViewConfig _viewConfig;

        public ListPopupDemoContext(ContextView view, ViewConfig viewConfig) : base(view, true)
        {
            _viewConfig = viewConfig;
        }

        protected override void mapBindings()
        {
            base.mapBindings();

            // Framewerk core
            injectionBinder.Bind<ViewConfig>().ToValue(_viewConfig);
            InstallBundle<FramewerkCoreBundle>();
            InstallBundle<PopupsBundle>();

            // VIEW
            mediationBinder.Bind<ItemListView>().To<ItemListMediator>();
            mediationBinder.Bind<ItemListItemView>().To<ItemListItemMediator>();
            mediationBinder.Bind<ItemPopupView>().To<ItemPopupMediator>();

            // SIGNALS
            injectionBinder.Bind<ItemClickedSignal>().ToSingleton();

            // COMMANDS
            commandBinder.Bind<ContextStartSignal>().To<ListPopupDemoStartCommand>();
            commandBinder.Bind<ItemClickedSignal>().To<ShowItemPopupCommand>();
        }
    }
}
