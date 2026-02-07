namespace Framewerk.StrangeCore.Bundles
{
    /// <summary>
    /// Interface for modular binding bundles that can be installed/uninstalled at runtime.
    /// Bundles group related bindings (injections, commands, mediations) into reusable modules.
    /// </summary>
    /// <remarks>
    /// Inspired by Robotlegs IConfig pattern. Bundles should be instantiated via the injector
    /// so that dependencies (binders) are automatically injected before Install() is called.
    /// </remarks>
    public interface IBindingBundle
    {
        /// <summary>
        /// Install all bindings for this bundle.
        /// Called after the bundle is instantiated and dependencies are injected.
        /// </summary>
        void Install();

        /// <summary>
        /// Remove all bindings created by this bundle.
        /// </summary>
        void Uninstall();

        /// <summary>
        /// Whether this bundle has been installed.
        /// </summary>
        bool IsInstalled { get; }
    }
}
