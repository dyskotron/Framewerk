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
    }

    public interface IPopupManager
    {
        void CloseAllPopups();

        Task<T> InstantiatePopupAsync<T>(string customPrefix = null, CancellationToken ct = default) where T : IPopupView;
        Task<T> InstantiatePopupAsync<T>(object[] popupMediatorInjects, string customPrefix = null, CancellationToken ct = default) where T : IPopupView;
        Task<T> InstantiatePopupAsync<T>(PopupButtonSetting[] popupOptions, string customPrefix = null, CancellationToken ct = default) where T : IPopupView;
        Task<T> InstantiatePopupAsync<T>(string text, PopupButtonSetting[] popupOptions, string customPrefix = null, CancellationToken ct = default) where T : IPopupView;
        Task<T> InstantiatePopupAsync<T>(string caption, string text, PopupButtonSetting[] popupOptions, string customPrefix = null, CancellationToken ct = default) where T : IPopupView;

        T InstantiatePopup<T>(string customPrefix = null) where T : IPopupView;
        T InstantiatePopup<T>(object[] popupMediatorInjects, string customPrefix = null) where T : IPopupView;
        T InstantiatePopup<T>(PopupButtonSetting[] popupOptions, string customPrefix = null) where T : IPopupView;
        T InstantiatePopup<T>(string text, PopupButtonSetting[] popupOptions, string customPrefix = null) where T : IPopupView;
        T InstantiatePopup<T>(string caption, string text, PopupButtonSetting[] popupOptions, string customPrefix = null) where T : IPopupView;
    }

    public class PopupManager : IPopupManager
    {
        [Inject] public PopupOpenedSignal PopupOpenedSignal { get; set; }
        [Inject] public IUiManager UiManager { get; set; }
        [Inject] public IInjectionBinder InjectionBinder { get; set; }

        private ViewConfig _viewConfig;
        private Transform _popupParent;
        private List<IPopupMediator> _popups = new List<IPopupMediator>();

        [Inject]
        public ViewConfig ViewConfig
        {
            get => _viewConfig;
            set
            {
                _viewConfig = value;
                _popupParent = value.Popups;
                PopupOpenedSignal.AddListener(OnPopupOpenedHandler);
            }
        }

        private string GetPopupPath(Type popupType, string customPrefix)
        {
            var viewName = UiManager.GetViewName(popupType);
            return AddressBuilder.BuildAddress(_viewConfig, customPrefix, AddressBuilder.TypeKeys.Popup, viewName);
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
            var uiObj = await UiManager.InstantiateViewAsync(path, _popupParent, ct);
            return uiObj.GetComponent<T>();
        }

        public async Task<T> InstantiatePopupAsync<T>(object[] popupMediatorInjects, string customPrefix = null, CancellationToken ct = default) where T : IPopupView
        {
            var path = GetPopupPath(typeof(T), customPrefix);
            var uiObj = await UiManager.InstantiateViewAsync(path, _popupParent, ct, popupMediatorInjects);
            return uiObj.GetComponent<T>();
        }

        public async Task<T> InstantiatePopupAsync<T>(PopupButtonSetting[] popupOptions, string customPrefix = null, CancellationToken ct = default) where T : IPopupView
        {
            var path = GetPopupPath(typeof(T), customPrefix);
            var uiObj = await UiManager.InstantiateViewAsync(path, _popupParent, ct, new List<PopupButtonSetting>(popupOptions));
            return uiObj.GetComponent<T>();
        }

        public async Task<T> InstantiatePopupAsync<T>(string text, PopupButtonSetting[] popupOptions, string customPrefix = null, CancellationToken ct = default) where T : IPopupView
        {
            var path = GetPopupPath(typeof(T), customPrefix);
            var uiObj = await UiManager.InstantiateViewAsync(path, _popupParent, ct, text, new List<PopupButtonSetting>(popupOptions));
            return uiObj.GetComponent<T>();
        }

        public async Task<T> InstantiatePopupAsync<T>(string caption, string text, PopupButtonSetting[] popupOptions, string customPrefix = null, CancellationToken ct = default) where T : IPopupView
        {
            var path = GetPopupPath(typeof(T), customPrefix);
            var uiObj = await UiManager.InstantiateViewAsync(path, _popupParent, ct, caption, text, new List<PopupButtonSetting>(popupOptions));
            return uiObj.GetComponent<T>();
        }

        public T InstantiatePopup<T>(string customPrefix = null) where T : IPopupView
        {
            var path = GetPopupPath(typeof(T), customPrefix);
            var uiObj = UiManager.InstantiateView(path, _popupParent);
            return uiObj.GetComponent<T>();
        }

        public T InstantiatePopup<T>(object[] popupMediatorInjects, string customPrefix = null) where T : IPopupView
        {
            var path = GetPopupPath(typeof(T), customPrefix);
            var uiObj = UiManager.InstantiateView(path, _popupParent, popupMediatorInjects);
            return uiObj.GetComponent<T>();
        }

        public T InstantiatePopup<T>(PopupButtonSetting[] popupOptions, string customPrefix = null) where T : IPopupView
        {
            var path = GetPopupPath(typeof(T), customPrefix);
            var uiObj = UiManager.InstantiateView(path, _popupParent, new List<PopupButtonSetting>(popupOptions));
            return uiObj.GetComponent<T>();
        }

        public T InstantiatePopup<T>(string text, PopupButtonSetting[] popupOptions, string customPrefix = null) where T : IPopupView
        {
            var path = GetPopupPath(typeof(T), customPrefix);
            var uiObj = UiManager.InstantiateView(path, _popupParent, text, new List<PopupButtonSetting>(popupOptions));
            return uiObj.GetComponent<T>();
        }

        public T InstantiatePopup<T>(string caption, string text, PopupButtonSetting[] popupOptions, string customPrefix = null) where T : IPopupView
        {
            var path = GetPopupPath(typeof(T), customPrefix);
            var uiObj = UiManager.InstantiateView(path, _popupParent, caption, text, new List<PopupButtonSetting>(popupOptions));
            return uiObj.GetComponent<T>();
        }

        private void OnPopupOpenedHandler(IPopupMediator popup)
        {
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
