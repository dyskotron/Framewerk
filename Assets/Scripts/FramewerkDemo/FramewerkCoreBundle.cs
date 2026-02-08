using Framewerk;
using Framewerk.Managers;
using Framewerk.StrangeCore.Bundles;
using strange.extensions.injector.api;

namespace FramewerkDemo
{
    public class FramewerkCoreBundle : BindingBundle
    {
        protected override void OnInstall()
        {
            // Expose injector for dynamic injection (views, mediators, etc.)
            BindInjection<IInjector>().ToValue(InjectionBinder.injector);

            // Tracked helpers — all bindings auto-cleaned on Uninstall()
            BindInjection<ICoroutineManager>().ToValue(CoroutineManager.Instance);
            BindInjection<IUpdater>().ToValue(Updater.Instance);
            BindInjection<IAppMonitor>().ToValue(AppMonitor.Instance);
            BindInjection<IAssetManager>().To<AssetManager>().ToSingleton();
            BindInjection<IUiManager>().To<UiManager>().ToSingleton();
        }
    }
}