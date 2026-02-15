using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framewerk.Managers
{
    /// <summary>
    /// Container for specifying multiple views to instantiate together.
    /// Use Add&lt;T&gt;() to register view types, then pass to UiManager.InstantiateViewsAsync().
    /// </summary>
    public class ViewGroup
    {
        private readonly List<ViewEntry> _entries = new List<ViewEntry>();

        public IReadOnlyList<ViewEntry> Entries => _entries;

        public ViewGroup Add<T>(Transform parent = null) where T : class
        {
            _entries.Add(new ViewEntry(typeof(T), parent));
            return this;
        }

        public ViewGroup Add(Type viewType, Transform parent = null)
        {
            _entries.Add(new ViewEntry(viewType, parent));
            return this;
        }

        public readonly struct ViewEntry
        {
            public readonly Type ViewType;
            public readonly Transform Parent;

            public ViewEntry(Type viewType, Transform parent)
            {
                ViewType = viewType;
                Parent = parent;
            }
        }
    }

    /// <summary>
    /// Result container from ViewGroup instantiation.
    /// Use Get&lt;T&gt;() to retrieve instantiated views by type.
    /// </summary>
    public class ViewGroupResult
    {
        private readonly Dictionary<Type, GameObject> _views = new Dictionary<Type, GameObject>();

        internal void Add(Type viewType, GameObject gameObject)
        {
            _views[viewType] = gameObject;
        }

        public T Get<T>() where T : class
        {
            if (_views.TryGetValue(typeof(T), out var go))
            {
                return go.GetComponent<T>();
            }
            return null;
        }

        public GameObject GetGameObject<T>() where T : class
        {
            _views.TryGetValue(typeof(T), out var go);
            return go;
        }

        public bool TryGet<T>(out T view) where T : class
        {
            view = Get<T>();
            return view != null;
        }
    }
}
