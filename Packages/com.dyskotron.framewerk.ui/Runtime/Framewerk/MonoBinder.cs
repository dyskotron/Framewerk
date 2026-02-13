using System;
using Framewerk.Utils;
using strange.extensions.injector.api;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framewerk
{
    /// <summary>
    /// Controls how types are bound in the injection container.
    /// </summary>
    public enum BindMode
    {
        /// <summary>Just the concrete type.</summary>
        ConcreteOnly,
        /// <summary>Concrete type + all implemented interfaces.</summary>
        IncludeInterfaces,
        /// <summary>Concrete type + base class hierarchy (excluding object).</summary>
        IncludeBaseClasses,
        /// <summary>Concrete type + interfaces + base classes.</summary>
        IncludeAll
    }

    /// <summary>
    /// Pairs a Unity Object reference with its binding mode.
    /// Each binding can have its own mode for fine-grained control.
    /// </summary>
    [Serializable]
    public struct ReferenceBinding
    {
        [Tooltip("The Unity Object to bind to the injection container.")]
        public Object reference;

        [Tooltip("Controls which types this reference is bound as:\n" +
                 "• ConcreteOnly: Just the concrete type\n" +
                 "• IncludeInterfaces: Concrete + all interfaces\n" +
                 "• IncludeBaseClasses: Concrete + base class hierarchy\n" +
                 "• IncludeAll: Concrete + interfaces + base classes")]
        public BindMode bindMode;

        public ReferenceBinding(Object reference, BindMode bindMode = BindMode.ConcreteOnly)
        {
            this.reference = reference;
            this.bindMode = bindMode;
        }
    }

    /// <summary>
    /// Binds serialized Unity Object references to the injection container.
    /// Attach to a GameObject and assign references in the Inspector.
    /// Each binding can have its own BindMode for fine-grained control.
    /// </summary>
    public class MonoBinder : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Unity Objects to bind, each with its own binding mode.")]
        private ReferenceBinding[] bindings;

        public ReferenceBinding[] Bindings => bindings;

        /// <summary>
        /// Binds all references to the injection container.
        /// Injects dependencies into each reference before binding.
        /// Each binding uses its own BindMode setting.
        /// </summary>
        public void Bind(IInjectionBinder binder)
        {
            if (bindings == null) return;

            foreach (var binding in bindings)
            {
                if (binding.reference == null) continue;

                // Inject dependencies into the reference first
                binder.injector.Inject(binding.reference);

                // Determine flags from this binding's mode
                var includeInterfaces = binding.bindMode == BindMode.IncludeInterfaces || binding.bindMode == BindMode.IncludeAll;
                var includeBaseClasses = binding.bindMode == BindMode.IncludeBaseClasses || binding.bindMode == BindMode.IncludeAll;

                // Get all types to bind based on mode
                var types = BindingUtils.GetBindTypes(binding.reference, includeInterfaces, includeBaseClasses);

                foreach (var type in types)
                {
                    binder.Bind(type).ToValue(binding.reference).ToSingleton();
                }
            }
        }
    }
}
