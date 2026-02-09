using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Plugins.Framewerk
{
    /// <summary>
    /// Shared utility for building Addressable IDs.
    /// Format: [ContextPrefix]/[CustomPrefix]/UI/[TypeKey]/[ClassName]
    /// Any part can be null/empty — it will be skipped.
    /// 
    /// Supports custom patterns stored in a project config file.
    /// If no config exists, uses the default format.
    /// </summary>
    public static class AddressBuilder
    {
        /// <summary>
        /// Path where the address resolver config is stored.
        /// </summary>
        public const string CONFIG_PATH = "Assets/Settings/Framewerk/AddressResolverConfig.asset";

        /// <summary>
        /// Default pattern used when no custom pattern is set.
        /// </summary>
        public const string DEFAULT_PATTERN = "{ContextPrefix}/{CustomPrefix}/UI/{TypeKey}/{ClassName}";

        /// <summary>
        /// Hardcoded root segment present in all addresses.
        /// </summary>
        public const string UI_ROOT = "UI";

        /// <summary>
        /// TypeKey constants for different component types.
        /// </summary>
        public static class TypeKeys
        {
            public const string View = "";           // Base views have no TypeKey
            public const string Popup = "Popup";
            public const string List = "List";
            public const string ListItem = "List.ListItem";
            public const string Tabs = "Tabs";
            public const string TabItem = "Tabs.TabItem";
            public const string ViewStack = "ViewStack";
        }

        /// <summary>
        /// Gets the current address pattern from the project config file.
        /// Returns default pattern if no config exists.
        /// </summary>
        public static string GetPattern()
        {
#if UNITY_EDITOR
            var config = UnityEditor.AssetDatabase.LoadAssetAtPath<ScriptableObject>(CONFIG_PATH);
            if (config != null)
            {
                // Use reflection to access Pattern property (config is in Editor assembly)
                var patternProp = config.GetType().GetProperty("Pattern");
                if (patternProp != null)
                {
                    string pattern = patternProp.GetValue(config) as string;
                    if (!string.IsNullOrEmpty(pattern))
                        return pattern;
                }
            }
            return DEFAULT_PATTERN;
#else
            return DEFAULT_PATTERN;
#endif
        }

        /// <summary>
        /// Sets the address pattern in the project config file.
        /// Creates the config file if it doesn't exist.
        /// Pass null or empty to reset to default (deletes the config file).
        /// </summary>
        public static void SetPattern(string pattern)
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(pattern) || pattern == DEFAULT_PATTERN)
            {
                // Reset to default = delete the config file
                if (System.IO.File.Exists(CONFIG_PATH))
                {
                    UnityEditor.AssetDatabase.DeleteAsset(CONFIG_PATH);
                    UnityEditor.AssetDatabase.SaveAssets();
                }
            }
            else
            {
                // Ensure directory exists
                string directory = System.IO.Path.GetDirectoryName(CONFIG_PATH);
                if (!UnityEditor.AssetDatabase.IsValidFolder(directory))
                {
                    string[] parts = directory.Split('/');
                    string currentPath = parts[0];
                    for (int i = 1; i < parts.Length; i++)
                    {
                        string nextPath = currentPath + "/" + parts[i];
                        if (!UnityEditor.AssetDatabase.IsValidFolder(nextPath))
                        {
                            UnityEditor.AssetDatabase.CreateFolder(currentPath, parts[i]);
                        }
                        currentPath = nextPath;
                    }
                }

                // Load or create config
                var config = UnityEditor.AssetDatabase.LoadAssetAtPath<ScriptableObject>(CONFIG_PATH);
                if (config == null)
                {
                    // Create new config using the type from the Editor assembly
                    var configType = System.Type.GetType("Framewerk.Editor.Settings.AddressResolverConfig, Framewerk.Editor");
                    if (configType != null)
                    {
                        config = ScriptableObject.CreateInstance(configType);
                        UnityEditor.AssetDatabase.CreateAsset(config, CONFIG_PATH);
                    }
                }

                if (config != null)
                {
                    // Set pattern via reflection
                    var patternProp = config.GetType().GetProperty("Pattern");
                    if (patternProp != null)
                    {
                        patternProp.SetValue(config, pattern);
                        UnityEditor.EditorUtility.SetDirty(config);
                        UnityEditor.AssetDatabase.SaveAssets();
                    }
                }
            }
#endif
        }

        /// <summary>
        /// Resolves a pattern into an address using the provided context.
        /// </summary>
        public static string ResolvePattern(string pattern, AddressContext ctx)
        {
            var tokens = new Dictionary<string, string>
            {
                { "ContextPrefix", ctx.ContextPrefix },
                { "CustomPrefix", ctx.CustomPrefix },
                { "TypeKey", ctx.TypeKey },
                { "ClassName", ctx.ClassName },
                { "UI", UI_ROOT }
            };

            // Replace tokens with their values
            string result = Regex.Replace(pattern, @"\{(\w+)\}", match =>
            {
                string key = match.Groups[1].Value;
                if (tokens.TryGetValue(key, out string value))
                {
                    // Empty tokens are skipped; double separators cleaned up below
                    return value ?? "";
                }
                return match.Value; // Keep unknown tokens as-is
            });

            // Clean up multiple separators (must be "/" for Addressables tree view)
            result = Regex.Replace(result, "/+", "/");
            result = result.Trim('/');

            return result;
        }

        /// <summary>
        /// Builds an Addressable ID using the current pattern and context.
        /// This is the main entry point when using custom patterns.
        /// </summary>
        /// <param name="ctx">The address context containing all variables.</param>
        /// <returns>The full Addressable ID</returns>
        public static string BuildAddress(AddressContext ctx)
        {
            string pattern = GetPattern();
            return ResolvePattern(pattern, ctx);
        }

        /// <summary>
        /// Builds an Addressable ID from the given parts.
        /// Format: contextPrefix/customPrefix/UI/typeKey/className
        /// Any null/empty part is skipped.
        /// </summary>
        /// <param name="contextPrefix">From ViewConfig.ContextPrefixSO.Prefix</param>
        /// <param name="customPrefix">Passed at runtime (optional feature grouping)</param>
        /// <param name="typeKey">Component type key (Popup, List, etc.) - empty for base views</param>
        /// <param name="className">The class name (always required)</param>
        /// <returns>The full Addressable ID</returns>
        public static string BuildAddress(string contextPrefix, string customPrefix, string typeKey, string className)
        {
            var ctx = new AddressContext
            {
                ContextPrefix = contextPrefix,
                CustomPrefix = customPrefix,
                TypeKey = typeKey,
                ClassName = className
            };
            return BuildAddress(ctx);
        }

        /// <summary>
        /// Builds an Addressable ID using ViewConfig settings.
        /// </summary>
        /// <param name="viewConfig">ViewConfig containing ContextPrefixSO</param>
        /// <param name="customPrefix">Optional custom prefix passed at runtime</param>
        /// <param name="typeKey">Component type key</param>
        /// <param name="className">The class name</param>
        public static string BuildAddress(ViewConfig viewConfig, string customPrefix, string typeKey, string className)
        {
            var ctx = new AddressContext
            {
                ContextPrefix = viewConfig?.ContextPrefixSO?.Prefix,
                CustomPrefix = customPrefix,
                TypeKey = typeKey,
                ClassName = className
            };
            return BuildAddress(ctx);
        }

        /// <summary>
        /// Builds an Addressable ID using ViewConfig settings without custom prefix.
        /// </summary>
        public static string BuildAddress(ViewConfig viewConfig, string typeKey, string className)
        {
            return BuildAddress(viewConfig, null, typeKey, className);
        }
    }
}
