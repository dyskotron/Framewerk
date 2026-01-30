using Framewerk.UI;

namespace Scripts
{
    public class YourMumItemMediator : ExtendedMediator<YourMumItemView>
    {
        private YourMumData _data;

        public void SetData(YourMumData data)
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
