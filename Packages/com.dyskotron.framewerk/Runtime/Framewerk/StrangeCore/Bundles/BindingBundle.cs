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
    /// </summary>
    /// <example>
    /// <code>
    /// public class ChatBundle : BindingBundle
    /// {
    ///     protected override void OnInstall()
    ///     {
    ///         BindInjection<IChatService>().To<ChatService>().ToSingleton();
    ///         BindCommand<SendMessageSignal, SendMessageCommand>();
    ///         BindMediation<ChatView, ChatMediator>();
    ///     }
    /// }
    ///
    /// // In context mapBindings():
    /// injectionBinder.Bind<ChatBundle>().To<ChatBundle>();
    /// var bundle = injectionBinder.GetInstance<ChatBundle>();
    /// bundle.Install();
    /// // Later: bundle.Uninstall();
    /// </code>
    /// </example>
    public abstract class BindingBundle : IBindingBundle
    {
        /// <summary>
        /// Injection binder for mapping interfaces to implementations.
        /// </summary>
        [Inject]
        public ICrossContextInjectionBinder InjectionBinder { get; set; }

        /// <summary>
        /// Command binder for mapping signals to commands.
        /// Optional - may be null in viewless contexts.
        /// </summary>
        [Inject]
        public ICommandBinder CommandBinder { get; set; }

        /// <summary>
        /// Mediation binder for mapping views to mediators.
        /// Optional - may be null in viewless contexts.
        /// </summary>
        [Inject]
        public IMediationBinder MediationBinder { get; set; }

        public bool IsInstalled { get; private set; }

        // Tracked bindings for cleanup
        private readonly List<TrackedInjectionBinding> _injectionBindings = new List<TrackedInjectionBinding>();
        private readonly List<Type> _commandBindings = new List<Type>();
        private readonly List<Type> _mediationBindings = new List<Type>();

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

        #region Injection Helpers

        /// <summary>
        /// Bind an interface/class for injection. Tracked for cleanup.
        /// </summary>
        protected IInjectionBinding BindInjection<T>()
        {
            var binding = InjectionBinder.Bind<T>();
            _injectionBindings.Add(new TrackedInjectionBinding(typeof(T), null));
            return binding;
        }

        /// <summary>
        /// Bind a named interface/class for injection. Tracked for cleanup.
        /// </summary>
        protected IInjectionBinding BindInjection<T>(object name)
        {
            var binding = InjectionBinder.Bind<T>().ToName(name);
            _injectionBindings.Add(new TrackedInjectionBinding(typeof(T), name));
            return binding;
        }

        #endregion

        #region Command Helpers

        /// <summary>
        /// Bind a signal to a command. Tracked for cleanup.
        /// </summary>
        protected ICommandBinding BindCommand<TSignal, TCommand>()
        {
            if (CommandBinder == null)
            {
                throw new InvalidOperationException(
                    $"CommandBinder is not available. Cannot bind {typeof(TSignal).Name} to {typeof(TCommand).Name}");
            }

            var binding = CommandBinder.Bind<TSignal>().To<TCommand>();
            _commandBindings.Add(typeof(TSignal));
            return binding;
        }

        /// <summary>
        /// Bind a signal to multiple commands. Tracked for cleanup.
        /// Returns the binding for further configuration (InSequence, Once, etc).
        /// </summary>
        protected ICommandBinding BindCommand<TSignal>()
        {
            if (CommandBinder == null)
            {
                throw new InvalidOperationException(
                    $"CommandBinder is not available. Cannot bind {typeof(TSignal).Name}");
            }

            var binding = CommandBinder.Bind<TSignal>();
            _commandBindings.Add(typeof(TSignal));
            return binding;
        }

        #endregion

        #region Mediation Helpers

        /// <summary>
        /// Bind a view to a mediator. Tracked for cleanup.
        /// </summary>
        protected IMediationBinding BindMediation<TView, TMediator>()
        {
            if (MediationBinder == null)
            {
                throw new InvalidOperationException(
                    $"MediationBinder is not available. Cannot bind {typeof(TView).Name} to {typeof(TMediator).Name}");
            }

            var binding = MediationBinder.Bind<TView>().To<TMediator>();
            _mediationBindings.Add(typeof(TView));
            return binding;
        }

        /// <summary>
        /// Bind a view type. Returns binding for further configuration.
        /// </summary>
        protected IMediationBinding BindMediation<TView>()
        {
            if (MediationBinder == null)
            {
                throw new InvalidOperationException(
                    $"MediationBinder is not available. Cannot bind {typeof(TView).Name}");
            }

            var binding = MediationBinder.Bind<TView>();
            _mediationBindings.Add(typeof(TView));
            return binding;
        }

        #endregion

        private struct TrackedInjectionBinding
        {
            public Type Key;
            public object Name;

            public TrackedInjectionBinding(Type key, object name)
            {
                Key = key;
                Name = name;
            }
        }
    }
}
