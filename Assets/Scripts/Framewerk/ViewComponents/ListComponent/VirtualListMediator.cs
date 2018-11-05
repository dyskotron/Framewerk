using System.Collections.Generic;
using UnityEngine;

namespace Framewerk.ViewComponents.ListComponent
{
    /// <summary>
    /// Mediates View containing list of items. Only visible items are rendered rest is virtualized so it can handle a lot of items.
    /// Needs VirtualListScroller to be attached on ScrollRect and assigned to ListView.
    /// </summary>
    /// <typeparam name="TView">View scipt attached to container gameobject</typeparam>
    /// <typeparam name="TMediator">Mediator of single item in list</typeparam>
    /// <typeparam name="TData">Dataprovider for single item in list </typeparam>
    public class VirtualListMediator<TView, TMediator, TData> : ListBaseMediator<TView, TMediator, TData>, IListContainerMediator<TData> where TView : ListContainerView
                                                                                  where TMediator : class, IListItemMediator<TData>, new()
                                                                                  where TData : class, IListItemDataProvider
    {
        private VirtualizedListMapper _mapper;
        private int _visibleMediatorsCount = 0;

        protected override void Init()
        {
            base.Init();

            if (View.ListScroller == null)
            {
                Debug.LogWarningFormat("<color=\"aqua\">{0} needs {1} to work properly.</color>", this, typeof(VirtualListScroller));
                return;
            }

            View.ListScroller.ItemIndexChangedEvent += ItemIndexChangedHandler;
            
            //Create list item mediators
            for (var i = 0; i < View.ListScroller.NumRenderers; i++)
            {
                ItemMediators.Add(CreateItemMediator(i, View.ItemPrefab, View.ContentsParent));
            }
            
            View.ListScroller.Init();
        }

        private void ItemIndexChangedHandler(int increment)
        {   
            //todo check min / max index
            _mapper.FirstDataIndex += increment;
            
            //scrolling down - update last item
            //scrolling up - update first item
            //var viewIndex = increment > 0 ? _mapper.LastViewIndex : _mapper.FirstViewIndex;
            var dataIndex = increment > 0 ? _mapper.LastDataIndex : _mapper.FirstDataIndex;
            var mediator = GetMediatorAt(dataIndex);
            
            SetItemData(mediator, DataProviders[dataIndex], dataIndex);
            mediator.SetSelected(SelectedItemIndexes.Contains(dataIndex));
        }

        #region Public API

        public virtual void SetData(List<TData> dataProviders)
        {
            base.SetData(dataProviders);
            
            _mapper = new VirtualizedListMapper(View.ListScroller.NumRenderers, dataProviders.Count);
            
            View.ListScroller.UpdateAfterSettingData( dataProviders.Count);
            
            //fill mediators with data
            for (var i = 0; i < _mapper.ViewCount; i++)
            {
                var mediator = GetMediatorAt(i);
                if(i >= _visibleMediatorsCount)
                    mediator.SetActive(true);
                
                SetItemData(mediator, dataProviders[i], i);
            }

            //hide remaining item renderers
            for (var i = _mapper.ViewCount; i < ItemMediators.Count; i++)
            {
                ItemMediators[i].SetActive(false);
            }

            _visibleMediatorsCount = _mapper.ViewCount;
        }
        
        #endregion
        
        #region Base class overrides
        
        public override void UnselectAll()
        {
            if(SelectedItemIndexes.Count == 0)
                return;
            
            //get lowestIndex
            var lowestIndex = _mapper.DataCount;
            foreach (var selectedItemIndex in SelectedItemIndexes)
                lowestIndex = Mathf.Min(lowestIndex, selectedItemIndex);
            
            
            foreach (var selectedItemIndex in SelectedItemIndexes)
            {
                if(_mapper.IsItemVisible(selectedItemIndex))
                    GetMediatorAt(selectedItemIndex).SetSelected(false);    
            }
            
            SelectedItemIndexes.Clear();
            
            View.ListScroller.LayoutAfterSelection(lowestIndex, SelectedItemIndexes);
        }
        
        public override void RemoveItemAt(int index)
        {
            var provider = GetDataproviderAt(index);
            if (provider == null)
                return;

            DataProviders.Remove(provider);
            
            //TODO: Introduce better optimalized method
            //currently it uses SetData method as its needed to do most of stuff done there(mainly remapping item mediators etc)
            SetData(DataProviders);
        }
        
        protected override void ApplyItemSelection(int index)
        {
            SelectedItemIndexes.Add(index);
            
            if(_mapper.IsItemVisible(index))
                GetMediatorAt(index).SetSelected(true); 
        }
        
        protected override void ApplyItemUnselection(int index)
        {   
            SelectedItemIndexes.Remove(index);  
            
            if(_mapper.IsItemVisible(index))
                GetMediatorAt(index).SetSelected(false);
        }

        protected override void SelectionUpdated(int index)
        {
            base.SelectionUpdated(index);
            View.ListScroller.LayoutAfterSelection(index, SelectedItemIndexes);
        }

        protected override TMediator GetMediatorAt(int index)
        {
            return _mapper.IsItemVisible(index) ? ItemMediators[_mapper.GetViewIndex(index)] : null;
        }
        
        #endregion
    }
}