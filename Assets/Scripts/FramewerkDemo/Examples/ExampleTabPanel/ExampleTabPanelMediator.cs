using Framewerk.Core;
using Framewerk.Managers;
using Framewerk.ViewComponents.TabComponent;
using UnityEngine;

namespace FramewerkDemo.Examples.ExampleTabPanel
{
    
    public class ExampleTabPanelMediator : TabContainerMediator<TabContainerView>
    {
        [Inject] private IUIManager _uiManager;
        
        protected override ITabMediator GetMediatorForContent(GameObject tabContentView, int tabIndex)
        {
            var mediator = _uiManager.CreateUIMediator<ExampleTabMediator>(tabContentView, View.ContentsParent);
            mediator.SetTabIndex(tabIndex);
            return mediator;
        }
    }
}