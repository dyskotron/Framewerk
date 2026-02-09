using strange.extensions.signal.impl;
using UnityEngine;

namespace Framewerk.UI.ViewStack
{
    /// <summary>
    /// Interface for ViewStack - a simple visibility controller for stacked children.
    /// Shows one child at a time, like Qt's QStackedWidget.
    /// </summary>
    public interface IViewStack
    {
        /// <summary>
        /// Index of currently visible view (-1 if empty).
        /// </summary>
        int CurrentIndex { get; }
        
        /// <summary>
        /// Number of children in the stack.
        /// </summary>
        int Count { get; }
        
        /// <summary>
        /// Fired when current view changes. Parameters: (newIndex, newTransform).
        /// </summary>
        Signal<int, Transform> CurrentChanged { get; }
        
        /// <summary>
        /// Shows the view at the specified index, hiding all others.
        /// </summary>
        void ShowByIndex(int index);
        
        /// <summary>
        /// Shows the specified transform, hiding all others.
        /// </summary>
        void ShowByTransform(Transform t);
        
        /// <summary>
        /// Gets the currently visible view's transform (null if empty).
        /// </summary>
        Transform GetCurrentView();
        
        /// <summary>
        /// Gets the child at the specified index.
        /// </summary>
        Transform GetViewAtIndex(int index);
        
        // Legacy API for TabViewConnector compatibility
        void ShowView(int index);
        int ViewCount { get; }
    }
}
