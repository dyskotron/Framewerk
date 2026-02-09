namespace Framewerk.Editor.Wizards
{
    public enum ComponentType
    {
        Popup,
        List,
        ListItem,
        View,
        VerticalTabs,
        HorizontalTabs,
        ViewStack
    }

    public static class CodeTemplates
    {
        public static string GetViewTemplate(ComponentType type, string name, string ns)
        {
            switch (type)
            {
                case ComponentType.Popup:
                    return GetPopupViewTemplate(name, ns);
                case ComponentType.List:
                    return GetListPanelViewTemplate(name, ns);
                case ComponentType.ListItem:
                    return GetListItemViewTemplate(name, ns);
                case ComponentType.VerticalTabs:
                case ComponentType.HorizontalTabs:
                    return GetTabContainerViewTemplate(name, ns);
                case ComponentType.ViewStack:
                    return GetViewStackViewTemplate(name, ns);
                case ComponentType.View:
                default:
                    return GetScreenViewTemplate(name, ns);
            }
        }

        public static string GetMediatorTemplate(ComponentType type, string name, string ns)
        {
            switch (type)
            {
                case ComponentType.Popup:
                    return GetPopupMediatorTemplate(name, ns);
                case ComponentType.List:
                    return GetListPanelMediatorTemplate(name, ns);
                case ComponentType.ListItem:
                    return GetListItemMediatorTemplate(name, ns);
                case ComponentType.VerticalTabs:
                case ComponentType.HorizontalTabs:
                    return GetTabContainerMediatorTemplate(name, ns);
                case ComponentType.ViewStack:
                    return GetViewStackMediatorTemplate(name, ns);
                case ComponentType.View:
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

        // ═══════════════════════════════════════════════════════════════════
        // Tab Container Templates (for VerticalTabs and HorizontalTabs)
        // ═══════════════════════════════════════════════════════════════════

        private static string GetTabContainerViewTemplate(string name, string ns)
        {
            return $@"using Framewerk.UI.List;
using Framewerk.UI.ViewStack;
using UnityEngine;

namespace {ns}
{{
    /// <summary>
    /// View for {name} tab container.
    /// Contains a tab list and a ViewStack for tab content.
    /// </summary>
    public class {name}View : ListView
    {{
        [Header(""Tab Container"")]
        [Tooltip(""ViewStack that shows content for selected tab"")]
        public ViewStackView ContentStack;
    }}
}}
";
        }

        private static string GetTabContainerMediatorTemplate(string name, string ns)
        {
            return $@"using Framewerk.UI.List;
using Framewerk.UI.ViewStack;

namespace {ns}
{{
    /// <summary>
    /// Mediator for {name} tab container.
    /// Uses TabViewConnector to sync tab selection with ViewStack.
    /// </summary>
    public class {name}Mediator : ListMediator<{name}View, {name}Data>
    {{
        private TabViewConnector<{name}Data> _tabConnector;
        private ViewStackMediator _viewStackMediator;

        public override void OnRegister()
        {{
            base.OnRegister();
            
            // Get the ViewStack mediator (injected via context or found in hierarchy)
            _viewStackMediator = View.ContentStack?.GetComponent<ViewStackMediator>();
            
            if (_viewStackMediator != null)
            {{
                _tabConnector = new TabViewConnector<{name}Data>(this, _viewStackMediator);
            }}
        }}

        public override void OnRemove()
        {{
            _tabConnector?.Destroy();
            _tabConnector = null;
            base.OnRemove();
        }}
    }}
}}
";
        }

        public static string GetTabDataTemplate(string name, string ns)
        {
            return $@"using Framewerk.UI.List;

namespace {ns}
{{
    /// <summary>
    /// Data provider for {name} tabs.
    /// </summary>
    public class {name}Data : IListItemDataProvider
    {{
        public string TabTitle {{ get; set; }}
        public int ContentIndex {{ get; set; }}
    }}
}}
";
        }

        public static string GetTabItemViewTemplate(string name, string ns)
        {
            return $@"using Framewerk.UI.List;
using TMPro;

namespace {ns}
{{
    /// <summary>
    /// View for individual tab item in {name}.
    /// </summary>
    public class {name}ItemView : ListItemView
    {{
        public TextMeshProUGUI Label;
    }}
}}
";
        }

        public static string GetTabItemMediatorTemplate(string name, string ns)
        {
            return $@"using Framewerk.UI.List;

namespace {ns}
{{
    /// <summary>
    /// Mediator for individual tab item in {name}.
    /// </summary>
    public class {name}ItemMediator : ListItemMediator<{name}ItemView, {name}Data>
    {{
        public override void OnRegister()
        {{
            base.OnRegister();
            
            if (Data != null)
            {{
                View.Label.text = Data.TabTitle;
            }}
        }}
    }}
}}
";
        }

        // ═══════════════════════════════════════════════════════════════════
        // ViewStack Templates
        // ═══════════════════════════════════════════════════════════════════

        private static string GetViewStackViewTemplate(string name, string ns)
        {
            return $@"using Framewerk.UI.ViewStack;

namespace {ns}
{{
    /// <summary>
    /// View for {name} ViewStack.
    /// Shows one child at a time.
    /// </summary>
    public class {name}View : ViewStackView
    {{
    }}
}}
";
        }

        private static string GetViewStackMediatorTemplate(string name, string ns)
        {
            return $@"using Framewerk.UI.ViewStack;

namespace {ns}
{{
    /// <summary>
    /// Mediator for {name} ViewStack.
    /// Manages visibility of stacked children.
    /// </summary>
    public class {name}Mediator : ViewStackMediator
    {{
        // ViewStackMediator base provides all functionality:
        // - ShowByIndex(int index)
        // - ShowByTransform(Transform t)
        // - CurrentIndex, Count properties
        // - CurrentChanged signal
    }}
}}
";
        }
    }
}
