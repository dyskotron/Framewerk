using System;
using System.Collections.Generic;
using Framewerk.Core;
using Framewerk.Mvcs;
using UnityEngine;

namespace Framewerk.ViewComponents.ListComponent
{
    /// <summary>
    /// Mediates View containing list of items.
    /// </summary>
    /// <typeparam name="TView">View scipt attached to container gameobject</typeparam>
    /// <typeparam name="TMediator">Mediator of single item in list</typeparam>
    /// <typeparam name="TData">Dataprovider for single item in list </typeparam>
    public class ListBaseMediator<TView, TMediator, TData> : Mediator<TView>, IListContainerMediator<TData> where TView : ListContainerView
                                                                                  where TMediator : class, IListItemMediator<TData>, new()
                                                                                  where TData : class, IListItemDataProvider
    {
        public IEventDispatcher EventBus{ get; private set; }
        public List<int> SelectedItemIndexes { get; private set; }
        public bool Multiselect { get; set; }
        public bool Unselectable { get; set; }

        protected List<TMediator> ItemMediators = new List<TMediator>();
        protected List<TData> DataProviders = new List<TData>();
        
        protected override void Init()
        {
            EventBus = new EventDispatcher();
            SelectedItemIndexes = new List<int>();
            
            EventBus.AddListener<ListItemClickedInternalEvent>(ListItemClickedHandler);
            
            base.Init();
        }

        #region Public API

        public virtual void SetData(List<TData> dataProviders)
        {
            //disable contents parent to avoid dirtying whole structure with every item
            //View.ContentsParent.gameObject.SetActive(false);
            
            //just remove selected indexes and shit, dont' do actual unselectng on list items?
            UnselectAll();
            
            if (View.EmptyContent != null)
                 View.EmptyContent.SetActive(dataProviders.Count == 0);

            DataProviders = dataProviders;
            
            //View.ContentsParent.gameObject.SetActive(true); 
        }

        public override void Destroy()
        {
            foreach (var itemMediator in ItemMediators)
            {
                itemMediator.Destroy();
            }
            ItemMediators.Clear();
            
            EventBus.RemoveAll();
            
            base.Destroy();
        }
        
        /// <summary>
        /// Method for selecting item in list from code instead of user input.
        /// Does not dispatch event or calls internal list method ListItemSelected.
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
        /// Does not dispatch event or calls internal list method ListItemUnselected.
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
        
        protected TMediator FindItemMediatorByCustom<TSearched>(Func<TData, TSearched, bool> comparator, TSearched searchedItem)
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

        protected virtual TMediator GetMediatorAt(int index)
        {
            return index < ItemMediators.Count ? ItemMediators[index] : null;
        }
        
        #endregion
        
        protected virtual TMediator CreateItemMediator(int index, GameObject prefab, RectTransform parent)
        {
            var mediator = UIManager.CreateUIMediator<TMediator>(prefab, parent);
            mediator.EventBus = EventBus;
            return mediator;    
        }
        
        protected virtual void SetItemData(TMediator itemMediator, TData itemDataProvider, int index)
        {
            itemMediator.SetData(itemDataProvider, index);    
        }
        
        protected virtual void ApplyItemSelection(int index)
        {
            SelectedItemIndexes.Add(index); 
            GetMediatorAt(index).SetSelected(true);    
        }
        
        protected virtual void ApplyItemUnselection(int index)
        {
            SelectedItemIndexes.Remove(index);
            GetMediatorAt(index).SetSelected(false);    
        }

        protected virtual void ListItemClicked(int index)
        {
            
        }
        
        protected virtual void ListItemSelected(int index)
        {
            
        }
        
        protected virtual void ListItemUnselected(int index)
        {
            
        }

        protected virtual void SelectionUpdated(int index)
        {
            
        }
        
        private void ListItemClickedHandler(ListItemClickedInternalEvent e)
        {   
            //select/unselected new item
            var selected = SelectedItemIndexes.Contains(e.ItemIndex);
            if (!selected)
            {
                SelectItemAt(e.ItemIndex);
                SelectionUpdated(e.ItemIndex);
                ListItemSelected(e.ItemIndex);
            }
            else if (Unselectable)
            {
                UnselectItemAt(e.ItemIndex);
                SelectionUpdated(e.ItemIndex);
                ListItemUnselected(e.ItemIndex);
            }
            
            ListItemClicked(e.ItemIndex);
            
            EventBus.DispatchEvent(new ListItemClickedEvent(e.ItemIndex));
            
            //fire corresponding events
            if (!selected)
                EventBus.DispatchEvent(new ListItemSelectedEvent(e.ItemIndex));
            else if (Unselectable)
                EventBus.DispatchEvent(new ListItemUnselectedEvent(e.ItemIndex));
            
        }
    }
}