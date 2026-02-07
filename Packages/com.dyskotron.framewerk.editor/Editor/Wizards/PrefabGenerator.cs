using System;
using UnityEngine;
using UnityEditor;

namespace Framewerk.Editor.Wizards
{
    /// <summary>
    /// AssetPostprocessor that swaps the base View component with the generated View script
    /// after prefab reimport. Unity preserves serialized field references when field names match.
    /// </summary>
    public class PrefabGenerator : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            // Check if we have a pending swap job
            if (!ComponentScaffoldCompleter.HasPendingSwap(out WizardJob job))
                return;

            bool mainPrefabProcessed = false;
            bool itemPrefabProcessed = false;

            // Check if any of the imported assets match our pending prefabs
            foreach (string assetPath in importedAssets)
            {
                if (assetPath == job.prefabPath)
                {
                    ProcessPrefabSwap(assetPath, job.viewTypeName, job.componentName);
                    mainPrefabProcessed = true;
                }
                else if (!string.IsNullOrEmpty(job.itemPrefabPath) && assetPath == job.itemPrefabPath)
                {
                    ProcessPrefabSwap(assetPath, job.itemViewTypeName, job.componentName + "Item");
                    itemPrefabProcessed = true;
                }
            }

            // If all expected prefabs have been processed, link and complete the job
            ComponentType type = (ComponentType)job.componentType;
            bool allDone = mainPrefabProcessed && (type != ComponentType.List || itemPrefabProcessed);

            if (allDone)
            {
                // Link the List's ItemPrefab field to the ListItem prefab
                if (type == ComponentType.List && !string.IsNullOrEmpty(job.itemPrefabPath))
                {
                    LinkListItemPrefab(job.prefabPath, job.itemPrefabPath);
                }

                ComponentScaffoldCompleter.CompletePrefabSwap(job);
            }
        }

        private static void ProcessPrefabSwap(string prefabPath, string viewTypeName, string prefabName)
        {
            Type viewType = ComponentScaffoldCompleter.FindType(viewTypeName);
            if (viewType == null)
            {
                Debug.LogError($"Could not find type: {viewTypeName}");
                return;
            }

            // Load prefab contents
            GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabContents == null)
            {
                Debug.LogError($"Failed to load prefab contents from {prefabPath}");
                return;
            }

            // Rename the root GameObject
            prefabContents.name = prefabName;

            // Swap the base View component with the new custom View
            Type baseViewType = GetBaseViewType(viewType);
            if (baseViewType != null)
            {
                SwapComponent(prefabContents, baseViewType, viewType);
            }
            else
            {
                Debug.LogWarning($"No base view type found for {viewType.Name}, adding component without swap");
                prefabContents.AddComponent(viewType);
            }

            // Save the modified prefab contents back to the asset
            PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);

            // Unload the prefab contents (cleanup)
            PrefabUtility.UnloadPrefabContents(prefabContents);

            Debug.Log($"Processed prefab swap: {prefabPath}");
        }

        private static Type GetBaseViewType(Type viewType)
        {
            if (viewType == null || viewType.BaseType == null)
                return null;

            Type baseType = viewType.BaseType;

            // Return the base framework View type that should be swapped
            if (baseType.Name == "ListView" ||
                baseType.Name == "ListItemView" ||
                baseType.Name == "PopupView" ||
                baseType.Name == "View")
            {
                return baseType;
            }

            return null;
        }

        private static void SwapComponent(GameObject go, Type oldType, Type newType)
        {
            Component oldComponent = go.GetComponent(oldType);
            if (oldComponent == null)
            {
                Debug.LogWarning($"Could not find component of type {oldType.Name} on {go.name}");
                go.AddComponent(newType);
                return;
            }

            // Add the new component first
            Component newComponent = go.AddComponent(newType);

            // Use SerializedObject to copy matching field values
            SerializedObject oldSO = new SerializedObject(oldComponent);
            SerializedObject newSO = new SerializedObject(newComponent);

            SerializedProperty oldProp = oldSO.GetIterator();
            int copiedFields = 0;

            // Iterate through all serialized properties of the old component
            while (oldProp.NextVisible(true))
            {
                // Skip the script reference
                if (oldProp.name == "m_Script")
                    continue;

                // Try to find the same property in the new component
                SerializedProperty newProp = newSO.FindProperty(oldProp.name);
                if (newProp != null && newProp.propertyType == oldProp.propertyType)
                {
                    try
                    {
                        newSO.CopyFromSerializedProperty(oldProp);
                        copiedFields++;
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Failed to copy field '{oldProp.name}': {e.Message}");
                    }
                }
            }

            // Apply the changes to the new component
            newSO.ApplyModifiedProperties();

            Debug.Log($"Swapped {oldType.Name} → {newType.Name} ({copiedFields} fields copied)");

            // Remove the old component
            UnityEngine.Object.DestroyImmediate(oldComponent);
        }

        private static void LinkListItemPrefab(string listPrefabPath, string itemPrefabPath)
        {
            // Load the item prefab asset
            GameObject itemPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(itemPrefabPath);
            if (itemPrefabAsset == null)
            {
                Debug.LogError($"Failed to load item prefab from {itemPrefabPath}");
                return;
            }

            // Load the list prefab contents
            GameObject listPrefabContents = PrefabUtility.LoadPrefabContents(listPrefabPath);
            if (listPrefabContents == null)
            {
                Debug.LogError($"Failed to load list prefab contents from {listPrefabPath}");
                return;
            }

            // Find the ListView component (or any component that extends it)
            Component viewComponent = listPrefabContents.GetComponent("ListView");
            if (viewComponent == null)
            {
                // Try to find any component that might extend ListView
                Component[] components = listPrefabContents.GetComponents<Component>();
                foreach (var component in components)
                {
                    if (component != null && component.GetType().BaseType?.Name == "ListView")
                    {
                        viewComponent = component;
                        break;
                    }
                }
            }

            if (viewComponent == null)
            {
                Debug.LogError($"Could not find ListView component on {listPrefabPath}");
                PrefabUtility.UnloadPrefabContents(listPrefabContents);
                return;
            }

            // Use SerializedObject to set the ItemPrefab field
            SerializedObject so = new SerializedObject(viewComponent);
            SerializedProperty itemPrefabProp = so.FindProperty("ItemPrefab");

            if (itemPrefabProp != null)
            {
                itemPrefabProp.objectReferenceValue = itemPrefabAsset;
                so.ApplyModifiedProperties();
                Debug.Log($"Linked ItemPrefab: {listPrefabPath} -> {itemPrefabPath}");
            }
            else
            {
                Debug.LogWarning($"ItemPrefab field not found on {viewComponent.GetType().Name}");
            }

            // Save the modified prefab
            PrefabUtility.SaveAsPrefabAsset(listPrefabContents, listPrefabPath);
            PrefabUtility.UnloadPrefabContents(listPrefabContents);
        }
    }
}
