using System;
using UnityEngine;
using UnityEditor;

namespace Framewerk.Editor.Wizards
{
    public static class PrefabGenerator
    {
        public static GameObject CreatePrefab(string prefabPath, Type viewType)
        {
            if (viewType == null)
            {
                Debug.LogError($"Cannot create prefab: viewType is null");
                return null;
            }

            // Create empty GameObject
            GameObject go = new GameObject(viewType.Name.Replace("View", ""));

            // Add the View component
            go.AddComponent(viewType);

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
    }
}
