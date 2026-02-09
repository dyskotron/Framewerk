using UnityEngine;

namespace Plugins.Framewerk
{
    /// <summary>
    /// Project-level configuration for address pattern.
    /// Stored as an asset file so it's shared across the team.
    /// </summary>
    public class AddressResolverConfig : ScriptableObject
    {
        [Tooltip("Pattern template for address generation. Use {tokens} for variables.\n" +
                 "Available tokens: {ContextPrefix}, {CustomPrefix}, {UI}, {TypeKey}, {ClassName}")]
        public string Pattern = AddressBuilder.DEFAULT_PATTERN;
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
