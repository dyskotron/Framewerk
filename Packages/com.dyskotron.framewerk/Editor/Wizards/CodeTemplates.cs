namespace Framewerk.Editor.Wizards
{
    public enum ComponentType
    {
        Screen,
        Popup,
        List,
        ListItem,
        View
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
                case ComponentType.View:
                    return GetScreenViewTemplate(name, ns);
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
                case ComponentType.View:
                    return GetScreenMediatorTemplate(name, ns);
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
    }}
}}
";
        }

        private static string GetListPanelMediatorTemplate(string name, string ns)
        {
            return $@"using Framewerk.UI.List;

namespace {ns}
{{
    public class {name}Mediator : ListMediator<{name}View, {name}Data>
    {{
        public override void OnRegister()
        {{
            base.OnRegister();
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
        public TextMeshProUGUI label;
    }}
}}
";
        }

        public static string GetListItemMediatorTemplate(string name, string ns)
        {
            return $@"using Framewerk.UI.List;

namespace {ns}
{{
    public class {name}ItemMediator : ListItemMediator<{name}ItemView, {name}Data>
    {{
        public override void OnRegister()
        {{
            base.OnRegister();
        }}
    }}
}}
";
        }

        public static string GetListDataTemplate(string name, string ns)
        {
            return $@"using Framewerk.UI.List;

namespace {ns}
{{
    public class {name}Data : IListItemDataProvider
    {{
    }}
}}
";
        }
    }
}
