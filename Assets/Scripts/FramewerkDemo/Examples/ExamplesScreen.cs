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
            await InstantiateViewAsync<TopMenuView>("Examples/");

            switch (exampleId)
            {
                case ExampleId.Popup:
                    await PopupManager.InstantiatePopupAsync<ExamplePopupView>();
                    break;
                case ExampleId.List:
                    await InstantiateViewAsync<ExampleListPanelView>("ListPanel/");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(exampleId), exampleId, null);
            }
        }
    }
}
