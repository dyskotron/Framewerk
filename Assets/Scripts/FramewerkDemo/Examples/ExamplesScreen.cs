using System;
using System.Threading.Tasks;
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

        public async Task InitExampleAsync(ExampleId exampleId)
        {
            switch (exampleId)
            {
                case ExampleId.Popup:
                    InstantiateView<TopMenuView>("Examples/");
                    await PopupManager.InstantiatePopupAsync<ExamplePopupView>();
                    break;
                case ExampleId.List:
                    InstantiateView<TopMenuView>("Examples/");
                    InstantiateView<ExampleListPanelView>("ListPanel/");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(exampleId), exampleId, null);
            }
        }
    }
}
