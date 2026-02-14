using Framewerk.UI.List;
using Framewerk.UI.ViewStack;
using UnityEngine;

namespace Framewerk.Examples.TabsViewStackExample
{
    public class TabContainerMediator : ListMediator<TabContainerView, TabData>
    {
        [Inject] public TabsData TabsData { get; set; }

        private TabViewConnector<TabData> _tabConnector;
        private ViewStackMediator _viewStackMediator;

        public override void OnRegister()
        {
            base.OnRegister();

            SetData(TabsData.Tabs);
            ConnectToViewStack();

            // Auto-select first tab
            SelectItemAt(0);
        }

        public override void OnRemove()
        {
            _tabConnector?.Destroy();
            _tabConnector = null;
            base.OnRemove();
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
