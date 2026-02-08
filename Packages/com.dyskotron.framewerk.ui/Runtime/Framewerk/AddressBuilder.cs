using System.Collections.Generic;
using UnityEngine;

namespace Plugins.Framewerk
{
    /// <summary>
    /// Shared utility for building Addressable IDs.
    /// Format: [ContextPrefix]/[CustomPrefix]/UI/[TypeKey]/[ClassName]
    /// Any part can be null/empty — it will be skipped.
    /// 
    /// Supports optional AddressResolverConfig for custom patterns.
    /// If no resolver is provided, uses the default format.
    /// </summary>
    public static class AddressBuilder
    {
        /// <summary>
        /// Conventional path where AddressResolverConfig is stored.
        /// </summary>
        public const string RESOLVER_CONFIG_PATH = "Assets/Settings/Framewerk/AddressResolverConfig.asset";

        /// <summary>
        /// Hardcoded root segment present in all addresses.
        /// </summary>
        public const string UI_ROOT = "UI";

        private static AddressResolverConfig _cachedResolver;
        private static bool _resolverCacheInitialized;

        /// <summary>
        /// Gets the project-wide AddressResolverConfig from the conventional path.
        /// Returns null if no config exists (uses default behavior).
        /// </summary>
        public static AddressResolverConfig GetResolver()
        {
            if (_resolverCacheInitialized)
                return _cachedResolver;

            _resolverCacheInitialized = true;

#if UNITY_EDITOR
            // In editor, load from AssetDatabase
            _cachedResolver = UnityEditor.AssetDatabase.LoadAssetAtPath<AddressResolverConfig>(RESOLVER_CONFIG_PATH);
#else
            // At runtime, load from Resources if available
            // Note: For runtime support, the config would need to be in a Resources folder
            // or loaded via Addressables. For now, runtime uses default behavior.
            _cachedResolver = null;
#endif

            return _cachedResolver;
        }

        /// <summary>
        /// Clears the cached resolver reference.
        /// Called when assets change in editor.
        /// </summary>
        public static void ClearCache()
        {
            _cachedResolver = null;
            _resolverCacheInitialized = false;
        }

        /// <summary>
        /// TypeKey constants for different component types.
        /// </summary>
        public static class TypeKeys
        {
            public const string View = "";           // Base views have no TypeKey
            public const string Popup = "Popup";
            public const string List = "List";
            public const string ListItem = "List.ListItem";
        }

        /// <summary>
        /// Builds an Addressable ID using a resolver and context.
        /// This is the main entry point when using custom patterns.
        /// </summary>
        /// <param name="resolver">Optional resolver config. If null, uses default behavior.</param>
        /// <param name="ctx">The address context containing all variables.</param>
        /// <returns>The full Addressable ID</returns>
        public static string BuildAddress(AddressResolverConfig resolver, AddressContext ctx)
        {
            if (resolver == null)
                return BuildAddressDefault(ctx);

            return resolver.Resolve(ctx);
        }

        /// <summary>
        /// Default address building (no custom resolver).
        /// Format: contextPrefix/customPrefix/UI/typeKey/className
        /// </summary>
        private static string BuildAddressDefault(AddressContext ctx)
        {
            return BuildAddress(ctx.ContextPrefix, ctx.CustomPrefix, ctx.TypeKey, ctx.ClassName);
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
            var parts = new List<string>();

            // Add context prefix if present
            if (!string.IsNullOrEmpty(contextPrefix))
                parts.Add(contextPrefix.Trim('/'));

            // Add custom prefix if present
            if (!string.IsNullOrEmpty(customPrefix))
                parts.Add(customPrefix.Trim('/'));

            // Always add UI root
            parts.Add(UI_ROOT);

            // Add type key if present
            if (!string.IsNullOrEmpty(typeKey))
                parts.Add(typeKey.Trim('/'));

            // Always add class name
            if (!string.IsNullOrEmpty(className))
                parts.Add(className.Trim('/'));

            return string.Join("/", parts);
        }

        /// <summary>
        /// Builds an Addressable ID using ViewConfig settings.
        /// Uses the project-wide resolver from conventional path if available.
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

            var resolver = GetResolver();
            return BuildAddress(resolver, ctx);
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
