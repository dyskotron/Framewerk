using System.Collections.Generic;
using Framewerk.Signals;
using strange.extensions.mediation.api;
using strange.extensions.signal.impl;

namespace Framewerk.UI.List
{
    public interface IListItemParent : IMediator
    {
        void RegisterMediator(IMediator mediator);
    }

    public interface IListMediator<TData> : IListItemParent where TData : class, IListItemDataProvider
    {
        /// <summary>
        /// Signal dispatched when selection changes. Parameter is the selected index (null if nothing selected).
        /// </summary>
        SelectionChangedSignal SelectionChangedSignal { get; }

        /// <summary>
        /// Signal dispatched when an item is clicked, regardless of whether the click changed selection.
        /// Parameters are the clicked index and its data.
        /// </summary>
        Signal<int, TData> ItemClickedSignal { get; }

        /// <summary>
        /// What user input changes selection. Initialized from ListView.SelectionTrigger; code may override at runtime.
        /// </summary>
        SelectionTrigger SelectionTrigger { get; set; }

        void SetData(List<TData> dataProviders);

        /// <summary>
        /// Method for selecting item in list from code instead of user input.
        /// </summary>
        /// <param name="index">item index</param>
        /// <param name="silent">If true ListItemSelectedEvent is not dispatched thru EventBus</param>
        void SelectItemAt(int index);

        void UnselectItemAt(int index);

        int? GetSelectedIndex();
        
        TData GetSelectedItem();
        List<TData> GetSelectedItems();
        
        void UnselectAll();
        int FindItemIndex(TData data);
        void RemoveItemAt(int index);
        TData GetDataproviderAt(int index);
    }
}