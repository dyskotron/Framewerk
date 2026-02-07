using UnityEditor;

namespace Framewerk.Editor.Wizards
{
    /// <summary>
    /// Clears SkinResolver cache when SkinConfig assets are modified.
    /// </summary>
    public class SkinConfigPostprocessor : AssetPostprocessor
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
                if (IsSkinConfigPath(path))
                {
                    shouldClearCache = true;
                    break;
                }
            }

            if (!shouldClearCache)
            {
                foreach (string path in deletedAssets)
                {
                    if (IsSkinConfigPath(path))
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
                    if (IsSkinConfigPath(path))
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
                    if (IsSkinConfigPath(path))
                    {
                        shouldClearCache = true;
                        break;
                    }
                }
            }

            if (shouldClearCache)
            {
                SkinResolver.ClearCache();
            }
        }

        private static bool IsSkinConfigPath(string path)
        {
            return path.EndsWith("SkinConfig.asset") || path.Contains("SkinConfig");
        }
    }
}
