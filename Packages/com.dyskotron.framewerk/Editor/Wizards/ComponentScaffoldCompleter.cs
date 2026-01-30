using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Framewerk.Editor.Wizards
{
    public static class ComponentScaffoldCompleter
    {
        private const string PENDING_JOB_KEY = "FramewerkWizard_PendingJob";

        [DidReloadScripts]
        private static void OnScriptsReloaded()
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
                CompleteJob(job);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to complete wizard job: {e.Message}\n{e.StackTrace}");
            }
        }

        private static void CompleteJob(WizardJob job)
        {
            Debug.Log($"Completing wizard job for {job.componentName}...");

            ComponentType type = (ComponentType)job.componentType;

            // Find the View type
            Type viewType = FindType(job.viewTypeName);
            if (viewType == null)
            {
                Debug.LogError($"Could not find type: {job.viewTypeName}. Prefab creation aborted.");
                return;
            }

            // Create main prefab
            var prefab = PrefabGenerator.CreatePrefab(job.prefabPath, viewType, type);
            if (prefab == null)
            {
                Debug.LogError($"Failed to create prefab at {job.prefabPath}");
                return;
            }

            Debug.Log($"Created prefab: {job.prefabPath}");

            // Mark as addressable
            if (job.markAddressable)
            {
                AddressableHelper.MarkAsAddressable(job.prefabPath, job.addressableAddress);
            }

            // Create item prefab for List
            if (type == ComponentType.List && !string.IsNullOrEmpty(job.itemViewTypeName))
            {
                Type itemViewType = FindType(job.itemViewTypeName);
                if (itemViewType != null)
                {
                    var itemPrefab = PrefabGenerator.CreatePrefab(job.itemPrefabPath, itemViewType, ComponentType.ListItem);
                    if (itemPrefab != null)
                    {
                        Debug.Log($"Created item prefab: {job.itemPrefabPath}");

                        if (job.markAddressable)
                        {
                            string itemAddress = $"UI/{job.componentName}Item";
                            AddressableHelper.MarkAsAddressable(job.itemPrefabPath, itemAddress);
                        }
                    }
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

            // Show success notification
            EditorUtility.DisplayDialog(
                "Success",
                $"Successfully generated {job.componentName} component!\n\n" +
                $"View: {job.viewTypeName}\n" +
                $"Mediator: {job.mediatorTypeName}\n" +
                $"Prefab: {job.prefabPath}",
                "OK"
            );

            Debug.Log($"<color=green>Wizard job completed successfully for {job.componentName}!</color>");
        }

        private static Type FindType(string fullTypeName)
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
