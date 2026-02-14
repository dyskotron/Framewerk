using Framewerk.UI.ViewStack;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Framewerk.Examples.TabsViewStackExample
{
    /// <summary>
    /// Example-specific ViewStackMediator that creates content panels from injected data.
    /// </summary>
    public class TabsViewStackMediator : ViewStackMediator
    {
        [Inject] public TabsData TabsData { get; set; }

        public override void OnRegister()
        {
            CreateContentPanels();
            base.OnRegister();
        }

        private void CreateContentPanels()
        {
            if (View.Content == null)
            {
                Debug.LogError("[TabsViewStackExample] ViewStack Content is null - check prefab configuration");
                return;
            }

            if (TabsData?.ContentPanels == null)
            {
                Debug.LogError("[TabsViewStackExample] TabsData or ContentPanels is null");
                return;
            }

            foreach (var panel in TabsData.ContentPanels)
            {
                CreateContentPanel(View.Content, panel);
            }

            Debug.Log($"[TabsViewStackExample] Created {TabsData.ContentPanels.Count} content panels");
        }

        private void CreateContentPanel(RectTransform parent, ContentPanelData data)
        {
            // Panel container
            var panelGo = new GameObject($"{data.Title}Panel");
            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.SetParent(parent, false);
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;

            // Background
            var bgImage = panelGo.AddComponent<Image>();
            bgImage.color = data.BackgroundColor;

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
            titleText.text = data.Title;
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
            descText.text = data.Description;
            descText.fontSize = 24;
            descText.alignment = TextAlignmentOptions.Center;
            descText.color = new Color(1f, 1f, 1f, 0.8f);
        }
    }
}
