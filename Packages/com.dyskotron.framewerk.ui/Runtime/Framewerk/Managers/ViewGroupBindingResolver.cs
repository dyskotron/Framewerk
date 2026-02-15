using System;
using System.Collections.Generic;
using System.Reflection;
using Framewerk.Utils;
using strange.extensions.injector.api;
using strange.extensions.mediation.api;
using UnityEngine;

namespace Framewerk.Managers
{
    /// <summary>
    /// Handles the scanning and binding of [ViewGroupShared] properties for view groups.
    /// Extracts binding logic from UIManager to maintain single responsibility.
    /// </summary>
    public class ViewGroupBindingResolver
    {
        private readonly IInjectionBinder _injectionBinder;
        private readonly IMediationBinder _mediationBinder;
        private readonly bool _bindInterfaces;
        private readonly bool _bindBaseClasses;

        public ViewGroupBindingResolver(
            IInjectionBinder injectionBinder,
            IMediationBinder mediationBinder,
            bool bindInterfaces = true,
            bool bindBaseClasses = false)
        {
            _injectionBinder = injectionBinder;
            _mediationBinder = mediationBinder;
            _bindInterfaces = bindInterfaces;
            _bindBaseClasses = bindBaseClasses;
        }

        /// <summary>
        /// Resolves all bindings needed for a view group.
        /// Scans all mediator types for [ViewGroupShared] properties and creates shared instances.
        /// Returns a scope object that should be disposed when the views are destroyed.
        /// </summary>
        /// <param name="viewTypes">The view types in the group</param>
        /// <param name="explicitBindings">Any explicit bindings provided by the caller</param>
        /// <returns>A scope containing all bindings that need cleanup</returns>
        public ViewGroupBindingScope Resolve(IEnumerable<Type> viewTypes, params object[] explicitBindings)
        {
            var scope = new ViewGroupBindingScope(_injectionBinder, _bindInterfaces, _bindBaseClasses);

            // Collect all mediator types
            var mediatorTypes = new List<Type>();
            foreach (var viewType in viewTypes)
            {
                var mediatorType = GetMediatorTypeForView(viewType);
                if (mediatorType != null)
                    mediatorTypes.Add(mediatorType);
            }

            // Build set of explicitly provided types
            var providedTypes = new HashSet<Type>();
            if (explicitBindings != null)
            {
                foreach (var binding in explicitBindings)
                {
                    if (binding == null) continue;
                    foreach (var type in BindingUtils.GetBindTypes(binding, _bindInterfaces, _bindBaseClasses))
                        providedTypes.Add(type);
                }
            }

            // Scan mediators for [ViewGroupShared] properties and collect types that need instances
            var sharedInstances = new List<object>();
            foreach (var mediatorType in mediatorTypes)
            {
                CollectViewGroupSharedInstances(mediatorType, providedTypes, sharedInstances, mediatorTypes);
            }

            // Bind explicit bindings
            if (explicitBindings != null)
            {
                foreach (var binding in explicitBindings)
                {
                    if (binding == null) continue;
                    scope.AddExplicitBinding(binding);
                }
            }

            // Bind shared instances using SupplyTo
            foreach (var instance in sharedInstances)
            {
                scope.AddSharedInstance(instance, mediatorTypes);
            }

            // Apply all bindings
            scope.Apply();

            return scope;
        }

        /// <summary>
        /// Gets the mediator type bound to a view type, or null if not found.
        /// </summary>
        private Type GetMediatorTypeForView(Type viewType)
        {
            if (_mediationBinder == null)
                return null;

            var binding = _mediationBinder.GetBinding(viewType) as IMediationBinding;
            if (binding?.value == null)
                return null;

            var values = binding.value as object[];
            if (values != null && values.Length > 0)
                return values[0] as Type;

            return null;
        }

