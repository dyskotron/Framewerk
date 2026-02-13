using Plugins.Framewerk;
using Framewerk.Managers;
using Framewerk.StrangeCore;
using strange.extensions.context.impl;

namespace Framewerk.Examples.ListExample
{
    public class ListExampleContext : FramewerkMVCSContext
    {
        private readonly ViewConfig _viewConfig;

        public ListExampleContext(ContextView view, ViewConfig viewConfig) : base(view, true)
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

            // SIGNALS
            injectionBinder.Bind<ContactListDataSignal>().ToSingleton();

            // MEDIATION
            mediationBinder.Bind<ContactListView>().To<ContactListMediator>();
            mediationBinder.Bind<ContactListItemView>().To<ContactListItemMediator>();

            // COMMANDS
            commandBinder.Bind<ContextStartSignal>().To<ListExampleStartCommand>();
        }
    }
}
