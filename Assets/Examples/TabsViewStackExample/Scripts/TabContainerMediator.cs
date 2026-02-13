using System.Collections.Generic;
using Framewerk.UI.List;
using Framewerk.UI.ViewStack;
using UnityEngine;

namespace Framewerk.Examples.TabsViewStackExample
{
    public class TabContainerMediator : ListMediator<TabContainerView, TabData>
    {
        [Inject] public TabDataSignal TabDataSignal { get; set; }
        
        private TabViewConnector<TabData> _tabConnector;
        private ViewStackMediator _viewStackMediator;

        public override void OnRegister()
        {
            base.OnRegister();
            TabDataSignal.AddListener(OnDataReceived);
        }

        public override void OnRemove()
        {
            TabDataSignal.RemoveListener(OnDataReceived);
            _tabConnector?.Destroy();
            _tabConnector = null;
            base.OnRemove();
        }

        private void OnDataReceived(List<TabData> data)
        {
            SetData(data);
            
            // Connect to ViewStack after data is set (gives time for mediation to complete)
            if (_tabConnector == null)
            {
                ConnectToViewStack();
            }
            
            // Auto-select first tab
            if (data.Count > 0)
            {
                SelectItemAt(0);
            }
        }
        
        private void ConnectToViewStack()
        {
            if (View.ContentStack == null)
            {
                Debug.LogWarning("[TabsViewStackExample] ContentStack not assigned on TabContainerView");
                return;
            }
            
            // Get the mediator - it should be on the same GameObject as the View
            _viewStackMediator = View.ContentStack.GetComponent<ViewStackMediator>();
            
            if (_viewStackMediator == null)
            {
                Debug.LogWarning("[TabsViewStackExample] ViewStackMediator not found on ContentStack");
                return;
            }
            
            _tabConnector = new TabViewConnector<TabData>(this, _viewStackMediator);
            Debug.Log($"[TabsViewStackExample] Connected tabs to ViewStack with {_viewStackMediator.Count} content panels");
        }
    }
}
