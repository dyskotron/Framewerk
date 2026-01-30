using Framewerk.UI;
using System.Collections.Generic;
using UnityEngine;

namespace FramewerkDemo.Examples.WizardList
{
    public class WizardListMediator : ExtendedMediator<WizardListView>
    {
        [Inject] public Framewerk.Managers.IAssetManager AssetManager { get; set; }
        [Inject] public Framewerk.Managers.IUiManager UiManager { get; set; }

        private List<GameObject> _itemInstances = new List<GameObject>();

        public override void OnRegister()
        {
            base.OnRegister();

            // Create dummy data
            var items = new List<WizardListData>
            {
                new WizardListData("Wizard Item 1", 42),
                new WizardListData("Wizard Item 2", 73),
                new WizardListData("Wizard Item 3", 15),
                new WizardListData("Wizard Item 4", 99),
                new WizardListData("Wizard Item 5", 27)
            };

            PopulateList(items);
        }

        private async void PopulateList(List<WizardListData> items)
        {
            // Clear existing items
            foreach (var item in _itemInstances)
            {
                if (item != null)
                    Object.Destroy(item);
            }
            _itemInstances.Clear();

            // Instantiate items
            foreach (var data in items)
            {
                var itemGo = await UiManager.InstantiateViewAsync("Examples/WizardList/WizardListItem", View.itemContainer);
                _itemInstances.Add(itemGo);

                var itemView = itemGo.GetComponent<WizardListItemView>();
                var itemMediator = itemGo.GetComponent<WizardListItemMediator>();
                if (itemMediator != null)
                {
                    itemMediator.SetData(data);
                }
            }
        }

        public override void OnRemove()
        {
            // Clean up items
            foreach (var item in _itemInstances)
            {
                if (item != null)
                    Object.Destroy(item);
            }
            _itemInstances.Clear();

            base.OnRemove();
        }
    }
}
