using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Framewerk.Editor.Wizards
{
    public static class ComponentScaffoldCompleter
    {
        private const string PENDING_JOB_KEY = "FramewerkWizard_PendingJob";
        private const string PENDING_SWAP_KEY = "FramewerkWizard_PendingPrefabSwap";

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        }

        private static void OnAfterAssemblyReload()
        {
            if (!EditorPrefs.HasKey(PENDING_JOB_KEY))
                return;

            string json = EditorPrefs.GetString(PENDING_JOB_KEY);
            if (string.IsNullOrEmpty(json))
                return;

            WizardJob job = JsonUtility.FromJson<WizardJob>(json);
            if (job == null)
                return;

            // Clear the pending job immediately to prevent reprocessing
            EditorPrefs.DeleteKey(PENDING_JOB_KEY);

            try
            {
                CopyTemplatePrefabs(job);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to copy template prefabs: {e.Message}\n{e.StackTrace}");
            }
        }

        private static void CopyTemplatePrefabs(WizardJob job)
        {
            ComponentType type = (ComponentType)job.componentType;
            string templatePath = GetTemplatePath(type);

            if (string.IsNullOrEmpty(templatePath))
            {
                Debug.LogError($"No template found for component type {type}");
                return;
            }

            // Copy main prefab
            if (!AssetDatabase.CopyAsset(templatePath, job.prefabPath))
            {
                Debug.LogError($"Failed to copy template from {templatePath} to {job.prefabPath}");
                return;
            }

            // Copy item prefab for List
            if (type == ComponentType.List && !string.IsNullOrEmpty(job.itemPrefabPath))
            {
                string itemTemplatePath = GetTemplatePath(ComponentType.ListItem);
                if (!string.IsNullOrEmpty(itemTemplatePath))
                {
                    if (!AssetDatabase.CopyAsset(itemTemplatePath, job.itemPrefabPath))
                    {
                        Debug.LogError($"Failed to copy item template from {itemTemplatePath} to {job.itemPrefabPath}");
                        return;
                    }
                }
            }

            // Store the job for the swap phase
            EditorPrefs.SetString(PENDING_SWAP_KEY, JsonUtility.ToJson(job));

            // Trigger reimport
            AssetDatabase.Refresh();
        }

        internal static void CompletePrefabSwap(WizardJob job)
        {
            ComponentType type = (ComponentType)job.componentType;

            // Mark as addressable
            if (job.markAddressable)
            {
                AddressableHelper.MarkAsAddressable(job.prefabPath, job.addressableAddress);

                if (type == ComponentType.List && !string.IsNullOrEmpty(job.itemPrefabPath))
                {
                    string itemAddress = !string.IsNullOrEmpty(job.itemAddressableAddress)
                        ? job.itemAddressableAddress
                        : job.componentName + "Item";
                    AddressableHelper.MarkAsAddressable(job.itemPrefabPath, itemAddress);
                }
            }

            // Inject context binding
            if (!string.IsNullOrEmpty(job.contextFilePath))
            {
                string viewName = job.componentName + "View";
                string mediatorName = job.componentName + "Mediator";
                ContextInjector.InjectBinding(job.contextFilePath, viewName, mediatorName, job.namespaceName);

                // Also inject item binding for List
                if (type == ComponentType.List && !string.IsNullOrEmpty(job.itemViewTypeName))
                {
                    string itemViewName = job.componentName + "ItemView";
                    string itemMediatorName = job.componentName + "ItemMediator";
                    ContextInjector.InjectBinding(job.contextFilePath, itemViewName, itemMediatorName, job.namespaceName);
                }

                AssetDatabase.Refresh();
            }

            // Open scripts if requested
            if (job.openAfterCreate)
            {
                OpenGeneratedScripts(job);
            }

            Debug.Log($"<color=green>✓ Wizard: {job.componentName} component created successfully</color>");

            // Highlight the generated prefab in the Project window
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(job.prefabPath);
            if (prefab != null)
            {
                EditorGUIUtility.PingObject(prefab);
                Selection.activeObject = prefab;
            }

            // Clear the pending swap key
            EditorPrefs.DeleteKey(PENDING_SWAP_KEY);
        }

        internal static bool HasPendingSwap(out WizardJob job)
        {
            if (EditorPrefs.HasKey(PENDING_SWAP_KEY))
            {
                string json = EditorPrefs.GetString(PENDING_SWAP_KEY);
                if (!string.IsNullOrEmpty(json))
                {
                    job = JsonUtility.FromJson<WizardJob>(json);
                    return job != null;
                }
            }

            job = null;
            return false;
        }

        private static string GetTemplatePath(ComponentType componentType)
        {
            const string TEMPLATE_PATH = "Packages/com.dyskotron.framewerk/Editor/Wizards/Templates/";

            switch (componentType)
            {
                case ComponentType.List:
                    return TEMPLATE_PATH + "ListTemplate.prefab";
                case ComponentType.ListItem:
                    return TEMPLATE_PATH + "ListItemTemplate.prefab";
                case ComponentType.Popup:
                    return TEMPLATE_PATH + "PopupTemplate.prefab";
                case ComponentType.Screen:
                    return TEMPLATE_PATH + "PanelTemplate.prefab";
                case ComponentType.View:
                    return TEMPLATE_PATH + "ViewTemplate.prefab";
                default:
                    return null;
            }
        }

        internal static Type FindType(string fullTypeName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    Type type = assembly.GetTypes().FirstOrDefault(t => t.FullName == fullTypeName);
                    if (type != null)
                        return type;
                }
                catch
                {
                    // Skip assemblies that throw exceptions
                }
            }
            return null;
        }

        private static void OpenGeneratedScripts(WizardJob job)
        {
            // Find and open the View script
            var guids = AssetDatabase.FindAssets($"{job.componentName}View t:Script");
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains(job.scriptFolder))
                {
                    var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                    if (script != null)
                    {
                        AssetDatabase.OpenAsset(script);
                        break;
                    }
                }
            }
        }
    }
}
