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

        public static GameObject CreatePrefab(string prefabPath, Type viewType)
        {
            if (viewType == null)
            {
                Debug.LogError($"Cannot create prefab: viewType is null");
                return null;
            }

            // Determine which template to use based on the view type
            string templatePath = GetTemplatePath(viewType);
            GameObject go;

            if (!string.IsNullOrEmpty(templatePath))
            {
                // Clone from template
                GameObject template = AssetDatabase.LoadAssetAtPath<GameObject>(templatePath);
                if (template != null)
                {
                    go = UnityEngine.Object.Instantiate(template);
                    go.name = viewType.Name.Replace("View", "");

                    // Swap the base View component with the new custom View
                    Type baseViewType = GetBaseViewType(viewType);
                    if (baseViewType != null)
                    {
                        SwapComponent(go, baseViewType, viewType);
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
                go = CreateEmptyPrefab(viewType);
            }

            // Ensure the directory exists
            string directory = System.IO.Path.GetDirectoryName(prefabPath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            // Save as prefab
            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);

            // Clean up the scene object
            UnityEngine.Object.DestroyImmediate(go);

            return prefabAsset;
        }

        private static GameObject CreateEmptyPrefab(Type viewType)
        {
            GameObject go = new GameObject(viewType.Name.Replace("View", ""));
            go.AddComponent(viewType);
            return go;
        }

        private static string GetTemplatePath(Type viewType)
        {
            if (viewType == null)
                return null;

            // Check the base type to determine which template to use
            Type baseType = viewType.BaseType;

            if (baseType.Name == "ListView")
                return TEMPLATE_PATH + "ListTemplate.prefab";
            else if (baseType.Name == "ListItemView")
                return TEMPLATE_PATH + "ListItemTemplate.prefab";
            else if (baseType.Name == "PopupView")
                return TEMPLATE_PATH + "PopupTemplate.prefab";
            else if (baseType.Name == "View")
                return TEMPLATE_PATH + "PanelTemplate.prefab";

            return null;
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

            // Use ComponentUtility to copy component values
            Component newComponent = go.AddComponent(newType);

            // Use Unity's built-in component copy utility which preserves references
            if (!ComponentUtility.CopyComponent(oldComponent))
            {
                Debug.LogError($"Failed to copy component {oldType.Name}");
            }

            if (!ComponentUtility.PasteComponentValues(newComponent))
            {
                Debug.LogError($"Failed to paste component values to {newType.Name}");
            }

            // Remove the old component
            UnityEngine.Object.DestroyImmediate(oldComponent);
        }
    }
}
