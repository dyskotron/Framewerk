using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using System.IO;

public static class AddressablesSetup
{
    private const string GroupName = "Framewerk Demo Assets";
    private const string ResourcesPrefabsPath = "Assets/Resources/Prefabs";

    [MenuItem("Framewerk/Setup Addressables")]
    public static void Setup()
    {
        // Create or get default settings
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            Debug.Log("[AddressablesSetup] Created default Addressables settings.");
        }

        // Find or create group
        var group = settings.FindGroup(GroupName);
        if (group == null)
        {
            group = settings.CreateGroup(GroupName, false, false, true,
                null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            Debug.Log($"[AddressablesSetup] Created group: {GroupName}");
        }

        // Find all prefabs under Resources/Prefabs
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { ResourcesPrefabsPath });
        int added = 0;

        foreach (var guid in prefabGuids)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);

            // Compute address: strip "Assets/Resources/" prefix and ".prefab" extension
            // e.g. "Assets/Resources/Prefabs/UI/Menu/Menu.prefab" -> "Prefabs/UI/Menu/Menu"
            var address = assetPath;
            if (address.StartsWith("Assets/Resources/"))
                address = address.Substring("Assets/Resources/".Length);
            if (address.EndsWith(".prefab"))
                address = address.Substring(0, address.Length - ".prefab".Length);

            // Add or update entry
            var entry = settings.CreateOrMoveEntry(guid, group, readOnly: false, postEvent: false);
            entry.address = address;
            added++;
            Debug.Log($"[AddressablesSetup] Added: {address} ({assetPath})");
        }

        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);
        AssetDatabase.SaveAssets();
        Debug.Log($"[AddressablesSetup] Done! Added {added} prefabs to group '{GroupName}'.");
    }
}
