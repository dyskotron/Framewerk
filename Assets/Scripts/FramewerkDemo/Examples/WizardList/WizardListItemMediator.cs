using Framewerk.UI;
using UnityEngine;

namespace FramewerkDemo.Examples.WizardList
{
    public class WizardListItemMediator : ExtendedMediator<WizardListItemView>
    {
        private WizardListData _data;

        public void SetData(WizardListData data)
        {
            _data = data;
            UpdateView();
        }

        public override void OnRegister()
        {
            base.OnRegister();

            if (View.button != null)
            {
                AddButtonListener(View.button, OnButtonClicked);
            }
        }

        private void UpdateView()
        {
            if (_data != null && View.label != null)
            {
                View.label.text = $"{_data.Title} (Value: {_data.Value})";
            }
        }

        private void OnButtonClicked()
        {
            Debug.Log($"Clicked: {_data?.Title}");
        }
    }
}
