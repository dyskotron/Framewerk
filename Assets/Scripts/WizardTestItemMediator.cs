using Framewerk.UI;

namespace Scripts
{
    public class WizardTestItemMediator : ExtendedMediator<WizardTestItemView>
    {
        private WizardTestData _data;

        public void SetData(WizardTestData data)
        {
            _data = data;
            UpdateView();
        }

        public override void OnRegister()
        {
            base.OnRegister();
        }

        private void UpdateView()
        {
            // TODO: Update view with data
        }
    }
}
