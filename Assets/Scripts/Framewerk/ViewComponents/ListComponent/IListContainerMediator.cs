using System.Collections.Generic;
using Framewerk.Core;
using Framewerk.Mvcs;

namespace Framewerk.ViewComponents.ListComponent
{
    public interface IListContainerMediator<TData> : IMediator where TData : class, IListItemDataProvider
    {
        IEventDispatcher EventBus { get; }
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