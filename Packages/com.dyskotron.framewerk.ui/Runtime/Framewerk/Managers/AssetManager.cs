using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

namespace Framewerk.Managers
{
    public interface IAssetManager
    {
        // Async methods
        Task PreloadAssetAsync(string address, CancellationToken ct = default);
        Task<T> GetAssetAsync<T>(string address, Transform parent = null, CancellationToken ct = default) where T : Object;
        Task<T> GetGameObjectAsync<T>(string address, Transform parent = null, CancellationToken ct = default) where T : MonoBehaviour;
        Task<T> LoadAssetAsync<T>(string address, CancellationToken ct = default) where T : Object;
        Task<Sprite> GetSpriteAsync(string address, CancellationToken ct = default);
        Task<Sprite> GetSpriteFromAtlasAsync(string atlasAddress, string spriteName, CancellationToken ct = default);
        Task<Texture2D> GetTextureAsync(string address, CancellationToken ct = default);
        Task<Material> GetMaterialAsync(string address, CancellationToken ct = default);

        // Sync methods
        void PreloadAsset(string address);
        bool IsAssetPreloaded(string address);
        T GetAsset<T>(string address, Transform parent = null) where T : Object;
        T GetGameObject<T>(string address, Transform parent = null) where T : MonoBehaviour;
        T LoadAsset<T>(string address) where T : Object;
        Sprite GetSprite(string address);
        Sprite GetSpriteFromAtlas(string atlasAddress, string spriteName);
        Texture2D GetTexture(string address);
        Material GetMaterial(string address);

        void ReleaseInstance(GameObject instance);
        void ReleaseAsset<T>(T asset) where T : Object;
        void Destroy();
    }

    public class AssetManager : IAssetManager
    {
        private Dictionary<string, AsyncOperationHandle> _preloadedHandles = new Dictionary<string, AsyncOperationHandle>();
        private List<GameObject> _instantiatedObjects = new List<GameObject>();

        public async Task PreloadAssetAsync(string address, CancellationToken ct = default)
        {
            if (_preloadedHandles.ContainsKey(address))
                return;

            var handle = Addressables.LoadAssetAsync<Object>(address);
            await handle.Task;
            _preloadedHandles[address] = handle;
        }

        public async Task<T> GetAssetAsync<T>(string address, Transform parent = null, CancellationToken ct = default) where T : Object
        {
            if (typeof(T) == typeof(GameObject) || typeof(T).IsSubclassOf(typeof(GameObject)))
            {
                // Use instantiateInWorldSpace = false to preserve prefab's local RectTransform offsets
                var go = await Addressables.InstantiateAsync(address, parent, false).Task;
                _instantiatedObjects.Add(go);
                return go as T;
            }

            var asset = await Addressables.LoadAssetAsync<T>(address).Task;
            return asset;
        }

        public async Task<T> GetGameObjectAsync<T>(string address, Transform parent = null, CancellationToken ct = default) where T : MonoBehaviour
        {
            // Use instantiateInWorldSpace = false to preserve prefab's local RectTransform offsets
            var go = await Addressables.InstantiateAsync(address, parent, false).Task;
            _instantiatedObjects.Add(go);
            var component = go.GetComponent<T>();

            if (component == null)
                Debug.LogError($"AssetManager.GetGameObjectAsync: No {typeof(T)} on {go.name}");

            return component;
        }

        public async Task<T> LoadAssetAsync<T>(string address, CancellationToken ct = default) where T : Object
        {
            return await Addressables.LoadAssetAsync<T>(address).Task;
        }

        public async Task<Sprite> GetSpriteAsync(string address, CancellationToken ct = default)
        {
            return await Addressables.LoadAssetAsync<Sprite>(address).Task;
        }

