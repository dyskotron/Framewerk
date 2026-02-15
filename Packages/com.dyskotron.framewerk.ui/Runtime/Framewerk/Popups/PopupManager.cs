using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Framewerk.Managers;
using Plugins.Framewerk;
using strange.extensions.injector.api;
using strange.extensions.promise.api;
using UnityEngine;

namespace Framewerk.Popups
{
    public class PopupButtonSetting
    {
        public string optionText;
        public Action clickHandler;
        public IPromise clickPromise;
        public bool closesPopup = true;

        /// <summary>
        /// Standard close button with "Close" text that closes the popup.
        /// </summary>
        public static PopupButtonSetting CloseButton => new PopupButtonSetting
        {
            optionText = "Close",
            closesPopup = true
        };
    }

    public interface IPopupManager
    {
        void CloseAllPopups();

        // Sync method - simple instantiation, no DI injection
        T InstantiatePopup<T>(string customPrefix = null) where T : IPopupView;

        // Async methods - full DI support with injectable parameters
        Task<T> InstantiatePopupAsync<T>(string customPrefix = null, CancellationToken ct = default) where T : IPopupView;
        Task<T> InstantiatePopupAsync<T>(object[] popupMediatorInjects, string customPrefix = null, CancellationToken ct = default) where T : IPopupView;
        Task<T> InstantiatePopupAsync<T>(PopupButtonSetting[] popupOptions, string customPrefix = null, CancellationToken ct = default) where T : IPopupView;
        Task<T> InstantiatePopupAsync<T>(string text, PopupButtonSetting[] popupOptions, string customPrefix = null, CancellationToken ct = default) where T : IPopupView;
        Task<T> InstantiatePopupAsync<T>(string caption, string text, PopupButtonSetting[] popupOptions, string customPrefix = null, CancellationToken ct = default) where T : IPopupView;
    }

    public class PopupManager : IPopupManager
    {
        [Inject] public PopupOpenedSignal PopupOpenedSignal { get; set; }
        [Inject] public IUiManager UiManager { get; set; }
        [Inject] public IInjectionBinder InjectionBinder { get; set; }
        [Inject] public ViewConfig ViewConfig { get; set; }

        private List<IPopupMediator> _popups = new List<IPopupMediator>();


        [PostConstruct]
        public void Init()
        {
            PopupOpenedSignal.AddListener(OnPopupOpenedHandler);
        }

        private string GetPopupPath(Type popupType, string customPrefix)
        {
            var viewName = UiManager.GetViewName(popupType);
            return AddressBuilder.BuildAddress(ViewConfig, customPrefix, AddressBuilder.TypeKeys.Popup, viewName);
        }

        public void CloseAllPopups()
        {
            foreach (var popup in _popups)
            {
                popup.PopupClosedSignal.RemoveListener(OnPopupClosed);
                popup.Close();
            }

            _popups.Clear();
        }

        public async Task<T> InstantiatePopupAsync<T>(string customPrefix = null, CancellationToken ct = default) where T : IPopupView
        {
            var path = GetPopupPath(typeof(T), customPrefix);
            var uiObj = await UiManager.InstantiateViewAsync(path, ViewConfig.Popups, ct);
            return uiObj.GetComponent<T>();
        }

        public async Task<T> InstantiatePopupAsync<T>(object[] popupMediatorInjects, string customPrefix = null, CancellationToken ct = default) where T : IPopupView
        {
            var path = GetPopupPath(typeof(T), customPrefix);
            var uiObj = await UiManager.InstantiateViewAsync(path, ViewConfig.Popups, ct, popupMediatorInjects);
            return uiObj.GetComponent<T>();
        }

        public async Task<T> InstantiatePopupAsync<T>(PopupButtonSetting[] popupOptions, string customPrefix = null, CancellationToken ct = default) where T : IPopupView
        {
            var path = GetPopupPath(typeof(T), customPrefix);
            var uiObj = await UiManager.InstantiateViewAsync(path, ViewConfig.Popups, ct, new List<PopupButtonSetting>(popupOptions));
            return uiObj.GetComponent<T>();
        }

        public async Task<T> InstantiatePopupAsync<T>(string text, PopupButtonSetting[] popupOptions, string customPrefix = null, CancellationToken ct = default) where T : IPopupView
        {
            var path = GetPopupPath(typeof(T), customPrefix);
            var uiObj = await UiManager.InstantiateViewAsync(path, ViewConfig.Popups, ct, text, new List<PopupButtonSetting>(popupOptions));
            return uiObj.GetComponent<T>();
        }

        public async Task<T> InstantiatePopupAsync<T>(string caption, string text, PopupButtonSetting[] popupOptions, string customPrefix = null, CancellationToken ct = default) where T : IPopupView
        {
            var path = GetPopupPath(typeof(T), customPrefix);
            var uiObj = await UiManager.InstantiateViewAsync(path, ViewConfig.Popups, ct, caption, text, new List<PopupButtonSetting>(popupOptions));
            return uiObj.GetComponent<T>();
        }

        // Sync method - simple instantiation without DI injection
        // Use async versions if you need to inject PopupButtonSettings or other parameters

        public T InstantiatePopup<T>(string customPrefix = null) where T : IPopupView
        {
            var path = GetPopupPath(typeof(T), customPrefix);
            var uiObj = UiManager.InstantiateView(path, ViewConfig.Popups);
            return uiObj.GetComponent<T>();
        }

        private void OnPopupOpenedHandler(IPopupMediator popup)
        {
            // Guard against race condition: if popup was destroyed before this handler ran,
            // the MonoBehaviour will be null (Unity's == override). Skip registration.
            if (popup is MonoBehaviour mb && mb == null)
                return;

            _popups.Add(popup);
            popup.PopupClosedSignal.AddListener(OnPopupClosed);
        }

        private void OnPopupClosed(IPopupMediator popup)
        {
            popup.PopupClosedSignal.RemoveListener(OnPopupClosed);
            _popups.Remove(popup);
        }
    }
}
