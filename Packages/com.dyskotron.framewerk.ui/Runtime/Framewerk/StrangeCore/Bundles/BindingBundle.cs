using System;
using System.Collections.Generic;
using strange.extensions.injector.api;
using strange.extensions.mediation.api;

namespace Framewerk.StrangeCore.Bundles
{
    /// <summary>
    /// Unity UI binding bundle with full mediation support.
    /// Extends <see cref="CoreBindingBundle"/> with View/Mediator binding capabilities.
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
    public abstract class BindingBundle : CoreBindingBundle
    {
        /// <summary>
        /// Mediation binder for mapping views to mediators.
        /// Optional — may be null in viewless contexts.
        /// </summary>
        [Inject]
        public IMediationBinder MediationBinder { get; set; }

        // Tracked mediation bindings for cleanup — only what THIS bundle bound
        private readonly List<Type> _mediationBindings = new();

        public new void Uninstall()
        {
            if (!IsInstalled)
            {
                return;
            }

            // Unbind mediations first (before base cleans up injections/commands)
            if (MediationBinder != null)
            {
                foreach (var viewType in _mediationBindings)
                {
                    MediationBinder.Unbind(viewType);
                }
            }
            _mediationBindings.Clear();

            // Let base class handle injection and command unbinding
            base.Uninstall();
        }

        #region Mediation Helpers

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
    }
}
