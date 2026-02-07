using System;
using System.Collections.Generic;
using strange.extensions.command.api;
using strange.extensions.injector.api;
using strange.extensions.mediation.api;

namespace Framewerk.StrangeCore.Bundles
{
    /// <summary>
    /// Base class for modular binding bundles with dependency injection.
    /// Binders are automatically injected when the bundle is instantiated via the injector.
    /// Use the tracked helper methods (BindInjection, BindCommand, BindMediation) instead of
    /// accessing binders directly — they ensure automatic cleanup on Uninstall().
    /// </summary>
    /// <example>
    /// <code>
    /// public class ChatBundle : BindingBundle
    /// {
    ///     protected override void OnInstall()
    ///     {
    ///         BindInjection&lt;IChatService&gt;().To&lt;ChatService&gt;().ToSingleton();
    ///         BindCommand&lt;SendMessageSignal, SendMessageCommand&gt;();
    ///         BindMediation&lt;ChatView, ChatMediator&gt;();
    ///
    ///         // Cooperative: skip if another bundle already bound it (no-op if already bound)
    ///         BindIfMissing&lt;ILogger&gt;().To&lt;UnityLogger&gt;().ToSingleton();
    ///     }
    /// }
    /// </code>
    /// </example>
    public abstract class BindingBundle : IBindingBundle
    {
        public static bool VerboseLogging = false;

        /// <summary>
        /// Injection binder for mapping interfaces to implementations.
        /// </summary>
        [Inject]
        public ICrossContextInjectionBinder InjectionBinder { get; set; }

        /// <summary>
        /// Command binder for mapping signals to commands.
        /// Optional — may be null in viewless contexts.
        /// </summary>
        [Inject]
        public ICommandBinder CommandBinder { get; set; }

        /// <summary>
        /// Mediation binder for mapping views to mediators.
        /// Optional — may be null in viewless contexts.
        /// </summary>
        [Inject]
        public IMediationBinder MediationBinder { get; set; }

        public bool IsInstalled { get; private set; }

        // Tracked bindings for cleanup — only what THIS bundle bound
        private readonly List<TrackedInjectionBinding> _injectionBindings = new();
        private readonly List<Type> _commandBindings = new();
        private readonly List<Type> _mediationBindings = new();

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

            // Unbind mediations
            if (MediationBinder != null)
            {
                foreach (var viewType in _mediationBindings)
                {
                    MediationBinder.Unbind(viewType);
                }
            }
            _mediationBindings.Clear();

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

        /// <summary>
        /// Bind a view to a mediator. Tracked for automatic cleanup on Uninstall.
        /// TView should extend View (strange.extensions.mediation.impl).
        /// TMediator should extend Mediator (strange.extensions.mediation.impl).
        /// </summary>
        protected void BindMediation<TView, TMediator>()
        {
            if (MediationBinder == null)
            {
                throw new InvalidOperationException(
                    $"[{GetType().Name}] MediationBinder is not available. Cannot bind {typeof(TView).Name} → {typeof(TMediator).Name}.");
            }

            MediationBinder.Bind<TView>().To<TMediator>();
            _mediationBindings.Add(typeof(TView));
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
#if UNITY_2021_1_OR_NEWER
                    UnityEngine.Debug.Log($"[{GetType().Name}] Skipping binding for {typeof(T).Name} — already bound.");
#else
                    System.Diagnostics.Debug.WriteLine($"[{GetType().Name}] Skipping binding for {typeof(T).Name} — already bound.");
#endif
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
#if UNITY_2021_1_OR_NEWER
                    UnityEngine.Debug.Log($"[{GetType().Name}] Skipping binding for {typeof(T).Name} (name: {name}) — already bound.");
#else
                    System.Diagnostics.Debug.WriteLine($"[{GetType().Name}] Skipping binding for {typeof(T).Name} (name: {name}) — already bound.");
#endif
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
#if UNITY_2021_1_OR_NEWER
                UnityEngine.Debug.LogWarning(
                    $"[{GetType().Name}] CommandBinder is not available. Cannot bind {typeof(TSignal).Name} → {typeof(TCommand).Name}.");
#else
                System.Diagnostics.Debug.WriteLine(
                    $"[{GetType().Name}] CommandBinder is not available. Cannot bind {typeof(TSignal).Name} → {typeof(TCommand).Name}.");
#endif
                return false;
            }

            if (CommandBinder.GetBinding<TSignal>() != null)
            {
                if (VerboseLogging)
                {
#if UNITY_2021_1_OR_NEWER
                    UnityEngine.Debug.Log($"[{GetType().Name}] Skipping binding for {typeof(TSignal).Name} — already bound.");
#else
                    System.Diagnostics.Debug.WriteLine($"[{GetType().Name}] Skipping binding for {typeof(TSignal).Name} — already bound.");
#endif
                }
                return false;
            }

            CommandBinder.Bind<TSignal>().To<TCommand>();
            _commandBindings.Add(typeof(TSignal));
            return true;
        }

        /// <summary>
        /// Bind a view to a mediator only if the view is not already bound.
        /// Returns true if bound, false if skipped.
        /// Skipped bindings are NOT tracked — they belong to whoever bound them first.
        /// </summary>
        protected bool BindMediationIfMissing<TView, TMediator>()
        {
            if (MediationBinder == null)
            {
#if UNITY_2021_1_OR_NEWER
                UnityEngine.Debug.LogWarning(
                    $"[{GetType().Name}] MediationBinder is not available. Cannot bind {typeof(TView).Name} → {typeof(TMediator).Name}.");
#else
                System.Diagnostics.Debug.WriteLine(
                    $"[{GetType().Name}] MediationBinder is not available. Cannot bind {typeof(TView).Name} → {typeof(TMediator).Name}.");
#endif
                return false;
            }

            if (MediationBinder.GetBinding<TView>() != null)
            {
                if (VerboseLogging)
                {
#if UNITY_2021_1_OR_NEWER
                    UnityEngine.Debug.Log($"[{GetType().Name}] Skipping binding for {typeof(TView).Name} — already bound.");
#else
                    System.Diagnostics.Debug.WriteLine($"[{GetType().Name}] Skipping binding for {typeof(TView).Name} — already bound.");
#endif
                }
                return false;
            }

            MediationBinder.Bind<TView>().To<TMediator>();
            _mediationBindings.Add(typeof(TView));
            return true;
        }

        #endregion

        private struct TrackedInjectionBinding
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
