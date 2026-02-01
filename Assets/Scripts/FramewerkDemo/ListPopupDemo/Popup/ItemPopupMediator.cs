using Framewerk.Popups;

namespace FramewerkDemo.ListPopupDemo
{
    public class ItemPopupMediator : PopupMediator<ItemPopupView>
    {
        private ItemData _itemData;

        public void Initialize(ItemData itemData)
        {
            _itemData = itemData;
        }

        public override void OnRegister()
        {
            base.OnRegister();

            if (_itemData != null)
            {
                View.SetValue(_itemData.Value);
            }

            AddButtonListener(View.CloseButton, OnCloseClicked);
        }

        private void OnCloseClicked()
        {
            Close();
        }
    }
}
