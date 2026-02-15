using strange.extensions.mediation.impl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Framewerk.Examples.TabsViewStackExample
{
    /// <summary>
    /// View for a content panel in the ViewStack.
    /// Contains Title and Description text fields, plus background image.
    /// </summary>
    public class ContentPanelView : View
    {
        public TextMeshProUGUI Title;
        public TextMeshProUGUI Description;
        public Image Background;
    }
}
