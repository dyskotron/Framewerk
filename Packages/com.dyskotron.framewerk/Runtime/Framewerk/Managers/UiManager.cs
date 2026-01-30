using System;
using System.Collections.Generic;
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
        Task<T> InstantiateViewAsync<T>(string path, Transform parent = null, CancellationToken ct = default, params object[] mediatorInjects) where T : IView;
        Task<T> InstantiateViewExplicitTypeAsync<T>(string path = "", Transform parent = null, CancellationToken ct = default, params Tuple<object, Type>[] mediatorInjectsWithTypes) where T : IView;

        GameObject InstantiateView(string path, Transform parent = null, params object[] mediatorInjects);
        GameObject InstantiateViewExplicitType(string path, Transform parent = null, params Tuple<object, Type>[] mediatorInjectsWithTypes);
        T InstantiateView<T>(string path = "", Transform parent = null, params object[] mediatorInjects) where T : IView;
        T InstantiateViewExplicitType<T>(string path = "", Transform parent = null, params Tuple<object, Type>[] mediatorInjectsWithTypes) where T : IView;

        string GetViewName(Type type);
    }

    public class UiManager : IUiManager
    {
        public const string VIEW_SUFFIX = "View";

        public string TypeKey { get; set; } = "UI";
        public bool TypeKeyIsPrefix { get; set; } = false;
        public bool BindInterfaces { get; set; } = true;
        public bool BindBaseClasses { get; set; } = false;

        [Inject]
        public IAssetManager AssetManager { get; set; }

        [Inject]
        public IInjectionBinder InjectionBinder { get; set; }

        [Inject]
        public ViewConfig ViewConfig
        {
            set
            {
                _uiParent = value.UiDefault;
            }
        }

        private Transform _uiParent;

        private string BuildAddress(params string[] segments)
        {
            var parts = new List<string>();
            foreach (var seg in segments)
            {
                if (string.IsNullOrEmpty(seg)) continue;
                parts.Add(seg.Trim('/'));
            }
            return string.Join("/", parts);
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

        public async Task<T> InstantiateViewAsync<T>(string path = "", Transform parent = null, CancellationToken ct = default, params object[] mediatorInjects) where T : IView
        {
            if (parent == null)
                parent = _uiParent;

            var uiObj = await InstantiateViewAsync(GetViewPath(typeof(T), path), parent, ct, mediatorInjects);
            var component = uiObj.GetComponent<T>();

            if (component == null)
                Debug.LogError($"UIManager.InstantiateViewAsync: No {typeof(T)} on {uiObj}");

            return component;
        }

        public async Task<T> InstantiateViewExplicitTypeAsync<T>(string path = "", Transform parent = null, CancellationToken ct = default, params Tuple<object, Type>[] mediatorInjectsWithTypes) where T : IView
        {
            if (parent == null)
                parent = _uiParent;

            var uiObj = await InstantiateViewExplicitTypeAsync(GetViewPath(typeof(T), path), parent, ct, mediatorInjectsWithTypes);
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

        public T InstantiateView<T>(string path = "", Transform parent = null, params object[] mediatorInjects) where T : IView
        {
            if (parent == null)
                parent = _uiParent;

            var uiObj = InstantiateView(GetViewPath(typeof(T), path), parent, mediatorInjects);
            var component = uiObj.GetComponent<T>();

            if (component == null)
                Debug.LogError($"UIManager.InstantiateView: No {typeof(T)} on {uiObj}");

            return component;
        }

        public T InstantiateViewExplicitType<T>(string path = "", Transform parent = null, params Tuple<object, Type>[] mediatorInjectsWithTypes) where T : IView
        {
            if (parent == null)
                parent = _uiParent;

            var uiObj = InstantiateViewExplicitType(GetViewPath(typeof(T), path), parent, mediatorInjectsWithTypes);
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

        protected virtual string GetViewPath(Type type, string customPath)
        {
            var viewName = GetViewName(type);
            if (TypeKeyIsPrefix)
                return BuildAddress(TypeKey, customPath, viewName);
            else
                return BuildAddress(customPath, TypeKey, viewName);
        }

    }
}
