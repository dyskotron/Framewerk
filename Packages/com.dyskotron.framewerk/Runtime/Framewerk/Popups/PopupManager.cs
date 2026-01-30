using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Framewerk.Managers;
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
        void Init(string resourcePath, Transform popupParent);
        void CloseAllPopups();

        Task<T> InstantiatePopupAsync<T>(CancellationToken ct = default) where T : IPopupView;
        Task<T> InstantiatePopupAsync<T>(object[] popupMediatorInjects, CancellationToken ct = default) where T : IPopupView;
        Task<T> InstantiatePopupAsync<T>(PopupButtonSetting[] popupOptions, CancellationToken ct = default) where T : IPopupView;
        Task<T> InstantiatePopupAsync<T>(string text, PopupButtonSetting[] popupOptions, CancellationToken ct = default) where T : IPopupView;
        Task<T> InstantiatePopupAsync<T>(string caption, string text, PopupButtonSetting[] popupOptions, CancellationToken ct = default) where T : IPopupView;

        T InstantiatePopup<T>() where T : IPopupView;
        T InstantiatePopup<T>(object[] popupMediatorInjects) where T : IPopupView;
        T InstantiatePopup<T>(PopupButtonSetting[] popupOptions) where T : IPopupView;
        T InstantiatePopup<T>(string text, PopupButtonSetting[] popupOptions) where T : IPopupView;
        T InstantiatePopup<T>(string caption, string text, PopupButtonSetting[] popupOptions) where T : IPopupView;
    }

    public class PopupManager : IPopupManager
    {
        public string TypeKey { get; set; } = "Popups";

        [Inject] public PopupOpenedSignal PopupOpenedSignal { get; set; }
        [Inject] public IUiManager UiManager { get; set; }
        [Inject] public IInjectionBinder InjectionBinder { get; set; }

        private List<IPopupMediator> _popups = new List<IPopupMediator>();

        private string _resourcePath;
        private Transform _popupParent;

        private string BuildAddress(params string[] segments)
        {
            var parts = segments.Where(seg => !string.IsNullOrEmpty(seg))
                                .Select(seg => seg.Trim('/'))
                                .ToList();
            return string.Join("/", parts);
        }

        private string GetPopupPath(Type popupType)
        {
            var viewName = UiManager.GetViewName(popupType);
            return BuildAddress(_resourcePath, TypeKey, viewName);
        }

        public void Init(string resourcePath, Transform popupParent)
        {
            PopupOpenedSignal.AddListener(OnPopupOpenedHandler);

            _resourcePath = resourcePath;
            _popupParent = popupParent;
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

        public async Task<T> InstantiatePopupAsync<T>(CancellationToken ct = default) where T : IPopupView
        {
            var path = GetPopupPath(typeof(T));
            var uiObj = await UiManager.InstantiateViewAsync(path, _popupParent, ct);
            return uiObj.GetComponent<T>();
        }

        public async Task<T> InstantiatePopupAsync<T>(object[] popupMediatorInjects, CancellationToken ct = default) where T : IPopupView
        {
            var path = GetPopupPath(typeof(T));
            var uiObj = await UiManager.InstantiateViewAsync(path, _popupParent, ct, popupMediatorInjects);
            return uiObj.GetComponent<T>();
        }

        public async Task<T> InstantiatePopupAsync<T>(PopupButtonSetting[] popupOptions, CancellationToken ct = default) where T : IPopupView
        {
            var path = GetPopupPath(typeof(T));
            var uiObj = await UiManager.InstantiateViewAsync(path, _popupParent, ct, new List<PopupButtonSetting>(popupOptions));
            return uiObj.GetComponent<T>();
        }

        public async Task<T> InstantiatePopupAsync<T>(string text, PopupButtonSetting[] popupOptions, CancellationToken ct = default) where T : IPopupView
        {
            var path = GetPopupPath(typeof(T));
            var uiObj = await UiManager.InstantiateViewAsync(path, _popupParent, ct, text, new List<PopupButtonSetting>(popupOptions));
            return uiObj.GetComponent<T>();
        }

        public async Task<T> InstantiatePopupAsync<T>(string caption, string text, PopupButtonSetting[] popupOptions, CancellationToken ct = default) where T : IPopupView
        {
            var path = GetPopupPath(typeof(T));
            var uiObj = await UiManager.InstantiateViewAsync(path, _popupParent, ct, caption, text, new List<PopupButtonSetting>(popupOptions));
            return uiObj.GetComponent<T>();
        }

        public T InstantiatePopup<T>() where T : IPopupView
        {
            var path = GetPopupPath(typeof(T));
            var uiObj = UiManager.InstantiateView(path, _popupParent);
            return uiObj.GetComponent<T>();
        }

        public T InstantiatePopup<T>(object[] popupMediatorInjects) where T : IPopupView
        {
            var path = GetPopupPath(typeof(T));
            var uiObj = UiManager.InstantiateView(path, _popupParent, popupMediatorInjects);
            return uiObj.GetComponent<T>();
        }

        public T InstantiatePopup<T>(PopupButtonSetting[] popupOptions) where T : IPopupView
        {
            var path = GetPopupPath(typeof(T));
            var uiObj = UiManager.InstantiateView(path, _popupParent, new List<PopupButtonSetting>(popupOptions));
            return uiObj.GetComponent<T>();
        }

        public T InstantiatePopup<T>(string text, PopupButtonSetting[] popupOptions) where T : IPopupView
        {
            var path = GetPopupPath(typeof(T));
            var uiObj = UiManager.InstantiateView(path, _popupParent, text, new List<PopupButtonSetting>(popupOptions));
            return uiObj.GetComponent<T>();
        }

        public T InstantiatePopup<T>(string caption, string text, PopupButtonSetting[] popupOptions) where T : IPopupView
        {
            var path = GetPopupPath(typeof(T));
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
