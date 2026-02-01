using Framewerk;
using Framewerk.Managers;
using Framewerk.Popups;
using Framewerk.StrangeCore;
using FramewerkDemo.ListPopupDemo.Signals;
using Plugins.Framewerk;
using strange.extensions.context.impl;
using strange.extensions.injector.api;
using System.Collections.Generic;

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

            injectionBinder.Bind<IInjector>().To(injectionBinder.injector);

            // Framewerk core
            injectionBinder.Bind<ViewConfig>().ToValue(_viewConfig);
            injectionBinder.Bind<ICoroutineManager>().ToValue(CoroutineManager.Instance);
            injectionBinder.Bind<IUpdater>().ToValue(Updater.Instance);
            injectionBinder.Bind<IAppMonitor>().ToValue(AppMonitor.Instance);
            injectionBinder.Bind<IAssetManager>().To<AssetManager>().ToSingleton();
            injectionBinder.Bind<IUiManager>().To<UiManager>().ToSingleton();

            // Popups
            injectionBinder.Bind<IPopupManager>().To<PopupManager>().ToSingleton();
            injectionBinder.Bind<List<PopupButtonSetting>>().ToValue(new List<PopupButtonSetting>());
            injectionBinder.Bind<PopupOpenedSignal>().ToSingleton();
            injectionBinder.Bind<PopupClosedSignal>().ToSingleton();

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
