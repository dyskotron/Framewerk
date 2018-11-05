﻿using System;
using System.Collections.Generic;
using Framewerk.Core;
using Framewerk.Events;
using Framewerk.Managers;
 using Framewerk.Mvcs.UIElements;
 using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Framewerk.Mvcs
{
    public interface IMediator
    {
        void RegisterView(GameObject ui);
        void SetInitData(ViewInitData data);
        void SetActive(bool value);
        void Show();
        void Hide();
        void Destroy();
        GameObject UIgo { get;}
        bool ActiveInHierarchy { get;}
    }
    
    public interface IPersistentMediator
    {
        
    }

    /// <summary>
    /// BaseMediator is base class representing single MediatorView element and is handling view-controller communication and view controlling itself.
    /// Class offers functionality common to all MediatorView elements e.g. registrating button handlers, slider & toggle handlers, open and close animations
    /// </summary>
    public class Mediator<T> : IMediator where T : IView
    {
        [Inject] protected IEventDispatcher EventDispatcher;

        [Inject] protected IUIManager UIManager;

        protected T View;

        protected bool inited = false;
        protected bool destroyed = false;

        // Handlers
        private Dictionary<GameObject, UnityAction> ButtonHandlers;
        private Dictionary<GameObject, UnityAction<bool>> ToggleHandlers;
        private Dictionary<GameObject, UnityAction<bool>> PointerHandlers;
        private Dictionary<GameObject, UnityAction<float>> SliderHandlers;
        private Dictionary<GameObject, UnityAction<string>> InputHandlers;
        private Dictionary<GameObject, UnityAction<DragStateType>> DragHandlers;

        private Canvas _mediatorCanvas;
        private GraphicRaycaster _raycaster;

        public GameObject UIgo { get; private set; }
        
        public bool ActiveInHierarchy { get { return UIgo.activeInHierarchy; } }

        #region Creation.

        public Mediator()
        {
            ButtonHandlers = new Dictionary<GameObject, UnityAction>();
            ToggleHandlers = new Dictionary<GameObject, UnityAction<bool>>();
            SliderHandlers = new Dictionary<GameObject, UnityAction<float>>();
            InputHandlers = new Dictionary<GameObject, UnityAction<string>>();
            PointerHandlers = new Dictionary<GameObject, UnityAction<bool>>();
            DragHandlers = new Dictionary<GameObject, UnityAction<DragStateType>>();
        }

        public virtual void RegisterView(GameObject ui)
        {
            if (UIgo != null)
                return;

            UIgo = ui;
            View = UIgo.GetComponent<T>();

            if (View == null)
                Debug.LogError("BaseMediator.RegisterView Prefab " + UIgo.name + "Does not have component type "+ typeof(T));

            Init();
        }

        public virtual void SetInitData(ViewInitData data)
        {

        }

        protected virtual void Init()
        {
            inited = true;
        }

        public virtual void SetActive(bool value)
        {
            if (_mediatorCanvas != null)
            {
                //hides mediator
                _mediatorCanvas.enabled = value;
                
                //disable raycasting (otherways we'll be clicking hidden stuff!) 
                if(_raycaster != null)
                    _raycaster.enabled = value;
                
                //TODO: not so nice & fast hotfix for nested mediators with canvases
                var raycasters = View.GameObject.GetComponentsInChildren<GraphicRaycaster>();
                foreach (var raycaster in raycasters)
                {
                    //when enabling, set raycaster enabled only when canvas is enabled too
                    if (value)
                    {
                        var canvas = raycaster.GetComponent<Canvas>();
                        if(canvas != null && canvas.enabled)
                            raycaster.enabled = true;
                    }
                    //on disable simply disable all children raycasters to
                    else
                    {
                        raycaster.enabled = value;
                    }
                }
            }
            else if (View != null)
            {
                View.GameObject.SetActive(value);
            }
        }

        public virtual void Show()
        {
            SetActive(true);
        }

        public virtual void Hide()
        {
            SetActive(false);
        }

        public void EnableHidingByCanvas(bool addRaycaster = true)
        {
            if (View.GameObject.GetComponent<Canvas>())
            {
                Debug.LogWarningFormat("<color=\"aqua\">{0} : {1}</color>", this, "View already has canvas. (Valid only for GameObjects with multiple views)");    
                return;
            }
            
            _mediatorCanvas = View.GameObject.AddComponent<Canvas>();
            
            if(addRaycaster)
                _raycaster = View.GameObject.AddComponent<GraphicRaycaster>();
        }

        #endregion

        #region MediatorView Listeners.

        private void RemoveListeners()
        {
            RemoveButtonListeners();
            RemoveToggleListeners();
            RemoveSliderListeners();
            RemoveInputListeners();
            RemovePointerListeners();
            RemoveDragListeners();
        }

        protected void RemoveSliderListeners()
        {
            foreach (var pair in SliderHandlers)
            {
                var s = pair.Key.GetComponent<Slider>();
                s.onValueChanged.RemoveListener(pair.Value);
            }
            SliderHandlers.Clear();
        }

        protected void RemoveButtonListeners()
        {
            foreach (var pair in ButtonHandlers)
            {
                var b = pair.Key.GetComponent<Button>();
                b.onClick.RemoveListener(pair.Value);
            }
            ButtonHandlers.Clear();
        }

        protected void RemoveToggleListeners()
        {
            foreach (var pair in ToggleHandlers)
            {
                var b = pair.Key.GetComponent<Toggle>();
                b.onValueChanged.RemoveListener(pair.Value);
            }
            ToggleHandlers.Clear();
        }

        protected void RemoveInputListeners()
        {
            foreach (var pair in InputHandlers)
            {
                var b = pair.Key.GetComponent<InputField>();
                b.onValueChanged.RemoveListener(pair.Value);
            }
            InputHandlers.Clear();
        }

        protected void RemovePointerListeners()
        {
            foreach (var pair in PointerHandlers)
            {
                var m = pair.Key.GetComponent<PointerElement>();
                m.OnPointerChanged.RemoveListener(pair.Value);
            }
            PointerHandlers.Clear();
        }

        protected void RemoveDragListeners()
        {
            foreach (var pair in DragHandlers)
            {
                var m = pair.Key.GetComponent<DragElement>();
                m.OnDragChanged.RemoveListener(pair.Value);
            }
            DragHandlers.Clear();
        }

        protected void AttachButtonListener(GameObject button, Action func)
        {
            var b = button.GetComponent<Button>();
            if (b == null)
            {
                Debug.LogError("BaseMediator.AttachButtonListener The GameObject : "
                               + button.name + " in " + this.GetType().ToString() +
                               " doesn't have Button component");
                return;
            }

            AttachButtonListener(b, func);
        }

        protected void AttachButtonListener(Button button, Action func)
        {
            UnityAction internalAction = () => { func(); };
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(internalAction);
            ButtonHandlers[button.gameObject] = internalAction;
        }

        protected void AttachSliderListener(GameObject slider, Action<float> func)
        {
            var s = slider.GetComponent<Slider>();
            if (s == null)
            {
                Debug.LogError("BaseMediator.AttachSliderListener The GameObject : " + s.name + " in " +
                               this.GetType().ToString() +
                               " doesn't have Button component");
                return;
            }

            AttachSliderListener(s, func);
        }

        protected void AttachSliderListener(Slider slider, Action<float> func)
        {
            UnityAction<float> internalAction = (val) => { func(val); };

            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener(internalAction);
            SliderHandlers[slider.gameObject] = internalAction;
        }

        protected void AttachToggleListener(GameObject toggle, Action<Boolean> func)
        {
            var t = toggle.GetComponent<Toggle>();
            if (t == null)
            {
                Debug.LogError("BaseMediator.AttachSliderListener The GameObject : " + t.name + " in " +
                               this.GetType().ToString() +
                               " doesn't have Button component");
                return;
            }

            AttachToggleListener(t, func);
        }

        protected void AttachToggleListener(Toggle toggle, Action<bool> func)
        {
            UnityAction<bool> internalAction = (val) => { func(val); };

            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener(internalAction);
            ToggleHandlers[toggle.gameObject] = internalAction;
        }

        protected void AttachInputListener(GameObject input, Action<string> func)
        {
            var t = input.GetComponent<InputField>();
            if (t == null)
            {
                Debug.LogError("BaseMediator.AttachInputListener The GameObject : " + t.name + " in " +
                               this.GetType().ToString() +
                               " doesn't have InputField component");
                return;
            }

            AttachInputListener(t, func);
        }

        protected void AttachInputListener(InputField input, Action<string> func)
        {
            UnityAction<string> internalAction = (val) => { func(val); };

            input.onValueChanged.RemoveAllListeners();
            input.onValueChanged.AddListener(internalAction);
            InputHandlers[input.gameObject] = internalAction;
        }
        
        protected void AttachPointerListener(GameObject mouseElement, Action<bool> func)
        {
            var t = mouseElement.GetComponent<PointerElement>();
            if (t == null)
            {
                Debug.LogError("BaseMediator.AttachMouseListener The GameObject : " + t.name + " in " +
                               this.GetType().ToString() +
                               " doesn't have MouseElement component");
                return;
            }

            AttachPointerListener(t, func);
        }
        
        protected void AttachPointerListener(PointerElement pointerElement, Action<bool> func)
        {
            UnityAction<bool> internalAction = (val) => { func(val); };

            pointerElement.OnPointerChanged.RemoveAllListeners();
            pointerElement.OnPointerChanged.AddListener(internalAction);
            PointerHandlers[pointerElement.gameObject] = internalAction;
        }

        protected void AttachDragListener(DragElement dragElement, Action<DragStateType> func)
        {
            UnityAction<DragStateType> internalAction = (val) => { func(val); };

            dragElement.OnDragChanged.RemoveAllListeners();
            dragElement.OnDragChanged.AddListener(internalAction);
        }

        #endregion

        #region Framewerk Listeners

        protected void DispatchEvent<TEvent>(TEvent e) where TEvent : AbstractEvent
        {
            EventDispatcher.DispatchEvent(e);
        }

        #endregion

        #region Destroy.

        public virtual void Destroy()
        {
            StartCloseAnimation();
        }

        protected virtual void StartCloseAnimation()
        {
            OnCloseAnimationComplete();
        }

        protected void OnCloseAnimationComplete()
        {
            if (destroyed)
                return;

            if (UIgo != null)
                DestroyView();
        }

        private void DestroyView()
        {
            RemoveListeners();
            Object.Destroy(UIgo);

            destroyed = true;
        }

        #endregion
    }
}