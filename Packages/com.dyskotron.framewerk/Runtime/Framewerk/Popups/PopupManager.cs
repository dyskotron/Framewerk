using System;
using System.Collections.Generic;
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
        public const string UI_PREFABS_ROOT = "Popups/";
        [Inject] public PopupOpenedSignal PopupOpenedSignal { get; set; }
        [Inject] public IUiManager UiManager { get; set; }
        [Inject] public IInjectionBinder InjectionBinder { get; set; }

        private List<IPopupMediator> _popups = new List<IPopupMediator>();

        private string _resourcePath;
        private Transform _popupParent;

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
            return await UiManager.InstantiateViewAsync<T>(_resourcePath, _popupParent, ct);
        }

        public async Task<T> InstantiatePopupAsync<T>(object[] popupMediatorInjects, CancellationToken ct = default) where T : IPopupView
        {
            return await UiManager.InstantiateViewAsync<T>(_resourcePath, _popupParent, ct, popupMediatorInjects);
        }

        public async Task<T> InstantiatePopupAsync<T>(PopupButtonSetting[] popupOptions, CancellationToken ct = default) where T : IPopupView
        {
            return await UiManager.InstantiateViewAsync<T>(_resourcePath, _popupParent, ct, new List<PopupButtonSetting>(popupOptions));
        }

        public async Task<T> InstantiatePopupAsync<T>(string text, PopupButtonSetting[] popupOptions, CancellationToken ct = default) where T : IPopupView
        {
            return await UiManager.InstantiateViewAsync<T>(_resourcePath, _popupParent, ct, text, new List<PopupButtonSetting>(popupOptions));
        }

        public async Task<T> InstantiatePopupAsync<T>(string caption, string text, PopupButtonSetting[] popupOptions, CancellationToken ct = default) where T : IPopupView
        {
            return await UiManager.InstantiateViewAsync<T>(_resourcePath, _popupParent, ct, caption, text, new List<PopupButtonSetting>(popupOptions));
        }

        public T InstantiatePopup<T>() where T : IPopupView
        {
            return UiManager.InstantiateView<T>(_resourcePath, _popupParent);
        }

        public T InstantiatePopup<T>(object[] popupMediatorInjects) where T : IPopupView
        {
            return UiManager.InstantiateView<T>(_resourcePath, _popupParent, popupMediatorInjects);
        }

        public T InstantiatePopup<T>(PopupButtonSetting[] popupOptions) where T : IPopupView
        {
            return UiManager.InstantiateView<T>(_resourcePath, _popupParent, new List<PopupButtonSetting>(popupOptions));
        }

        public T InstantiatePopup<T>(string text, PopupButtonSetting[] popupOptions) where T : IPopupView
        {
            return UiManager.InstantiateView<T>(_resourcePath, _popupParent, text, new List<PopupButtonSetting>(popupOptions));
        }

        public T InstantiatePopup<T>(string caption, string text, PopupButtonSetting[] popupOptions) where T : IPopupView
        {
            return UiManager.InstantiateView<T>(_resourcePath, _popupParent, caption, text, new List<PopupButtonSetting>(popupOptions));
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
