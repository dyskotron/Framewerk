using System.Collections.Generic;
using System.Threading;
using Framewerk.Managers;
using Framewerk.UI.ViewStack;
using Plugins.Framewerk;
using strange.extensions.command.impl;
using UnityEngine;
using UnityEngine.UI;

namespace Framewerk.Examples.TabsViewStackExample
{
    /// <summary>
    /// Start command demonstrating the proper ViewGroup pattern.
    /// 
    /// The ViewGroup pattern instantiates MULTIPLE views together, sharing state via
    /// [ViewGroupShared] injection. The framework automatically creates and shares
    /// a SelectionChangedSignal instance between all mediators in the group that
    /// have [Inject, ViewGroupShared] on the same type.
    /// 
    /// Key principles:
    /// 1. Views are SEPARATE - each prefab contains ONE view type
    /// 2. Views are instantiated TOGETHER via InstantiateViewsAsync(ViewGroup)
    /// 3. Shared state is automatic - no manual signal creation/passing needed
    /// 4. Each mediator handles its OWN concern (separation of concerns)
    /// </summary>
    public class TabsViewStackExampleStartCommand : Command
    {
        [Inject] public IUiManager UiManager { get; set; }
        [Inject] public ViewConfig ViewConfig { get; set; }

        public override void Execute()
        {
            var tabs = new List<TabData>
            {
                new TabData { Title = "Home", ContentIndex = 0 },
                new TabData { Title = "Profile", ContentIndex = 1 },
                new TabData { Title = "Settings", ContentIndex = 2 }
            };

            var contentPanels = new List<ContentPanelData>
            {
                new ContentPanelData
                {
                    Title = "Home",
                    BackgroundColor = new Color(0.2f, 0.4f, 0.6f, 1f),
                    Description = "Welcome to the Home screen!\n\nThis is where you'd see your main content, dashboard, or feed."
                },
                new ContentPanelData
                {
                    Title = "Profile",
                    BackgroundColor = new Color(0.4f, 0.6f, 0.3f, 1f),
                    Description = "This is the Profile screen.\n\nUser information, avatar, and account details would go here."
                },
                new ContentPanelData
                {
                    Title = "Settings",
                    BackgroundColor = new Color(0.6f, 0.4f, 0.2f, 1f),
                    Description = "Settings screen.\n\nConfigure your preferences, notifications, and app behavior here."
                }
            };

            var tabsData = new TabsData
            {
                Tabs = tabs,
                ContentPanels = contentPanels
            };

            // Create layout container for both views
            var layoutParent = CreateLayoutContainer();
            var tabsParent = CreateTabsArea(layoutParent);
            var contentParent = CreateContentArea(layoutParent);

            // ViewGroup Pattern: Instantiate BOTH views together as a group.
            // The framework automatically:
            // 1. Scans both mediators for [ViewGroupShared] properties
            // 2. Creates shared instances (SelectionChangedSignal) 
            // 3. Injects the SAME instance into both mediators
            // No manual signal creation needed!
            var viewGroup = new ViewGroup()
                .Add<TabContainerView>(tabsParent)
                .Add<ViewStackView>(contentParent);

            // tabsData is passed as a binding - both mediators receive it
            // Fire and forget - the async operation will complete on its own
            _ = UiManager.InstantiateViewsAsync(viewGroup, CancellationToken.None, tabsData);
        }

        private RectTransform CreateLayoutContainer()
        {
            var go = new GameObject("TabsViewStackLayout");
            var rect = go.AddComponent<RectTransform>();
            rect.SetParent(ViewConfig.UiDefault, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 0;

            return rect;
        }

        private Transform CreateTabsArea(RectTransform parent)
        {
            var go = new GameObject("TabsArea");
            var rect = go.AddComponent<RectTransform>();
            rect.SetParent(parent, false);

            var layoutElement = go.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 60;
            layoutElement.flexibleHeight = 0;

            return rect;
        }

        private Transform CreateContentArea(RectTransform parent)
        {
            var go = new GameObject("ContentArea");
            var rect = go.AddComponent<RectTransform>();
            rect.SetParent(parent, false);

            var layoutElement = go.AddComponent<LayoutElement>();
            layoutElement.flexibleHeight = 1;

            return rect;
        }
    }
}
