using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private const string PREFS_MARK_ADDRESSABLE = "FramewerkWizard_MarkAddressable";
        private const string PREFS_OPEN_AFTER = "FramewerkWizard_OpenAfter";
        private const string PREFS_ADDRESSABLE_PREFIX = "FramewerkWizard_AddressablePrefix";
        private const string PREFS_UI_TYPE_KEY = "FramewerkWizard_UiTypeKey";
        private const string PREFS_UI_TYPE_KEY_IS_PREFIX = "FramewerkWizard_UiTypeKeyIsPrefix";
        private const string PREFS_POPUP_TYPE_KEY = "FramewerkWizard_PopupTypeKey";

        private string componentName = "";
        private ComponentType componentType = ComponentType.Popup;
        private string namespaceName = "";
        private string scriptFolder = "Assets/Scripts";
        private string prefabFolder = "Assets/Prefabs";
        private int selectedContextIndex = 0;
        private bool setupInContext = true;
        private bool markAddressable = true;
        private bool openAfterCreate = false;
        private string addressablePrefix = "";
        private string uiTypeKey = "UI";
        private bool uiTypeKeyIsPrefix = false;
        private string popupTypeKey = "Popups";

        private List<Type> contextTypes = new List<Type>();
        private string[] contextNames = new string[0];

        private Vector2 scrollPosition;

        [MenuItem("Framewerk/Create UI Component")]
        public static void ShowWindow()
        {
            var window = GetWindow<ComponentScaffoldWizard>("Framewerk Component Wizard");
            window.minSize = new Vector2(450, 500);
            window.Show();
        }

        private void OnEnable()
        {
            LoadPreferences();
            FindContextTypes();
        }

        private void LoadPreferences()
        {
            namespaceName = EditorPrefs.GetString(PREFS_NAMESPACE, "");
            scriptFolder = EditorPrefs.GetString(PREFS_SCRIPT_FOLDER, "Assets/Scripts");
            prefabFolder = EditorPrefs.GetString(PREFS_PREFAB_FOLDER, "Assets/Prefabs");
            selectedContextIndex = EditorPrefs.GetInt(PREFS_CONTEXT_INDEX, 0);
            setupInContext = EditorPrefs.GetBool(PREFS_SETUP_IN_CONTEXT, true);
            markAddressable = EditorPrefs.GetBool(PREFS_MARK_ADDRESSABLE, true);
            openAfterCreate = EditorPrefs.GetBool(PREFS_OPEN_AFTER, false);
            addressablePrefix = EditorPrefs.GetString(PREFS_ADDRESSABLE_PREFIX, "");
            uiTypeKey = EditorPrefs.GetString(PREFS_UI_TYPE_KEY, "UI");
            uiTypeKeyIsPrefix = EditorPrefs.GetBool(PREFS_UI_TYPE_KEY_IS_PREFIX, false);
            popupTypeKey = EditorPrefs.GetString(PREFS_POPUP_TYPE_KEY, "Popups");
        }

        private void SavePreferences()
        {
            EditorPrefs.SetString(PREFS_NAMESPACE, namespaceName);
            EditorPrefs.SetString(PREFS_SCRIPT_FOLDER, scriptFolder);
            EditorPrefs.SetString(PREFS_PREFAB_FOLDER, prefabFolder);
            EditorPrefs.SetInt(PREFS_CONTEXT_INDEX, selectedContextIndex);
            EditorPrefs.SetBool(PREFS_SETUP_IN_CONTEXT, setupInContext);
            EditorPrefs.SetBool(PREFS_MARK_ADDRESSABLE, markAddressable);
            EditorPrefs.SetBool(PREFS_OPEN_AFTER, openAfterCreate);
            EditorPrefs.SetString(PREFS_ADDRESSABLE_PREFIX, addressablePrefix);
            // UI TypeKey, TypeKey is Prefix, and Popup TypeKey are now saved by FramewerkSettingsWindow
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

        private string BuildAddress(params string[] segments)
        {
            var parts = new List<string>();
            foreach (var seg in segments)
            {
                if (string.IsNullOrEmpty(seg)) continue;
                parts.Add(seg.Trim('/'));
            }
            return string.Join("/", parts);
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Framewerk Component Wizard", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Settings...", GUILayout.Width(80)))
            {
                FramewerkSettingsWindow.ShowWindow();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Component Type - exclude Screen from dropdown
            ComponentType[] validTypes = new ComponentType[] { ComponentType.Popup, ComponentType.List };
            string[] typeNames = new string[] { "Popup", "List" };
            int currentIndex = System.Array.IndexOf(validTypes, componentType);
            if (currentIndex == -1) currentIndex = 0; // Default to Popup if Screen was somehow selected

            int newIndex = EditorGUILayout.Popup("Component Type", currentIndex, typeNames);
            componentType = validTypes[newIndex];

            // Component Name
            componentName = EditorGUILayout.TextField("Component Name", componentName);

            // Addressable Prefix
            addressablePrefix = EditorGUILayout.TextField("Addressable Prefix", addressablePrefix);

            GUILayout.Space(10);

            // Namespace
            EditorGUILayout.BeginHorizontal();
            namespaceName = EditorGUILayout.TextField("Namespace", namespaceName);
            if (GUILayout.Button("Auto", GUILayout.Width(50)))
            {
                namespaceName = NamespaceResolver.ResolveFromPath(scriptFolder);
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Script Folder
            EditorGUILayout.BeginHorizontal();
            scriptFolder = EditorGUILayout.TextField("Script Folder", scriptFolder);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Script Folder", scriptFolder, "");
                if (!string.IsNullOrEmpty(path))
                {
                    // Convert absolute path to relative
                    if (path.StartsWith(Application.dataPath))
                    {
                        scriptFolder = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            // Prefab Folder
            EditorGUILayout.BeginHorizontal();
            prefabFolder = EditorGUILayout.TextField("Prefab Folder", prefabFolder);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Prefab Folder", prefabFolder, "");
                if (!string.IsNullOrEmpty(path))
                {
                    // Convert absolute path to relative
                    if (path.StartsWith(Application.dataPath))
                    {
                        prefabFolder = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Options
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

            markAddressable = EditorGUILayout.Toggle("Mark prefab as Addressable", markAddressable);

            openAfterCreate = EditorGUILayout.Toggle("Open generated scripts after creation", openAfterCreate);

            GUILayout.Space(10);

            // Preview
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
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (string.IsNullOrEmpty(componentName))
            {
                EditorGUILayout.LabelField("Enter a component name to see preview", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                string previewName = componentType == ComponentType.List ? componentName + "List" : componentName;

                EditorGUILayout.LabelField("Will generate:", EditorStyles.miniBoldLabel);

                EditorGUILayout.LabelField($"  • {previewName}View.cs", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"  • {previewName}Mediator.cs", EditorStyles.miniLabel);

                if (componentType == ComponentType.List)
                {
                    EditorGUILayout.LabelField($"  • {previewName}Data.cs", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField($"  • {previewName}ItemView.cs", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField($"  • {previewName}ItemMediator.cs", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField($"  • {previewName}Item.prefab", EditorStyles.miniLabel);
                }

                EditorGUILayout.LabelField($"  • {previewName}.prefab", EditorStyles.miniLabel);

                if (contextTypes.Count > 0 && selectedContextIndex < contextTypes.Count)
                {
                    EditorGUILayout.LabelField($"  • Context binding in {contextTypes[selectedContextIndex].Name}", EditorStyles.miniLabel);
                }
            }

            EditorGUILayout.EndVertical();
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
            var existingFiles = new System.Collections.Generic.List<string>();
            string viewPath = Path.Combine(scriptFolder, $"{effectiveName}View.cs");
            string mediatorPath = Path.Combine(scriptFolder, $"{effectiveName}Mediator.cs");
            string prefabPath = Path.Combine(prefabFolder, $"{effectiveName}.prefab");

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

            // Build addressable address matching manager settings
            string mainAddress;
            if (componentType == ComponentType.Popup)
            {
                // Popup always uses PopupManager's TypeKey in postfix position
                mainAddress = BuildAddress(addressablePrefix, popupTypeKey, effectiveName);
            }
            else
            {
                // List uses UiManager's TypeKey and respects TypeKeyIsPrefix setting
                if (uiTypeKeyIsPrefix)
                    mainAddress = BuildAddress(uiTypeKey, addressablePrefix, effectiveName);
                else
                    mainAddress = BuildAddress(addressablePrefix, uiTypeKey, effectiveName);
            }

            // Create wizard job
            WizardJob job = new WizardJob
            {
                componentName = effectiveName,
                componentType = (int)componentType,
                namespaceName = namespaceName,
                scriptFolder = scriptFolder,
                prefabFolder = prefabFolder,
                contextFilePath = setupInContext ? GetContextFilePath() : null,
                markAddressable = markAddressable,
                openAfterCreate = openAfterCreate,
                viewTypeName = $"{namespaceName}.{effectiveName}View",
                mediatorTypeName = $"{namespaceName}.{effectiveName}Mediator",
                prefabPath = Path.Combine(prefabFolder, $"{effectiveName}.prefab"),
                addressableAddress = mainAddress
            };

            if (componentType == ComponentType.List)
            {
                string itemName = effectiveName + "Item";
                // List items use the same UiManager settings as the parent list
                string itemAddress;
                if (uiTypeKeyIsPrefix)
                    itemAddress = BuildAddress(uiTypeKey, addressablePrefix, itemName);
                else
                    itemAddress = BuildAddress(addressablePrefix, uiTypeKey, itemName);

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
