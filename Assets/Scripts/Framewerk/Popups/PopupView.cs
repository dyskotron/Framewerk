using strange.extensions.mediation.impl;
using UnityEngine;
using UnityEngine.UI;

namespace Framewerk.Popups
{
    public class PopupView : View, IPopupView
    {
        public Transform buttonContainer;
        public GameObject buttonPrefab;
    }
}