using Framewerk;
using Framewerk.Managers;
using Framewerk.StrangeCore.Bundles;

namespace FramewerkDemo
{
    public class FramewerkCoreBundle : BindingBundle
    {
        protected override void OnInstall()
        {
            // Tracked helpers — all bindings auto-cleaned on Uninstall()
            BindInjection<ICoroutineManager>().ToValue(CoroutineManager.Instance);
            BindInjection<IUpdater>().ToValue(Updater.Instance);
            BindInjection<IAppMonitor>().ToValue(AppMonitor.Instance);
            BindInjection<IAssetManager>().To<AssetManager>().ToSingleton();
            BindInjection<IUiManager>().To<UiManager>().ToSingleton();
        }
    }
}