        /// <summary>
        /// Scans a mediator type for [ViewGroupShared] properties.
        /// For each property type not already provided, creates a new instance.
        /// </summary>
        private void CollectViewGroupSharedInstances(
            Type mediatorType,
            HashSet<Type> providedTypes,
            List<object> sharedInstances,
            List<Type> allMediatorTypes)
        {
            var properties = mediatorType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                var hasInject = prop.GetCustomAttribute<Inject>() != null;
                var hasViewGroupShared = prop.GetCustomAttribute<ViewGroupShared>() != null;

                if (hasInject && hasViewGroupShared)
                {
                    var propType = prop.PropertyType;

                    // Skip if already provided
                    if (providedTypes.Contains(propType))
                        continue;

                    // Skip if binding already exists in the injection binder (global binding)
                    var existingBinding = _injectionBinder.GetBinding(propType);
                    if (existingBinding != null)
                        continue;

                    // Create a new instance
                    try
                    {
                        var instance = Activator.CreateInstance(propType);
                        if (instance != null)
                        {
                            sharedInstances.Add(instance);
                            // Add to providedTypes to prevent duplicates
                            foreach (var type in BindingUtils.GetBindTypes(instance, _bindInterfaces, _bindBaseClasses))
                                providedTypes.Add(type);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"ViewGroupBindingResolver: Failed to create instance for {propType.Name}: {ex.Message}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Represents a scope of bindings created for a view group.
    /// Handles cleanup (Unsupply + Unbind) when disposed.
    /// </summary>
    public class ViewGroupBindingScope : IDisposable
    {
        private readonly IInjectionBinder _injectionBinder;
        private readonly bool _bindInterfaces;
        private readonly bool _bindBaseClasses;

        // Track explicit bindings (standard Bind/Unbind)
        private readonly List<object> _explicitBindings = new();

        // Track shared instances with their target mediator types (SupplyTo/Unsupply)
        private readonly List<(object instance, List<Type> targetTypes)> _sharedBindings = new();

        private bool _applied;
        private bool _disposed;

        public ViewGroupBindingScope(
            IInjectionBinder injectionBinder,
            bool bindInterfaces,
            bool bindBaseClasses)
        {
            _injectionBinder = injectionBinder;
            _bindInterfaces = bindInterfaces;
            _bindBaseClasses = bindBaseClasses;
        }

        /// <summary>
        /// Adds an explicit binding (will use standard Bind/Unbind).
        /// </summary>
        public void AddExplicitBinding(object instance)
        {
            _explicitBindings.Add(instance);
        }

        /// <summary>
        /// Adds a shared instance that should be SupplyTo'd to specific mediator types.
        /// </summary>
        public void AddSharedInstance(object instance, List<Type> targetMediatorTypes)
        {
            _sharedBindings.Add((instance, new List<Type>(targetMediatorTypes)));
        }

        /// <summary>
        /// Applies all bindings. Call this before instantiating views.
        /// </summary>
        public void Apply()
        {
            if (_applied) return;
            _applied = true;

            // Apply explicit bindings
            BindingUtils.Bind(_injectionBinder, _bindInterfaces, _bindBaseClasses, _explicitBindings.ToArray());

            // Apply shared bindings with SupplyTo
            foreach (var (instance, targetTypes) in _sharedBindings)
            {
                BindingUtils.SupplyToTypes(_injectionBinder, instance, targetTypes, _bindInterfaces, _bindBaseClasses);
            }
        }

        /// <summary>
        /// Cleans up all bindings. Call this when the view group is destroyed.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            // Remove explicit bindings
            BindingUtils.Unbind(_injectionBinder, _bindInterfaces, _bindBaseClasses, _explicitBindings.ToArray());

            // Remove shared bindings (Unsupply + Unbind)
            foreach (var (instance, targetTypes) in _sharedBindings)
            {
                BindingUtils.UnsupplyAndUnbind(_injectionBinder, instance, targetTypes, _bindInterfaces, _bindBaseClasses);
            }
        }
    }
}
