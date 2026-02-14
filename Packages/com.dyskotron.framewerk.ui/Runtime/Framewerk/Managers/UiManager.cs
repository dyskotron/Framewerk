using System;
using Framewerk.Utils;
using Plugins.Framewerk;
using strange.extensions.injector.api;
using strange.extensions.mediation.api;
using strange.extensions.promise.api;
using strange.extensions.promise.impl;
using UnityEngine;

namespace Framewerk.Managers
{
    public interface IUiManager
    {
        // Sync methods - simple instantiation, no DI injection params
        GameObject InstantiateView(GameObject viewPrefab, Transform parent = null);
        GameObject InstantiateView(string path, Transform parent = null);
        T InstantiateView<T>(string customPrefix = null, Transform parent = null) where T : IView;

        // Promise-based async methods - full DI support with injectable parameters
        IPromise<GameObject> InstantiateViewAsync(string path, Transform parent = null, params object[] mediatorInjects);
        IPromise<GameObject> InstantiateViewExplicitTypeAsync(string path, Transform parent = null, params Tuple<object, Type>[] mediatorInjectsWithTypes);
        IPromise<T> InstantiateViewAsync<T>(string customPrefix = null, Transform parent = null, params object[] mediatorInjects) where T : IView;
        IPromise<T> InstantiateViewExplicitTypeAsync<T>(string customPrefix = null, Transform parent = null, params Tuple<object, Type>[] mediatorInjectsWithTypes) where T : IView;

        string GetViewName(Type type);
    }

    public class UiManager : IUiManager
    {
        public const string VIEW_SUFFIX = "View";

        /// <summary>
        /// TypeKey for views loaded by this manager. Default is empty (base views).
        /// Override for specialized managers (e.g. ListManager could use "List").
        /// </summary>
        public string TypeKey { get; set; } = AddressBuilder.TypeKeys.View;

        public bool BindInterfaces { get; set; } = true;
        public bool BindBaseClasses { get; set; } = false;

        [Inject]
        public IAssetManager AssetManager { get; set; }

        [Inject]
        public IInjectionBinder InjectionBinder { get; set; }

        private ViewConfig _viewConfig;
        private Transform _uiParent;

        [Inject]
        public ViewConfig ViewConfig
        {
            get => _viewConfig;
            set
            {
                _viewConfig = value;
                _uiParent = value.UiDefault;
            }
        }

        #region Promise-based Async Methods

        public IPromise<GameObject> InstantiateViewAsync(string path, Transform parent = null, params object[] mediatorInjects)
        {
            var promise = new Promise<GameObject>();

            if (parent == null)
                parent = _uiParent;

            BindingUtils.Bind(InjectionBinder, BindInterfaces, BindBaseClasses, mediatorInjects);

            AssetManager.GetAssetAsync<GameObject>(path, parent)
                .Then(uiObj =>
                {
                    BindingUtils.Unbind(InjectionBinder, BindInterfaces, BindBaseClasses, mediatorInjects);
                    promise.Dispatch(uiObj);
                })
                .Fail(ex =>
                {
                    BindingUtils.Unbind(InjectionBinder, BindInterfaces, BindBaseClasses, mediatorInjects);
                    promise.ReportFail(ex);
                });

            return promise;
        }

        public IPromise<GameObject> InstantiateViewExplicitTypeAsync(string path, Transform parent = null, params Tuple<object, Type>[] mediatorInjectsWithTypes)
        {
            var promise = new Promise<GameObject>();

            if (parent == null)
                parent = _uiParent;

            BindingUtils.Bind(InjectionBinder, mediatorInjectsWithTypes);

            AssetManager.GetAssetAsync<GameObject>(path, parent)
                .Then(uiObj =>
                {
                    BindingUtils.Unbind(InjectionBinder, mediatorInjectsWithTypes);
                    promise.Dispatch(uiObj);
                })
                .Fail(ex =>
                {
                    BindingUtils.Unbind(InjectionBinder, mediatorInjectsWithTypes);
                    promise.ReportFail(ex);
                });

            return promise;
        }

        public IPromise<T> InstantiateViewAsync<T>(string customPrefix = null, Transform parent = null, params object[] mediatorInjects) where T : IView
        {
            var promise = new Promise<T>();

            if (parent == null)
                parent = _uiParent;

            InstantiateViewAsync(GetViewPath(typeof(T), customPrefix), parent, mediatorInjects)
                .Then(uiObj =>
                {
                    var component = uiObj.GetComponent<T>();
                    if (component == null)
                    {
                        Debug.LogError($"UIManager.InstantiateViewAsync: No {typeof(T)} on {uiObj}");
                        promise.ReportFail(new System.Exception($"No {typeof(T)} on {uiObj}"));
                    }
                    else
                    {
                        promise.Dispatch(component);
                    }
                })
                .Fail(ex => promise.ReportFail(ex));

            return promise;
        }

        public IPromise<T> InstantiateViewExplicitTypeAsync<T>(string customPrefix = null, Transform parent = null, params Tuple<object, Type>[] mediatorInjectsWithTypes) where T : IView
        {
            var promise = new Promise<T>();

            if (parent == null)
                parent = _uiParent;

            InstantiateViewExplicitTypeAsync(GetViewPath(typeof(T), customPrefix), parent, mediatorInjectsWithTypes)
                .Then(uiObj =>
                {
                    var component = uiObj.GetComponent<T>();
                    if (component == null)
                    {
                        Debug.LogError($"UIManager.InstantiateViewExplicitTypeAsync: No {typeof(T)} on {uiObj}");
                        promise.ReportFail(new System.Exception($"No {typeof(T)} on {uiObj}"));
                    }
                    else
                    {
                        promise.Dispatch(component);
                    }
                })
                .Fail(ex => promise.ReportFail(ex));

            return promise;
        }

        #endregion

        #region Synchronous Methods

        public GameObject InstantiateView(string path, Transform parent = null)
        {
            if (parent == null)
                parent = _uiParent;

            return AssetManager.GetAsset<GameObject>(path, parent);
        }

        public T InstantiateView<T>(string customPrefix = null, Transform parent = null) where T : IView
        {
            if (parent == null)
                parent = _uiParent;

            var uiObj = InstantiateView(GetViewPath(typeof(T), customPrefix), parent);
            var component = uiObj.GetComponent<T>();

            if (component == null)
                Debug.LogError($"UIManager.InstantiateView: No {typeof(T)} on {uiObj}");

            return component;
        }

        public GameObject InstantiateView(GameObject viewPrefab, Transform parent = null)
        {
            if (viewPrefab == null)
                Debug.LogError("UIManager.InstantiateView: viewPrefab is null");

            if (parent == null)
                parent = _uiParent;

            return GameObject.Instantiate(viewPrefab, parent, false);
        }

        #endregion

        public string GetViewName(Type type)
        {
            var name = type.Name;
            return name.Substring(0, name.Length - VIEW_SUFFIX.Length);
        }

        /// <summary>
        /// Builds the full addressable path for a view type.
        /// Uses ViewConfig's ContextPrefixSO for address building.
        /// </summary>
        /// <param name="type">The view type</param>
        /// <param name="customPrefix">Optional custom prefix for feature grouping</param>
        protected virtual string GetViewPath(Type type, string customPrefix)
        {
            var viewName = GetViewName(type);
            return AddressBuilder.BuildAddress(_viewConfig, customPrefix, TypeKey, viewName);
        }
    }
}
