using UnityEditor;

namespace Framewerk.Editor.Wizards
{
    /// <summary>
    /// Clears TemplateSetResolver cache when TemplateSetConfig assets are modified.
    /// </summary>
    public class TemplateSetConfigPostprocessor : AssetPostprocessor
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
                if (IsTemplateSetConfigPath(path))
                {
                    shouldClearCache = true;
                    break;
                }
            }

            if (!shouldClearCache)
            {
                foreach (string path in deletedAssets)
                {
                    if (IsTemplateSetConfigPath(path))
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
                    if (IsTemplateSetConfigPath(path))
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
                    if (IsTemplateSetConfigPath(path))
                    {
                        shouldClearCache = true;
                        break;
                    }
                }
            }

            if (shouldClearCache)
            {
                TemplateSetResolver.ClearCache();
            }
        }

        private static bool IsTemplateSetConfigPath(string path)
        {
            return path.EndsWith("TemplateSetConfig.asset") || path.Contains("TemplateSetConfig");
        }
    }
}
