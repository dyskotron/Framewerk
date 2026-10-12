using System;
using System.Collections.Generic;
using Framewerk.Managers;
using Framewerk.Signals;
using strange.extensions.mediation.api;
using strange.extensions.signal.impl;
using UnityEngine;

namespace Framewerk.UI.List
{
    /// <summary>
    /// Mediates View containing list of items.
    /// </summary>
    /// <typeparam name="TView">View scipt attached to container GameObject</typeparam>
    /// <typeparam name="TData">Dataprovider for single item in list </typeparam>
    public class ListBaseMediator<TView, TData> : ExtendedMediator<TView>, IListMediator<TData> where TView : ListView where TData : class, IListItemDataProvider
    {
        [Inject] public IUiManager UiManager { get; set; }
        
        [Inject, ViewGroupShared] public SelectionChangedSignal SelectionChangedSignal { get; set; }
        public Signal<int, TData> ItemClickedSignal { get; } = new Signal<int, TData>();
        public List<int> SelectedItemIndexes { get; private set; }
        public bool Multiselect { get; set; }
        public bool Unselectable { get; set; }
        public bool HoverSelects { get; set; }
        public bool SelectOnClick { get; set; } = true;

        protected int CreatedMediatorsCount = 0;
        protected readonly List<IListItemMediator<TData>> ItemMediators = new();
        protected List<TData> DataProviders = new();
        
        public override void OnRegister()
        {
            SelectedItemIndexes = new List<int>();
            
            base.OnRegister();
        }

        #region Public API

        public virtual void RegisterMediator(IMediator mediator)
        {
            var itemIndex = ItemMediators.Count;
            var itemMediator = (IListItemMediator<TData>) mediator;
            
            ItemMediators.Add(itemMediator);  
            
            //if we have data set them to mediator
            if(DataProviders.Count >= ItemMediators.Count)
                SetItemData(itemMediator, DataProviders[itemIndex], itemIndex);
            //if we dont, hide item
            else
                itemMediator.SetActive(false);
            
            itemMediator.ListItemClickedSignal.AddListener(ListItemClickedHandler);
            itemMediator.ListItemHoverSignal.AddListener(ListItemHoverHandler);
        }
        
        public virtual void SetData(List<TData> dataProviders)
        {
            //TODO: just remove selected indexes and shit, dont' do actual unselecting on list items?
            UnselectAll();
            
            if (View.EmptyContent != null)
                 View.EmptyContent.SetActive(dataProviders.Count == 0);

            DataProviders = dataProviders;
        }

        public void Destroy()
        {
            foreach (var itemMediator in ItemMediators)
            {
                itemMediator.Destroy();
            }

            ItemMediators.Clear();
            ItemClickedSignal.RemoveAllListeners();
            GameObject.Destroy(View.gameObject);
        }
        
        /// <summary>
        /// Method for selecting item in list from code instead of user input.
        /// Does not call internal list method ListItemSelected.
        /// </summary>
        /// <param name="index">item index</param>
        public void SelectItemAt(int index)
        {
            if(SelectedItemIndexes.Contains(index))
                return;
            
            //unselect old item
            if (!Multiselect && SelectedItemIndexes.Count > 0)
                ApplyItemUnselection(SelectedItemIndexes[0]);
            
            ApplyItemSelection(index);
            
            SelectionUpdated(index);
        }

        /// <summary>
        /// Method for unselecting item in list from code instead of user input.
        /// Does not call internal list method ListItemUnselected.
        /// </summary>
        /// <param name="index">item index</param>
        public void UnselectItemAt(int index)
        {   
            if(!SelectedItemIndexes.Contains(index))
                return;
            
            ApplyItemUnselection(index);
            
            SelectionUpdated(index);
        }
        
        public TData GetSelectedItem()
        {
            return SelectedItemIndexes.Count == 0 ? null : DataProviders[SelectedItemIndexes[0]];
        }
        
        public List<TData> GetSelectedItems()
        {
            List<TData> selectedItems = new List<TData>();
            foreach (var SelectedItemIndex in SelectedItemIndexes)
            {
                selectedItems.Add(DataProviders[SelectedItemIndex]);    
            }
            
            return selectedItems;
        }

        public int? GetSelectedIndex()
        {
            if (SelectedItemIndexes.Count > 0)
                return SelectedItemIndexes[0];
            
            return null;
        }

        public virtual void UnselectAll()
        {
            foreach (var selectedItemIndex in SelectedItemIndexes)
            {
                GetMediatorAt(selectedItemIndex).SetSelected(false);    
            }
            
            SelectedItemIndexes.Clear();
        }
        
