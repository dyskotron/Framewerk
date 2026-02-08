using System.Collections.Generic;
using Framewerk.Popups;
using Framewerk.StrangeCore.Bundles;

namespace FramewerkDemo
{
    /// <summary>
    /// Bundle for popup system bindings.
    /// </summary>
    public class PopupsBundle : BindingBundle
    {
        protected override void OnInstall()
        {
            BindInjection<IPopupManager>().To<PopupManager>().ToSingleton();
            BindInjection<PopupOpenedSignal>().ToSingleton();
            BindInjection<PopupClosedSignal>().ToSingleton();

            // Default empty list for popups that don't define button settings.
            // PopupMediator injects this; overridden per-popup via UiManager's mediatorInjects.
            BindInjection<List<PopupButtonSetting>>().ToValue(new List<PopupButtonSetting>());
        }
    }
}
