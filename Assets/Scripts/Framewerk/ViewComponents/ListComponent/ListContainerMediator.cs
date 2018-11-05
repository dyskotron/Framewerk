using System.Collections.Generic;

namespace Framewerk.ViewComponents.ListComponent
{
    /// <summary>
    /// Mediates View containing list of items.
    /// </summary>
    /// <typeparam name="TView">View scipt attached to container gameobject</typeparam>
    /// <typeparam name="TMediator">Mediator of single item in list</typeparam>
    /// <typeparam name="TData">Dataprovider for single item in list </typeparam>
    public class ListContainerMediator<TView, TMediator, TData> : ListBaseMediator<TView, TMediator, TData>, IListContainerMediator<TData> where TView : ListContainerView
        where TMediator : class, IListItemMediator<TData>, new()
        where TData : class, IListItemDataProvider
    {
        public virtual void SetData(List<TData> dataProviders)
        {
            base.SetData(dataProviders);

            //reenable / create item mediators when there's more data than spawned mediators
            //fill mediator with data
            for (var i = 0; i < dataProviders.Count; i++)
            {
                var listItem = GetMediator(i);
                SetItemData(listItem, dataProviders[i], i);
            }

            //hide item renderers when there's more spawned mediators than data
            for (var i = dataProviders.Count; i < ItemMediators.Count; i++)
            {
                ItemMediators[i].SetActive(false);
            }
        }

        private TMediator GetMediator(int index)
        {
            TMediator listItem;

            if (index < 0)
                return null;

            if (index < ItemMediators.Count)
            {
                listItem = ItemMediators[index];
                listItem.SetActive(true);
            }
            else
            {
                listItem = CreateItemMediator(index, View.ItemPrefab, View.ContentsParent);
                ItemMediators.Add(listItem);
            }

            return listItem;
        }
    }
}