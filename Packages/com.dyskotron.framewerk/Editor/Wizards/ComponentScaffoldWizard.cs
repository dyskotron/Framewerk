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
        private const string PREFS_MARK_ADDRESSABLE = "FramewerkWizard_MarkAddressable";
        private const string PREFS_OPEN_AFTER = "FramewerkWizard_OpenAfter";

        private string componentName = "";
        private ComponentType componentType = ComponentType.Screen;
        private string namespaceName = "";
        private string scriptFolder = "Assets/Scripts";
        private string prefabFolder = "Assets/Prefabs";
        private int selectedContextIndex = 0;
        private bool markAddressable = true;
        private bool openAfterCreate = true;

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
            markAddressable = EditorPrefs.GetBool(PREFS_MARK_ADDRESSABLE, true);
            openAfterCreate = EditorPrefs.GetBool(PREFS_OPEN_AFTER, true);
        }

        private void SavePreferences()
        {
            EditorPrefs.SetString(PREFS_NAMESPACE, namespaceName);
            EditorPrefs.SetString(PREFS_SCRIPT_FOLDER, scriptFolder);
            EditorPrefs.SetString(PREFS_PREFAB_FOLDER, prefabFolder);
            EditorPrefs.SetInt(PREFS_CONTEXT_INDEX, selectedContextIndex);
            EditorPrefs.SetBool(PREFS_MARK_ADDRESSABLE, markAddressable);
            EditorPrefs.SetBool(PREFS_OPEN_AFTER, openAfterCreate);
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

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Framewerk Component Wizard", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // Component Type
            componentType = (ComponentType)EditorGUILayout.EnumPopup("Component Type", componentType);

            // Component Name
            componentName = EditorGUILayout.TextField("Component Name", componentName);

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

            // Target Context
            EditorGUILayout.BeginHorizontal();
            selectedContextIndex = EditorGUILayout.Popup("Target Context", selectedContextIndex, contextNames);
            if (GUILayout.Button("Refresh", GUILayout.Width(60)))
            {
                FindContextTypes();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Options
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
                EditorGUILayout.LabelField("Will generate:", EditorStyles.miniBoldLabel);

                EditorGUILayout.LabelField($"  • {componentName}View.cs", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"  • {componentName}Mediator.cs", EditorStyles.miniLabel);

                if (componentType == ComponentType.List)
                {
                    EditorGUILayout.LabelField($"  • {componentName}Data.cs", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField($"  • {componentName}ItemView.cs", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField($"  • {componentName}ItemMediator.cs", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField($"  • {componentName}Item.prefab", EditorStyles.miniLabel);
                }

                EditorGUILayout.LabelField($"  • {componentName}.prefab", EditorStyles.miniLabel);

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

            if (contextTypes.Count == 0 || selectedContextIndex >= contextTypes.Count)
            {
                if (!EditorUtility.DisplayDialog("Warning", "No valid Context found. Continue without context binding?", "Yes", "No"))
                {
                    return;
                }
            }

            // Save preferences
            SavePreferences();

            // Create folders if needed
            if (!Directory.Exists(scriptFolder))
            {
                Directory.CreateDirectory(scriptFolder);
            }

            if (!Directory.Exists(prefabFolder))
            {
                Directory.CreateDirectory(prefabFolder);
            }

            // Generate script files
            GenerateScripts();

            // Create wizard job
            WizardJob job = new WizardJob
            {
                componentName = componentName,
                componentType = (int)componentType,
                namespaceName = namespaceName,
                scriptFolder = scriptFolder,
                prefabFolder = prefabFolder,
                contextFilePath = GetContextFilePath(),
                markAddressable = markAddressable,
                openAfterCreate = openAfterCreate,
                viewTypeName = $"{namespaceName}.{componentName}View",
                mediatorTypeName = $"{namespaceName}.{componentName}Mediator",
                prefabPath = Path.Combine(prefabFolder, $"{componentName}.prefab"),
                addressableAddress = $"UI/{componentName}"
            };

            if (componentType == ComponentType.List)
            {
                job.dataTypeName = $"{namespaceName}.{componentName}Data";
                job.itemViewTypeName = $"{namespaceName}.{componentName}ItemView";
                job.itemMediatorTypeName = $"{namespaceName}.{componentName}ItemMediator";
                job.itemPrefabPath = Path.Combine(prefabFolder, $"{componentName}Item.prefab");
            }

            // Save job to EditorPrefs
            string json = JsonUtility.ToJson(job);
            EditorPrefs.SetString("FramewerkWizard_PendingJob", json);

            // Refresh to trigger recompile
            AssetDatabase.Refresh();

            Debug.Log($"Generated scripts for {componentName}. Waiting for recompile to complete prefab generation...");
        }

        private void GenerateScripts()
        {
            // Generate View
            string viewCode = CodeTemplates.GetViewTemplate(componentType, componentName, namespaceName);
            string viewPath = Path.Combine(scriptFolder, $"{componentName}View.cs");
            File.WriteAllText(viewPath, viewCode);

            // Generate Mediator
            string mediatorCode = CodeTemplates.GetMediatorTemplate(componentType, componentName, namespaceName);
            string mediatorPath = Path.Combine(scriptFolder, $"{componentName}Mediator.cs");
            File.WriteAllText(mediatorPath, mediatorCode);

            // For List, generate additional files
            if (componentType == ComponentType.List)
            {
                string dataCode = CodeTemplates.GetListDataTemplate(componentName, namespaceName);
                string dataPath = Path.Combine(scriptFolder, $"{componentName}Data.cs");
                File.WriteAllText(dataPath, dataCode);

                string itemViewCode = CodeTemplates.GetListItemViewTemplate(componentName, namespaceName);
                string itemViewPath = Path.Combine(scriptFolder, $"{componentName}ItemView.cs");
                File.WriteAllText(itemViewPath, itemViewCode);

                string itemMediatorCode = CodeTemplates.GetListItemMediatorTemplate(componentName, namespaceName);
                string itemMediatorPath = Path.Combine(scriptFolder, $"{componentName}ItemMediator.cs");
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
