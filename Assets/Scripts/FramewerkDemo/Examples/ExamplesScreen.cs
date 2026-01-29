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
            InstantiateView<TopMenuView>("Examples/");

            switch (exampleId)
            {
                case ExampleId.Popup:
                    PopupManager.InstantiatePopup<ExamplePopupView>();
                    break;
                case ExampleId.List:
                    UiManager.InstantiateView<ExampleListPanelView>("ListPanel/");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(exampleId), exampleId, null);
            }
        }
    }
}
