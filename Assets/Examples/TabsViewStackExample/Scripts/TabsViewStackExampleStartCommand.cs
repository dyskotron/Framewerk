using System.Collections.Generic;
using Framewerk.Managers;
using strange.extensions.command.impl;
using UnityEngine;

namespace Framewerk.Examples.TabsViewStackExample
{
    public class TabsViewStackExampleStartCommand : Command
    {
        [Inject] public IUiManager UiManager { get; set; }

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

            // Instantiate with tabsData injected - mediators will receive it via [Inject]
            UiManager.InstantiateViewAsync<TabContainerView>(null, null, tabsData);
        }
    }
}
