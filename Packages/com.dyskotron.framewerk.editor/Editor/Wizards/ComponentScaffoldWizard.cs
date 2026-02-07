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
                case ComponentType.View:
                default:
                    return AddressBuilder.TypeKeys.View;
            }
        }

        /// <summary>
        /// Gets the prefab name with type suffix appended.
        /// Popup appends "Popup", View is basic (no suffix), List already has "List" in effectiveName.
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

            ComponentType[] validTypes = new ComponentType[] { ComponentType.Popup, ComponentType.List, ComponentType.View };
            string[] typeNames = new string[] { "Popup", "List", "View" };
            int currentIndex = System.Array.IndexOf(validTypes, componentType);
            if (currentIndex == -1) currentIndex = 0;

            int newIndex = EditorGUILayout.Popup("Type", currentIndex, typeNames);
            componentType = validTypes[newIndex];
            
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

                // Get ViewConfig settings
                string contextPrefix = "";
                if (bootstrapConfigs.Count > 0 && selectedBootstrapIndex < bootstrapConfigs.Count)
                {
                    var config = bootstrapConfigs[selectedBootstrapIndex];
                    contextPrefix = config.ContextPrefixSO?.Prefix;
                }

                // Compute addressable addresses for preview
                string typeKey = GetTypeKey(componentType);
                string effectiveCustomPrefix = string.IsNullOrEmpty(customPrefix) ? null : customPrefix;
                string mainAddress = AddressBuilder.BuildAddress(contextPrefix, effectiveCustomPrefix, typeKey, prefabName);
                string itemAddress = componentType == ComponentType.List 
                    ? AddressBuilder.BuildAddress(contextPrefix, effectiveCustomPrefix, AddressBuilder.TypeKeys.ListItem, previewName + "Item") 
                    : null;

                EditorGUILayout.LabelField("Scripts:", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField($"  {previewName}View.cs", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"  {previewName}Mediator.cs", EditorStyles.miniLabel);

                if (componentType == ComponentType.List)
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

                // Item prefab row (for Lists)
                if (componentType == ComponentType.List)
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

            // Save preferences
            SavePreferences();

            // Effective name: append "List" suffix for List components
            string effectiveName = componentType == ComponentType.List ? componentName + "List" : componentName;
            string prefabName = GetPrefabName(effectiveName);

            // Get ViewConfig settings
            string contextPrefix = "";
            if (bootstrapConfigs.Count > 0 && selectedBootstrapIndex < bootstrapConfigs.Count)
            {
                var config = bootstrapConfigs[selectedBootstrapIndex];
                contextPrefix = config.ContextPrefixSO?.Prefix;
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

            if (componentType == ComponentType.List)
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

            // Build addressable address using the new system
            string typeKey = GetTypeKey(componentType);
            string mainAddress = AddressBuilder.BuildAddress(contextPrefix, effectiveCustomPrefix, typeKey, prefabName);

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

            if (componentType == ComponentType.List)
            {
                string itemName = effectiveName + "Item";
                string itemAddress = AddressBuilder.BuildAddress(contextPrefix, effectiveCustomPrefix, AddressBuilder.TypeKeys.ListItem, itemName);

                job.dataTypeName = $"{namespaceName}.{effectiveName}Data";
                job.itemViewTypeName = $"{namespaceName}.{itemName}View";
                job.itemMediatorTypeName = $"{namespaceName}.{itemName}Mediator";
                job.itemPrefabPath = Path.Combine(prefabFolder, $"{itemName}.prefab");
                job.itemAddressableAddress = itemAddress;
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
