using UnityEngine;

namespace Framewerk.Editor.Wizards
{
    public static class AddressableHelper
    {
        public static void MarkAsAddressable(string assetPath, string address)
        {
#if FRAMEWERK_ADDRESSABLES
            try
            {
                var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
                if (settings == null)
                {
                    Debug.LogWarning($"Addressables settings not initialized. Skipping marking {assetPath} as addressable.");
                    return;
                }

                var guid = UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);
                if (string.IsNullOrEmpty(guid))
                {
                    Debug.LogError($"Could not find GUID for asset at path: {assetPath}");
                    return;
                }

                var entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
                if (entry != null)
                {
                    entry.address = address;
                    Debug.Log($"Marked {assetPath} as addressable with address: {address}");
                }
                else
                {
                    Debug.LogError($"Failed to create addressable entry for {assetPath}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to mark {assetPath} as addressable: {e.Message}");
            }
#else
            Debug.LogWarning($"Addressables package not installed. Skipping marking {assetPath} as addressable.");
#endif
        }
    }
}
