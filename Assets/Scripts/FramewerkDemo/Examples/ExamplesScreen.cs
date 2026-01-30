using System;
using Framewerk.AppStateMachine;
using Framewerk.Popups;
using FramewerkDemo.Examples.ExampleListPanel;
using FramewerkDemo.Examples.ExamplePopup;
using FramewerkDemo.MainMenu.Model;

namespace FramewerkDemo.Examples
{
    public class ExamplesScreen : AppStateScreen
    {
        [Inject] public IPopupManager PopupManager { get; set; }

        public void InitExample(ExampleId exampleId)
        {
            switch (exampleId)
            {
                case ExampleId.Popup:
                    InstantiateView<TopMenuView>("Examples");
                    PopupManager.InstantiatePopup<ExamplePopupView>();
                    break;
                case ExampleId.List:
                    InstantiateView<TopMenuView>("Examples");
                    InstantiateView<ExampleListPanelView>("Examples/ListPanel");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(exampleId), exampleId, null);
            }
        }
    }
}
