using System.Collections.Generic;

namespace Plugins.Framewerk
{
    /// <summary>
    /// Shared utility for building Addressable IDs.
    /// Format: [ContextPrefix]/[CustomPrefix]/UI/[TypeKey]/[ClassName]
    /// Any part can be null/empty — it will be skipped.
    /// </summary>
    public static class AddressBuilder
    {
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
        /// </summary>
        /// <param name="viewConfig">ViewConfig containing ContextPrefixSO</param>
        /// <param name="customPrefix">Optional custom prefix passed at runtime</param>
        /// <param name="typeKey">Component type key</param>
        /// <param name="className">The class name</param>
        public static string BuildAddress(ViewConfig viewConfig, string customPrefix, string typeKey, string className)
        {
            string contextPrefix = viewConfig?.ContextPrefixSO?.Prefix;
            return BuildAddress(contextPrefix, customPrefix, typeKey, className);
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
