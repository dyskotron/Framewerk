using System.Threading.Tasks;
using Framewerk.Managers;
using Framewerk.UI.ViewStack;
using UnityEngine;

namespace Framewerk.Examples.TabsViewStackExample
{
    /// <summary>
    /// ViewStack mediator - handles ONLY content display.
    /// 
    /// This mediator follows the ViewGroup pattern:
    /// - It handles its own concern (content panels) and nothing else
    /// - SelectionChangedSignal (inherited from ViewStackMediator with [ViewGroupShared])
    ///   is automatically shared with TabContainerMediator
    /// - When TabContainerMediator dispatches a selection change, this mediator
    ///   automatically shows the corresponding content panel
    /// 
    /// No manual wiring, no GetComponent, no coupling to tabs.
    /// </summary>
    public class TabsViewStackMediator : ViewStackMediator
    {
        [Inject] public TabsData TabsData { get; set; }
        [Inject] public IUiManager UiManager { get; set; }

        public override void OnRegister()
        {
            // Create panels before base.OnRegister() so ShowByIndex(0) has panels to show
            _ = CreateContentPanelsAsync();
        }

        private async Task CreateContentPanelsAsync()
        {
            if (View.Content == null)
            {
                Debug.LogError("[TabsViewStackExample] ViewStack Content is null - check prefab configuration");
                base.OnRegister();
                return;
            }

            if (TabsData?.ContentPanels == null)
            {
                Debug.LogError("[TabsViewStackExample] TabsData or ContentPanels is null");
                base.OnRegister();
                return;
            }

            foreach (var panel in TabsData.ContentPanels)
            {
                await UiManager.InstantiateViewAsync<ContentPanelView>(
                    customPrefix: null,
                    parent: View.Content,
                    ct: default,
                    mediatorInjects: panel);
            }

            Debug.Log($"[TabsViewStackExample] Created {TabsData.ContentPanels.Count} content panels");
            
            // Call base after panels are created
            base.OnRegister();
        }
    }
}
