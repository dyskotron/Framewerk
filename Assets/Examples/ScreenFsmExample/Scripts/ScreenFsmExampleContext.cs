using Framewerk.AppStateMachine;
using Plugins.Framewerk;
using Framewerk.Managers;
using Framewerk.StrangeCore;
using strange.extensions.context.impl;

namespace Framewerk.Examples.ScreenFsmExample
{
    public class ScreenFsmExampleContext : FramewerkMVCSContext
    {
        private readonly ViewConfig _viewConfig;

        public ScreenFsmExampleContext(ContextView view, ViewConfig viewConfig) : base(view, true)
        {
            _viewConfig = viewConfig;
        }

        protected override void mapBindings()
        {
            base.mapBindings();

            // Framewerk core
            injectionBinder.Bind<ViewConfig>().ToValue(_viewConfig);
            
            // Framewerk managers
            injectionBinder.Bind<IAssetManager>().To<AssetManager>().ToSingleton();
            injectionBinder.Bind<IUiManager>().To<UiManager>().ToSingleton();

            // FSM
            injectionBinder.Bind<IAppFsm>().To<AppFsm>().ToSingleton();
            injectionBinder.Bind<AppStateEnterSignal>().ToSingleton();
            injectionBinder.Bind<AppStateExitSignal>().ToSingleton();

            // SCREENS
            injectionBinder.Bind<MenuScreen>().ToSingleton();
            injectionBinder.Bind<AboutScreen>().ToSingleton();
            injectionBinder.Bind<GameScreen>().ToSingleton();
            injectionBinder.Bind<SettingsScreen>().ToSingleton();
            injectionBinder.Bind<LeaderboardsScreen>().ToSingleton();

            // MODELS
            injectionBinder.Bind<MainMenuModel>().ToSingleton();

            // SIGNALS
            injectionBinder.Bind<NavigateToScreenSignal>().ToSingleton();

            // MEDIATION
            mediationBinder.Bind<MainMenuView>().To<MainMenuMediator>();
            mediationBinder.Bind<MainMenuItemView>().To<MainMenuItemMediator>();
            mediationBinder.Bind<AboutContentView>().To<AboutContentMediator>();
            mediationBinder.Bind<GameContentView>().To<GameContentMediator>();
            mediationBinder.Bind<SettingsContentView>().To<SettingsContentMediator>();
            mediationBinder.Bind<LeaderboardsContentView>().To<LeaderboardsContentMediator>();

            // COMMANDS
            commandBinder.Bind<ContextStartSignal>().To<ScreenFsmExampleStartCommand>();
            commandBinder.Bind<NavigateToScreenSignal>().To<NavigateToScreenCommand>();
        }
    }
}
