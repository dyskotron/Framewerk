using System.Collections.Generic;
using UnityEngine;

namespace Framewerk.Examples.TabsViewStackExample
{
    /// <summary>
    /// Provides data for the TabsViewStack example.
    /// Separates data creation from command logic.
    /// </summary>
    public class TabsViewStackDataProvider
    {
        public List<TabData> GetTabsData()
        {
            return new List<TabData>
            {
                new TabData { Title = "Home", ContentIndex = 0 },
                new TabData { Title = "Profile", ContentIndex = 1 },
                new TabData { Title = "Settings", ContentIndex = 2 }
            };
        }

        public List<ContentPanelData> GetContentPanelsData()
        {
            return new List<ContentPanelData>
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
        }

        public TabsData GetTabsViewData()
        {
            return new TabsData
            {
                Tabs = GetTabsData(),
                ContentPanels = GetContentPanelsData()
            };
        }
    }
}
