using UnityEditor;
using Plugins.Framewerk;

namespace Framewerk.Editor.Settings
{
    /// <summary>
    /// Clears AddressBuilder cache when AddressResolverConfig assets are modified.
    /// </summary>
    public class AddressResolverConfigPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            bool shouldClearCache = false;

            foreach (string path in importedAssets)
            {
                if (IsAddressResolverConfigPath(path))
                {
                    shouldClearCache = true;
                    break;
                }
            }

            if (!shouldClearCache)
            {
                foreach (string path in deletedAssets)
                {
                    if (IsAddressResolverConfigPath(path))
                    {
                        shouldClearCache = true;
                        break;
                    }
                }
            }

            if (!shouldClearCache)
            {
                foreach (string path in movedAssets)
                {
                    if (IsAddressResolverConfigPath(path))
                    {
                        shouldClearCache = true;
                        break;
                    }
                }
            }

            if (!shouldClearCache)
            {
                foreach (string path in movedFromAssetPaths)
                {
                    if (IsAddressResolverConfigPath(path))
                    {
                        shouldClearCache = true;
                        break;
                    }
                }
            }

            if (shouldClearCache)
            {
                AddressBuilder.ClearCache();
            }
        }

        private static bool IsAddressResolverConfigPath(string path)
        {
            return path.EndsWith("AddressResolverConfig.asset") || path.Contains("AddressResolverConfig");
        }
    }
}
