using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using Framewerk.UI.List;
using Framewerk.Popups;

namespace FramewerkDemo.Editor
{
    /// <summary>
    /// Verification script to check template prefabs are properly built
    /// </summary>
    public static class VerifyTemplatePrefabs
    {
        private const string TemplatesPath = "Packages/com.dyskotron.framewerk/Editor/Wizards/Templates/";

        [MenuItem("Framewerk/Verify Template Prefabs")]
        public static void VerifyAll()
        {
            bool allValid = true;

            allValid &= VerifyListTemplate();
            allValid &= VerifyListItemTemplate();
            allValid &= VerifyPanelTemplate();
            allValid &= VerifyPopupTemplate();

            if (allValid)
            {
                Debug.Log("<color=green><b>✓ All template prefabs verified successfully!</b></color>");
            }
            else
            {
                Debug.LogError("<b>✗ Some template prefabs have issues. See above for details.</b>");
            }
        }

        private static bool VerifyListTemplate()
        {
            string path = TemplatesPath + "ListTemplate.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
            {
                Debug.LogError($"✗ ListTemplate: Prefab not found at {path}");
                return false;
            }

            // Check for required components
            var scrollRect = prefab.GetComponent<ScrollRect>();
            var listView = prefab.GetComponent<ListView>();
            var image = prefab.GetComponent<Image>();

            if (scrollRect == null)
            {
                Debug.LogError("✗ ListTemplate: Missing ScrollRect component");
                return false;
            }

            if (listView == null)
            {
                Debug.LogError("✗ ListTemplate: Missing ListView component");
                return false;
            }

            if (image == null)
            {
                Debug.LogError("✗ ListTemplate: Missing Image component (background)");
                return false;
            }

            // Check hierarchy
            var viewport = prefab.transform.Find("Viewport");
            if (viewport == null)
            {
                Debug.LogError("✗ ListTemplate: Missing Viewport child");
                return false;
            }

            var content = viewport.Find("Content");
            if (content == null)
            {
                Debug.LogError("✗ ListTemplate: Missing Content child under Viewport");
                return false;
            }

            var layoutGroup = content.GetComponent<VerticalLayoutGroup>();
            var sizeFitter = content.GetComponent<ContentSizeFitter>();

            if (layoutGroup == null)
            {
                Debug.LogError("✗ ListTemplate: Missing VerticalLayoutGroup on Content");
                return false;
            }

            if (sizeFitter == null)
            {
                Debug.LogError("✗ ListTemplate: Missing ContentSizeFitter on Content");
                return false;
            }

            // Verify references
            if (scrollRect.content != content.GetComponent<RectTransform>())
            {
                Debug.LogError("✗ ListTemplate: ScrollRect.content not properly wired to Content");
                return false;
            }

            if (listView.ContentsParent != content.GetComponent<RectTransform>())
            {
                Debug.LogError("✗ ListTemplate: ListView.ContentsParent not properly wired to Content");
                return false;
            }

            Debug.Log("<color=green>✓ ListTemplate: All checks passed</color>");
            return true;
        }

        private static bool VerifyListItemTemplate()
        {
            string path = TemplatesPath + "ListItemTemplate.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
            {
                Debug.LogError($"✗ ListItemTemplate: Prefab not found at {path}");
                return false;
            }

            var button = prefab.GetComponent<Button>();
            var listItemView = prefab.GetComponent<ListItemView>();
            var image = prefab.GetComponent<Image>();
            var layoutElement = prefab.GetComponent<LayoutElement>();

            if (button == null)
            {
                Debug.LogError("✗ ListItemTemplate: Missing Button component");
                return false;
            }

            if (listItemView == null)
            {
                Debug.LogError("✗ ListItemTemplate: Missing ListItemView component");
                return false;
            }

            if (image == null)
            {
                Debug.LogError("✗ ListItemTemplate: Missing Image component");
                return false;
            }

            if (layoutElement == null)
            {
                Debug.LogError("✗ ListItemTemplate: Missing LayoutElement component");
                return false;
            }

            // Check children
            var imageChild = prefab.transform.Find("Image");
            var labelChild = prefab.transform.Find("LabelText");

            if (imageChild == null)
            {
                Debug.LogError("✗ ListItemTemplate: Missing Image child");
                return false;
            }

            if (labelChild == null)
            {
                Debug.LogError("✗ ListItemTemplate: Missing LabelText child");
                return false;
            }

            var label = labelChild.GetComponent<TextMeshProUGUI>();
            if (label == null)
            {
                Debug.LogError("✗ ListItemTemplate: LabelText missing TextMeshProUGUI component");
                return false;
            }

            // Verify references
            if (listItemView.SelectButton != button)
            {
                Debug.LogError("✗ ListItemTemplate: ListItemView.SelectButton not properly wired");
                return false;
            }

            Debug.Log("<color=green>✓ ListItemTemplate: All checks passed</color>");
            return true;
        }

