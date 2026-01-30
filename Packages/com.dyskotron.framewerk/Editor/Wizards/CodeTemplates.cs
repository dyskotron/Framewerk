namespace Framewerk.Editor.Wizards
{
    public enum ComponentType
    {
        Screen,
        Popup,
        List,
        ListItem
    }

    public static class CodeTemplates
    {
        public static string GetViewTemplate(ComponentType type, string name, string ns)
        {
            switch (type)
            {
                case ComponentType.Screen:
                    return GetScreenViewTemplate(name, ns);
                case ComponentType.Popup:
                    return GetPopupViewTemplate(name, ns);
                case ComponentType.List:
                    return GetListPanelViewTemplate(name, ns);
                case ComponentType.ListItem:
                    return GetListItemViewTemplate(name, ns);
                default:
                    return GetScreenViewTemplate(name, ns);
            }
        }

        public static string GetMediatorTemplate(ComponentType type, string name, string ns)
        {
            switch (type)
            {
                case ComponentType.Screen:
                    return GetScreenMediatorTemplate(name, ns);
                case ComponentType.Popup:
                    return GetPopupMediatorTemplate(name, ns);
                case ComponentType.List:
                    return GetListPanelMediatorTemplate(name, ns);
                case ComponentType.ListItem:
                    return GetListItemMediatorTemplate(name, ns);
                default:
                    return GetScreenMediatorTemplate(name, ns);
            }
        }

        private static string GetScreenViewTemplate(string name, string ns)
        {
            return $@"using strange.extensions.mediation.impl;

namespace {ns}
{{
    public class {name}View : View
    {{
    }}
}}
";
        }

        private static string GetScreenMediatorTemplate(string name, string ns)
        {
            return $@"using Framewerk.UI;

namespace {ns}
{{
    public class {name}Mediator : ExtendedMediator<{name}View>
    {{
        public override void OnRegister()
        {{
            base.OnRegister();
        }}
    }}
}}
";
        }

        private static string GetPopupViewTemplate(string name, string ns)
        {
            return $@"using Framewerk.Popups;

namespace {ns}
{{
    public class {name}View : PopupView, IPopupView
    {{
        // Inherited from PopupView:
        // public Transform buttonContainer;
        // public GameObject buttonPrefab;
    }}
}}
";
        }

        private static string GetPopupMediatorTemplate(string name, string ns)
        {
            return $@"using Framewerk.Popups;

namespace {ns}
{{
    public class {name}Mediator : PopupMediator<{name}View>
    {{
        public override void OnRegister()
        {{
            base.OnRegister();
        }}
    }}
}}
";
        }

        private static string GetListPanelViewTemplate(string name, string ns)
        {
            return $@"using Framewerk.UI.List;
using UnityEngine;

namespace {ns}
{{
    public class {name}View : ListView
    {{
        // Inherited from ListView:
        // public RectTransform ContentsParent;
        // public GameObject ItemPrefab;
        // public GameObject EmptyContent;
    }}
}}
";
        }

        private static string GetListPanelMediatorTemplate(string name, string ns)
        {
            return $@"using Framewerk.UI;
using System.Collections.Generic;

namespace {ns}
{{
    public class {name}Mediator : ExtendedMediator<{name}View>
    {{
        public override void OnRegister()
        {{
            base.OnRegister();
        }}

        private void PopulateList(List<{name}Data> items)
        {{
            // TODO: Implement list population logic
        }}
    }}
}}
";
        }

        public static string GetListItemViewTemplate(string name, string ns)
        {
            return $@"using Framewerk.UI.List;
using UnityEngine.UI;
using TMPro;

namespace {ns}
{{
    public class {name}ItemView : ListItemView
    {{
        // Inherited from ListItemView:
        // public Button SelectButton;

        public TextMeshProUGUI label;
    }}
}}
";
        }

        public static string GetListItemMediatorTemplate(string name, string ns)
        {
            return $@"using Framewerk.UI;

namespace {ns}
{{
    public class {name}ItemMediator : ExtendedMediator<{name}ItemView>
    {{
        private {name}Data _data;

        public void SetData({name}Data data)
        {{
            _data = data;
            UpdateView();
        }}

        public override void OnRegister()
        {{
            base.OnRegister();
        }}

        private void UpdateView()
        {{
            // TODO: Update view with data
        }}
    }}
}}
";
        }

        public static string GetListDataTemplate(string name, string ns)
        {
            return $@"namespace {ns}
{{
    public class {name}Data
    {{
        // TODO: Add data fields
    }}
}}
";
        }
    }
}
