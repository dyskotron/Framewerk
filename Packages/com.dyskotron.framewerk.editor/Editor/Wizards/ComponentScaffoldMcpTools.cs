using System;
using System.IO;
using MCPForUnity.Editor.Helpers;
using MCPForUnity.Editor.Tools;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Framewerk.Editor.Wizards
{
    [McpForUnityTool("framewerk_scaffold")]
    /// <summary>
    /// MCP tool for creating Framewerk component prefabs (Popup, List) and marking them as addressable.
    /// </summary>
    public static class ComponentScaffoldMcpTools
    {
        private const string SupportedActions = "create_popup, create_list, create_tabs, create_view, create_viewstack, mark_addressable, create_scene";

        public static object HandleCommand(JObject @params)
        {
            if (@params == null)
            {
                return new ErrorResponse("Parameters cannot be null.");
            }

            string action = @params["action"]?.ToString()?.ToLowerInvariant();
            if (string.IsNullOrEmpty(action))
            {
                return new ErrorResponse($"Action parameter is required. Valid actions are: {SupportedActions}.");
            }

            try
            {
                switch (action)
                {
                    case "create_popup":
                        return CreatePopup(@params);
                    case "create_list":
                        return CreateList(@params);
                    case "create_tabs":
                        return CreateTabs(@params);
                    case "create_view":
                        return CreateView(@params);
                    case "create_viewstack":
                        return CreateViewStack(@params);
                    case "mark_addressable":
                        return MarkAddressable(@params);
                    case "create_scene":
                        return CreateScene(@params);
                    default:
                        return new ErrorResponse($"Unknown action: '{action}'. Valid actions are: {SupportedActions}.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ComponentScaffoldMcpTools] Action '{action}' failed: {e}");
                return new ErrorResponse($"Internal error: {e.Message}");
            }
        }

        private static object CreatePopup(JObject @params)
        {
            string name = @params["name"]?.ToString();
            if (string.IsNullOrEmpty(name))
            {
                return new ErrorResponse("'name' parameter is required for create_popup.");
            }

            string namespaceName = @params["namespace"]?.ToString();
            if (string.IsNullOrEmpty(namespaceName))
            {
                return new ErrorResponse("'namespace' parameter is required for create_popup.");
            }

            string viewTypeName = @params["viewTypeName"]?.ToString();
            if (string.IsNullOrEmpty(viewTypeName))
            {
                return new ErrorResponse("'viewTypeName' parameter is required for create_popup.");
            }

            string prefabFolder = @params["prefabFolder"]?.ToString();
            if (string.IsNullOrEmpty(prefabFolder))
            {
                return new ErrorResponse("'prefabFolder' parameter is required for create_popup.");
            }

            bool overwrite = @params["overwrite"]?.ToObject<bool>() ?? false;

            // Ensure prefab folder exists
            EnsureDirectoryExists(prefabFolder);

            // Build prefab path
            string prefabPath = Path.Combine(prefabFolder, $"{name}.prefab").Replace("\\", "/");

            // Check if prefab already exists
            if (!overwrite && File.Exists(prefabPath))
            {
                return new ErrorResponse(
                    $"Prefab already exists at '{prefabPath}'. Set overwrite=true to replace it.",
                    new { prefabPath }
                );
            }

            // Find the view type to ensure it exists
            Type viewType = ComponentScaffoldCompleter.FindType(viewTypeName);
            if (viewType == null)
            {
                return new ErrorResponse($"Could not find type: {viewTypeName}");
            }

            // Get template path
            string templatePath = GetTemplatePath(ComponentType.Popup);
            if (string.IsNullOrEmpty(templatePath))
            {
                return new ErrorResponse("Popup template not found.");
            }

            // Copy template to target path
            if (!AssetDatabase.CopyAsset(templatePath, prefabPath))
            {
                return new ErrorResponse($"Failed to copy template from {templatePath} to {prefabPath}");
            }

            // Swap the base View component with the custom View
            try
            {
                ProcessPrefabSwap(prefabPath, viewTypeName, name);
            }
            catch (Exception e)
            {
                return new ErrorResponse($"Failed to swap component: {e.Message}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return new SuccessResponse(
                $"Popup prefab created successfully at '{prefabPath}'.",
                new { prefabPath, success = true }
            );
        }

        private static object CreateView(JObject @params)
        {
            string name = @params["name"]?.ToString();
            if (string.IsNullOrEmpty(name))
            {
                return new ErrorResponse("'name' parameter is required for create_view.");
            }

            string namespaceName = @params["namespace"]?.ToString();
            if (string.IsNullOrEmpty(namespaceName))
            {
                return new ErrorResponse("'namespace' parameter is required for create_view.");
            }

            string viewTypeName = @params["viewTypeName"]?.ToString();
            if (string.IsNullOrEmpty(viewTypeName))
            {
                return new ErrorResponse("'viewTypeName' parameter is required for create_view.");
            }

            string prefabFolder = @params["prefabFolder"]?.ToString();
            if (string.IsNullOrEmpty(prefabFolder))
            {
                return new ErrorResponse("'prefabFolder' parameter is required for create_view.");
            }

            bool overwrite = @params["overwrite"]?.ToObject<bool>() ?? false;

            // Ensure prefab folder exists
            EnsureDirectoryExists(prefabFolder);

            // Build prefab path
            string prefabPath = Path.Combine(prefabFolder, $"{name}.prefab").Replace("\\", "/");

            // Check if prefab already exists
            if (!overwrite && File.Exists(prefabPath))
            {
                return new ErrorResponse(
                    $"Prefab already exists at '{prefabPath}'. Set overwrite=true to replace it.",
                    new { prefabPath }
                );
            }

            // Find the view type to ensure it exists
            Type viewType = ComponentScaffoldCompleter.FindType(viewTypeName);
            if (viewType == null)
            {
                return new ErrorResponse($"Could not find type: {viewTypeName}");
            }

            // Get template path
            string templatePath = GetTemplatePath(ComponentType.View);
            if (string.IsNullOrEmpty(templatePath))
            {
                return new ErrorResponse("View template not found.");
            }

            // Copy template to target path
            if (!AssetDatabase.CopyAsset(templatePath, prefabPath))
            {
                return new ErrorResponse($"Failed to copy template from {templatePath} to {prefabPath}");
            }

            // Swap the base View component with the custom View
            try
            {
                ProcessPrefabSwap(prefabPath, viewTypeName, name);
            }
            catch (Exception e)
            {
                return new ErrorResponse($"Failed to swap component: {e.Message}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return new SuccessResponse(
                $"View prefab created successfully at '{prefabPath}'.",
                new { prefabPath, success = true }
            );
        }

        private static object CreateViewStack(JObject @params)
        {
            string name = @params["name"]?.ToString();
            if (string.IsNullOrEmpty(name))
            {
                return new ErrorResponse("'name' parameter is required for create_viewstack.");
            }

            string namespaceName = @params["namespace"]?.ToString();
            if (string.IsNullOrEmpty(namespaceName))
            {
                return new ErrorResponse("'namespace' parameter is required for create_viewstack.");
            }

            string viewTypeName = @params["viewTypeName"]?.ToString();
            if (string.IsNullOrEmpty(viewTypeName))
            {
                return new ErrorResponse("'viewTypeName' parameter is required for create_viewstack.");
            }

            string prefabFolder = @params["prefabFolder"]?.ToString();
            if (string.IsNullOrEmpty(prefabFolder))
            {
                return new ErrorResponse("'prefabFolder' parameter is required for create_viewstack.");
            }

            bool overwrite = @params["overwrite"]?.ToObject<bool>() ?? false;

            // Ensure prefab folder exists
            EnsureDirectoryExists(prefabFolder);

            // Build prefab path
            string prefabPath = Path.Combine(prefabFolder, $"{name}.prefab").Replace("\\", "/");

            // Check if prefab already exists
            if (!overwrite && File.Exists(prefabPath))
            {
                return new ErrorResponse(
                    $"Prefab already exists at '{prefabPath}'. Set overwrite=true to replace it.",
                    new { prefabPath }
                );
            }

            // Find the view type to ensure it exists
            Type viewType = ComponentScaffoldCompleter.FindType(viewTypeName);
            if (viewType == null)
            {
                return new ErrorResponse($"Could not find type: {viewTypeName}");
            }

            // Get template path
            string templatePath = GetTemplatePath(ComponentType.ViewStack);
            if (string.IsNullOrEmpty(templatePath))
            {
                return new ErrorResponse("ViewStack template not found.");
            }

            // Copy template to target path
            if (!AssetDatabase.CopyAsset(templatePath, prefabPath))
            {
                return new ErrorResponse($"Failed to copy template from {templatePath} to {prefabPath}");
            }

            // Swap the base ViewStackView component with the custom View
            try
            {
                ProcessPrefabSwap(prefabPath, viewTypeName, name);
            }
            catch (Exception e)
            {
                return new ErrorResponse($"Failed to swap component: {e.Message}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return new SuccessResponse(
                $"ViewStack prefab created successfully at '{prefabPath}'.",
                new { prefabPath, success = true }
            );
        }

        private static object CreateList(JObject @params)
        {
            string name = @params["name"]?.ToString();
            if (string.IsNullOrEmpty(name))
            {
                return new ErrorResponse("'name' parameter is required for create_list.");
            }

            string namespaceName = @params["namespace"]?.ToString();
            if (string.IsNullOrEmpty(namespaceName))
            {
                return new ErrorResponse("'namespace' parameter is required for create_list.");
            }

            string viewTypeName = @params["viewTypeName"]?.ToString();
            if (string.IsNullOrEmpty(viewTypeName))
            {
                return new ErrorResponse("'viewTypeName' parameter is required for create_list.");
            }

            string itemViewTypeName = @params["itemViewTypeName"]?.ToString();
            if (string.IsNullOrEmpty(itemViewTypeName))
            {
                return new ErrorResponse("'itemViewTypeName' parameter is required for create_list.");
            }

            string prefabFolder = @params["prefabFolder"]?.ToString();
            if (string.IsNullOrEmpty(prefabFolder))
            {
                return new ErrorResponse("'prefabFolder' parameter is required for create_list.");
            }

            bool overwrite = @params["overwrite"]?.ToObject<bool>() ?? false;

            // Ensure prefab folder exists
            EnsureDirectoryExists(prefabFolder);

            // Build prefab paths
            string listPrefabPath = Path.Combine(prefabFolder, $"{name}.prefab").Replace("\\", "/");
            string itemPrefabPath = Path.Combine(prefabFolder, $"{name}Item.prefab").Replace("\\", "/");

            // Check if prefabs already exist
            if (!overwrite)
            {
                if (File.Exists(listPrefabPath))
                {
                    return new ErrorResponse(
                        $"List prefab already exists at '{listPrefabPath}'. Set overwrite=true to replace it.",
                        new { prefabPath = listPrefabPath }
                    );
                }
                if (File.Exists(itemPrefabPath))
                {
                    return new ErrorResponse(
                        $"Item prefab already exists at '{itemPrefabPath}'. Set overwrite=true to replace it.",
                        new { prefabPath = itemPrefabPath }
                    );
                }
            }

            // Find the view types to ensure they exist
            Type listViewType = ComponentScaffoldCompleter.FindType(viewTypeName);
            if (listViewType == null)
            {
                return new ErrorResponse($"Could not find type: {viewTypeName}");
            }

            Type itemViewType = ComponentScaffoldCompleter.FindType(itemViewTypeName);
            if (itemViewType == null)
            {
                return new ErrorResponse($"Could not find type: {itemViewTypeName}");
            }

            // Get template paths
            string listTemplatePath = GetTemplatePath(ComponentType.List);
            string itemTemplatePath = GetTemplatePath(ComponentType.ListItem);

            if (string.IsNullOrEmpty(listTemplatePath) || string.IsNullOrEmpty(itemTemplatePath))
            {
                return new ErrorResponse("List or ListItem template not found.");
            }

            // Copy templates
            if (!AssetDatabase.CopyAsset(listTemplatePath, listPrefabPath))
            {
                return new ErrorResponse($"Failed to copy list template from {listTemplatePath} to {listPrefabPath}");
            }

            if (!AssetDatabase.CopyAsset(itemTemplatePath, itemPrefabPath))
            {
                return new ErrorResponse($"Failed to copy item template from {itemTemplatePath} to {itemPrefabPath}");
            }

            // Swap components
            try
            {
                ProcessPrefabSwap(listPrefabPath, viewTypeName, name);
                ProcessPrefabSwap(itemPrefabPath, itemViewTypeName, name + "Item");
            }
            catch (Exception e)
            {
                return new ErrorResponse($"Failed to swap components: {e.Message}");
            }

            // Link the List's ItemPrefab field to the ListItem prefab
            try
            {
                LinkListItemPrefab(listPrefabPath, itemPrefabPath);
            }
            catch (Exception e)
            {
                return new ErrorResponse($"Failed to link item prefab: {e.Message}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return new SuccessResponse(
                $"List prefabs created successfully at '{listPrefabPath}' and '{itemPrefabPath}'.",
                new {
                    listPrefabPath,
                    itemPrefabPath,
                    success = true
                }
            );
        }

        private static object CreateTabs(JObject @params)
        {
            string name = @params["name"]?.ToString();
            if (string.IsNullOrEmpty(name))
            {
                return new ErrorResponse("'name' parameter is required for create_tabs.");
            }

            string namespaceName = @params["namespace"]?.ToString();
            if (string.IsNullOrEmpty(namespaceName))
            {
                return new ErrorResponse("'namespace' parameter is required for create_tabs.");
            }

            string viewTypeName = @params["viewTypeName"]?.ToString();
            if (string.IsNullOrEmpty(viewTypeName))
            {
                return new ErrorResponse("'viewTypeName' parameter is required for create_tabs.");
            }

            string itemViewTypeName = @params["itemViewTypeName"]?.ToString();
            if (string.IsNullOrEmpty(itemViewTypeName))
            {
                return new ErrorResponse("'itemViewTypeName' parameter is required for create_tabs.");
            }

            string prefabFolder = @params["prefabFolder"]?.ToString();
            if (string.IsNullOrEmpty(prefabFolder))
            {
                return new ErrorResponse("'prefabFolder' parameter is required for create_tabs.");
            }

            // orientation: "horizontal" (default) or "vertical"
            string orientation = @params["orientation"]?.ToString()?.ToLowerInvariant() ?? "horizontal";
            ComponentType componentType = orientation == "vertical" 
                ? ComponentType.VerticalTabs 
                : ComponentType.HorizontalTabs;

            bool overwrite = @params["overwrite"]?.ToObject<bool>() ?? false;

            // Ensure prefab folder exists
            EnsureDirectoryExists(prefabFolder);

            // Build prefab paths
            string tabsPrefabPath = Path.Combine(prefabFolder, $"{name}.prefab").Replace("\\", "/");
            string itemPrefabPath = Path.Combine(prefabFolder, $"{name}Item.prefab").Replace("\\", "/");

            // Check if prefabs already exist
            if (!overwrite)
            {
                if (File.Exists(tabsPrefabPath))
                {
                    return new ErrorResponse(
                        $"Tabs prefab already exists at '{tabsPrefabPath}'. Set overwrite=true to replace it.",
                        new { prefabPath = tabsPrefabPath }
                    );
                }
                if (File.Exists(itemPrefabPath))
                {
                    return new ErrorResponse(
                        $"Tab item prefab already exists at '{itemPrefabPath}'. Set overwrite=true to replace it.",
                        new { prefabPath = itemPrefabPath }
                    );
                }
            }

            // Find the view types to ensure they exist
            Type tabsViewType = ComponentScaffoldCompleter.FindType(viewTypeName);
            if (tabsViewType == null)
            {
                return new ErrorResponse($"Could not find type: {viewTypeName}");
            }

            Type itemViewType = ComponentScaffoldCompleter.FindType(itemViewTypeName);
            if (itemViewType == null)
            {
                return new ErrorResponse($"Could not find type: {itemViewTypeName}");
            }

            // Get template paths based on orientation
            string tabsTemplatePath = TemplateSetResolver.GetDefaultTemplatePath(componentType);
            string itemTemplatePath = TemplateSetResolver.GetDefaultTabItemTemplatePath(componentType);

            if (string.IsNullOrEmpty(tabsTemplatePath) || string.IsNullOrEmpty(itemTemplatePath))
            {
                return new ErrorResponse($"Tabs or TabItem template not found for {orientation} orientation.");
            }

            // Copy templates
            if (!AssetDatabase.CopyAsset(tabsTemplatePath, tabsPrefabPath))
            {
                return new ErrorResponse($"Failed to copy tabs template from {tabsTemplatePath} to {tabsPrefabPath}");
            }

            if (!AssetDatabase.CopyAsset(itemTemplatePath, itemPrefabPath))
            {
                return new ErrorResponse($"Failed to copy tab item template from {itemTemplatePath} to {itemPrefabPath}");
            }

            // Swap components
            try
            {
                ProcessPrefabSwap(tabsPrefabPath, viewTypeName, name);
                ProcessPrefabSwap(itemPrefabPath, itemViewTypeName, name + "Item");
            }
            catch (Exception e)
            {
                return new ErrorResponse($"Failed to swap components: {e.Message}");
            }

            // Link the Tabs' ItemPrefab field to the TabItem prefab
            try
            {
                LinkListItemPrefab(tabsPrefabPath, itemPrefabPath);
            }
            catch (Exception e)
            {
                return new ErrorResponse($"Failed to link tab item prefab: {e.Message}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return new SuccessResponse(
                $"Tabs prefabs created successfully at '{tabsPrefabPath}' and '{itemPrefabPath}' ({orientation} orientation).",
                new {
                    tabsPrefabPath,
                    itemPrefabPath,
                    orientation,
                    success = true
                }
            );
        }

        private static object MarkAddressable(JObject @params)
        {
            string prefabPath = @params["prefabPath"]?.ToString();
            if (string.IsNullOrEmpty(prefabPath))
            {
                return new ErrorResponse("'prefabPath' parameter is required for mark_addressable.");
            }

            // Check if prefab exists
            if (!File.Exists(prefabPath))
            {
                return new ErrorResponse($"Prefab not found at '{prefabPath}'.");
            }

            // Get or generate address
            string address = @params["address"]?.ToString();
            if (string.IsNullOrEmpty(address))
            {
                // Default to UI/{prefabName}
                string prefabName = Path.GetFileNameWithoutExtension(prefabPath);
                address = $"UI/{prefabName}";
            }

            try
            {
                AddressableHelper.MarkAsAddressable(prefabPath, address);
                AssetDatabase.SaveAssets();
            }
            catch (Exception e)
            {
                return new ErrorResponse($"Failed to mark as addressable: {e.Message}");
            }

            return new SuccessResponse(
                $"Prefab marked as addressable with address '{address}'.",
                new { address, prefabPath, success = true }
            );
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            string fullPath = Path.Combine(Application.dataPath, "..", path).Replace("\\", "/");
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            }
        }

        private static string GetTemplatePath(ComponentType componentType)
        {
            const string TEMPLATE_PATH = "Packages/com.dyskotron.framewerk.editor/Editor/Wizards/Templates/";

            switch (componentType)
            {
                case ComponentType.List:
                    return TEMPLATE_PATH + "ListTemplate.prefab";
                case ComponentType.ListItem:
                    return TEMPLATE_PATH + "ListItemTemplate.prefab";
                case ComponentType.Popup:
                    return TEMPLATE_PATH + "PopupTemplate.prefab";
                case ComponentType.View:
                    return TEMPLATE_PATH + "ViewTemplate.prefab";
                case ComponentType.ViewStack:
                    return TEMPLATE_PATH + "ViewStackTemplate.prefab";
                default:
                    return null;
            }
        }

        private static void ProcessPrefabSwap(string prefabPath, string viewTypeName, string prefabName)
        {
            Type viewType = ComponentScaffoldCompleter.FindType(viewTypeName);
            if (viewType == null)
            {
                throw new Exception($"Could not find type: {viewTypeName}");
            }

            // Load prefab contents
            GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabContents == null)
            {
                throw new Exception($"Failed to load prefab contents from {prefabPath}");
            }

            try
            {
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
            }
            finally
            {
                // Unload the prefab contents (cleanup)
                PrefabUtility.UnloadPrefabContents(prefabContents);
            }

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
                baseType.Name == "ViewStackView" ||
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
                throw new Exception($"Failed to load item prefab from {itemPrefabPath}");
            }

            // Load the list prefab contents
            GameObject listPrefabContents = PrefabUtility.LoadPrefabContents(listPrefabPath);
            if (listPrefabContents == null)
            {
                throw new Exception($"Failed to load list prefab contents from {listPrefabPath}");
            }

            try
            {
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
                    throw new Exception($"Could not find ListView component on {listPrefabPath}");
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
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(listPrefabContents);
            }
        }

        private static object CreateScene(JObject @params)
        {
            string sceneName = @params["sceneName"]?.ToString();
            if (string.IsNullOrEmpty(sceneName))
            {
                return new ErrorResponse("'sceneName' parameter is required for create_scene.");
            }

            string namespaceName = @params["namespace"]?.ToString();
            if (string.IsNullOrEmpty(namespaceName))
            {
                return new ErrorResponse("'namespace' parameter is required for create_scene.");
            }

            string sceneFolder = @params["sceneFolder"]?.ToString();
            if (string.IsNullOrEmpty(sceneFolder))
            {
                return new ErrorResponse("'sceneFolder' parameter is required for create_scene.");
            }

            string scriptFolder = @params["scriptFolder"]?.ToString();
            if (string.IsNullOrEmpty(scriptFolder))
            {
                return new ErrorResponse("'scriptFolder' parameter is required for create_scene.");
            }

            // Ensure folders exist
            EnsureDirectoryExists(sceneFolder);
            EnsureDirectoryExists(scriptFolder);

            // Build paths
            string scenePath = Path.Combine(sceneFolder, $"{sceneName}.unity").Replace("\\", "/");
            string bootstrapPath = Path.Combine(scriptFolder, $"{sceneName}Bootstrap.cs").Replace("\\", "/");
            string contextPath = Path.Combine(scriptFolder, $"{sceneName}Context.cs").Replace("\\", "/");
            string startCommandPath = Path.Combine(scriptFolder, $"{sceneName}StartCommand.cs").Replace("\\", "/");

            // Check for existing files
            if (File.Exists(scenePath))
            {
                return new ErrorResponse($"Scene already exists at '{scenePath}'.");
            }

            if (File.Exists(bootstrapPath) || File.Exists(contextPath) || File.Exists(startCommandPath))
            {
                return new ErrorResponse($"One or more script files already exist for {sceneName}.");
            }

            // Load templates
            string templatePath = "Packages/com.dyskotron.framewerk.editor/Editor/Wizards/Templates/";
            string bootstrapTemplate = File.ReadAllText(templatePath + "Bootstrap.cs.txt");
            string contextTemplate = File.ReadAllText(templatePath + "Context.cs.txt");
            string startCommandTemplate = File.ReadAllText(templatePath + "StartCommand.cs.txt");

            // Replace placeholders
            string bootstrapCode = bootstrapTemplate
                .Replace("#SCENENAME#", sceneName)
                .Replace("#NAMESPACE#", namespaceName);
            string contextCode = contextTemplate
                .Replace("#SCENENAME#", sceneName)
                .Replace("#NAMESPACE#", namespaceName);
            string startCommandCode = startCommandTemplate
                .Replace("#SCENENAME#", sceneName)
                .Replace("#NAMESPACE#", namespaceName);

            // Write scripts
            File.WriteAllText(bootstrapPath, bootstrapCode);
            File.WriteAllText(contextPath, contextCode);
            File.WriteAllText(startCommandPath, startCommandCode);

            // Copy scene template
            string sceneTemplatePath = "Packages/com.dyskotron.framewerk.editor/Editor/Wizards/Templates/SceneTemplate.unity";
            if (!File.Exists(sceneTemplatePath))
            {
                return new ErrorResponse("SceneTemplate.unity not found in Templates folder.");
            }

            File.Copy(sceneTemplatePath, scenePath);

            // Create SceneJob for post-domain reload setup
            SceneJob job = new SceneJob
            {
                sceneName = sceneName,
                namespaceName = namespaceName,
                sceneFolder = sceneFolder,
                scriptFolder = scriptFolder,
                scenePath = scenePath,
                bootstrapTypeName = $"{namespaceName}.{sceneName}Bootstrap",
                contextTypeName = $"{namespaceName}.{sceneName}Context",
                startCommandTypeName = $"{namespaceName}.{sceneName}StartCommand"
            };

            // Save job to EditorPrefs
            string json = JsonUtility.ToJson(job);
            EditorPrefs.SetString("FramewerkWizard_PendingSceneJob", json);

            // Refresh to trigger recompile
            AssetDatabase.Refresh();

            Debug.Log($"Generated scene and scripts for {sceneName}. Waiting for recompile to complete scene setup...");

            return new SuccessResponse(
                $"Scene '{sceneName}' created successfully. Scripts generated, waiting for compile...",
                new {
                    scenePath,
                    bootstrapPath,
                    contextPath,
                    startCommandPath,
                    success = true
                }
            );
        }
    }
}
