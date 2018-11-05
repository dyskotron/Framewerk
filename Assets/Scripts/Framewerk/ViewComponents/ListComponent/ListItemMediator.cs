using Framewerk.Core;
using Framewerk.Mvcs;

namespace Framewerk.ViewComponents.ListComponent
{
    public interface IListItemDataProvider
    {

    }
    
    public interface IListItemMediator<T> : IMediator where T : IListItemDataProvider
    {
        void SetData(T dataProvider, int index);
        void SetIndex(int index);
        void SetSelected(bool selected);
        IEventDispatcher EventBus { set; }
    }

    public abstract class ListItemMediator<TView, TData> : Mediator<TView>, IListItemMediator<TData> where TView : ListItemView where TData : IListItemDataProvider
    {
        public IEventDispatcher EventBus { protected get; set;}
        
        protected bool IsSelected;
        protected bool IsEnabled = true;
        protected TData DataProvider;
        protected int Index;

        protected override void Init()
        {
            base.Init();
            
            if(View.SelectButton != null)
                AttachButtonListener(View.SelectButton, Select);
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
        
        public virtual void SetEnabled(bool enabled)
        {
            IsEnabled = enabled;
            View.SelectButton.interactable = enabled;
        }

        public virtual void SetSelected(bool selected)
        {
            IsSelected = selected;
        }

        protected void Select()
        {
            if(IsEnabled && EventBus != null)
                EventBus.DispatchEvent(new ListItemClickedInternalEvent(Index));    
        }
    }
}