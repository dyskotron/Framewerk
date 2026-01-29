using System;
using System.Threading;
using System.Threading.Tasks;
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

        string GetViewName(Type type);
    }

    public class UiManager : IUiManager
    {
        public const string UI_PREFABS_ROOT = "Prefabs/UI/";
        public const string VIEW_SUFFIX = "View";

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

        public async Task<GameObject> InstantiateViewAsync(string path, Transform parent = null, CancellationToken ct = default, params object[] mediatorInjects)
        {
            if (parent == null)
                parent = _uiParent;

            BindParams(mediatorInjects);
            GameObject uiObj = await AssetManager.GetAssetAsync<GameObject>(UI_PREFABS_ROOT + path, parent, ct);
            UnbindParams(mediatorInjects);

            return uiObj;
        }

        public async Task<GameObject> InstantiateViewExplicitTypeAsync(string path, Transform parent = null, CancellationToken ct = default, params Tuple<object, Type>[] mediatorInjectsWithTypes)
        {
            if (parent == null)
                parent = _uiParent;

            BindParams(mediatorInjectsWithTypes);
            GameObject uiObj = await AssetManager.GetAssetAsync<GameObject>(UI_PREFABS_ROOT + path, parent, ct);
            UnbindParams(mediatorInjectsWithTypes);

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

        public GameObject InstantiateView(GameObject viewPrefab, Transform parent = null, params object[] mediatorInjects)
        {
            if (viewPrefab == null)
                Debug.LogError("UIManager.InstantiateView: viewPrefab is null");

            if (parent == null)
                parent = _uiParent;

            BindParams(mediatorInjects);
            GameObject view = GameObject.Instantiate(viewPrefab, parent, false);
            UnbindParams(mediatorInjects);

            return view;
        }

        public GameObject InstantiateViewExplicitType(GameObject viewPrefab, Transform parent = null, params Tuple<object, Type>[] mediatorInjectsWithType)
        {
            if (viewPrefab == null)
                Debug.LogError("UIManager.InstantiateView: viewPrefab is null");

            if (parent == null)
                parent = _uiParent;

            BindParams(mediatorInjectsWithType);
            GameObject view = GameObject.Instantiate(viewPrefab, parent, false);
            UnbindParams(mediatorInjectsWithType);

            return view;
        }

        public string GetViewName(Type type)
        {
            var name = type.Name;
            return name.Substring(0, name.Length - VIEW_SUFFIX.Length);
        }

        protected virtual string GetViewPath(Type type, string customPath)
        {
            return customPath + GetViewName(type);
        }

        private void BindParams(params Tuple<object, Type>[] bindparamsWithType)
        {
            if (bindparamsWithType != null)
            {
                foreach (var param in bindparamsWithType)
                {
                    InjectionBinder.Bind(param.Item2).ToValue(param.Item1);
                }
            }
        }

        private void UnbindParams(params Tuple<object, Type>[] bindparamsWithType)
        {
            if (bindparamsWithType != null)
            {
                foreach (var param in bindparamsWithType)
                {
                    InjectionBinder.Unbind(param.Item2);
                }
            }
        }

        private void BindParams(params object[] bindparams)
        {
            if (bindparams != null)
            {
                foreach (var param in bindparams)
                {
                    InjectionBinder.Bind(param.GetType()).ToValue(param);
                }
            }
        }

        private void UnbindParams(params object[] bindparams)
        {
            if (bindparams != null)
            {
                foreach (var param in bindparams)
                {
                    InjectionBinder.Unbind(param.GetType());
                }
            }
        }
    }
}
