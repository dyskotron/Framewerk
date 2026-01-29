using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

[InitializeOnLoad]
public static class AddressablesPreBuildHook
{
    static AddressablesPreBuildHook()
    {
        BuildPlayerWindow.RegisterBuildPlayerHandler(BuildPlayerHandler);
    }

    private static void BuildPlayerHandler(BuildPlayerOptions options)
    {
        Debug.Log("[AddressablesPreBuildHook] Building Addressables before player build...");

        AddressableAssetSettings.BuildPlayerContent(out var result);

        if (!string.IsNullOrEmpty(result.Error))
        {
            Debug.LogError($"[AddressablesPreBuildHook] Addressables build failed: {result.Error}");
            return; // Don't proceed with player build if Addressables failed
        }

        Debug.Log($"[AddressablesPreBuildHook] Addressables build complete. Proceeding with player build.");
        BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
    }
}
