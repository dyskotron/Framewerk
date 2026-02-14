using System.Collections.Generic;
using strange.extensions.promise.api;
using strange.extensions.promise.impl;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

namespace Framewerk.Managers
{
    public interface IAssetManager
    {
        // Promise-based async methods
        IPromise PreloadAssetAsync(string address);
        IPromise<T> GetAssetAsync<T>(string address, Transform parent = null) where T : Object;
        IPromise<T> GetGameObjectAsync<T>(string address, Transform parent = null) where T : MonoBehaviour;
        IPromise<T> LoadAssetAsync<T>(string address) where T : Object;
        IPromise<Sprite> GetSpriteAsync(string address);
        IPromise<Sprite> GetSpriteFromAtlasAsync(string atlasAddress, string spriteName);
        IPromise<Texture2D> GetTextureAsync(string address);
        IPromise<Material> GetMaterialAsync(string address);

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

        #region Promise-based Async Methods

        public IPromise PreloadAssetAsync(string address)
        {
            var promise = new Promise();

            if (_preloadedHandles.ContainsKey(address))
            {
                promise.Dispatch();
                return promise;
            }

            var handle = Addressables.LoadAssetAsync<Object>(address);
            handle.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    _preloadedHandles[address] = handle;
                    promise.Dispatch();
                }
                else
                {
                    promise.ReportFail(new System.Exception($"Failed to preload asset: {address}"));
                }
            };

            return promise;
        }

        public IPromise<T> GetAssetAsync<T>(string address, Transform parent = null) where T : Object
        {
            var promise = new Promise<T>();

            if (typeof(T) == typeof(GameObject) || typeof(T).IsSubclassOf(typeof(GameObject)))
            {
                var handle = Addressables.InstantiateAsync(address, parent, false);
                handle.Completed += op =>
                {
                    if (op.Status == AsyncOperationStatus.Succeeded)
                    {
                        _instantiatedObjects.Add(op.Result);
                        promise.Dispatch(op.Result as T);
                    }
                    else
                    {
                        promise.ReportFail(new System.Exception($"Failed to instantiate asset: {address}"));
                    }
                };
            }
            else
            {
                var handle = Addressables.LoadAssetAsync<T>(address);
                handle.Completed += op =>
                {
                    if (op.Status == AsyncOperationStatus.Succeeded)
                    {
                        promise.Dispatch(op.Result);
                    }
                    else
                    {
                        promise.ReportFail(new System.Exception($"Failed to load asset: {address}"));
                    }
                };
            }

            return promise;
        }

        public IPromise<T> GetGameObjectAsync<T>(string address, Transform parent = null) where T : MonoBehaviour
        {
            var promise = new Promise<T>();

            var handle = Addressables.InstantiateAsync(address, parent, false);
            handle.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    _instantiatedObjects.Add(op.Result);
                    var component = op.Result.GetComponent<T>();

                    if (component == null)
                    {
                        Debug.LogError($"AssetManager.GetGameObject: No {typeof(T)} on {op.Result.name}");
                        promise.ReportFail(new System.Exception($"No {typeof(T)} on {op.Result.name}"));
                    }
                    else
                    {
                        promise.Dispatch(component);
                    }
                }
                else
                {
                    promise.ReportFail(new System.Exception($"Failed to instantiate asset: {address}"));
                }
            };

            return promise;
        }

        public IPromise<T> LoadAssetAsync<T>(string address) where T : Object
        {
            var promise = new Promise<T>();

            var handle = Addressables.LoadAssetAsync<T>(address);
            handle.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    promise.Dispatch(op.Result);
                }
                else
                {
                    promise.ReportFail(new System.Exception($"Failed to load asset: {address}"));
                }
            };

            return promise;
        }

        public IPromise<Sprite> GetSpriteAsync(string address)
        {
            var promise = new Promise<Sprite>();

            var handle = Addressables.LoadAssetAsync<Sprite>(address);
            handle.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    promise.Dispatch(op.Result);
                }
                else
                {
                    promise.ReportFail(new System.Exception($"Failed to load sprite: {address}"));
                }
            };

            return promise;
        }

        public IPromise<Sprite> GetSpriteFromAtlasAsync(string atlasAddress, string spriteName)
        {
            var promise = new Promise<Sprite>();

            var handle = Addressables.LoadAssetAsync<SpriteAtlas>(atlasAddress);
            handle.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    var sprite = op.Result.GetSprite(spriteName);
                    if (sprite == null)
                    {
                        Debug.LogError($"AssetManager.GetSpriteFromAtlas: No sprite '{spriteName}' in atlas '{atlasAddress}'");
                        promise.ReportFail(new System.Exception($"No sprite '{spriteName}' in atlas '{atlasAddress}'"));
                    }
                    else
                    {
                        promise.Dispatch(sprite);
                    }
                }
                else
                {
                    promise.ReportFail(new System.Exception($"Failed to load atlas: {atlasAddress}"));
                }
            };

            return promise;
        }

        public IPromise<Texture2D> GetTextureAsync(string address)
        {
            var promise = new Promise<Texture2D>();

            var handle = Addressables.LoadAssetAsync<Texture2D>(address);
            handle.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    promise.Dispatch(op.Result);
                }
                else
                {
                    promise.ReportFail(new System.Exception($"Failed to load texture: {address}"));
                }
            };

            return promise;
        }

        public IPromise<Material> GetMaterialAsync(string address)
        {
            var promise = new Promise<Material>();

            var handle = Addressables.LoadAssetAsync<Material>(address);
            handle.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    promise.Dispatch(op.Result);
                }
                else
                {
                    promise.ReportFail(new System.Exception($"Failed to load material: {address}"));
                }
            };

            return promise;
        }

        #endregion

        #region Synchronous Methods

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
                var go = Addressables.InstantiateAsync(address, parent, false).WaitForCompletion();
                _instantiatedObjects.Add(go);
                return go as T;
            }

            var asset = Addressables.LoadAssetAsync<T>(address).WaitForCompletion();
            return asset;
        }

        public T GetGameObject<T>(string address, Transform parent = null) where T : MonoBehaviour
        {
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

        #endregion

        #region Resource Management

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

        #endregion
    }
}
