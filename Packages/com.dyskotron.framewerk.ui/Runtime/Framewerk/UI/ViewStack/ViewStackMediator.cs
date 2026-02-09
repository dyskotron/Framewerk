using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using UnityEngine;

namespace Framewerk.UI.ViewStack
{
    /// <summary>
    /// Mediator for ViewStack - manages visibility of children in Content.
    /// Shows one child at a time, like Qt's QStackedWidget.
    /// Children are just GameObjects in hierarchy, no special components needed.
    /// </summary>
    public class ViewStackMediator : Mediator, IViewStack
    {
        [Inject]
        public ViewStackView View { get; set; }
        
        private int _currentIndex = -1;
        
        public int CurrentIndex => _currentIndex;
        
        public int Count => View.Content != null ? View.Content.childCount : 0;
        
        public int ViewCount => Count; // Legacy API
        
        public Signal<int, Transform> CurrentChanged { get; } = new Signal<int, Transform>();
        
        public override void OnRegister()
        {
            base.OnRegister();
            
            // Initialize: show first child if any exist
            if (Count > 0)
            {
                ShowByIndex(0);
            }
        }
        
        public void ShowByIndex(int index)
        {
            if (View.Content == null || Count == 0)
            {
                _currentIndex = -1;
                return;
            }
            
            if (index < 0 || index >= Count)
            {
                Debug.LogWarning($"[ViewStack] Index {index} out of range (0-{Count - 1})");
                return;
            }
            
            // Hide all children, show the selected one
            for (int i = 0; i < Count; i++)
            {
                View.Content.GetChild(i).gameObject.SetActive(i == index);
            }
            
            _currentIndex = index;
            CurrentChanged.Dispatch(_currentIndex, GetCurrentView());
        }
        
        public void ShowByTransform(Transform t)
        {
            if (View.Content == null || t == null)
                return;
            
            for (int i = 0; i < Count; i++)
            {
                if (View.Content.GetChild(i) == t)
                {
                    ShowByIndex(i);
                    return;
                }
            }
            
            Debug.LogWarning($"[ViewStack] Transform '{t.name}' not found in Content");
        }
        
        public Transform GetCurrentView()
        {
            if (_currentIndex < 0 || _currentIndex >= Count)
                return null;
            
            return View.Content.GetChild(_currentIndex);
        }
        
        public Transform GetViewAtIndex(int index)
        {
            if (View.Content == null || index < 0 || index >= Count)
                return null;
            
            return View.Content.GetChild(index);
        }
        
        // Legacy API for TabViewConnector
        public void ShowView(int index) => ShowByIndex(index);
    }
}
