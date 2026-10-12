using strange.extensions.mediation.api;
using strange.extensions.signal.impl;
using UnityEngine;

namespace Framewerk.UI.List
{
    public interface IListItemDataProvider
    {
    }

    public interface IListItemMediator<TData> : IMediator where TData : IListItemDataProvider
    {
        Signal<int, TData> ListItemClickedSignal { get; }

        /// <summary>
        /// Dispatched on pointer enter (true) / exit (false). Requires ListItemView.PointerElement to be assigned.
        /// </summary>
        Signal<int, TData, bool> ListItemHoverSignal { get; }

        void SetData(TData dataProvider, int index);
        void SetIndex(int index);
        void SetSelected(bool selected);
        void SetHovered(bool hovered);
        void SetActive(bool active);
        void Destroy();
    }

    public abstract class ListItemMediator<TView, TData> : ExtendedMediator<TView>, IListItemMediator<TData> where TView : ListItemView
                                                                                                      where TData : IListItemDataProvider
    {

        public Signal<int, TData> ListItemClickedSignal { get; } = new Signal<int, TData>();
        public Signal<int, TData, bool> ListItemHoverSignal { get; } = new Signal<int, TData, bool>();

        protected bool IsSelected;
        protected bool IsHovered;
        protected bool IsEnabled = true;
        protected TData DataProvider;
        protected int Index;

        public override void OnRegister()
        {
            base.OnRegister();

            RegisterToList();

            if (View.SelectButton != null)
                AddButtonListener(View.SelectButton, Select);

            if (View.PointerElement != null)
                AddHoverListener(View.PointerElement, Hover);
        }

        public virtual void SetData(TData dataProvider, int index)
        {
            DataProvider = dataProvider;
            Index = index;
        }

        public void SetIndex(int index)
        {
            Index = index;
        }
        
        public void SetActive(bool active)
        {
            View.gameObject.SetActive(active);
        }

        public virtual void SetEnabled(bool enabled)
        {
            IsEnabled = enabled;
            View.SelectButton.interactable = enabled;
        }

        public virtual void EnableSelectButton(bool state)
        {
            View.SelectButton.enabled = state;
        }

        public virtual void SetSelected(bool selected)
        {
            IsSelected = selected;
        }

        /// <summary>
        /// Called when the pointer enters (true) or exits (false) the item. Override for hover visuals.
        /// Hover is independent from selection; the list decides whether hover changes selection.
        /// </summary>
        public virtual void SetHovered(bool hovered)
        {
            IsHovered = hovered;
        }

        public void Destroy()
        {
            ListItemClickedSignal.RemoveAllListeners();
            ListItemHoverSignal.RemoveAllListeners();
        }

        protected virtual void OnClick()
        {

        }

        private void Select()
        {
            if (!IsEnabled)
                return;

            ListItemClickedSignal.Dispatch(Index, DataProvider);
            OnClick();
        }

        private void Hover(bool hovered)
        {
            if (!IsEnabled)
                return;

            SetHovered(hovered);
            ListItemHoverSignal.Dispatch(Index, DataProvider, hovered);
        }

        private void RegisterToList()
        {
            const int LOOP_MAX = 100;
            int loopLimiter = 0;
            Transform trans = gameObject.transform;
            while (trans.parent != null && loopLimiter < LOOP_MAX)
            {
                loopLimiter++;
                trans = trans.parent;
                IListItemParent itemParent = trans.gameObject.GetComponent<IListItemParent>();
                if (itemParent != null)
                {
                    itemParent.RegisterMediator(this);
                    return;
                }
            }

            Debug.LogError($"<color=\"aqua\">ListItemMediator.RegisterToList() : IListItemParent not found</color>");
        }
    }
}