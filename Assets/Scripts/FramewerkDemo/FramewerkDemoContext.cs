using Framewerk;
using Framewerk.AppStateMachine;
using Framewerk.Managers;
using Framewerk.Popups;
using Framewerk.StrangeCore;
using FramewerkDemo.Examples;
using FramewerkDemo.Examples.ExampleListPanel;
using FramewerkDemo.Examples.ExamplePopup;
using FramewerkDemo.MainMenu;
using FramewerkDemo.MainMenu.Controller;
using FramewerkDemo.MainMenu.Model;
using Plugins.Framewerk;
using strange.extensions.context.impl;
using strange.extensions.injector.api;

namespace FramewerkDemo
{
    public class FramewerkDemoContext : FramewerkMVCSContext
    {
        private readonly ViewConfig _viewConfig;

        public FramewerkDemoContext(ContextView view, ViewConfig viewConfig) : base(view, true)
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

            // FSM
            injectionBinder.Bind<IAppFsm>().To<AppFsm>().ToSingleton();
            injectionBinder.Bind<AppStateEnterSignal>().ToSingleton();
            injectionBinder.Bind<AppStateExitSignal>().ToSingleton();

            // Popups
            injectionBinder.Bind<IPopupManager>().To<PopupManager>().ToSingleton();
            injectionBinder.Bind<PopupOpenedSignal>().ToSingleton();
            injectionBinder.Bind<PopupClosedSignal>().ToSingleton();

            // MODEL
            injectionBinder.Bind<IMenuModel>().To<MenuModel>().ToSingleton();

            // VIEW
            mediationBinder.Bind<MenuItemView>().To<MenuItemMediator>();
            mediationBinder.Bind<MenuView>().To<MenuPanelMediator>();
            mediationBinder.Bind<TopMenuView>().To<TopMenuMediator>();
            mediationBinder.Bind<ExamplePopupView>().To<ExamplePopupMediator>();
            mediationBinder.Bind<ExampleListItemView>().To<ExampleLisItemMediator>();
            mediationBinder.Bind<ExampleListPanelView>().To<ExampleListPanelMediator>();

            // SIGNALS
            injectionBinder.Bind<MenuItemSelectedSignal>().ToSingleton();
            injectionBinder.Bind<ShowMenuSignal>().ToSingleton();

            // COMMANDS
            commandBinder.Bind<ContextStartSignal>().To<FramewerkStartCommand>();
            commandBinder.Bind<MenuItemSelectedSignal>().To<MenuItemSelectedCommand>();
            commandBinder.Bind<ShowMenuSignal>().To<ShowMenuCommand>();
        }
    }
}
