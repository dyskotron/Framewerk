using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Plugins.Framewerk
{
    /// <summary>
    /// ScriptableObject-based configuration for customizing Addressable ID generation.
    /// Uses pattern-based token replacement for flexible address formats.
    /// </summary>
    [CreateAssetMenu(fileName = "AddressResolverConfig", menuName = "Framewerk/Address Resolver Config")]
    public class AddressResolverConfig : ScriptableObject
    {
        [Tooltip("Pattern template for address generation. Use {tokens} for variables.\n" +
                 "Available tokens: {ContextPrefix}, {CustomPrefix}, {UI}, {TypeKey}, {ClassName}")]
        public string Pattern = "{ContextPrefix}/{CustomPrefix}/UI/{TypeKey}/{ClassName}";

        /// <summary>
        /// Resolves the pattern into an address using the provided context.
        /// </summary>
        public string Resolve(AddressContext ctx)
        {
            var tokens = new Dictionary<string, string>
            {
                { "ContextPrefix", ctx.ContextPrefix },
                { "CustomPrefix", ctx.CustomPrefix },
                { "TypeKey", ctx.TypeKey },
                { "ClassName", ctx.ClassName },
                { "UI", AddressBuilder.UI_ROOT }
            };

            // Replace tokens with their values
            string result = Regex.Replace(Pattern, @"\{(\w+)\}", match =>
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

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Test with sample values
            var sample = Resolve(new AddressContext
            {
                ContextPrefix = "Examples",
                CustomPrefix = "",
                TypeKey = "Popup",
                ClassName = "ExamplePopup"
            });
            Debug.Log($"[AddressResolverConfig] Preview: {sample}");

            if (!Pattern.Contains("{ClassName}"))
            {
                Debug.LogWarning("[AddressResolverConfig] Pattern should include {ClassName}");
            }
        }
#endif
    }

    /// <summary>
    /// Context data for address resolution.
    /// Contains all the variables that can be used in address patterns.
    /// </summary>
    public struct AddressContext
    {
        /// <summary>From ViewConfig.ContextPrefixSO.Prefix</summary>
        public string ContextPrefix;

        /// <summary>Runtime grouping prefix (optional)</summary>
        public string CustomPrefix;

        /// <summary>Component type key (Popup, List, etc.)</summary>
        public string TypeKey;

        /// <summary>The component class name</summary>
        public string ClassName;
    }
}
