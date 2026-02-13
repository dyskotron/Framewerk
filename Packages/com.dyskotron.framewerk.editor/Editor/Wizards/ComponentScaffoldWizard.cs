using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Plugins.Framewerk;
using UnityEditor;
using UnityEngine;

namespace Framewerk.Editor.Wizards
{
    /// <summary>
    /// Popup picker that shows only valid button prefabs (has Button + TextMeshProUGUI, not a list item).
    /// </summary>
    public class ButtonPrefabPicker : EditorWindow
    {
        private System.Action<GameObject> onSelected;
        private List<GameObject> validPrefabs = new List<GameObject>();
        private Vector2 scrollPosition;
        private string searchFilter = "";
        private GUIStyle itemStyle;
        private GUIStyle selectedStyle;
        private int selectedIndex = -1;
        
        public static void Show(EditorWindow parent, System.Action<GameObject> callback)
        {
            var picker = CreateInstance<ButtonPrefabPicker>();
            picker.onSelected = callback;
            picker.titleContent = new GUIContent("Select Button Prefab");
            picker.FindValidButtonPrefabs();
            
            // Position near parent window
            var parentPos = parent.position;
            picker.position = new Rect(parentPos.x + 50, parentPos.y + 100, 350, 400);
            picker.ShowUtility();
        }
        
        private void FindValidButtonPrefabs()
        {
            validPrefabs.Clear();
            
            // Find all prefabs in the project
            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                
                // Skip Editor folders (content doesn't make it to builds)
                if (path.Contains("/Editor/")) continue;
                
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab == null) continue;
                
                // Skip template prefabs (they're blueprints, not usable buttons)
                if (prefab.name.Contains("Template")) continue;
                
                // Check if valid button prefab
                if (IsValidButtonPrefab(prefab))
                {
                    validPrefabs.Add(prefab);
                }
            }
            
