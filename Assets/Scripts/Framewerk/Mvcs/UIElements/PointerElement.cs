using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Framewerk.Mvcs.UIElements
{
    public class PointerElement : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public class OnPointerChangeEvent : UnityEvent<bool>
        {

        }

        public OnPointerChangeEvent OnPointerChanged
        {
            get { return _onPointerChanged; }
        }

        private readonly OnPointerChangeEvent _onPointerChanged = new OnPointerChangeEvent();

        public void OnPointerDown(PointerEventData eventData)
        {
            _onPointerChanged.Invoke(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _onPointerChanged.Invoke(false);
        }
    }
}