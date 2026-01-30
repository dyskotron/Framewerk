using Framewerk.UI;

namespace FramewerkDemo.Examples.WizardList
{
    public class WizTest_2ItemMediator : ExtendedMediator<WizTest_2ItemView>
    {
        private WizTest_2Data _data;

        public void SetData(WizTest_2Data data)
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
