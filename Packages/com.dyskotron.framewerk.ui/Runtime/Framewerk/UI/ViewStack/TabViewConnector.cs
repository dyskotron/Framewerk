using Framewerk.UI.List;

namespace Framewerk.UI.ViewStack
{
    /// <summary>
    /// Connects a List (used as tabs) to a ViewStack.
    /// When a tab is selected, the corresponding view in the ViewStack is shown.
    /// </summary>
    /// <remarks>
    /// This is a plain C# class, not a MonoBehaviour. Instantiate it and call Init() to set up,
    /// and Destroy() when done to clean up signal subscriptions.
    /// </remarks>
    /// <typeparam name="TData">The data provider type used by the List.</typeparam>
    public class TabViewConnector<TData> where TData : class, IListItemDataProvider
    {
        private IListMediator<TData> _tabs;
        private IViewStack _viewStack;
        private bool _initialized;

        public TabViewConnector()
        {
        }

        /// <summary>
        /// Creates and initializes the connector in one call.
        /// </summary>
        public TabViewConnector(IListMediator<TData> tabs, IViewStack viewStack)
        {
            Init(tabs, viewStack);
        }

        /// <summary>
        /// Initializes the connector with the given tabs and view stack.
        /// </summary>
        /// <param name="tabs">The List component acting as tabs.</param>
        /// <param name="viewStack">The ViewStack to control.</param>
        public void Init(IListMediator<TData> tabs, IViewStack viewStack)
        {
            if (_initialized)
            {
                Destroy();
            }

            _tabs = tabs;
            _viewStack = viewStack;

            _tabs.SelectionChangedSignal.AddListener(OnTabSelectionChanged);
            _initialized = true;

            // Sync initial state - if a tab is already selected, show that view
            var selectedIndex = _tabs.GetSelectedIndex();
            if (selectedIndex.HasValue)
            {
                _viewStack.ShowView(selectedIndex.Value);
            }
        }

        /// <summary>
        /// Cleans up signal subscriptions. Call this when the connector is no longer needed.
        /// </summary>
        public void Destroy()
        {
            if (!_initialized)
                return;

            _tabs?.SelectionChangedSignal.RemoveListener(OnTabSelectionChanged);
            _tabs = null;
            _viewStack = null;
            _initialized = false;
        }

        private void OnTabSelectionChanged(int? selectedIndex)
        {
            if (selectedIndex.HasValue)
            {
                _viewStack.ShowView(selectedIndex.Value);
            }
        }
    }
}
