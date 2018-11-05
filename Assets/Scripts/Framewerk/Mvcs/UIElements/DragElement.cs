using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Framewerk.Mvcs.UIElements
{
    public enum DragStateType
    {
        Start,
        Drag,
        End
    }

    public class DragElement : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        public class OnDragChangeEvent : UnityEvent<DragStateType>
        {

        }
        public OnDragChangeEvent OnDragChanged
        {
            get
            {
                return _onDragEvent;
            }
        }
        private readonly OnDragChangeEvent _onDragEvent = new OnDragChangeEvent();

        public void OnBeginDrag(PointerEventData eventData)
        {
            _onDragEvent.Invoke(DragStateType.Start);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _onDragEvent.Invoke(DragStateType.End);
        }


    }
}