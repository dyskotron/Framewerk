using Framewerk.Mvcs;
using UnityEngine.UI;
using UnityEngine;

namespace Framewerk.ViewComponents.ButtonComponent
{
    public class SelectButtonView : View
    {
		public bool Interactable {

			get { return Button.interactable; }
			set 
			{
				Button.interactable = value; 
				if(Label != null)
				{
					Label.color = value ? _defaulfLabelColor : Button.colors.disabledColor;
				}
			}
		}

        public Text Label;
        public Button Button;

		Color _defaulfLabelColor;

		void Awake()
		{
			if(Label != null)
				_defaulfLabelColor = Label.color;
				
		}

        public void SetLabel(string text)
        {
            if (Label != null)
                Label.text = text;
        }
    }
}