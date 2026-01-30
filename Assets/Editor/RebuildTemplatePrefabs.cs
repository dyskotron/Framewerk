using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using Framewerk.UI.List;
using Framewerk.Popups;

namespace FramewerkDemo.Editor
{
    /// <summary>
    /// Editor utility to rebuild template prefabs with proper UI structure
    /// </summary>
    public static class RebuildTemplatePrefabs
    {
        private const string TemplatesPath = "Packages/com.dyskotron.framewerk/Editor/Wizards/Templates/";

        [MenuItem("Framewerk/Rebuild Template Prefabs")]
        public static void RebuildAll()
        {
            RebuildListTemplate();
            RebuildListItemTemplate();
            RebuildPanelTemplate();
            RebuildPopupTemplate();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("All template prefabs rebuilt successfully!");
        }

        private static void RebuildListTemplate()
        {
            string path = TemplatesPath + "ListTemplate.prefab";

            // Create root GameObject
            GameObject root = new GameObject("ListTemplate");
            RectTransform rootRect = root.AddComponent<RectTransform>();

            // Full screen anchoring with margins
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = new Vector2(20, 20);
            rootRect.offsetMax = new Vector2(-20, -80);

            // Add background image
            Image bgImage = root.AddComponent<Image>();
            bgImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/roundedRect.png");
            bgImage.type = Image.Type.Sliced;
            bgImage.color = new Color(0.157f, 0.157f, 0.157f, 1f);

            // Add ScrollRect
            ScrollRect scrollRect = root.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.1f;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.135f;
            scrollRect.scrollSensitivity = 1f;

            // Create Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(root.transform);
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewportRect.pivot = new Vector2(0, 1);

            // Add Mask to Viewport
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Resources/unity_builtin_extra");
            viewportImage.type = Image.Type.Sliced;

            // Create Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform);
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0, 1);
            contentRect.sizeDelta = new Vector2(0, 300);
            contentRect.anchoredPosition = Vector2.zero;

            // Add VerticalLayoutGroup
            VerticalLayoutGroup layoutGroup = content.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 10f;
            layoutGroup.padding = new RectOffset(20, 20, 20, 20);
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childAlignment = TextAnchor.UpperCenter;

            // Add ContentSizeFitter
            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Wire ScrollRect
            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;

            // Add ListView component
            ListView listView = root.AddComponent<ListView>();
            listView.ContentsParent = contentRect;

            // Save prefab
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);

            Debug.Log($"Rebuilt: {path}");
        }

        private static void RebuildListItemTemplate()
        {
            string path = TemplatesPath + "ListItemTemplate.prefab";

            // Create root GameObject
            GameObject root = new GameObject("ListItemTemplate");
            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(0, 50);

            // Add background image
            Image bgImage = root.AddComponent<Image>();
            bgImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/roundedRect.png");
            bgImage.type = Image.Type.Sliced;
            bgImage.color = Color.white;

            // Add Button
            Button button = root.AddComponent<Button>();
            button.targetGraphic = bgImage;

            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.96f, 0.96f, 0.96f, 1f);
            colors.pressedColor = new Color(0.97f, 0.25f, 0.25f, 1f);
            button.colors = colors;

            // Add LayoutElement
            LayoutElement layoutElement = root.AddComponent<LayoutElement>();
            layoutElement.minHeight = 50f;
            layoutElement.preferredHeight = -1f;
            layoutElement.flexibleHeight = -1f;

            // Create Image child
            GameObject imageObj = new GameObject("Image");
            imageObj.transform.SetParent(root.transform);
            RectTransform imageRect = imageObj.AddComponent<RectTransform>();
            imageRect.anchorMin = new Vector2(0, 0.5f);
            imageRect.anchorMax = new Vector2(0, 0.5f);
            imageRect.pivot = new Vector2(0, 0.5f);
            imageRect.anchoredPosition = new Vector2(10, 0);
            imageRect.sizeDelta = new Vector2(40, 40);

            Image itemImage = imageObj.AddComponent<Image>();

            // Create Label child
            GameObject labelObj = new GameObject("LabelText");
            labelObj.transform.SetParent(root.transform);
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(1, 1);
            labelRect.offsetMin = new Vector2(60, 5);
            labelRect.offsetMax = new Vector2(-10, -5);

            TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
            label.text = "List Item";
            label.fontSize = 18;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.color = Color.black;

            // Add ListItemView component
            ListItemView listItemView = root.AddComponent<ListItemView>();
            listItemView.SelectButton = button;

            // Save prefab
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);

            Debug.Log($"Rebuilt: {path}");
        }

        private static void RebuildPanelTemplate()
        {
            string path = TemplatesPath + "PanelTemplate.prefab";

            // Create root GameObject
            GameObject root = new GameObject("PanelTemplate");
            RectTransform rootRect = root.AddComponent<RectTransform>();

            // Full screen
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.sizeDelta = Vector2.zero;

            // Add background image
            Image bgImage = root.AddComponent<Image>();
            bgImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/roundedRect.png");
            bgImage.type = Image.Type.Sliced;
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            // Create Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(root.transform);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -20);
            titleRect.sizeDelta = new Vector2(-40, 50);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Panel Title";
            titleText.fontSize = 24;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;

            // Save prefab
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);

            Debug.Log($"Rebuilt: {path}");
        }

        private static void RebuildPopupTemplate()
        {
            string path = TemplatesPath + "PopupTemplate.prefab";

            // Create root GameObject
            GameObject root = new GameObject("PopupTemplate");
            RectTransform rootRect = root.AddComponent<RectTransform>();

            // Centered, not full screen
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.sizeDelta = new Vector2(500, 400);

            // Add background image with transparency
            Image bgImage = root.AddComponent<Image>();
            bgImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/roundedRect.png");
            bgImage.type = Image.Type.Sliced;
            bgImage.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);

            // Create Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(root.transform);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -20);
            titleRect.sizeDelta = new Vector2(-40, 50);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Popup Title";
            titleText.fontSize = 22;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;

            // Create Content area
            GameObject contentObj = new GameObject("ContentArea");
            contentObj.transform.SetParent(root.transform);
            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.offsetMin = new Vector2(20, 80);
            contentRect.offsetMax = new Vector2(-20, -80);

            TextMeshProUGUI contentText = contentObj.AddComponent<TextMeshProUGUI>();
            contentText.text = "Popup content goes here";
            contentText.fontSize = 16;
            contentText.alignment = TextAlignmentOptions.Top;
            contentText.color = new Color(0.9f, 0.9f, 0.9f, 1f);

            // Create Button Container
            GameObject buttonContainer = new GameObject("ButtonContainer");
            buttonContainer.transform.SetParent(root.transform);
            RectTransform buttonRect = buttonContainer.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0, 0);
            buttonRect.anchorMax = new Vector2(1, 0);
            buttonRect.pivot = new Vector2(0.5f, 0);
            buttonRect.anchoredPosition = new Vector2(0, 20);
            buttonRect.sizeDelta = new Vector2(-40, 50);

            // Add VerticalLayoutGroup to button container
            VerticalLayoutGroup layoutGroup = buttonContainer.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 10f;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;

            // Add PopupView component
            PopupView popupView = root.AddComponent<PopupView>();
            popupView.buttonContainer = buttonRect;

            // Save prefab
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);

            Debug.Log($"Rebuilt: {path}");
        }
    }
}
