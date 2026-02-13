using Framewerk;
using Framewerk.Managers;
using Framewerk.Popups;
using Framewerk.StrangeCore;
using Plugins.Framewerk;
using strange.extensions.context.impl;

namespace Framewerk.Examples.PopupExample
{
    public class PopupExampleContext : FramewerkMVCSContext
    {
        private readonly ViewConfig _viewConfig;

        public PopupExampleContext(ContextView view, ViewConfig viewConfig) : base(view, true)
        {
            _viewConfig = viewConfig;
        }

        protected override void mapBindings()
        {
            base.mapBindings();

            // Framewerk core
            injectionBinder.Bind<ViewConfig>().ToValue(_viewConfig);
            
            // Framewerk managers (bind interfaces to implementations)
            injectionBinder.Bind<IAssetManager>().To<AssetManager>().ToSingleton();
            injectionBinder.Bind<IUiManager>().To<UiManager>().ToSingleton();
            injectionBinder.Bind<IPopupManager>().To<PopupManager>().ToSingleton();
            
            // Framewerk signals
            injectionBinder.Bind<PopupOpenedSignal>().ToSingleton();
            injectionBinder.Bind<PopupClosedSignal>().ToSingleton();

            // MEDIATION
            mediationBinder.Bind<BasicPopupView>().To<BasicPopupMediator>();

            // COMMANDS
            commandBinder.Bind<ContextStartSignal>().To<PopupExampleStartCommand>();
        }
    }
}
