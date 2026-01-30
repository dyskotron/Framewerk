using Framewerk.UI;

namespace Scripts
{
    public class DeleteMeItemMediator : ExtendedMediator<DeleteMeItemView>
    {
        private DeleteMeData _data;

        public void SetData(DeleteMeData data)
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
