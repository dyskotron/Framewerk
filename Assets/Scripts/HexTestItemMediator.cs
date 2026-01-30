using Framewerk.UI;

namespace Scripts
{
    public class HexTestItemMediator : ExtendedMediator<HexTestItemView>
    {
        private HexTestData _data;

        public void SetData(HexTestData data)
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
