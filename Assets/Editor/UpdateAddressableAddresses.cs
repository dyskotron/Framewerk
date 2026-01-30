using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.Collections.Generic;

public class UpdateAddressableAddresses : MonoBehaviour
{
    [MenuItem("Tools/Update UI Addressable Addresses")]
    public static void UpdateAddresses()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressable Asset Settings not found!");
            return;
        }

        // Define the mapping of old addresses to new addresses
        var addressMapping = new Dictionary<string, string>
        {
            // Menu
            { "Prefabs/UI/Menu/Menu", "UI/Examples/Menu/Menu" },
            { "Prefabs/UI/Menu/MenuItem", "UI/Examples/Menu/MenuItem" },

            // Top Menu
            { "Prefabs/UI/Examples/TopMenu", "UI/Examples/TopMenu" },

            // List Panel
            { "Prefabs/UI/ListPanel/ExampleListPanel", "UI/Examples/ListPanel/ExampleListPanel" },
            { "Prefabs/UI/ListPanel/ExampleListItem", "UI/Examples/ListPanel/ExampleListItem" },

            // Popups
            { "Prefabs/UI/Popups/ExamplePopup", "UI/Examples/Popups/ExamplePopup" },

            // Tab Panel
            { "Prefabs/UI/TabPanel/ExampleTabPanel", "UI/Examples/TabPanel/ExampleTabPanel" },
            { "Prefabs/UI/TabPanel/TabItem1", "UI/Examples/TabPanel/TabItem1" },
            { "Prefabs/UI/TabPanel/TabItem2", "UI/Examples/TabPanel/TabItem2" },
            { "Prefabs/UI/TabPanel/TabItem3", "UI/Examples/TabPanel/TabItem3" },

            // Virtual List Panel
            { "Prefabs/UI/VirtualListPanel/ExampleVirtualListPanel", "UI/Examples/VirtualListPanel/ExampleVirtualListPanel" },
            { "Prefabs/UI/VirtualListPanel/ExampleVitualListItem", "UI/Examples/VirtualListPanel/ExampleVitualListItem" },

            // Preload
            { "Prefabs/UI/Preload/PreloadPanel", "UI/Examples/Preload/PreloadPanel" }
        };

        int updatedCount = 0;
        int notFoundCount = 0;

        foreach (var kvp in addressMapping)
        {
            string oldAddress = kvp.Key;
            string newAddress = kvp.Value;

            // Find the entry by old address
            var entry = FindEntryByAddress(settings, oldAddress);

            if (entry != null)
            {
                entry.address = newAddress;
                updatedCount++;
                Debug.Log($"Updated address: {oldAddress} -> {newAddress}");
            }
            else
            {
                notFoundCount++;
                Debug.LogWarning($"Could not find entry with address: {oldAddress}");
            }
        }

        if (updatedCount > 0)
        {
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            Debug.Log($"Successfully updated {updatedCount} Addressable addresses.");
        }

        if (notFoundCount > 0)
        {
            Debug.LogWarning($"{notFoundCount} entries were not found.");
        }
    }

    private static AddressableAssetEntry FindEntryByAddress(AddressableAssetSettings settings, string address)
    {
        foreach (var group in settings.groups)
        {
            if (group == null) continue;

            foreach (var entry in group.entries)
            {
                if (entry.address == address)
                {
                    return entry;
                }
            }
        }
        return null;
    }
}
