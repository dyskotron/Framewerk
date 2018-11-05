using System;
using System.Collections.Generic;
using Framewerk.Mvcs;
using UnityEngine.Events;

namespace Framewerk.ViewComponents.ButtonComponent
{
    [Serializable]
    public class ItemSelectedEvent : UnityEvent<int> { }

    public abstract class BaseSelectButtonMediator<TView, TValue> : Mediator<TView> where TView : SelectButtonView
    {
        public ItemSelectedEvent onSelect;
        private int _index;
        private List<TValue> _items;

        public int SelectedIndex { get { return _index; } }
        public TValue SelectedItem { get { return _items[_index]; } }

        public void SetItems(List<TValue> items)
        {
            if(items.Count < 1)
                return;

            _items = items;

            //reset index + set label without dispatching event
            _index = 0;
            DisplayIndex();
        }

        public void SetIndex(int index, bool silent = false)
        {
            if(_items == null)
                return;

            if(_index == index)
                return;

            _index = (index % _items.Count);

            DisplayIndex();

            if(!silent)
                onSelect.Invoke(_index);
        }

        protected override void Init()
        {
            onSelect = new ItemSelectedEvent();
            base.Init();
            AttachButtonListener(View.Button, ButtonClickHandler);
        }

        protected abstract void DisplayIndex();
        
        private void ButtonClickHandler()
        {
            SetIndex(_index + 1);
        }
    }
}