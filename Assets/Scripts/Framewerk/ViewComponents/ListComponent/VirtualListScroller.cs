using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Framewerk.ViewComponents.ListComponent
{
    [RequireComponent(typeof(ScrollRect))]
    public class VirtualListScroller : MonoBehaviour
    {
        public int NumRenderers = 10;
        
        public delegate void ItemIndexChangedDelegate(int itemIndexIncrement);
        public event ItemIndexChangedDelegate ItemIndexChangedEvent;
        
        [Header("Layout")]
        public float PaddingTop;
        public float PaddingBottom;
        public float NormalItemSize;
        public float SelectedItemSize;
        
        private List<int> _selectedItemIndexes = new List<int>();
        
        private ScrollRect _scrollRect;
        private RectTransform _content;
        private bool _isHorizontal;
        private bool _isVertical;
        private List<RectTransform> _items;

        private RectTransform _scrollRectTransform;

        private VirtualizedListMapper _mapper;
        
        public void Init()
        {
            _scrollRect = GetComponent<ScrollRect>();
            _scrollRect.onValueChanged.AddListener(OnScroll);
            _scrollRectTransform = GetComponent<RectTransform>();
            _scrollRectTransform.pivot = Vector2.up;

            NumRenderers = Mathf.CeilToInt(_scrollRectTransform.rect.height / NormalItemSize) + 2;
            
            _content = _scrollRect.content;
            
            _items = new List<RectTransform>();
            //TODO feed items directly from list mediator
            for (int i = 0; i < _content.childCount; i++)
            {
                var item = _scrollRect.content.GetChild(i).GetComponent<RectTransform>();
                if (item.GetComponent<ListItemView>())
                {
                    item.pivot = new Vector2(item.pivot.x, 1);
                    _items.Add(item);
                }
            }

            if (_scrollRect.horizontal)
            {
                Debug.LogErrorFormat("{0} currently support only vertical scroll", this);
            }
        }

        public void UpdateAfterSettingData(int dataprovidersCount)
        {
            _mapper = new VirtualizedListMapper(NumRenderers, dataprovidersCount);
            
            //update item pos
            for (var i = 0; i < _mapper.ViewCount; i++)
            {
                UpdateItemPos(i);    
            }    
            
            UpdateContentSize();
            
            //reset position
            _content.anchoredPosition = Vector2.zero;
        }

        public void LayoutAfterSelection(int changeDataIndex, List<int> selectedIndexes)
        {
            _selectedItemIndexes = selectedIndexes;
            
            for (var i = _mapper.FirstDataIndex; i <= _mapper.LastDataIndex; i++)
            {
                UpdateItemPos(i);
            }

            UpdateContentSize();
        }

        private void UpdateItemPos(int dataIndex)
        {
            var item = _items[_mapper.GetViewIndex(dataIndex)];
            var newY = dataIndex * NormalItemSize + (SelectedItemSize - NormalItemSize) * GetSelectedItemsCountBefore(dataIndex) + PaddingTop;
            
            var anchoredPosition = item.anchoredPosition;
            anchoredPosition.y = -newY;
            item.anchoredPosition = anchoredPosition;
        }

        private void UpdateContentSize()
        {
            var height = _mapper.DataCount * NormalItemSize + (SelectedItemSize - NormalItemSize) * _selectedItemIndexes.Count + PaddingBottom + PaddingTop;
            _content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,  height);
        }

        private int GetSelectedItemsCountBefore(int itemIndex)
        {
            var i = 0;
            foreach (var selectedIndex in _selectedItemIndexes)
            {
                if (selectedIndex < itemIndex)
                    i++;
            }

            return i;
        }

        private void OnScroll(Vector2 pos)
        {
            //todo: deselect all items when scrolling starts
            
            //when scroll is inited before ListMediator sets data
            if(_mapper == null)
                return;
            
            //get first item
            var firstItem = _items[_mapper.FirstViewIndex];

            var i = 0;
            while (i < _mapper.ViewCount)
            {

                //is first item higher than remove item treshold?
                if (_scrollRect.transform.InverseTransformPoint(firstItem.position).y > 1.5 * NormalItemSize && _mapper.LastDataIndex < _mapper.DataCount - 1)
                {
                    //MOVE FIRST ITEM TO BOTTOM
                    UpdateVirtualIndex(1);
                    UpdateItemPos(_mapper.LastDataIndex);
                }
                //is first item lower than add item treshold?
                // Trigger for removing item always have to be more than item size lower than trigger for adding new item
                // so it will not trigger adding item right after removing one
                else if (_scrollRect.transform.InverseTransformPoint(firstItem.position).y < NormalItemSize / 2 && _mapper.FirstDataIndex > 0)
                {
                    //MOVE LAST ITEM TO TOP
                    UpdateVirtualIndex(-1);
                    UpdateItemPos(_mapper.FirstDataIndex);
                }
                else
                {
                    return;
                }
                
                firstItem = _items[_mapper.FirstViewIndex];
                i++;
            }
        }

        private void UpdateVirtualIndex(int change)
        {
            _mapper.FirstDataIndex += change;
            
            if(ItemIndexChangedEvent != null)
                ItemIndexChangedEvent(change);
        }
    }
}