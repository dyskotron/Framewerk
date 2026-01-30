using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Framewerk.Editor.Wizards
{
    /// <summary>
    /// Generates prefabs from template prefabs by cloning and swapping the base View component
    /// with the generated View script. Unity preserves serialized field references when
    /// field names match between the old and new component.
    /// </summary>
    public static class PrefabGenerator
    {
        private const string TEMPLATE_PATH = "Packages/com.dyskotron.framewerk/Editor/Wizards/Templates/";

        public static GameObject CreatePrefab(string prefabPath, Type viewType, ComponentType componentType)
        {
            if (viewType == null)
            {
                Debug.LogError($"Cannot create prefab: viewType is null");
                return null;
            }

            // Determine which template to use based on the component type
            string templatePath = GetTemplatePath(componentType);
            GameObject go;

            if (!string.IsNullOrEmpty(templatePath))
            {
                // Clone from template
                GameObject template = AssetDatabase.LoadAssetAtPath<GameObject>(templatePath);
                if (template != null)
                {
                    Debug.Log($"Loading template from {templatePath} for view type {viewType.Name}");
                    go = UnityEngine.Object.Instantiate(template);
                    go.name = viewType.Name.Replace("View", "");

                    // Swap the base View component with the new custom View
                    Type baseViewType = GetBaseViewType(viewType);
                    if (baseViewType != null)
                    {
                        Debug.Log($"Swapping component: {baseViewType.Name} → {viewType.Name}");
                        SwapComponent(go, baseViewType, viewType);
                    }
                    else
                    {
                        Debug.LogWarning($"No base view type found for {viewType.Name}, adding component without swap");
                        go.AddComponent(viewType);
                    }
                }
                else
                {
                    Debug.LogWarning($"Template not found at {templatePath}, creating empty GameObject");
                    go = CreateEmptyPrefab(viewType);
                }
            }
            else
            {
                // No template available, create empty GameObject
                Debug.Log($"No template found for {viewType.Name}, creating empty GameObject");
                go = CreateEmptyPrefab(viewType);
            }

            // Save as prefab
            try
            {
                // Ensure the directory exists before saving
                string directory = System.IO.Path.GetDirectoryName(prefabPath);
                if (!System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                    Debug.Log($"Created directory: {directory}");
                }

                GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);

                if (prefabAsset == null)
                {
                    Debug.LogError($"Failed to create prefab at {prefabPath} - SaveAsPrefabAsset returned null");
                }
                else
                {
                    Debug.Log($"Successfully created prefab at {prefabPath}");
                }

                // Clean up the scene object
                UnityEngine.Object.DestroyImmediate(go);

                return prefabAsset;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception while creating prefab at {prefabPath}: {e.Message}\n{e.StackTrace}");

                // Clean up the scene object even on error
                UnityEngine.Object.DestroyImmediate(go);

                return null;
            }
        }

        private static GameObject CreateEmptyPrefab(Type viewType)
        {
            GameObject go = new GameObject(viewType.Name.Replace("View", ""));
            go.AddComponent(viewType);
            return go;
        }

        private static string GetTemplatePath(ComponentType componentType)
        {
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
                default:
                    return null;
            }
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
                // Just add the new component
                go.AddComponent(newType);
                return;
            }

            // Add the new component first
            Component newComponent = go.AddComponent(newType);

            // Use SerializedObject to copy matching field values
            // This preserves references to child objects and other serialized data
            SerializedObject oldSO = new SerializedObject(oldComponent);
            SerializedObject newSO = new SerializedObject(newComponent);

            SerializedProperty oldProp = oldSO.GetIterator();
            int copiedFields = 0;
            int failedFields = 0;

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
                        // Copy the property value
                        newSO.CopyFromSerializedProperty(oldProp);
                        copiedFields++;
                        Debug.Log($"Copied field '{oldProp.name}' from {oldType.Name} to {newType.Name}");
                    }
                    catch (System.Exception e)
                    {
                        failedFields++;
                        Debug.LogWarning($"Failed to copy field '{oldProp.name}': {e.Message}");
                    }
                }
            }

            // Apply the changes to the new component
            newSO.ApplyModifiedProperties();

            Debug.Log($"Component swap complete: {copiedFields} fields copied, {failedFields} failed. " +
                     $"Swapped {oldType.Name} → {newType.Name} on {go.name}");

            // Remove the old component
            UnityEngine.Object.DestroyImmediate(oldComponent);
        }
    }
}
