using System;
using System.Collections.Generic;
using strange.extensions.command.api;
using strange.extensions.injector.api;

namespace Framewerk.StrangeCore.Bundles
{
    /// <summary>
    /// Base class for modular binding bundles with dependency injection.
    /// Pure .NET compatible — no Unity or mediation dependencies.
    /// 
    /// For Unity UI with mediation support, use <see cref="BindingBundle"/> from the UI package.
    /// </summary>
    /// <example>
    /// <code>
    /// public class ServiceBundle : CoreBindingBundle
    /// {
    ///     protected override void OnInstall()
    ///     {
    ///         BindInjection&lt;IMyService&gt;().To&lt;MyService&gt;().ToSingleton();
    ///         BindCommand&lt;DoSomethingSignal, DoSomethingCommand&gt;();
    ///     }
    /// }
    /// </code>
    /// </example>
    public abstract class CoreBindingBundle : IBindingBundle
    {
        public static bool VerboseLogging = false;

        /// <summary>
        /// Injection binder for mapping interfaces to implementations.
        /// </summary>
        [Inject]
        public ICrossContextInjectionBinder InjectionBinder { get; set; }

        /// <summary>
        /// Command binder for mapping signals to commands.
        /// Optional — may be null in some contexts.
        /// </summary>
        [Inject]
        public ICommandBinder CommandBinder { get; set; }

        public bool IsInstalled { get; private set; }

        // Tracked bindings for cleanup — only what THIS bundle bound
        private readonly List<TrackedInjectionBinding> _injectionBindings = new();
        private readonly List<Type> _commandBindings = new();

        public void Install()
        {
            if (IsInstalled)
            {
                throw new InvalidOperationException($"Bundle {GetType().Name} is already installed.");
            }

            OnInstall();
            IsInstalled = true;
        }

        /// <summary>
        /// Override this to define your bundle's bindings.
        /// </summary>
        protected abstract void OnInstall();

        public void Uninstall()
        {
            if (!IsInstalled)
            {
                return;
            }

            // Unbind injections
            foreach (var binding in _injectionBindings)
            {
                InjectionBinder.Unbind(binding.Key, binding.Name);
            }
            _injectionBindings.Clear();

            // Unbind commands
            if (CommandBinder != null)
            {
                foreach (var signalType in _commandBindings)
                {
                    CommandBinder.Unbind(signalType);
                }
            }
            _commandBindings.Clear();

            IsInstalled = false;
        }

        #region Tracked Exclusive Helpers

        /// <summary>
        /// Bind a type for injection. Tracked for automatic cleanup on Uninstall.
        /// </summary>
        protected IInjectionBinding BindInjection<T>()
        {
            var binding = InjectionBinder.Bind<T>();
            _injectionBindings.Add(new TrackedInjectionBinding(typeof(T), null));
            return binding;
        }

        /// <summary>
        /// Bind a named type for injection. Tracked for automatic cleanup on Uninstall.
        /// </summary>
        protected IInjectionBinding BindInjection<T>(object name)
        {
            var binding = InjectionBinder.Bind<T>().ToName(name);
            _injectionBindings.Add(new TrackedInjectionBinding(typeof(T), name));
            return binding;
        }

        /// <summary>
        /// Bind a signal to a command. Tracked for automatic cleanup on Uninstall.
        /// TSignal should extend Signal (strange.extensions.signal.impl).
        /// TCommand should extend Command (strange.extensions.command.impl).
        /// </summary>
        protected void BindCommand<TSignal, TCommand>()
        {
            if (CommandBinder == null)
            {
                throw new InvalidOperationException(
                    $"[{GetType().Name}] CommandBinder is not available. Cannot bind {typeof(TSignal).Name} → {typeof(TCommand).Name}.");
            }

            CommandBinder.Bind<TSignal>().To<TCommand>();
            _commandBindings.Add(typeof(TSignal));
        }

        #endregion

        #region Cooperative IfMissing Helpers

        /// <summary>
        /// Bind a type for injection only if not already bound.
        /// Returns the binding for chaining, or a no-op <see cref="NullInjectionBinding"/> if skipped.
        /// Skipped bindings are NOT tracked — they belong to whoever bound them first.
        /// </summary>
        protected IInjectionBinding BindIfMissing<T>()
        {
            if (InjectionBinder.GetBinding<T>() != null)
            {
                if (VerboseLogging)
                {
                    System.Diagnostics.Debug.WriteLine($"[{GetType().Name}] Skipping binding for {typeof(T).Name} — already bound.");
                }
                return NullInjectionBinding.Instance;
            }

            var binding = InjectionBinder.Bind<T>();
            _injectionBindings.Add(new TrackedInjectionBinding(typeof(T), null));
            return binding;
        }

        /// <summary>
        /// Bind a named type for injection only if not already bound with that name.
        /// Returns the binding for chaining, or a no-op <see cref="NullInjectionBinding"/> if skipped.
        /// Skipped bindings are NOT tracked — they belong to whoever bound them first.
        /// </summary>
        protected IInjectionBinding BindIfMissing<T>(object name)
        {
            if (InjectionBinder.GetBinding<T>(name) != null)
            {
                if (VerboseLogging)
                {
                    System.Diagnostics.Debug.WriteLine($"[{GetType().Name}] Skipping binding for {typeof(T).Name} (name: {name}) — already bound.");
                }
                return NullInjectionBinding.Instance;
            }

            var binding = InjectionBinder.Bind<T>().ToName(name);
            _injectionBindings.Add(new TrackedInjectionBinding(typeof(T), name));
            return binding;
        }

        /// <summary>
        /// Bind a signal to a command only if the signal is not already bound.
        /// Returns true if bound, false if skipped.
        /// Skipped bindings are NOT tracked — they belong to whoever bound them first.
        /// </summary>
        protected bool BindCommandIfMissing<TSignal, TCommand>()
        {
            if (CommandBinder == null)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[{GetType().Name}] CommandBinder is not available. Cannot bind {typeof(TSignal).Name} → {typeof(TCommand).Name}.");
                return false;
            }

            if (CommandBinder.GetBinding<TSignal>() != null)
            {
                if (VerboseLogging)
                {
                    System.Diagnostics.Debug.WriteLine($"[{GetType().Name}] Skipping binding for {typeof(TSignal).Name} — already bound.");
                }
                return false;
            }

            CommandBinder.Bind<TSignal>().To<TCommand>();
            _commandBindings.Add(typeof(TSignal));
            return true;
        }

        #endregion

        protected struct TrackedInjectionBinding
        {
            public readonly Type Key;
            public readonly object Name;

            public TrackedInjectionBinding(Type key, object name)
            {
                Key = key;
                Name = name;
            }
        }
    }
}
