using System;
using System.Collections.Generic;
using Framewerk.Managers;
using Plugins.Framewerk;
using strange.extensions.injector.api;
using strange.extensions.promise.api;
using strange.extensions.promise.impl;
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

        // Sync method - simple instantiation, no DI injection
        T InstantiatePopup<T>(string customPrefix = null) where T : IPopupView;

        // Promise-based async methods - full DI support with injectable parameters
        IPromise<T> InstantiatePopupAsync<T>(string customPrefix = null) where T : IPopupView;
        IPromise<T> InstantiatePopupAsync<T>(object[] popupMediatorInjects, string customPrefix = null) where T : IPopupView;
        IPromise<T> InstantiatePopupAsync<T>(PopupButtonSetting[] popupOptions, string customPrefix = null) where T : IPopupView;
        IPromise<T> InstantiatePopupAsync<T>(string text, PopupButtonSetting[] popupOptions, string customPrefix = null) where T : IPopupView;
        IPromise<T> InstantiatePopupAsync<T>(string caption, string text, PopupButtonSetting[] popupOptions, string customPrefix = null) where T : IPopupView;
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

        #region Promise-based Async Methods

        public IPromise<T> InstantiatePopupAsync<T>(string customPrefix = null) where T : IPopupView
        {
            var promise = new Promise<T>();
            var path = GetPopupPath(typeof(T), customPrefix);

            UiManager.InstantiateViewAsync(path, ViewConfig.Popups)
                .Then(uiObj => promise.Dispatch(uiObj.GetComponent<T>()))
                .Fail(ex => promise.ReportFail(ex));

            return promise;
        }

        public IPromise<T> InstantiatePopupAsync<T>(object[] popupMediatorInjects, string customPrefix = null) where T : IPopupView
        {
            var promise = new Promise<T>();
            var path = GetPopupPath(typeof(T), customPrefix);

            UiManager.InstantiateViewAsync(path, ViewConfig.Popups, popupMediatorInjects)
                .Then(uiObj => promise.Dispatch(uiObj.GetComponent<T>()))
                .Fail(ex => promise.ReportFail(ex));

            return promise;
        }

        public IPromise<T> InstantiatePopupAsync<T>(PopupButtonSetting[] popupOptions, string customPrefix = null) where T : IPopupView
        {
            var promise = new Promise<T>();
            var path = GetPopupPath(typeof(T), customPrefix);

            UiManager.InstantiateViewAsync(path, ViewConfig.Popups, new List<PopupButtonSetting>(popupOptions))
                .Then(uiObj => promise.Dispatch(uiObj.GetComponent<T>()))
                .Fail(ex => promise.ReportFail(ex));

            return promise;
        }

        public IPromise<T> InstantiatePopupAsync<T>(string text, PopupButtonSetting[] popupOptions, string customPrefix = null) where T : IPopupView
        {
            var promise = new Promise<T>();
            var path = GetPopupPath(typeof(T), customPrefix);

            UiManager.InstantiateViewAsync(path, ViewConfig.Popups, text, new List<PopupButtonSetting>(popupOptions))
                .Then(uiObj => promise.Dispatch(uiObj.GetComponent<T>()))
                .Fail(ex => promise.ReportFail(ex));

            return promise;
        }

        public IPromise<T> InstantiatePopupAsync<T>(string caption, string text, PopupButtonSetting[] popupOptions, string customPrefix = null) where T : IPopupView
        {
            var promise = new Promise<T>();
            var path = GetPopupPath(typeof(T), customPrefix);

            UiManager.InstantiateViewAsync(path, ViewConfig.Popups, caption, text, new List<PopupButtonSetting>(popupOptions))
                .Then(uiObj => promise.Dispatch(uiObj.GetComponent<T>()))
                .Fail(ex => promise.ReportFail(ex));

            return promise;
        }

        #endregion

        #region Synchronous Methods

        public T InstantiatePopup<T>(string customPrefix = null) where T : IPopupView
        {
            var path = GetPopupPath(typeof(T), customPrefix);
            var uiObj = UiManager.InstantiateView(path, ViewConfig.Popups);
            return uiObj.GetComponent<T>();
        }

        #endregion

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
