using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Plugins.Framewerk
{
    /// <summary>
    /// How to handle empty/null tokens in the address pattern.
    /// </summary>
    public enum EmptyTokenBehavior
    {
        /// <summary>Omit empty segments (current behavior).</summary>
        Skip,
        /// <summary>Keep empty string (may create double separators).</summary>
        KeepEmpty
    }

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

        [Tooltip("How to handle empty/null tokens.")]
        public EmptyTokenBehavior EmptyTokenBehavior = EmptyTokenBehavior.Skip;

        [Tooltip("Separator between path segments.")]
        public string Separator = "/";

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
                    if (string.IsNullOrEmpty(value) && EmptyTokenBehavior == EmptyTokenBehavior.Skip)
                        return ""; // Will create double separators, cleaned up below
                    return value ?? "";
                }
                return match.Value; // Keep unknown tokens as-is
            });

            // Clean up multiple separators
            if (!string.IsNullOrEmpty(Separator))
            {
                string escapedSep = Regex.Escape(Separator);
                result = Regex.Replace(result, $"{escapedSep}+", Separator);
                result = result.Trim(Separator[0]);
            }

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
