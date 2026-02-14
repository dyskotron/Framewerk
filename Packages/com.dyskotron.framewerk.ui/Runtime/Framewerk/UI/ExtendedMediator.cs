using System;
using System.Collections.Generic;
using Framewerk.UI.Components;
using strange.extensions.mediation.impl;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Framewerk.UI
{
    public class ExtendedMediator<TView> : Mediator
    {
        [Inject] public TView View { get; set; }
        
        // Handlers - using lists to support multiple listeners per component
        private Dictionary<GameObject, List<UnityAction>> ButtonHandlers = new Dictionary<GameObject, List<UnityAction>>();
        private Dictionary<GameObject, List<UnityAction<bool>>> ToggleHandlers = new Dictionary<GameObject, List<UnityAction<bool>>>();
        private Dictionary<GameObject, List<UnityAction<bool, Vector2>>> PointerHandlers = new Dictionary<GameObject, List<UnityAction<bool, Vector2>>>();
        private Dictionary<GameObject, List<UnityAction<float>>> SliderHandlers = new Dictionary<GameObject, List<UnityAction<float>>>();
        private Dictionary<GameObject, List<UnityAction<string>>> InputHandlers = new Dictionary<GameObject, List<UnityAction<string>>>();
        private Dictionary<GameObject, List<Action<DragData>>> DragHandlers = new Dictionary<GameObject, List<Action<DragData>>>();

        public override void OnRemove()
        {
            RemoveListeners();
            base.OnRemove();
        }

        #region RemovingListeners

        protected void RemoveListeners()
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
                foreach (var handler in pair.Value)
                    s.onValueChanged.RemoveListener(handler);
            }
            SliderHandlers.Clear();
        }

        protected void RemoveButtonListeners()
        {
            foreach (var pair in ButtonHandlers)
            {
                var b = pair.Key.GetComponent<Button>();
                foreach (var handler in pair.Value)
                    b.onClick.RemoveListener(handler);
            }
            ButtonHandlers.Clear();
        }

        protected void RemoveToggleListeners()
        {
            foreach (var pair in ToggleHandlers)
            {
                var b = pair.Key.GetComponent<Toggle>();
                foreach (var handler in pair.Value)
                    b.onValueChanged.RemoveListener(handler);
            }
            ToggleHandlers.Clear();
        }

        protected void RemoveInputListeners()
        {
            foreach (var pair in InputHandlers)
            {
                var b = pair.Key.GetComponent<InputField>();
                foreach (var handler in pair.Value)
                    b.onValueChanged.RemoveListener(handler);
            }
            InputHandlers.Clear();
        }

        protected void RemovePointerListeners()
        {
            foreach (var pair in PointerHandlers)
            {
                var m = pair.Key.GetComponent<PointerElement>();
                foreach (var handler in pair.Value)
                    m.OnPointerChanged.RemoveListener(handler);
            }
            PointerHandlers.Clear();
        }

        protected void RemoveDragListeners()
        {
            foreach (var pair in DragHandlers)
            {
                var m = pair.Key.GetComponent<DragElement>();
                foreach (var handler in pair.Value)
                    m.DragChangedSignal.RemoveListener(handler);
            }
            DragHandlers.Clear();
        }

        #endregion

        #region Adding Listeners

        protected void AddButtonListener(Button button, Action func)
        {
            UnityAction internalAction = () => { func(); };
            button.onClick.AddListener(internalAction);
            
            if (!ButtonHandlers.TryGetValue(button.gameObject, out var handlers))
            {
                handlers = new List<UnityAction>();
                ButtonHandlers[button.gameObject] = handlers;
            }
            handlers.Add(internalAction);
        }

        protected void AddSliderListener(Slider slider, Action<float> func)
        {
            UnityAction<float> internalAction = (val) => { func(val); };
            slider.onValueChanged.AddListener(internalAction);
            
            if (!SliderHandlers.TryGetValue(slider.gameObject, out var handlers))
            {
                handlers = new List<UnityAction<float>>();
                SliderHandlers[slider.gameObject] = handlers;
            }
            handlers.Add(internalAction);
        }

        protected void AddToggleListener(Toggle toggle, Action<bool> func)
        {
            UnityAction<bool> internalAction = (val) => { func(val); };
            toggle.onValueChanged.AddListener(internalAction);
            
            if (!ToggleHandlers.TryGetValue(toggle.gameObject, out var handlers))
            {
                handlers = new List<UnityAction<bool>>();
                ToggleHandlers[toggle.gameObject] = handlers;
            }
            handlers.Add(internalAction);
        }

        protected void AddInputListener(InputField input, Action<string> func)
        {
            UnityAction<string> internalAction = (val) => { func(val); };
            input.onEndEdit.AddListener(internalAction);
            
            if (!InputHandlers.TryGetValue(input.gameObject, out var handlers))
            {
                handlers = new List<UnityAction<string>>();
                InputHandlers[input.gameObject] = handlers;
            }
            handlers.Add(internalAction);
        }
        
        protected void AddPointerListener(PointerElement pointerElement, Action<bool, Vector2> func)
        {
            UnityAction<bool, Vector2> internalAction = (state, pos) => { func(state, pos); };
            pointerElement.OnPointerChanged.AddListener(internalAction);
            
            if (!PointerHandlers.TryGetValue(pointerElement.gameObject, out var handlers))
            {
                handlers = new List<UnityAction<bool, Vector2>>();
                PointerHandlers[pointerElement.gameObject] = handlers;
            }
            handlers.Add(internalAction);
        }

        protected void AddDragListener(DragElement dragElement, Action<DragData> handler)
        {
            dragElement.DragChangedSignal.AddListener(handler);
            
            if (!DragHandlers.TryGetValue(dragElement.gameObject, out var handlers))
            {
                handlers = new List<Action<DragData>>();
                DragHandlers[dragElement.gameObject] = handlers;
            }
            handlers.Add(handler);
        }

        #endregion
    }
}
