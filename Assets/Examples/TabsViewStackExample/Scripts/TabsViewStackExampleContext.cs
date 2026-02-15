using Plugins.Framewerk;
using Framewerk.Managers;
using Framewerk.StrangeCore;
using Framewerk.UI.ViewStack;
using strange.extensions.context.impl;

namespace Framewerk.Examples.TabsViewStackExample
{
    public class TabsViewStackExampleContext : FramewerkMVCSContext
    {
        private readonly ViewConfig _viewConfig;

        public TabsViewStackExampleContext(ContextView view, ViewConfig viewConfig) : base(view, true)
        {
            _viewConfig = viewConfig;
        }

        protected override void mapBindings()
        {
            base.mapBindings();

            // Framewerk core
            injectionBinder.Bind<ViewConfig>().ToValue(_viewConfig);

            // Data providers
            injectionBinder.Bind<TabsViewStackDataProvider>().ToSingleton();

            // Framewerk managers
            injectionBinder.Bind<IAssetManager>().To<AssetManager>().ToSingleton();
            injectionBinder.Bind<IUiManager>().To<UiManager>().ToSingleton();

            // MEDIATION
            mediationBinder.Bind<TabsViewStackContainerView>().To<TabsViewStackContainerMediator>();
            mediationBinder.Bind<TabContainerView>().To<TabContainerMediator>();
            mediationBinder.Bind<TabItemView>().To<TabItemMediator>();
            mediationBinder.Bind<ViewStackView>().To<TabsViewStackMediator>();
            mediationBinder.Bind<ContentPanelView>().To<ContentPanelMediator>();

            // COMMANDS
            commandBinder.Bind<ContextStartSignal>().To<TabsViewStackExampleStartCommand>();
        }
    }
}
