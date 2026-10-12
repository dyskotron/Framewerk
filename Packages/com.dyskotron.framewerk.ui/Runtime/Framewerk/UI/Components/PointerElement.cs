using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Framewerk.UI.Components
{
    public class PointerElement : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public class OnPointerChangeEvent : UnityEvent<bool, Vector2>
        {

        }

        public class OnHoverChangeEvent : UnityEvent<bool>
        {

        }

        /// <summary>
        /// Invoked on pointer down (true) and pointer up (false) with the pointer position.
        /// </summary>
        public OnPointerChangeEvent OnPointerChanged
        {
            get { return _onPointerChanged; }
        }

        /// <summary>
        /// Invoked on pointer enter (true) and pointer exit (false).
        /// </summary>
        public OnHoverChangeEvent OnHoverChanged
        {
            get { return _onHoverChanged; }
        }

        private readonly OnPointerChangeEvent _onPointerChanged = new OnPointerChangeEvent();
        private readonly OnHoverChangeEvent _onHoverChanged = new OnHoverChangeEvent();

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            _onPointerChanged.Invoke(true, eventData.position);
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            _onPointerChanged.Invoke(false, eventData.position);
        }

        public void OnPointerClick(PointerEventData eventData)
        {

        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            _onHoverChanged.Invoke(true);
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            _onHoverChanged.Invoke(false);
        }
    }
}
