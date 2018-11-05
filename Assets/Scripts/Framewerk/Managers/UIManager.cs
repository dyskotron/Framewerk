using System;
using Framewerk.Core;
using Framewerk.Mvcs;
using Framewerk.ViewUtils;
using UnityEngine;

namespace Framewerk.Managers
{
    public interface IUIManager
    {
        /// <summary>
        /// Creates Component and return gameobjects component(T)
        /// </summary>
        /// <param name="parent">Where UI prefab should be instantiated</param>
        /// <param name="customPath">Custom path to UI prefab from Components root</param>
        /// <typeparam name="T">BaseMediator Class</typeparam>
        /// <returns>BaseMediator instance</returns>
        T CreateComponent<T>(RectTransform parent = null, string customPath = "");

        T CreateComponent<T>(string customPath = "");

        /// <summary>
        /// Creates Component with view and return its mediator.
        /// </summary>
        /// <param name="parent">Where UI prefab should be instantiated</param>
        /// <param name="customPath">Custom path to UI prefab from Components root</param>
        /// <typeparam name="T">BaseMediator Class</typeparam>
        /// <returns>BaseMediator instance</returns>
        T CreateComponentMediator<T>(RectTransform parent = null, string customPath = "") where T : IMediator, new();

        T CreateComponentMediator<T>(string customPath = "") where T : IMediator, new();

        /// <summary>
        /// Creates UI with view script and return its mediator
        /// </summary>
        /// <param name="parent">New Parent of UI prefab</param>
        /// <param name="customPath">Custom path to UI prefab from UI root</param>
        /// <typeparam name="T">BaseMediator Class</typeparam>
        /// <returns>Mediator instance</returns>
        T CreateUIMediator<T>(RectTransform parent, string customPath = "", ViewInitData data = null) where T : IMediator, new();

        T CreateUIMediator<T>(string customPath = "", ViewInitData data = null) where T : IMediator, new();

        /// <summary>
        /// Clones given prefab instead of loading from resources
        /// </summary>
        /// <param name="createdView">Prefab to be cloned</param>
        /// <param name="parent">New Parent of UI prefab</param>
        /// <param name="clone">Wheter we should clone view as new gameobject or mediate existing</param>
        /// <typeparam name="T">Mediator instance</typeparam>
        /// <returns></returns>
        T CreateUIMediator<T>(GameObject viewPrefab, RectTransform parent = null, ViewInitData data = null) where T : IMediator, new();

        //TODO: create overload which takes directly view class
        /// <summary>
        /// Creates mediator and mediate already instantiated prefab
        /// </summary>
        /// <param name="viewPrefab"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T AttachUIMediator<T>(GameObject viewPrefab) where T : IMediator, new();
        
        /// <summary>
        /// Creates mediator and mediate given view from already instantiated prefab
        /// </summary>
        /// <param name="view"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T AttachUIMediator<T>(View view) where T : IMediator, new();

        [Obsolete("ShowUI is deprecated, please use CreateUIMediator instead.")]
        GameObject ShowUI(string path, Transform parent = null);

        [Obsolete("ShowUI is deprecated, please use CreateUIMediator instead.")]
        T ShowUI<T>(string path, Transform parent = null) where T : MonoBehaviour;

        string GetMediatorName(Type type);
    }

    public class UIManager : IUIManager
    {
        public const string UI_PREFABS_ROOT = "Prefabs/UI/";
        public const string SUBFOLDER_COMPONENTS = "Components/";
        public const string MEDIATOR_SUFFIX = "Mediator";

        [Inject]
        protected IAssetManager Assets;

        [Inject]
        protected IInjector Injector;

        [Inject]
        protected BaseViewSettings ViewSettings
        {
            get
            {
                return _viewSettings;
            }
            set
            {
                _viewSettings = value;
                _uiParent = _viewSettings.UIParent;
            }
        }

        private BaseViewSettings _viewSettings;
        private RectTransform _uiParent;

