using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEngine;

public static class AddressablesBuildScript
{
    [MenuItem("Framewerk/Build Addressables")]
    public static void Build()
    {
        Debug.Log("[AddressablesBuild] Starting Addressables build...");
        AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);

        if (!string.IsNullOrEmpty(result.Error))
            Debug.LogError($"[AddressablesBuild] Build failed: {result.Error}");
        else
            Debug.Log($"[AddressablesBuild] Build complete! Output: {result.OutputPath}");
    }

    [MenuItem("Framewerk/Setup + Build Addressables")]
    public static void SetupAndBuild()
    {
        AddressablesSetup.Setup();
        Build();
    }
}