            // Sort by name
            validPrefabs.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));
        }
        
        private bool IsValidButtonPrefab(GameObject prefab)
        {
            // Must have Button component
            var button = prefab.GetComponent<UnityEngine.UI.Button>();
            if (button == null) return false;
            
            // Must NOT be a list item
            if (IsListItemPrefab(prefab)) return false;
            
            // Must have TextMeshProUGUI component (direct or in children)
            var components = prefab.GetComponentsInChildren<Component>();
            bool hasTMPText = false;
            foreach (var comp in components)
            {
                if (comp != null && comp.GetType().Name == "TextMeshProUGUI")
                {
                    hasTMPText = true;
                    break;
                }
            }
            if (!hasTMPText) return false;
            
            return true;
        }
        
        private bool IsListItemPrefab(GameObject prefab)
        {
            string prefabName = prefab.name;
            
            // Check name patterns
            if (prefabName.Contains("ListItem") || prefabName.Contains("list_item") || prefabName.Contains("listitem"))
                return true;

            // Check for list-related component types
            var allComponents = prefab.GetComponentsInChildren<Component>(true);
            foreach (var comp in allComponents)
            {
                if (comp == null) continue;
                
                Type compType = comp.GetType();
                string typeName = compType.Name;
                
                if (typeName.Contains("ListItem") || typeName.Contains("ListView"))
                    return true;

                // Check inheritance hierarchy
                Type baseType = compType.BaseType;
                while (baseType != null)
                {
                    string baseName = baseType.Name;
                    if (baseName.Contains("ListItemView") || baseName.Contains("ListItemMediator") ||
                        baseName.StartsWith("ListView") || baseName.StartsWith("ListMediator"))
                        return true;
                    baseType = baseType.BaseType;
                }
            }

            return false;
        }
        
        private void OnGUI()
        {
            // Initialize styles
            if (itemStyle == null)
            {
                itemStyle = new GUIStyle(EditorStyles.label)
                {
                    padding = new RectOffset(8, 8, 4, 4),
                    margin = new RectOffset(0, 0, 0, 0)
                };
            }
            
            if (selectedStyle == null)
            {
                selectedStyle = new GUIStyle(itemStyle);
                selectedStyle.normal.background = EditorGUIUtility.whiteTexture;
            }
            
            // Search field
            EditorGUILayout.Space(4);
            EditorGUI.BeginChangeCheck();
            searchFilter = EditorGUILayout.TextField("Search", searchFilter);
            if (EditorGUI.EndChangeCheck())
            {
                selectedIndex = -1;
            }
            
            EditorGUILayout.Space(4);
            
            // Info
            int matchCount = 0;
            foreach (var p in validPrefabs)
            {
                if (MatchesSearch(p.name)) matchCount++;
            }
            EditorGUILayout.LabelField($"Found {matchCount} valid button prefab(s)", EditorStyles.centeredGreyMiniLabel);
            
            // Separator
            var rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
            
            // Prefab list
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            int displayIndex = 0;
            for (int i = 0; i < validPrefabs.Count; i++)
            {
                var prefab = validPrefabs[i];
                if (!MatchesSearch(prefab.name)) continue;
                
                bool isSelected = (displayIndex == selectedIndex);
                
                // Draw selection background
                var itemRect = EditorGUILayout.BeginHorizontal();
                if (isSelected)
                {
                    EditorGUI.DrawRect(itemRect, new Color(0.24f, 0.48f, 0.9f, 0.5f));
                }
                
                // Prefab icon + name
                var icon = AssetPreview.GetMiniThumbnail(prefab);
                GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(20));
                
                if (GUILayout.Button(prefab.name, itemStyle, GUILayout.ExpandWidth(true)))
                {
                    if (selectedIndex == displayIndex)
                    {
                        // Double-click behavior: select and close
                        SelectAndClose(prefab);
                    }
                    else
                    {
                        selectedIndex = displayIndex;
                    }
                }
                
                EditorGUILayout.EndHorizontal();
                displayIndex++;
            }
            
            EditorGUILayout.EndScrollView();
            
            // Bottom buttons
            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            GUI.enabled = selectedIndex >= 0;
            if (GUILayout.Button("Select", GUILayout.Width(80)))
            {
                // Find the selected prefab
                int idx = 0;
                foreach (var prefab in validPrefabs)
                {
                    if (!MatchesSearch(prefab.name)) continue;
                    if (idx == selectedIndex)
                    {
                        SelectAndClose(prefab);
                        break;
                    }
                    idx++;
                }
            }
            GUI.enabled = true;
            
            if (GUILayout.Button("Cancel", GUILayout.Width(80)))
            {
                Close();
            }
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);
        }
        
        private bool MatchesSearch(string name)
        {
            if (string.IsNullOrEmpty(searchFilter)) return true;
            return name.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        
        private void SelectAndClose(GameObject prefab)
        {
            onSelected?.Invoke(prefab);
            Close();
        }
    }
    
    public class ComponentScaffoldWizard : EditorWindow
    {
        private const string PREFS_NAMESPACE = "FramewerkWizard_Namespace";
        private const string PREFS_SCRIPT_FOLDER = "FramewerkWizard_ScriptFolder";
        private const string PREFS_PREFAB_FOLDER = "FramewerkWizard_PrefabFolder";
        private const string PREFS_CONTEXT_INDEX = "FramewerkWizard_ContextIndex";
        private const string PREFS_SETUP_IN_CONTEXT = "FramewerkWizard_SetupInContext";
        private const string PREFS_BOOTSTRAP_INDEX = "FramewerkWizard_BootstrapIndex";
        private const string PREFS_CUSTOM_PREFIX = "FramewerkWizard_CustomPrefix";

        private string componentName = "";
        private ComponentType componentType = ComponentType.Popup;
        private string namespaceName = "";
        private string scriptFolder = "Assets/Scripts";
        private string prefabFolder = "Assets/Prefabs";
        private int selectedContextIndex = 0;
        private bool setupInContext = true;
        private int selectedBootstrapIndex = 0;
        private string customPrefix = "";

        private List<Type> contextTypes = new List<Type>();
        private string[] contextNames = new string[0];
        
        private List<ViewConfig> bootstrapConfigs = new List<ViewConfig>();
        private string[] bootstrapNames = new string[0];

        private Vector2 scrollPosition;
        
        // Button prefab handling for popups
        private int buttonPrefabOption = 0; // 0 = use existing, 1 = create new
        private string[] buttonPrefabOptions = new[] { "Use Existing", "Create New" };
        private GameObject selectedButtonPrefab = null;
        private string buttonPrefabValidationError = null;
        
        // Cached styles for section boxes
        private GUIStyle sectionBoxStyle;
        private GUIStyle sectionHeaderStyle;

        [MenuItem("Framewerk/Create UI Component")]
        public static void ShowWindow()
        {
            var window = GetWindow<ComponentScaffoldWizard>("Framewerk Component Wizard");
            window.minSize = new Vector2(450, 580);
            window.Show();
        }

        private void OnEnable()
        {
            LoadPreferences();
            FindContextTypes();
            FindBootstrapConfigs();
            InitStyles();
        }
        
        private void InitStyles()
        {
            // Initialize styles lazily in OnGUI if not yet created
            sectionBoxStyle = null;
            sectionHeaderStyle = null;
        }
        
        private void EnsureStyles()
        {
            if (sectionBoxStyle == null)
            {
                sectionBoxStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(16, 16, 10, 10),
                    margin = new RectOffset(10, 10, 4, 8)
                };
            }
            
            if (sectionHeaderStyle == null)
            {
                sectionHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 12,
                    margin = new RectOffset(0, 0, 0, 4)
                };
            }
        }
        
        private void BeginSection(string title)
        {
            EnsureStyles();
            EditorGUILayout.BeginVertical(sectionBoxStyle);
            EditorGUILayout.LabelField(title, sectionHeaderStyle);
            GUILayout.Space(2);
        }
        
        private void EndSection()
        {
            EditorGUILayout.EndVertical();
        }

        private void LoadPreferences()
        {
            namespaceName = EditorPrefs.GetString(PREFS_NAMESPACE, "");
            scriptFolder = EditorPrefs.GetString(PREFS_SCRIPT_FOLDER, "Assets/Scripts");
            prefabFolder = EditorPrefs.GetString(PREFS_PREFAB_FOLDER, "Assets/Prefabs");
            selectedContextIndex = EditorPrefs.GetInt(PREFS_CONTEXT_INDEX, 0);
            setupInContext = EditorPrefs.GetBool(PREFS_SETUP_IN_CONTEXT, true);
            selectedBootstrapIndex = EditorPrefs.GetInt(PREFS_BOOTSTRAP_INDEX, 0);
            customPrefix = EditorPrefs.GetString(PREFS_CUSTOM_PREFIX, "");
        }

        private void SavePreferences()
        {
            EditorPrefs.SetString(PREFS_NAMESPACE, namespaceName);
            EditorPrefs.SetString(PREFS_SCRIPT_FOLDER, scriptFolder);
            EditorPrefs.SetString(PREFS_PREFAB_FOLDER, prefabFolder);
            EditorPrefs.SetInt(PREFS_CONTEXT_INDEX, selectedContextIndex);
            EditorPrefs.SetBool(PREFS_SETUP_IN_CONTEXT, setupInContext);
            EditorPrefs.SetInt(PREFS_BOOTSTRAP_INDEX, selectedBootstrapIndex);
            EditorPrefs.SetString(PREFS_CUSTOM_PREFIX, customPrefix);
        }

        private void FindContextTypes()
        {
            contextTypes.Clear();

            // Find all types that extend MVCSContext in all assemblies
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.IsClass && !t.IsAbstract && IsContextType(t))
                        .ToList();
                    contextTypes.AddRange(types);
                }
                catch (ReflectionTypeLoadException)
                {
                    // Skip assemblies that can't be loaded
                }
            }

            contextNames = contextTypes.Select(t => t.Name).ToArray();

            if (contextTypes.Count == 0)
            {
                contextNames = new string[] { "No Context Found" };
            }

            // Clamp selected index
            if (selectedContextIndex >= contextTypes.Count)
            {
                selectedContextIndex = 0;
            }
        }

        private void FindBootstrapConfigs()
        {
            bootstrapConfigs.Clear();

            // Find all ViewConfig components in the scene and prefabs
            var viewConfigs = Resources.FindObjectsOfTypeAll<ViewConfig>();
            
            foreach (var vc in viewConfigs)
            {
                // Only include scene objects and prefab roots (not prefab instances)
                if (vc.gameObject.scene.IsValid() || PrefabUtility.IsPartOfPrefabAsset(vc.gameObject))
                {
                    bootstrapConfigs.Add(vc);
                }
            }

            // Also search in loaded scenes
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;

                var rootObjects = scene.GetRootGameObjects();
                foreach (var root in rootObjects)
                {
                    var configs = root.GetComponentsInChildren<ViewConfig>(true);
                    foreach (var config in configs)
                    {
                        if (!bootstrapConfigs.Contains(config))
                        {
                            bootstrapConfigs.Add(config);
                        }
                    }
                }
            }

            // Build display names
            bootstrapNames = bootstrapConfigs
                .Select(vc => {
                    string sceneName = vc.gameObject.scene.IsValid() ? vc.gameObject.scene.name : "Prefab";
                    string prefix = vc.ContextPrefixSO != null ? $"[{vc.ContextPrefixSO.Prefix}] " : "";
                    return $"{prefix}{vc.gameObject.name} ({sceneName})";
                })
                .ToArray();

            if (bootstrapConfigs.Count == 0)
            {
                bootstrapNames = new string[] { "No ViewConfig Found (Open a scene)" };
            }

            // Clamp selected index
            if (selectedBootstrapIndex >= bootstrapConfigs.Count)
            {
                selectedBootstrapIndex = 0;
            }
        }

        /// <summary>
        /// Validates if a prefab is suitable as a popup button.
        /// Returns null if valid, error message if invalid.
        /// </summary>
        private string ValidateButtonPrefab(GameObject prefab)
        {
            if (prefab == null)
                return null;

            // Must be a prefab asset, not a scene object
            if (!PrefabUtility.IsPartOfPrefabAsset(prefab))
                return "Must be a prefab asset";

            // Check if it has a Button component
            var button = prefab.GetComponent<UnityEngine.UI.Button>();
            if (button == null)
                return "Missing Button component";

            // Check if it's a list item (should be excluded)
            if (IsListItemPrefab(prefab, prefab.name))
                return "List item prefabs cannot be used as popup buttons";

            // Check if it has a TextMeshProUGUI component (direct or in children)
            var components = prefab.GetComponentsInChildren<Component>();
            bool hasTMPText = false;
            foreach (var comp in components)
            {
                if (comp != null && comp.GetType().Name == "TextMeshProUGUI")
                {
                    hasTMPText = true;
                    break;
                }
            }
            if (!hasTMPText)
                return "Missing TextMeshProUGUI component for button label";

            return null; // Valid
        }

        /// <summary>
        /// Draws an ObjectField-like control for button prefabs that uses our custom picker.
        /// Supports drag/drop with validation, and clicking the picker dot opens ButtonPrefabPicker.
        /// </summary>
        private GameObject DrawButtonPrefabObjectField(string label, GameObject currentValue)
        {
            // Get the rect for the entire field
            Rect totalRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
            
            // Split into label and field rects
            Rect labelRect = new Rect(totalRect.x, totalRect.y, EditorGUIUtility.labelWidth, totalRect.height);
            Rect fieldRect = new Rect(totalRect.x + EditorGUIUtility.labelWidth, totalRect.y, 
                                       totalRect.width - EditorGUIUtility.labelWidth, totalRect.height);
            
            // Draw label
            EditorGUI.LabelField(labelRect, label);
            
            // Picker button rect (small circle on the right, ~18px wide)
            float pickerWidth = 18f;
            Rect pickerRect = new Rect(fieldRect.xMax - pickerWidth, fieldRect.y, pickerWidth, fieldRect.height);
            Rect objectRect = new Rect(fieldRect.x, fieldRect.y, fieldRect.width - pickerWidth, fieldRect.height);
            
            // Draw the object field background
            bool hasValue = currentValue != null;
            
            // Handle events
            Event evt = Event.current;
            int controlId = GUIUtility.GetControlID(FocusType.Passive, objectRect);
            
            GameObject result = currentValue;
            
            switch (evt.type)
            {
                case EventType.Repaint:
                    // Draw field background (looks like ObjectField)
                    EditorStyles.objectField.Draw(objectRect, GUIContent.none, controlId, false, objectRect.Contains(evt.mousePosition));
                    
                    // Draw content inside the field
                    Rect contentRect = new Rect(objectRect.x + 2, objectRect.y, objectRect.width - 4, objectRect.height);
                    
                    if (hasValue)
                    {
                        // Draw object icon
                        Texture icon = AssetPreview.GetMiniThumbnail(currentValue);
                        Rect iconRect = new Rect(contentRect.x, contentRect.y + 1, 14, 14);
                        if (icon != null)
                        {
                            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
                        }
                        
                        // Draw object name
                        Rect textRect = new Rect(contentRect.x + 16, contentRect.y, contentRect.width - 18, contentRect.height);
                        GUI.Label(textRect, currentValue.name, EditorStyles.label);
                    }
                    else
                    {
                        // Show placeholder
                        GUIStyle placeholderStyle = new GUIStyle(EditorStyles.label);
                        placeholderStyle.fontStyle = FontStyle.Italic;
                        placeholderStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUI.Label(contentRect, "None (Game Object)", placeholderStyle);
                    }
                    
                    // Draw picker button (circle with dot) - this is Unity's built-in style
                    GUIStyle pickerStyle = "ObjectFieldButton";
                    pickerStyle.Draw(pickerRect, GUIContent.none, controlId, false, pickerRect.Contains(evt.mousePosition));
                    break;
                    
                case EventType.MouseDown:
                    if (evt.button == 0)
                    {
                        // Click on picker button - open our custom picker
                        if (pickerRect.Contains(evt.mousePosition))
                        {
                            ButtonPrefabPicker.Show(this, (prefab) =>
                            {
                                selectedButtonPrefab = prefab;
                                buttonPrefabValidationError = ValidateButtonPrefab(selectedButtonPrefab);
                                Repaint();
                            });
                            evt.Use();
                        }
                        // Click on object field - ping the object if it exists
                        else if (objectRect.Contains(evt.mousePosition) && hasValue)
                        {
                            EditorGUIUtility.PingObject(currentValue);
                            evt.Use();
                        }
                    }
                    break;
                    
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (objectRect.Contains(evt.mousePosition) || pickerRect.Contains(evt.mousePosition))
                    {
                        // Check if dragged object is a valid button prefab
                        GameObject draggedObject = null;
                        if (DragAndDrop.objectReferences.Length > 0)
                        {
                            draggedObject = DragAndDrop.objectReferences[0] as GameObject;
                        }
                        
                        bool isValid = draggedObject != null && 
                                       PrefabUtility.IsPartOfPrefabAsset(draggedObject) &&
                                       ValidateButtonPrefab(draggedObject) == null;
                        
                        if (evt.type == EventType.DragUpdated)
                        {
                            DragAndDrop.visualMode = isValid ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
                            evt.Use();
                        }
                        else if (evt.type == EventType.DragPerform && isValid)
                        {
                            DragAndDrop.AcceptDrag();
                            result = draggedObject;
                            GUI.changed = true;
                            evt.Use();
                        }
                    }
                    break;
            }
            
            return result;
        }

        /// <summary>
        /// Checks if a prefab is a list item (should be excluded from button search).
        /// </summary>
        private bool IsListItemPrefab(GameObject prefab, string prefabName)
        {
            // Check name patterns
            if (prefabName.Contains("ListItem") || prefabName.Contains("list_item") || prefabName.Contains("listitem"))
                return true;

            // Check for list-related component types by examining inheritance hierarchy
            var allComponents = prefab.GetComponentsInChildren<Component>(true);
            foreach (var comp in allComponents)
            {
                if (comp == null) continue;
                
                Type compType = comp.GetType();
                
                // Check type name for list-related patterns
                string typeName = compType.Name;
                if (typeName.Contains("ListItem") || typeName.Contains("ListView"))
                    return true;

                // Check inheritance hierarchy for Framewerk list base classes
                Type baseType = compType.BaseType;
                while (baseType != null)
                {
                    string baseName = baseType.Name;
                    // FramewerkListItemView, ListView, or any generic ListItemView
                    if (baseName.Contains("ListItemView") || baseName.Contains("ListItemMediator") ||
                        baseName.StartsWith("ListView") || baseName.StartsWith("ListMediator"))
                        return true;
                    baseType = baseType.BaseType;
                }
            }

            return false;
        }

        private bool IsContextType(Type type)
        {
            var baseType = type.BaseType;
            while (baseType != null)
            {
                if (baseType.Name == "MVCSContext" || baseType.Name == "FramewerkMVCSContext")
                    return true;
                baseType = baseType.BaseType;
            }
            return false;
        }

        /// <summary>
        /// Gets the TypeKey for a given component type.
        /// </summary>
        private string GetTypeKey(ComponentType type)
        {
            switch (type)
            {
                case ComponentType.Popup:
                    return AddressBuilder.TypeKeys.Popup;
                case ComponentType.List:
                    return AddressBuilder.TypeKeys.List;
                case ComponentType.ListItem:
                    return AddressBuilder.TypeKeys.ListItem;
                case ComponentType.VerticalTabs:
                case ComponentType.HorizontalTabs:
                    return AddressBuilder.TypeKeys.Tabs;
                case ComponentType.ViewStack:
                    return AddressBuilder.TypeKeys.ViewStack;
                case ComponentType.View:
                default:
                    return AddressBuilder.TypeKeys.View;
            }
        }

        /// <summary>
        /// Gets the prefab name with type suffix appended.
        /// Popup appends "Popup", Button appends "Button", View is basic (no suffix), List already has "List" in effectiveName.
        /// </summary>
        private string GetPrefabName(string effectiveName)
        {
            if (componentType == ComponentType.Popup)
                return effectiveName + "Popup";
            return effectiveName;
        }

        private void OnGUI()
        {
            EnsureStyles();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Space(10);

            // ═══════════════════════════════════════════════════════════════════
            // SECTION: Component
            // ═══════════════════════════════════════════════════════════════════
            BeginSection("Component");
            
            componentName = EditorGUILayout.TextField("Name", componentName);

            ComponentType[] validTypes = new ComponentType[] 
            { 
                ComponentType.Popup, 
                ComponentType.List, 
                ComponentType.VerticalTabs,
                ComponentType.HorizontalTabs,
                ComponentType.ViewStack,
                ComponentType.View
            };
            string[] typeNames = new string[] { "Popup", "List", "Vertical Tabs", "Horizontal Tabs", "ViewStack", "View" };
            int currentIndex = System.Array.IndexOf(validTypes, componentType);
            if (currentIndex == -1) currentIndex = 0;

            int newIndex = EditorGUILayout.Popup("Type", currentIndex, typeNames);
            componentType = validTypes[newIndex];

            // Show button prefab options for Popup type
            if (componentType == ComponentType.Popup)
            {
                GUILayout.Space(8);
                EditorGUILayout.LabelField("Button Prefab", EditorStyles.miniLabel);
                
                buttonPrefabOption = EditorGUILayout.Popup(buttonPrefabOption, buttonPrefabOptions);

                if (buttonPrefabOption == 0) // Use Existing
                {
                    EditorGUI.indentLevel++;
                    
                    // Custom ObjectField with our picker
                    EditorGUI.BeginChangeCheck();
                    selectedButtonPrefab = DrawButtonPrefabObjectField("Select Button", selectedButtonPrefab);
                    if (EditorGUI.EndChangeCheck())
                    {
                        buttonPrefabValidationError = ValidateButtonPrefab(selectedButtonPrefab);
                    }
                    
                    // Show validation error if any
                    if (!string.IsNullOrEmpty(buttonPrefabValidationError))
                    {
                        EditorGUILayout.HelpBox(buttonPrefabValidationError, MessageType.Warning);
                    }
                    else if (selectedButtonPrefab == null)
                    {
                        EditorGUILayout.HelpBox("Drag a button prefab or click ○ to browse valid buttons.", MessageType.Info);
                    }
                    
                    EditorGUI.indentLevel--;
                }
                else if (buttonPrefabOption == 1) // Create New
                {
                    EditorGUILayout.HelpBox("Will create: PopupButton.prefab", MessageType.Info);
                }
            }
            
            EndSection();

            // ═══════════════════════════════════════════════════════════════════
            // SECTION: Paths
            // ═══════════════════════════════════════════════════════════════════
            BeginSection("Paths");
            
            EditorGUILayout.BeginHorizontal();
            scriptFolder = EditorGUILayout.TextField("Script Folder", scriptFolder);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Script Folder", scriptFolder, "");
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith(Application.dataPath))
                    {
                        scriptFolder = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            prefabFolder = EditorGUILayout.TextField("Prefab Folder", prefabFolder);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Prefab Folder", prefabFolder, "");
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith(Application.dataPath))
                    {
                        prefabFolder = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EndSection();

            // ═══════════════════════════════════════════════════════════════════
            // SECTION: Code Generation
            // ═══════════════════════════════════════════════════════════════════
            BeginSection("Code Generation");
            
            EditorGUILayout.BeginHorizontal();
            namespaceName = EditorGUILayout.TextField("Namespace", namespaceName);
            if (GUILayout.Button("Auto", GUILayout.Width(50)))
            {
                namespaceName = NamespaceResolver.ResolveFromPath(scriptFolder);
            }
            EditorGUILayout.EndHorizontal();

            setupInContext = EditorGUILayout.Toggle("Setup in Context", setupInContext);

            if (setupInContext)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                selectedContextIndex = EditorGUILayout.Popup("Target Context", selectedContextIndex, contextNames);
                if (GUILayout.Button("Refresh", GUILayout.Width(60)))
                {
                    FindContextTypes();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }
            
            EndSection();

            // ═══════════════════════════════════════════════════════════════════
            // SECTION: Address Configuration (moved to end)
            // ═══════════════════════════════════════════════════════════════════
            BeginSection("Address Configuration");
            
            EditorGUILayout.BeginHorizontal();
            selectedBootstrapIndex = EditorGUILayout.Popup("Target Bootstrap", selectedBootstrapIndex, bootstrapNames);
            if (GUILayout.Button("Refresh", GUILayout.Width(60)))
            {
                FindBootstrapConfigs();
            }
            EditorGUILayout.EndHorizontal();

            if (bootstrapConfigs.Count > 0 && selectedBootstrapIndex < bootstrapConfigs.Count)
            {
                var config = bootstrapConfigs[selectedBootstrapIndex];
                EditorGUI.indentLevel++;
                
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    string contextPrefixValue = config.ContextPrefixSO != null ? config.ContextPrefixSO.Prefix : "(none)";
                    EditorGUILayout.TextField("Context Prefix (SO)", contextPrefixValue);
                }
                
                EditorGUI.indentLevel--;
            }

            customPrefix = EditorGUILayout.TextField("Custom Prefix (optional)", customPrefix);
            EditorGUILayout.HelpBox("Custom Prefix is passed at runtime. Leave empty if not needed.", MessageType.None);
            
            EndSection();

            // ═══════════════════════════════════════════════════════════════════
            // SECTION: Preview
            // ═══════════════════════════════════════════════════════════════════
            DrawPreview();

            GUILayout.Space(10);

            // Buttons
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Generate", GUILayout.Width(100), GUILayout.Height(30)))
            {
                Generate();
            }

            if (GUILayout.Button("Cancel", GUILayout.Width(100), GUILayout.Height(30)))
            {
                Close();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.EndScrollView();
        }

        private void DrawPreview()
        {
            BeginSection("Preview");

            if (string.IsNullOrEmpty(componentName))
            {
                EditorGUILayout.LabelField("Enter a component name to see preview", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                string previewName = componentType == ComponentType.List ? componentName + "List" : componentName;
                string prefabName = GetPrefabName(previewName);
                bool isTabContainer = componentType == ComponentType.VerticalTabs || componentType == ComponentType.HorizontalTabs;

                // Get ViewConfig for address building (uses resolver if configured)
                ViewConfig viewConfig = null;
                if (bootstrapConfigs.Count > 0 && selectedBootstrapIndex < bootstrapConfigs.Count)
                {
                    viewConfig = bootstrapConfigs[selectedBootstrapIndex];
                }

                // Compute addressable addresses for preview (uses resolver from ViewConfig if available)
                string typeKey = GetTypeKey(componentType);
                string effectiveCustomPrefix = string.IsNullOrEmpty(customPrefix) ? null : customPrefix;
                string mainAddress = AddressBuilder.BuildAddress(viewConfig, effectiveCustomPrefix, typeKey, prefabName);
                string itemAddress = null;
                
                if (componentType == ComponentType.List)
                {
                    itemAddress = AddressBuilder.BuildAddress(viewConfig, effectiveCustomPrefix, AddressBuilder.TypeKeys.ListItem, previewName + "Item");
                }
                else if (isTabContainer)
                {
                    itemAddress = AddressBuilder.BuildAddress(viewConfig, effectiveCustomPrefix, AddressBuilder.TypeKeys.TabItem, previewName + "Item");
                }

                EditorGUILayout.LabelField("Scripts:", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField($"  {previewName}View.cs", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"  {previewName}Mediator.cs", EditorStyles.miniLabel);

                if (componentType == ComponentType.List || isTabContainer)
                {
                    EditorGUILayout.LabelField($"  {previewName}Data.cs", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField($"  {previewName}ItemView.cs", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField($"  {previewName}ItemMediator.cs", EditorStyles.miniLabel);
                }

                GUILayout.Space(5);

                // Prefabs table
                EditorGUILayout.LabelField("Prefabs:", EditorStyles.miniBoldLabel);

                // Table header
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("File", EditorStyles.miniLabel, GUILayout.Width(150));
                EditorGUILayout.LabelField("Addressable ID", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();

                // Separator line
                var rect = EditorGUILayout.GetControlRect(false, 1);
                EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));

                // Main prefab row
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"  {prefabName}.prefab", EditorStyles.miniLabel, GUILayout.Width(150));
                EditorGUILayout.LabelField(mainAddress, EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();

                // Item prefab row (for Lists and Tab Containers)
                if (componentType == ComponentType.List || isTabContainer)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"  {previewName}Item.prefab", EditorStyles.miniLabel, GUILayout.Width(150));
                    EditorGUILayout.LabelField(itemAddress, EditorStyles.miniLabel);
                    EditorGUILayout.EndHorizontal();
                }

                if (setupInContext && contextTypes.Count > 0 && selectedContextIndex < contextTypes.Count)
                {
                    GUILayout.Space(5);
                    EditorGUILayout.LabelField($"Context binding: {contextTypes[selectedContextIndex].Name}", EditorStyles.miniLabel);
                }
            }

            EndSection();
        }

        private void Generate()
        {
            // Validate
            if (string.IsNullOrEmpty(componentName))
            {
                EditorUtility.DisplayDialog("Validation Error", "Component name is required.", "OK");
                return;
            }

            if (string.IsNullOrEmpty(namespaceName))
            {
                EditorUtility.DisplayDialog("Validation Error", "Namespace is required.", "OK");
                return;
            }

            if (bootstrapConfigs.Count == 0)
            {
                if (!EditorUtility.DisplayDialog("Warning", "No ViewConfig found. The addressable ID will use default settings. Continue?", "Yes", "No"))
                {
                    return;
                }
            }

            if (setupInContext && (contextTypes.Count == 0 || selectedContextIndex >= contextTypes.Count))
            {
                if (!EditorUtility.DisplayDialog("Warning", "No valid Context found. Continue without context binding?", "Yes", "No"))
                {
                    return;
                }
            }

            // Validate button prefab for Popup type
            if (componentType == ComponentType.Popup && buttonPrefabOption == 0)
            {
                if (selectedButtonPrefab == null)
                {
                    EditorUtility.DisplayDialog("Validation Error", "Please select a button prefab or choose 'Create New'.", "OK");
                    return;
                }
                
                string validationError = ValidateButtonPrefab(selectedButtonPrefab);
                if (!string.IsNullOrEmpty(validationError))
                {
                    if (!EditorUtility.DisplayDialog("Warning", $"Selected button prefab has issues:\n\n{validationError}\n\nContinue anyway?", "Yes", "No"))
                    {
                        return;
                    }
                }
            }

            // Save preferences
            SavePreferences();

            // Effective name: append "List" suffix for List components
            string effectiveName = componentType == ComponentType.List ? componentName + "List" : componentName;
            string prefabName = GetPrefabName(effectiveName);

            // Get ViewConfig for address building (uses resolver if configured)
            ViewConfig viewConfig = null;
            if (bootstrapConfigs.Count > 0 && selectedBootstrapIndex < bootstrapConfigs.Count)
            {
                viewConfig = bootstrapConfigs[selectedBootstrapIndex];
            }
            string effectiveCustomPrefix = string.IsNullOrEmpty(customPrefix) ? null : customPrefix;

            // Create folders if needed
            if (!Directory.Exists(scriptFolder))
            {
                Directory.CreateDirectory(scriptFolder);
            }

            if (!Directory.Exists(prefabFolder))
            {
                Directory.CreateDirectory(prefabFolder);
            }

            // Check for existing files
            var existingFiles = new List<string>();
            string viewPath = Path.Combine(scriptFolder, $"{effectiveName}View.cs");
            string mediatorPath = Path.Combine(scriptFolder, $"{effectiveName}Mediator.cs");
            string prefabPath = Path.Combine(prefabFolder, $"{prefabName}.prefab");

            if (File.Exists(viewPath)) existingFiles.Add(viewPath);
            if (File.Exists(mediatorPath)) existingFiles.Add(mediatorPath);
            if (File.Exists(prefabPath)) existingFiles.Add(prefabPath);

            bool isTabContainer = componentType == ComponentType.VerticalTabs || componentType == ComponentType.HorizontalTabs;
            
            if (componentType == ComponentType.List || isTabContainer)
            {
                string dataPath = Path.Combine(scriptFolder, $"{effectiveName}Data.cs");
                string itemViewPath = Path.Combine(scriptFolder, $"{effectiveName}ItemView.cs");
                string itemMediatorPath = Path.Combine(scriptFolder, $"{effectiveName}ItemMediator.cs");
                string itemPrefabPath = Path.Combine(prefabFolder, $"{effectiveName}Item.prefab");

                if (File.Exists(dataPath)) existingFiles.Add(dataPath);
                if (File.Exists(itemViewPath)) existingFiles.Add(itemViewPath);
                if (File.Exists(itemMediatorPath)) existingFiles.Add(itemMediatorPath);
                if (File.Exists(itemPrefabPath)) existingFiles.Add(itemPrefabPath);
            }

            if (existingFiles.Count > 0)
            {
                string fileList = string.Join("\n", existingFiles);
                if (!EditorUtility.DisplayDialog("Files Already Exist",
                    $"The following files already exist:\n\n{fileList}\n\nOverwrite?",
                    "Overwrite", "Cancel"))
                {
                    return;
                }
            }

            // Generate script files
            GenerateScripts(effectiveName);

            // Build addressable address (uses resolver from ViewConfig if available)
            string typeKey = GetTypeKey(componentType);
            string mainAddress = AddressBuilder.BuildAddress(viewConfig, effectiveCustomPrefix, typeKey, prefabName);

            // Create wizard job (always marks as addressable)
            WizardJob job = new WizardJob
            {
                componentName = effectiveName,
                componentType = (int)componentType,
                namespaceName = namespaceName,
                scriptFolder = scriptFolder,
                prefabFolder = prefabFolder,
                contextFilePath = setupInContext ? GetContextFilePath() : null,
                markAddressable = true, // Always mark as addressable
                openAfterCreate = false,
                viewTypeName = $"{namespaceName}.{effectiveName}View",
                mediatorTypeName = $"{namespaceName}.{effectiveName}Mediator",
                prefabPath = Path.Combine(prefabFolder, $"{prefabName}.prefab"),
                addressableAddress = mainAddress
            };

            bool isTabContainerJob = componentType == ComponentType.VerticalTabs || componentType == ComponentType.HorizontalTabs;
            
            if (componentType == ComponentType.List)
            {
                string itemName = effectiveName + "Item";
                string itemAddress = AddressBuilder.BuildAddress(viewConfig, effectiveCustomPrefix, AddressBuilder.TypeKeys.ListItem, itemName);

                job.dataTypeName = $"{namespaceName}.{effectiveName}Data";
                job.itemViewTypeName = $"{namespaceName}.{itemName}View";
                job.itemMediatorTypeName = $"{namespaceName}.{itemName}Mediator";
                job.itemPrefabPath = Path.Combine(prefabFolder, $"{itemName}.prefab");
                job.itemAddressableAddress = itemAddress;
            }
            else if (isTabContainerJob)
            {
                string itemName = effectiveName + "Item";
                string itemAddress = AddressBuilder.BuildAddress(viewConfig, effectiveCustomPrefix, AddressBuilder.TypeKeys.TabItem, itemName);

                job.dataTypeName = $"{namespaceName}.{effectiveName}Data";
                job.itemViewTypeName = $"{namespaceName}.{itemName}View";
                job.itemMediatorTypeName = $"{namespaceName}.{itemName}Mediator";
                job.itemPrefabPath = Path.Combine(prefabFolder, $"{itemName}.prefab");
                job.itemAddressableAddress = itemAddress;
            }

            // Handle button prefab for Popup type
            if (componentType == ComponentType.Popup)
            {
                if (buttonPrefabOption == 0 && selectedButtonPrefab != null) // Use Existing
                {
                    job.existingButtonPrefabPath = AssetDatabase.GetAssetPath(selectedButtonPrefab);
                    job.createButtonPrefab = false;
                }
                else // Create New (buttonPrefabOption == 1 or no selection)
                {
                    job.buttonPrefabPath = Path.Combine(prefabFolder, "PopupButton.prefab");
                    job.createButtonPrefab = true;
                }
            }

            // Save job to EditorPrefs
            string json = JsonUtility.ToJson(job);
            EditorPrefs.SetString("FramewerkWizard_PendingJob", json);

            // Refresh to trigger recompile
            AssetDatabase.Refresh();

            Debug.Log($"Generated scripts for {effectiveName}. Waiting for recompile to complete prefab generation...");
        }

        private void GenerateScripts(string effectiveName)
        {
            // Generate View
            string viewCode = CodeTemplates.GetViewTemplate(componentType, effectiveName, namespaceName);
            string viewPath = Path.Combine(scriptFolder, $"{effectiveName}View.cs");
            File.WriteAllText(viewPath, viewCode);

            // Generate Mediator
            string mediatorCode = CodeTemplates.GetMediatorTemplate(componentType, effectiveName, namespaceName);
            string mediatorPath = Path.Combine(scriptFolder, $"{effectiveName}Mediator.cs");
            File.WriteAllText(mediatorPath, mediatorCode);

            // For List, generate additional files
            if (componentType == ComponentType.List)
            {
                string itemName = effectiveName + "Item";

                string dataCode = CodeTemplates.GetListDataTemplate(effectiveName, namespaceName);
                string dataPath = Path.Combine(scriptFolder, $"{effectiveName}Data.cs");
                File.WriteAllText(dataPath, dataCode);

                string itemViewCode = CodeTemplates.GetListItemViewTemplate(effectiveName, namespaceName);
                string itemViewPath = Path.Combine(scriptFolder, $"{itemName}View.cs");
                File.WriteAllText(itemViewPath, itemViewCode);

                string itemMediatorCode = CodeTemplates.GetListItemMediatorTemplate(effectiveName, namespaceName);
                string itemMediatorPath = Path.Combine(scriptFolder, $"{itemName}Mediator.cs");
                File.WriteAllText(itemMediatorPath, itemMediatorCode);
            }
            // For Tab Containers, generate tab item files
            else if (componentType == ComponentType.VerticalTabs || componentType == ComponentType.HorizontalTabs)
            {
                string itemName = effectiveName + "Item";

                string dataCode = CodeTemplates.GetTabDataTemplate(effectiveName, namespaceName);
                string dataPath = Path.Combine(scriptFolder, $"{effectiveName}Data.cs");
                File.WriteAllText(dataPath, dataCode);

                string itemViewCode = CodeTemplates.GetTabItemViewTemplate(effectiveName, namespaceName);
                string itemViewPath = Path.Combine(scriptFolder, $"{itemName}View.cs");
                File.WriteAllText(itemViewPath, itemViewCode);

                string itemMediatorCode = CodeTemplates.GetTabItemMediatorTemplate(effectiveName, namespaceName);
                string itemMediatorPath = Path.Combine(scriptFolder, $"{itemName}Mediator.cs");
                File.WriteAllText(itemMediatorPath, itemMediatorCode);
            }
        }

        private string GetContextFilePath()
        {
            if (contextTypes.Count == 0 || selectedContextIndex >= contextTypes.Count)
                return null;

            Type contextType = contextTypes[selectedContextIndex];

            // Find the source file for this type
            var guids = AssetDatabase.FindAssets($"t:Script {contextType.Name}");
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (asset != null && asset.GetClass() == contextType)
                {
                    return path;
                }
            }

            return null;
        }
    }
}

