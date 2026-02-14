using System.Collections.Generic;
using Framewerk.UI;
using strange.extensions.signal.impl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Framewerk.Popups
{
    public class PopupMediator<TView> : ExtendedMediator<TView>, IPopupMediator
    {
        [Inject] public List<PopupButtonSetting> PopupOptionSettings { get; set; }
        [Inject] public PopupOpenedSignal PopupOpenedSignal { get; set; }

        public Signal<IPopupMediator> PopupClosedSignal { get; } = new();

        private List<Button> _buttons = new();

        public override void OnRegister()
        {
            base.OnRegister();
            PopupOpenedSignal.Dispatch(this);
        }

        protected virtual void Init(PopupView popupView)
        {
            foreach (var setting in PopupOptionSettings)
            {
                var buttonInst = Instantiate(popupView.buttonPrefab.gameObject, popupView.buttonContainer.transform, false);
                var textComp = buttonInst.GetComponentInChildren<TextMeshProUGUI>();
                if (textComp == null)
                {
                    Debug.LogWarningFormat($"TextMeshProUGUI text component not found on popup button prefab {popupView.buttonPrefab.name} when creating popup {this.gameObject.name}.");
                }
                else
                {
                    textComp.text = setting.optionText;
                }

                var buttonComp = buttonInst.GetComponent<Button>();
                buttonComp.onClick.AddListener(() =>
                {
                    setting.clickHandler?.Invoke();
                    setting.clickPromise?.Dispatch();
                    if (setting.closesPopup)
                        Destroy(gameObject);
                });
                
                _buttons.Add(buttonComp);
            }            
        }

        public override void OnRemove()
        {
            PopupClosedSignal.Dispatch(this);

            foreach (var button in _buttons)
            {
                button.onClick.RemoveAllListeners();
            }
            
            base.OnRemove();
        }

        public void Close()
        {
            Destroy(gameObject);
        }
    }
}