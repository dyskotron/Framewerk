using System.Collections.Generic;
using Framewerk.Managers;
using Plugins.Framewerk;
using strange.extensions.mediation.api;
using strange.extensions.promise.api;
using strange.extensions.promise.impl;
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

        #region Instantiating UI / Game Prefabs (Promise-based)

        protected IPromise<GameObject> InstantiateViewAsync(string path = "", Transform parent = null)
        {
            var promise = new Promise<GameObject>();

            UiManager.InstantiateViewAsync(path, parent)
                .Then(view =>
                {
                    _views.Add(view);
                    promise.Dispatch(view);
                })
                .Fail(ex => promise.ReportFail(ex));

            return promise;
        }

        protected IPromise<T> InstantiateViewAsync<T>(string path = "", Transform parent = null) where T : MonoBehaviour, IView
        {
            var promise = new Promise<T>();

            UiManager.InstantiateViewAsync<T>(path, parent)
                .Then(view =>
                {
                    _views.Add(view.gameObject);
                    promise.Dispatch(view);
                })
                .Fail(ex => promise.ReportFail(ex));

            return promise;
        }

        protected IPromise<T> InstantiateGamePrefabAsync<T>(string path = "", Transform parent = null) where T : MonoBehaviour, IView
        {
            var promise = new Promise<T>();

            if (parent == null)
                parent = ViewConfig.Container3d;

            path += UiManager.GetViewName(typeof(T));

            AssetManager.GetAssetAsync<GameObject>("GamePrefabs/" + path)
                .Then(go =>
                {
                    go.transform.SetParent(parent, false);
                    var component = go.GetComponent<T>();

                    if (component == null)
                    {
                        Debug.LogError($"AppStateScreen.InstantiateGamePrefabAsync: Can't find {typeof(T)} on {go}");
                        promise.ReportFail(new System.Exception($"Can't find {typeof(T)} on {go}"));
                        return;
                    }

                    _views.Add(go);
                    promise.Dispatch(component);
                })
                .Fail(ex => promise.ReportFail(ex));

            return promise;
        }

        protected IPromise<GameObject> InstantiateGamePrefabAsync(string path, Transform parent = null)
        {
            var promise = new Promise<GameObject>();

            if (parent == null)
                parent = ViewConfig.Container3d;

            AssetManager.GetAssetAsync<GameObject>("GamePrefabs/" + path)
                .Then(go =>
                {
                    go.transform.SetParent(parent, false);
                    _views.Add(go);
                    promise.Dispatch(go);
                })
                .Fail(ex => promise.ReportFail(ex));

            return promise;
        }

        #endregion

        #region Instantiating UI / Game Prefabs (Synchronous)

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

        #endregion

        #region FSM API

        public void PerformEnter()
        {
            TransitionType = TransitionType.Enter;
            Enter();
            TransitionType = TransitionType.None;
            EnterFinishedSignal.Dispatch();
        }

        public void PerformExit()
        {
            TransitionType = TransitionType.Exit;
            Exit();
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

        protected virtual void Enter()
        {
        }

        protected virtual void Exit()
        {
        }

        #endregion
    }
}
