using System;
using System.Collections.Generic;
using Framewerk.Core;
using Framewerk.ViewUtils;
using UnityEngine;

namespace Framewerk.Managers.Popup
{
    public interface IPopUpManager
    {
        void ClosePopUp(IPopupMediator popupMediator);
        void ClosePopUp<T>() where T : IPopupMediator;
        T ShowPopUp<T>(string customPath = "", ViewInitData data = null) where T : IPopupMediator, new();
        T ShowPopUp<T>(ViewInitData data) where T : IPopupMediator, new();
        void CloseAll();
        bool HandleBackButon();
    }

    public class PopUpManager : IPopUpManager
    {
        public const string SUBFOLDER_POPUPS = "Popups/";
        
        [Inject] protected IUIManager UI;
        [Inject] protected IEventDispatcher EventDispatcher;
        [Inject] protected BaseViewSettings ViewSettings;

        private List<IPopupMediator> _popups;
        
        public PopUpManager()
        {
            _popups = new List<IPopupMediator>();
        }

        public T ShowPopUp<T>(string customPath = "", ViewInitData data = null) where T : IPopupMediator, new()
        {
            var popup = UI.CreateUIMediator<T>(ViewSettings.PopupParent, SUBFOLDER_POPUPS + customPath, data);
            _popups.Add(popup);
            return popup;
        }
        
        public void ClosePopUp<T>() where T : IPopupMediator
        {
            for (var i = _popups.Count - 1; i >= 0; i--)
            {
                if(_popups[i].GetType() == typeof(T))
                    _popups[i].Close();    
            } 
        }
        
        public T ShowPopUp<T>(ViewInitData data) where T : IPopupMediator, new()
        {
            return ShowPopUp<T>("", data);
        }

        public void ClosePopUp(IPopupMediator popupMediator)
        {
            if (_popups.Contains(popupMediator))
                _popups.Remove(popupMediator);
            
            try{
                //TODO: remove! Fallback because of old modal and other popups, remove after cleaning all old shiath
                var type = popupMediator.GetType();
                popupMediator.Destroy();
                EventDispatcher.DispatchEvent(new PopupClosedEvent(type));
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat("<color=\"red\">{0} : {1}</color>", this, e);
            }
        }

        public virtual bool HandleBackButon()
        {
            if (_popups.Count > 0)
            {
                var lastIndex = _popups.Count - 1;
                if (_popups[lastIndex] != null)
                    _popups[lastIndex].HandleBackButton();
                else
                    return HandleBackButon();

                return true;
            }

            return false;
        }

        public void CloseAll()
        {

            for (var i = _popups.Count - 1; i >= 0; i--)
            {
                //when popup is closed, it removes itself from _popups list(via close popup method)
                if(_popups[i] != null)
                    _popups[i].Close();
            }
        }
    }
}