        public virtual T CreateComponent<T>(RectTransform parent = null, string customPath = "")
        {
            if (parent == null)
                parent = UIParent();

            var view = Assets.GetAsset<GameObject>(UI_PREFABS_ROOT + SUBFOLDER_COMPONENTS + customPath);
            view.transform.SetParent(parent, false);

            return view.GetComponent<T>();
        }

        public virtual T CreateComponent<T>(string customPath = "")
        {
            return CreateComponent<T>(null, customPath);
        }

        public virtual T CreateComponentMediator<T>(RectTransform parent = null, string customPath = "") where T : IMediator, new()
        {
            return CreateUIMediator<T>(parent, SUBFOLDER_COMPONENTS + customPath);
        }

        public virtual T CreateComponentMediator<T>(string customPath = "") where T : IMediator, new()
        {
            return CreateComponentMediator<T>(null, customPath);
        }

        public virtual T CreateUIMediator<T>(RectTransform parent, string customPath = "", ViewInitData data = null) where T : IMediator, new()
        {
            if (parent == null)
                parent = UIParent();

            //view
            var view = Assets.GetAsset<GameObject>(GetMediatorPath(typeof(T), customPath));
            view.transform.SetParent(parent, false);

            //mediator
            var mediator = new T();
            Injector.InjectInto(mediator);

            if (data != null)
                mediator.SetInitData(data);

            mediator.RegisterView(view);

            return mediator;
        }

        public T CreateUIMediator<T>(string customPath = "", ViewInitData data = null) where T : IMediator, new()
        {
            return CreateUIMediator<T>(null, customPath, data);
        }

        public T CreateUIMediator<T>(GameObject viewPrefab, RectTransform parent = null, ViewInitData data = null)
            where T : IMediator, new()
        {
            if (viewPrefab == null)
                Debug.LogErrorFormat("UIManager.ShowUI viewPrefab for {0} is null", typeof(T));

            if (parent == null)
                parent = UIParent();

            //view
            GameObject view = GameObject.Instantiate(viewPrefab, parent, false);

            //mediator
            var mediator = new T();
            Injector.InjectInto(mediator);

            if (data != null)
                mediator.SetInitData(data);

            mediator.RegisterView(view);

            return mediator;
        }

        public T AttachUIMediator<T>(GameObject viewPrefab) where T : IMediator, new()
        {
            //mediator
            var mediator = new T();
            Injector.InjectInto(mediator);
            mediator.RegisterView(viewPrefab);

            return mediator;
        }

        public T AttachUIMediator<T>(View view) where T : IMediator, new()
        {
            //TODO: create overload with pasing diectly view to mediator itself
            return AttachUIMediator<T>(view.gameObject);
        }

        /// <summary>
        /// Instantiate UI by path.
        /// </summary>
        /// <param name="path">Path to UI prefab from UI root</param>
        /// <param name="parent">Where UI prefab should be instantiated</param>
        /// <returns></returns>
        [Obsolete("ShowUI is deprecated, please use CreateUIMediator instead.")]
        public GameObject ShowUI(string path, Transform parent = null)
        {
            if (parent == null)
                parent = UIParent();

            GameObject uiObj = Assets.GetAsset<GameObject>(UI_PREFABS_ROOT + path);
            uiObj.transform.SetParent(parent, false);

            return uiObj;
        }

        [Obsolete("ShowUI is deprecated, please use CreateUIMediator instead.")]
        public T ShowUI<T>(string path, Transform parent = null) where T : MonoBehaviour
        {
            if (parent == null)
                parent = UIParent();

            var uiObj = ShowUI(path, parent);
            var component = uiObj.GetComponent<T>();

            if (component == null)
                Debug.LogErrorFormat("UIManager.ShowUI There is no {0} script attached on {1} Prefab", typeof(T), uiObj);

            return component;
        }

        public string GetMediatorName(Type type)
        {
            var name = type.Name;
            return name.Substring(0, name.Length - MEDIATOR_SUFFIX.Length);
        }

        protected virtual string GetMediatorPath(Type type, string customPath)
        {
            return UI_PREFABS_ROOT + customPath + GetMediatorName(type);
        }

        protected virtual RectTransform UIParent()
        {
            return _uiParent;
        }
    }
}