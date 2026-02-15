using System.Threading;
using Framewerk.Managers;
using Framewerk.UI.ViewStack;
using strange.extensions.mediation.impl;
using UnityEngine;

namespace Framewerk.Examples.TabsViewStackExample
{
    /// <summary>
    /// Mediator for TabsViewStackContainerView.
    /// 
    /// Spawns the ViewGroup (TabContainerView + ViewStackView) into the container's
    /// designated areas. This keeps all instantiation going through UiManager.
    /// 
    /// Pattern: Command spawns container → Container mediator spawns ViewGroup
    /// </summary>
    public class TabsViewStackContainerMediator : Mediator
    {
        [Inject] public TabsViewStackContainerView View { get; set; }
        [Inject] public IUiManager UiManager { get; set; }
        [Inject] public TabsViewStackDataProvider DataProvider { get; set; }

        public override void OnRegister()
        {
            var tabsData = DataProvider.GetTabsViewData();

            // ViewGroup Pattern: Instantiate both views together as a group.
            // The framework automatically:
            // 1. Scans both mediators for [ViewGroupShared] properties
            // 2. Creates shared instances (SelectionChangedSignal)
            // 3. Injects the SAME instance into both mediators
            var viewGroup = new ViewGroup()
                .Add<TabContainerView>(View.TabsParent)
                .Add<ViewStackView>(View.ContentParent);

            // tabsData is passed as a binding - both mediators receive it
            _ = UiManager.InstantiateViewsAsync(viewGroup, CancellationToken.None, tabsData);
        }
    }
}
