using System.Collections.Generic;
using System.Threading.Tasks;
using Framewerk.Managers;
using Plugins.Framewerk;
using strange.extensions.mediation.api;
using strange.extensions.signal.impl;
using UnityEngine;

namespace Framewerk.AppStateMachine
{
    public abstract class AppStateScreen
    {
        [Inject] public IAssetManager AssetManager { get; set; }
        [Inject] public ViewConfig ViewConfig { get; set; }
        [Inject] public IUiManager UiManager { get; set; }

        public readonly Signal EnterFinishedSignal = new Signal();
        public readonly Signal ExitFinishedSignal = new Signal();

        protected TransitionType TransitionType;

        private List<GameObject> _views = new List<GameObject>();

        #region Instantiating UI / Game Prefabs

        protected async Task<GameObject> InstantiateViewAsync(string path = "", Transform parent = null)
        {
            var view = await UiManager.InstantiateViewAsync(path, parent);
            _views.Add(view);
            return view;
        }

        protected async Task<T> InstantiateViewAsync<T>(string path = "", Transform parent = null) where T : MonoBehaviour, IView
        {
            var view = await UiManager.InstantiateViewAsync<T>(path, parent);
            _views.Add(view.gameObject);
            return view;
        }

        protected GameObject InstantiateView(string path = "", Transform parent = null)
        {
            var view = UiManager.InstantiateView(path, parent);
            _views.Add(view);
            return view;
        }

        protected T InstantiateView<T>(string path = "", Transform parent = null) where T : MonoBehaviour, IView
        {
            var view = UiManager.InstantiateView<T>(path, parent);
            _views.Add(view.gameObject);
            return view;
        }

        protected T InstantiateGamePrefab<T>(string path = "", Transform parent = null) where T : MonoBehaviour, IView
        {
            if (parent == null)
                parent = ViewConfig.Container3d;

            path += UiManager.GetViewName(typeof(T));
            var go = AssetManager.GetAsset<GameObject>("GamePrefabs/" + path);
            go.transform.SetParent(parent, false);

            var component = go.GetComponent<T>();
            if (component == null)
            {
                Debug.LogError($"AppStateScreen.InstantiateGamePrefab: Can't find {typeof(T)} on {go}");
                return null;
            }

            _views.Add(go);
            return component;
        }

        protected GameObject InstantiateGamePrefab(string path, Transform parent = null)
        {
            if (parent == null)
                parent = ViewConfig.Container3d;

            var go = AssetManager.GetAsset<GameObject>("GamePrefabs/" + path);
            go.transform.SetParent(parent, false);

            _views.Add(go);
            return go;
        }

        protected async Task<T> InstantiateGamePrefabAsync<T>(string path = "", Transform parent = null) where T : MonoBehaviour, IView
        {
            if (parent == null)
                parent = ViewConfig.Container3d;

            path += UiManager.GetViewName(typeof(T));
            var go = await AssetManager.GetAssetAsync<GameObject>("GamePrefabs/" + path);
            go.transform.SetParent(parent, false);

            var component = go.GetComponent<T>();
            if (component == null)
            {
                Debug.LogError($"AppStateScreen.InstantiateGamePrefabAsync: Can't find {typeof(T)} on {go}");
                return null;
            }

            _views.Add(go);
            return component;
        }

        protected async Task<GameObject> InstantiateGamePrefabAsync(string path, Transform parent = null)
        {
            if (parent == null)
                parent = ViewConfig.Container3d;

            var go = await AssetManager.GetAssetAsync<GameObject>("GamePrefabs/" + path);
            go.transform.SetParent(parent, false);

            _views.Add(go);
            return go;
        }

        #endregion

        #region FSM API

        public async Task PerformEnterAsync()
        {
            TransitionType = TransitionType.Enter;
            await EnterAsync();
            TransitionType = TransitionType.None;
            EnterFinishedSignal.Dispatch();
        }

        public async Task PerformExitAsync()
        {
            TransitionType = TransitionType.Exit;
            await ExitAsync();
            TransitionType = TransitionType.None;
            ExitFinishedSignal.Dispatch();
        }

        public virtual void Destroy()
        {
            foreach (var go in _views)
            {
                if (go != null)
                    AssetManager.ReleaseInstance(go);
            }

            _views.Clear();
        }

        #endregion

        #region Life cycle

        protected virtual Task EnterAsync()
        {
            return Task.CompletedTask;
        }

        protected virtual Task ExitAsync()
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}
