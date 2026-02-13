using System.Collections.Generic;
using Framewerk.Managers;
using Framewerk.UI.ViewStack;
using strange.extensions.command.impl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Framewerk.Examples.TabsViewStackExample
{
    public class TabsViewStackExampleStartCommand : Command
    {
        [Inject] public IUiManager UiManager { get; set; }
        [Inject] public TabDataSignal TabDataSignal { get; set; }

        public override async void Execute()
        {
            // Create sample tab data
            var tabs = new List<TabData>
            {
                new TabData { Title = "Home", ContentIndex = 0 },
                new TabData { Title = "Profile", ContentIndex = 1 },
                new TabData { Title = "Settings", ContentIndex = 2 }
            };

            // Instantiate the tab container (prefab has layout pre-configured)
            var tabContainer = await UiManager.InstantiateViewAsync<TabContainerView>();
            
            // Create content panels for the ViewStack
            CreateContentPanels(tabContainer.ContentStack, tabs);
            
            // Dispatch tab data to populate the UI
            TabDataSignal.Dispatch(tabs);
        }
        
        private void CreateContentPanels(ViewStackView viewStack, List<TabData> tabs)
        {
            if (viewStack == null || viewStack.Content == null)
            {
                Debug.LogError("[TabsViewStackExample] ViewStack or Content is null - check prefab configuration");
                return;
            }
            
            var colors = new[]
            {
                new Color(0.2f, 0.4f, 0.6f, 1f),  // Home - Blue
                new Color(0.4f, 0.6f, 0.3f, 1f),  // Profile - Green  
                new Color(0.6f, 0.4f, 0.2f, 1f)   // Settings - Orange
            };
            
            var descriptions = new[]
            {
                "Welcome to the Home screen!\n\nThis is where you'd see your main content, dashboard, or feed.",
                "This is the Profile screen.\n\nUser information, avatar, and account details would go here.",
                "Settings screen.\n\nConfigure your preferences, notifications, and app behavior here."
            };
            
            for (int i = 0; i < tabs.Count; i++)
            {
                CreateContentPanel(viewStack.Content, tabs[i].Title, colors[i], descriptions[i]);
            }
            
            Debug.Log($"[TabsViewStackExample] Created {tabs.Count} content panels");
        }
        
        private void CreateContentPanel(RectTransform parent, string title, Color bgColor, string description)
        {
            // Panel container
            var panelGo = new GameObject($"{title}Panel");
            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.SetParent(parent, false);
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            
            // Background
            var bgImage = panelGo.AddComponent<Image>();
            bgImage.color = bgColor;
            
            // Vertical layout for content
            var layout = panelGo.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(40, 40, 40, 40);
            layout.spacing = 20;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            
            // Title text
            var titleGo = new GameObject("Title");
            var titleRect = titleGo.AddComponent<RectTransform>();
            titleRect.SetParent(panelRect, false);
            titleRect.sizeDelta = new Vector2(0, 60);
            
            var titleText = titleGo.AddComponent<TextMeshProUGUI>();
            titleText.text = title;
            titleText.fontSize = 48;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;
            
            // Description text
            var descGo = new GameObject("Description");
            var descRect = descGo.AddComponent<RectTransform>();
            descRect.SetParent(panelRect, false);
            descRect.sizeDelta = new Vector2(0, 120);
            
            var descText = descGo.AddComponent<TextMeshProUGUI>();
            descText.text = description;
            descText.fontSize = 24;
            descText.alignment = TextAlignmentOptions.Center;
            descText.color = new Color(1f, 1f, 1f, 0.8f);
        }
    }
}