        public async Task<Sprite> GetSpriteFromAtlasAsync(string atlasAddress, string spriteName, CancellationToken ct = default)
        {
            var atlas = await Addressables.LoadAssetAsync<SpriteAtlas>(atlasAddress).Task;
            var sprite = atlas.GetSprite(spriteName);

            if (sprite == null)
                Debug.LogError($"AssetManager.GetSpriteFromAtlasAsync: No sprite '{spriteName}' in atlas '{atlasAddress}'");

            return sprite;
        }

        public async Task<Texture2D> GetTextureAsync(string address, CancellationToken ct = default)
        {
            return await Addressables.LoadAssetAsync<Texture2D>(address).Task;
        }

        public async Task<Material> GetMaterialAsync(string address, CancellationToken ct = default)
        {
            return await Addressables.LoadAssetAsync<Material>(address).Task;
        }

        // Synchronous methods
        public void PreloadAsset(string address)
        {
            if (_preloadedHandles.ContainsKey(address))
                return;

            var handle = Addressables.LoadAssetAsync<Object>(address);
            handle.WaitForCompletion();
            _preloadedHandles[address] = handle;
        }

        public bool IsAssetPreloaded(string address)
        {
            return _preloadedHandles.TryGetValue(address, out var handle) && handle.IsDone;
        }

        public T GetAsset<T>(string address, Transform parent = null) where T : Object
        {
            if (typeof(T) == typeof(GameObject) || typeof(T).IsSubclassOf(typeof(GameObject)))
            {
                // Use instantiateInWorldSpace = false to preserve prefab's local RectTransform offsets
                var go = Addressables.InstantiateAsync(address, parent, false).WaitForCompletion();
                _instantiatedObjects.Add(go);
                return go as T;
            }

            var asset = Addressables.LoadAssetAsync<T>(address).WaitForCompletion();
            return asset;
        }

        public T GetGameObject<T>(string address, Transform parent = null) where T : MonoBehaviour
        {
            // Use instantiateInWorldSpace = false to preserve prefab's local RectTransform offsets
            var go = Addressables.InstantiateAsync(address, parent, false).WaitForCompletion();
            _instantiatedObjects.Add(go);
            var component = go.GetComponent<T>();

            if (component == null)
                Debug.LogError($"AssetManager.GetGameObject: No {typeof(T)} on {go.name}");

            return component;
        }

        public T LoadAsset<T>(string address) where T : Object
        {
            return Addressables.LoadAssetAsync<T>(address).WaitForCompletion();
        }

        public Sprite GetSprite(string address)
        {
            return Addressables.LoadAssetAsync<Sprite>(address).WaitForCompletion();
        }

        public Sprite GetSpriteFromAtlas(string atlasAddress, string spriteName)
        {
            var atlas = Addressables.LoadAssetAsync<SpriteAtlas>(atlasAddress).WaitForCompletion();
            var sprite = atlas.GetSprite(spriteName);

            if (sprite == null)
                Debug.LogError($"AssetManager.GetSpriteFromAtlas: No sprite '{spriteName}' in atlas '{atlasAddress}'");

            return sprite;
        }

        public Texture2D GetTexture(string address)
        {
            return Addressables.LoadAssetAsync<Texture2D>(address).WaitForCompletion();
        }

        public Material GetMaterial(string address)
        {
            return Addressables.LoadAssetAsync<Material>(address).WaitForCompletion();
        }

        public void ReleaseInstance(GameObject instance)
        {
            _instantiatedObjects.Remove(instance);
            Addressables.ReleaseInstance(instance);
        }

        public void ReleaseAsset<T>(T asset) where T : Object
        {
            Addressables.Release(asset);
        }

        public void Destroy()
        {
            foreach (var handle in _preloadedHandles.Values)
                Addressables.Release(handle);
            _preloadedHandles.Clear();

            foreach (var go in _instantiatedObjects)
            {
                if (go != null)
                    Addressables.ReleaseInstance(go);
            }
            _instantiatedObjects.Clear();
        }
    }
}
