using strange.extensions.mediation.impl;
using TMPro;
using UnityEngine.UI;

namespace Framewerk.Examples.ScreenFsmExample
{
    public class GameSubstateView : View
    {
        public TMP_Text SubstateLabel;
        public Button SwitchButton;

        public void SetSubstate(string substateName)
        {
            if (SubstateLabel != null)
                SubstateLabel.text = $"Substate: {substateName}";
        }
    }
}