        public virtual void RemoveItemAt(int index)
        {
            var provider = GetDataproviderAt(index);
            if (provider == null)
                return;

            DataProviders.Remove(provider);
            
            //set new index & disable unused items
            for (var i = index; i < ItemMediators.Count; i++)
            {
                if(i < DataProviders.Count)
                    GetMediatorAt(i).SetIndex(i);
                else
                    GetMediatorAt(i).SetActive(false);
            }
        }
        
        public TData GetDataproviderAt(int index)
        {
            return index < DataProviders.Count ? DataProviders[index] : null;
        }
        
        public int FindItemIndex(TData data)
        {
            var index = DataProviders.FindIndex((d) => d == data);
            return index;
        }
        
        #endregion

        #region Search helpers
        
        protected IListItemMediator<TData> FindItemMediatorByCustom<TSearched>(Func<TData, TSearched, bool> comparator, TSearched searchedItem)
        {
            var index = FindItemIndexByCustom(comparator, searchedItem);
            return index >= 0 ? GetMediatorAt(index) : null;
        }
        
        protected TData FindDataproviderByCustom<TSearched>(Func<TData, TSearched, bool> comparator, TSearched searchedItem)
        {
            var index = FindItemIndexByCustom(comparator, searchedItem);
            return index >= 0 ? GetDataproviderAt(index) : null;
        }
        
        protected int FindItemIndexByCustom<TSearched>(Func<TData, TSearched, bool> comparator, TSearched searchedItem)
        {
            for (var i = 0; i < DataProviders.Count; i++)
            {
                if (comparator(DataProviders[i], searchedItem))
                    return i;
            }

            return -1;
        }

        protected virtual IListItemMediator<TData> GetMediatorAt(int index)
        {
            return index < ItemMediators.Count ? ItemMediators[index] : null;
        }
        
        #endregion
        
        protected virtual GameObject CreateItemMediator(int index, GameObject prefab, RectTransform parent)
        {
            var view = UiManager.InstantiateView(prefab, parent);
            CreatedMediatorsCount++;
            
            return view;
        }
        
        protected virtual void SetItemData(IListItemMediator<TData> itemMediator, TData itemDataProvider, int index)
        {
            itemMediator.SetData(itemDataProvider, index);    
        }
        
        protected virtual void ApplyItemSelection(int index)
        {
            SelectedItemIndexes.Add(index);
            var mediator = GetMediatorAt(index);
            if (mediator != null)
                mediator.SetSelected(true);
        }
        
        protected virtual void ApplyItemUnselection(int index)
        {
            SelectedItemIndexes.Remove(index);
            var mediator = GetMediatorAt(index);
            if (mediator != null)
                mediator.SetSelected(false);
        }

        protected virtual void ListItemClicked(int index, TData dataProvider)
        {
            
        }
        
        protected virtual void ListItemSelected(int index, TData dataProvider)
        {
            
        }
        
        protected virtual void ListItemUnselected(int index, TData dataProvider)
        {

        }

        /// <summary>
        /// Called when the pointer enters an item. Requires ListItemView.PointerElement on the item.
        /// </summary>
        protected virtual void ListItemHoverEnter(int index, TData dataProvider)
        {

        }

        /// <summary>
        /// Called when the pointer exits an item. Requires ListItemView.PointerElement on the item.
        /// </summary>
        protected virtual void ListItemHoverExit(int index, TData dataProvider)
        {

        }

        protected virtual void SelectionUpdated(int index)
        {
            SelectionChangedSignal.Dispatch(GetSelectedIndex());
        }

        private void ListItemClickedHandler(int itemIndex, TData dataProvider)
        {
            if (SelectOnClick)
            {
                //select/unselected new item
                var selected = SelectedItemIndexes.Contains(itemIndex);
                if (!selected)
                {
                    SelectItemAt(itemIndex);
                    ListItemSelected(itemIndex, dataProvider);
                }
                else if (Unselectable)
                {
                    UnselectItemAt(itemIndex);
                    ListItemUnselected(itemIndex, dataProvider);
                }
            }

            ListItemClicked(itemIndex, dataProvider);
            ItemClickedSignal.Dispatch(itemIndex, dataProvider);
        }

        private void ListItemHoverHandler(int itemIndex, TData dataProvider, bool hovered)
        {
            if (hovered)
            {
                if (HoverSelects && !SelectedItemIndexes.Contains(itemIndex))
                {
                    SelectItemAt(itemIndex);
                    ListItemSelected(itemIndex, dataProvider);
                }

                ListItemHoverEnter(itemIndex, dataProvider);
            }
            else
            {
                ListItemHoverExit(itemIndex, dataProvider);
            }
        }
    }
}