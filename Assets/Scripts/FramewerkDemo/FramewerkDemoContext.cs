using Framewerk;
using Framewerk.AppStateMachine;
using Framewerk.StrangeCore;
using FramewerkDemo.Examples;
using FramewerkDemo.Examples.ExampleListPanel;
using FramewerkDemo.Examples.ExamplePopup;
using FramewerkDemo.MainMenu;
using FramewerkDemo.MainMenu.Controller;
using FramewerkDemo.MainMenu.Model;
using Plugins.Framewerk;
using strange.extensions.context.impl;

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

            // Framewerk core
            injectionBinder.Bind<ViewConfig>().ToValue(_viewConfig);
            InstallBundle<FramewerkCoreBundle>();
            InstallBundle<PopupsBundle>();

            // FSM
            injectionBinder.Bind<IAppFsm>().To<AppFsm>().ToSingleton();
            injectionBinder.Bind<MenuScreen>().To<MenuScreen>();
            injectionBinder.Bind<ExamplesScreen>().To<ExamplesScreen>();
            injectionBinder.Bind<AppStateEnterSignal>().ToSingleton();
            injectionBinder.Bind<AppStateExitSignal>().ToSingleton();

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