        private static bool VerifyPanelTemplate()
        {
            string path = TemplatesPath + "PanelTemplate.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
            {
                Debug.LogError($"✗ PanelTemplate: Prefab not found at {path}");
                return false;
            }

            var image = prefab.GetComponent<Image>();
            if (image == null)
            {
                Debug.LogError("✗ PanelTemplate: Missing Image component");
                return false;
            }

            var title = prefab.transform.Find("Title");
            if (title == null)
            {
                Debug.LogError("✗ PanelTemplate: Missing Title child");
                return false;
            }

            var titleText = title.GetComponent<TextMeshProUGUI>();
            if (titleText == null)
            {
                Debug.LogError("✗ PanelTemplate: Title missing TextMeshProUGUI component");
                return false;
            }

            Debug.Log("<color=green>✓ PanelTemplate: All checks passed</color>");
            return true;
        }

        private static bool VerifyPopupTemplate()
        {
            string path = TemplatesPath + "PopupTemplate.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
            {
                Debug.LogError($"✗ PopupTemplate: Prefab not found at {path}");
                return false;
            }

            var popupView = prefab.GetComponent<PopupView>();
            var image = prefab.GetComponent<Image>();

            if (popupView == null)
            {
                Debug.LogError("✗ PopupTemplate: Missing PopupView component");
                return false;
            }

            if (image == null)
            {
                Debug.LogError("✗ PopupTemplate: Missing Image component");
                return false;
            }

            var title = prefab.transform.Find("Title");
            var contentArea = prefab.transform.Find("ContentArea");
            var buttonContainer = prefab.transform.Find("ButtonContainer");

            if (title == null)
            {
                Debug.LogError("✗ PopupTemplate: Missing Title child");
                return false;
            }

            if (contentArea == null)
            {
                Debug.LogError("✗ PopupTemplate: Missing ContentArea child");
                return false;
            }

            if (buttonContainer == null)
            {
                Debug.LogError("✗ PopupTemplate: Missing ButtonContainer child");
                return false;
            }

            var titleText = title.GetComponent<TextMeshProUGUI>();
            var contentText = contentArea.GetComponent<TextMeshProUGUI>();
            var layoutGroup = buttonContainer.GetComponent<VerticalLayoutGroup>();

            if (titleText == null)
            {
                Debug.LogError("✗ PopupTemplate: Title missing TextMeshProUGUI component");
                return false;
            }

            if (contentText == null)
            {
                Debug.LogError("✗ PopupTemplate: ContentArea missing TextMeshProUGUI component");
                return false;
            }

            if (layoutGroup == null)
            {
                Debug.LogError("✗ PopupTemplate: ButtonContainer missing VerticalLayoutGroup component");
                return false;
            }

            // Verify references
            if (popupView.buttonContainer != buttonContainer.GetComponent<RectTransform>())
            {
                Debug.LogError("✗ PopupTemplate: PopupView.buttonContainer not properly wired");
                return false;
            }

            Debug.Log("<color=green>✓ PopupTemplate: All checks passed</color>");
            return true;
        }
    }
}
