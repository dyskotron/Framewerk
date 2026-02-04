using System;
using strange.extensions.injector.api;
using strange.framework.api;

namespace Framewerk.StrangeCore.Bundles
{
    /// <summary>
    /// A no-op implementation of <see cref="IInjectionBinding"/> used as a Null Object.
    /// Returned by <see cref="BindingBundle.BindIfMissing{T}()"/> when the binding already exists,
    /// allowing callers to chain fluently without null-conditional operators.
    /// All fluent methods return <c>this</c> and do nothing.
    /// </summary>
    public sealed class NullInjectionBinding : IInjectionBinding
    {
        public static readonly NullInjectionBinding Instance = new();

        private static readonly object[] EmptySupply = Array.Empty<object>();

        private NullInjectionBinding() { }

        // IInjectionBinding fluent methods — all return this

        public IInjectionBinding ToSingleton() => this;
        public IInjectionBinding ToValue(object o) => this;
        public IInjectionBinding SetValue(object o) => this;
        public IInjectionBinding CrossContext() => this;
        public IInjectionBinding SupplyTo<T>() => this;
        public IInjectionBinding SupplyTo(Type type) => this;
        public IInjectionBinding Unsupply<T>() => this;
        public IInjectionBinding Unsupply(Type type) => this;
        public IInjectionBinding ToInject(bool value) => this;

        public object[] GetSupply() => EmptySupply;
        public bool isCrossContext => false;
        public bool toInject => false;
        public InjectionBindingType type { get; set; } = InjectionBindingType.DEFAULT;

        // IInjectionBinding re-declared (new) fluent methods

        IInjectionBinding IInjectionBinding.Bind<T>() => this;
        IInjectionBinding IInjectionBinding.Bind(object key) => this;
        IInjectionBinding IInjectionBinding.To<T>() => this;
        IInjectionBinding IInjectionBinding.To(object o) => this;
        IInjectionBinding IInjectionBinding.ToName<T>() => this;
        IInjectionBinding IInjectionBinding.ToName(object o) => this;
        IInjectionBinding IInjectionBinding.Named<T>() => this;
        IInjectionBinding IInjectionBinding.Named(object o) => this;

        // IBinding fluent methods

        IBinding IBinding.Bind<T>() => this;
        IBinding IBinding.Bind(object key) => this;
        IBinding IBinding.To<T>() => this;
        IBinding IBinding.To(object o) => this;
        IBinding IBinding.ToName<T>() => this;
        IBinding IBinding.ToName(object o) => this;
        IBinding IBinding.Named<T>() => this;
        IBinding IBinding.Named(object o) => this;
        IBinding IBinding.Weak() => this;

        // IBinding void methods — no-ops

        public void RemoveKey(object o) { }
        public void RemoveValue(object o) { }
        public void RemoveName(object o) { }

        // IBinding properties

        public object key => null;
        public object name => null;
        public object value => null;
        public BindingConstraintType keyConstraint { get; set; } = BindingConstraintType.ONE;
        public BindingConstraintType valueConstraint { get; set; } = BindingConstraintType.ONE;
        public bool isWeak => false;
    }
}
