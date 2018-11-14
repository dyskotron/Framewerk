using System;
using Framewerk.Mvcs;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

namespace Framewerk.ViewComponents.ButtonComponent
{
    public class ButtonView : View
    {
		public bool Interactable {

			get { return Button.interactable; }
			set 
			{
				Button.interactable = value; 
				Label.color = value ? _defaultLabelColor : Button.colors.disabledColor;
			}
		}

        public Text Label;
	    
	    public Button Button { get { return _button; }}

	    [FormerlySerializedAs("Button")]
	    [SerializeField]
	    private Button _button;

		Color _defaultLabelColor;

		void Awake()
		{
			_defaultLabelColor = Label.color;
		}

        public void SetLabel(string text)
        {
            Label.text = text;
        }
    }
}