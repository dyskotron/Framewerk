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
        Task PreloadAssetAsync(string address, CancellationToken ct = default);
        bool IsAssetPreloaded(string address);
        Task<T> GetAssetAsync<T>(string address, Transform parent = null, CancellationToken ct = default) where T : Object;
        Task<T> GetGameObjectAsync<T>(string address, Transform parent = null, CancellationToken ct = default) where T : MonoBehaviour;
        Task<T> LoadAssetAsync<T>(string address, CancellationToken ct = default) where T : Object;
        Task<Sprite> GetSpriteAsync(string address, CancellationToken ct = default);
        Task<Sprite> GetSpriteFromAtlasAsync(string atlasAddress, string spriteName, CancellationToken ct = default);
        Task<Texture2D> GetTextureAsync(string address, CancellationToken ct = default);
        Task<Material> GetMaterialAsync(string address, CancellationToken ct = default);
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

        public bool IsAssetPreloaded(string address)
        {
            return _preloadedHandles.ContainsKey(address);
        }

        public async Task<T> GetAssetAsync<T>(string address, Transform parent = null, CancellationToken ct = default) where T : Object
        {
            if (typeof(T) == typeof(GameObject) || typeof(T).IsSubclassOf(typeof(GameObject)))
            {
                var go = await Addressables.InstantiateAsync(address, parent).Task;
                _instantiatedObjects.Add(go);
                return go as T;
            }

            var asset = await Addressables.LoadAssetAsync<T>(address).Task;
            return asset;
        }

        public async Task<T> GetGameObjectAsync<T>(string address, Transform parent = null, CancellationToken ct = default) where T : MonoBehaviour
        {
            var go = await Addressables.InstantiateAsync(address, parent).Task;
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
