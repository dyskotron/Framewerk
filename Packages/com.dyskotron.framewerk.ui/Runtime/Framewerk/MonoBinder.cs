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
    /// Binds serialized Unity Object references to the injection container.
    /// Attach to a GameObject and assign references in the Inspector.
    /// </summary>
    public class MonoBinder : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Unity Objects to bind to the injection container.")]
        private Object[] references;

        [SerializeField]
        [Tooltip("Controls which types each reference is bound as:\n" +
                 "• ConcreteOnly: Just the concrete type\n" +
                 "• IncludeInterfaces: Concrete + all interfaces\n" +
                 "• IncludeBaseClasses: Concrete + base class hierarchy\n" +
                 "• IncludeAll: Concrete + interfaces + base classes")]
        private BindMode bindMode = BindMode.ConcreteOnly;

        public Object[] References => references;
        public BindMode Mode => bindMode;

        /// <summary>
        /// Binds all references to the injection container.
        /// Injects dependencies into each reference before binding.
        /// </summary>
        public void Bind(IInjectionBinder binder)
        {
            var includeInterfaces = bindMode == BindMode.IncludeInterfaces || bindMode == BindMode.IncludeAll;
            var includeBaseClasses = bindMode == BindMode.IncludeBaseClasses || bindMode == BindMode.IncludeAll;

            foreach (var reference in references)
            {
                if (reference == null) continue;

                // Inject dependencies into the reference first
                binder.injector.Inject(reference);

                // Get all types to bind based on mode
                var types = BindingUtils.GetBindTypes(reference, includeInterfaces, includeBaseClasses);

                foreach (var type in types)
                {
                    binder.Bind(type).ToValue(reference).ToSingleton();
                }
            }
        }
    }
}
