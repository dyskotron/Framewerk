using System;
using System.Threading;
using System.Threading.Tasks;
using Framewerk.Utils;
using Plugins.Framewerk;
using strange.extensions.injector.api;
using strange.extensions.mediation.api;
using UnityEngine;

namespace Framewerk.Managers
{
    public interface IUiManager
    {
        GameObject InstantiateView(GameObject viewPrefab, Transform parent = null, params object[] mediatorInjects);
        GameObject InstantiateViewExplicitType(GameObject viewPrefab, Transform parent = null, params Tuple<object, Type>[] mediatorInjectsWithType);

        Task<GameObject> InstantiateViewAsync(string path, Transform parent = null, CancellationToken ct = default, params object[] mediatorInjects);
        Task<GameObject> InstantiateViewExplicitTypeAsync(string path, Transform parent = null, CancellationToken ct = default, params Tuple<object, Type>[] mediatorInjectsWithTypes);
        Task<T> InstantiateViewAsync<T>(string customPrefix = null, Transform parent = null, CancellationToken ct = default, params object[] mediatorInjects) where T : IView;
        Task<T> InstantiateViewExplicitTypeAsync<T>(string customPrefix = null, Transform parent = null, CancellationToken ct = default, params Tuple<object, Type>[] mediatorInjectsWithTypes) where T : IView;

        GameObject InstantiateView(string path, Transform parent = null, params object[] mediatorInjects);
        GameObject InstantiateViewExplicitType(string path, Transform parent = null, params Tuple<object, Type>[] mediatorInjectsWithTypes);
        T InstantiateView<T>(string customPrefix = null, Transform parent = null, params object[] mediatorInjects) where T : IView;
        T InstantiateViewExplicitType<T>(string customPrefix = null, Transform parent = null, params Tuple<object, Type>[] mediatorInjectsWithTypes) where T : IView;

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

        public async Task<GameObject> InstantiateViewAsync(string path, Transform parent = null, CancellationToken ct = default, params object[] mediatorInjects)
        {
            if (parent == null)
                parent = _uiParent;

            BindingUtils.Bind(InjectionBinder, BindInterfaces, BindBaseClasses, mediatorInjects);
            GameObject uiObj = await AssetManager.GetAssetAsync<GameObject>(path, parent, ct);
            BindingUtils.Unbind(InjectionBinder, BindInterfaces, BindBaseClasses, mediatorInjects);

            return uiObj;
        }

        public async Task<GameObject> InstantiateViewExplicitTypeAsync(string path, Transform parent = null, CancellationToken ct = default, params Tuple<object, Type>[] mediatorInjectsWithTypes)
        {
            if (parent == null)
                parent = _uiParent;

            BindingUtils.Bind(InjectionBinder, mediatorInjectsWithTypes);
            GameObject uiObj = await AssetManager.GetAssetAsync<GameObject>(path, parent, ct);
            BindingUtils.Unbind(InjectionBinder, mediatorInjectsWithTypes);

            return uiObj;
        }

        public async Task<T> InstantiateViewAsync<T>(string customPrefix = null, Transform parent = null, CancellationToken ct = default, params object[] mediatorInjects) where T : IView
        {
            if (parent == null)
                parent = _uiParent;

            var uiObj = await InstantiateViewAsync(GetViewPath(typeof(T), customPrefix), parent, ct, mediatorInjects);
            var component = uiObj.GetComponent<T>();

            if (component == null)
                Debug.LogError($"UIManager.InstantiateViewAsync: No {typeof(T)} on {uiObj}");

            return component;
        }

        public async Task<T> InstantiateViewExplicitTypeAsync<T>(string customPrefix = null, Transform parent = null, CancellationToken ct = default, params Tuple<object, Type>[] mediatorInjectsWithTypes) where T : IView
        {
            if (parent == null)
                parent = _uiParent;

            var uiObj = await InstantiateViewExplicitTypeAsync(GetViewPath(typeof(T), customPrefix), parent, ct, mediatorInjectsWithTypes);
            var component = uiObj.GetComponent<T>();

            if (component == null)
                Debug.LogError($"UIManager.InstantiateViewExplicitTypeAsync: No {typeof(T)} on {uiObj}");

            return component;
        }

        public GameObject InstantiateView(string path, Transform parent = null, params object[] mediatorInjects)
        {
            if (parent == null)
                parent = _uiParent;

            BindingUtils.Bind(InjectionBinder, BindInterfaces, BindBaseClasses, mediatorInjects);
            GameObject uiObj = AssetManager.GetAsset<GameObject>(path, parent);
            BindingUtils.Unbind(InjectionBinder, BindInterfaces, BindBaseClasses, mediatorInjects);

            return uiObj;
        }

        public GameObject InstantiateViewExplicitType(string path, Transform parent = null, params Tuple<object, Type>[] mediatorInjectsWithTypes)
        {
            if (parent == null)
                parent = _uiParent;

            BindingUtils.Bind(InjectionBinder, mediatorInjectsWithTypes);
            GameObject uiObj = AssetManager.GetAsset<GameObject>(path, parent);
            BindingUtils.Unbind(InjectionBinder, mediatorInjectsWithTypes);

            return uiObj;
        }

        public T InstantiateView<T>(string customPrefix = null, Transform parent = null, params object[] mediatorInjects) where T : IView
        {
            if (parent == null)
                parent = _uiParent;

            var uiObj = InstantiateView(GetViewPath(typeof(T), customPrefix), parent, mediatorInjects);
            var component = uiObj.GetComponent<T>();

            if (component == null)
                Debug.LogError($"UIManager.InstantiateView: No {typeof(T)} on {uiObj}");

            return component;
        }

        public T InstantiateViewExplicitType<T>(string customPrefix = null, Transform parent = null, params Tuple<object, Type>[] mediatorInjectsWithTypes) where T : IView
        {
            if (parent == null)
                parent = _uiParent;

            var uiObj = InstantiateViewExplicitType(GetViewPath(typeof(T), customPrefix), parent, mediatorInjectsWithTypes);
            var component = uiObj.GetComponent<T>();

            if (component == null)
                Debug.LogError($"UIManager.InstantiateViewExplicitType: No {typeof(T)} on {uiObj}");

            return component;
        }

        public GameObject InstantiateView(GameObject viewPrefab, Transform parent = null, params object[] mediatorInjects)
        {
            if (viewPrefab == null)
                Debug.LogError("UIManager.InstantiateView: viewPrefab is null");

            if (parent == null)
                parent = _uiParent;

            BindingUtils.Bind(InjectionBinder, BindInterfaces, BindBaseClasses, mediatorInjects);
            GameObject view = GameObject.Instantiate(viewPrefab, parent, false);
            BindingUtils.Unbind(InjectionBinder, BindInterfaces, BindBaseClasses, mediatorInjects);

            return view;
        }

        public GameObject InstantiateViewExplicitType(GameObject viewPrefab, Transform parent = null, params Tuple<object, Type>[] mediatorInjectsWithType)
        {
            if (viewPrefab == null)
                Debug.LogError("UIManager.InstantiateView: viewPrefab is null");

            if (parent == null)
                parent = _uiParent;

            BindingUtils.Bind(InjectionBinder, mediatorInjectsWithType);
            GameObject view = GameObject.Instantiate(viewPrefab, parent, false);
            BindingUtils.Unbind(InjectionBinder, mediatorInjectsWithType);

            return view;
        }

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
