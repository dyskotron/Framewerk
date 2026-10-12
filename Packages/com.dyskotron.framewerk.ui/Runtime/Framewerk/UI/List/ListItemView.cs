using Framewerk.UI.Components;
using strange.extensions.mediation.impl;
using UnityEngine;
using UnityEngine.UI;

namespace Framewerk.UI.List
{
    public class ListItemView : View
    {
        public Button SelectButton;

        [Tooltip("Optional. When assigned, the item reports pointer enter/exit as hover to its list.")]
        public PointerElement PointerElement;
    }
}