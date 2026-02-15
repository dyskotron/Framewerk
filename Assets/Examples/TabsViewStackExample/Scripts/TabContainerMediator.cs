using Framewerk.UI.List;

namespace Framewerk.Examples.TabsViewStackExample
{
    /// <summary>
    /// Tab container mediator - handles ONLY tab selection.
    /// 
    /// This mediator follows the ViewGroup pattern:
    /// - It handles its own concern (tab list) and nothing else
    /// - SelectionChangedSignal (inherited from ListBaseMediator with [ViewGroupShared])
    ///   is automatically shared with ViewStackMediator
    /// - When a tab is selected, the signal is dispatched
    /// - ViewStackMediator listens to the same signal and shows the corresponding content
    /// 
    /// No manual wiring, no GetComponent, no coupling to ViewStack.
    /// </summary>
    public class TabContainerMediator : ListMediator<TabContainerView, TabData>
    {
        [Inject] public TabsData TabsData { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();

            SetData(TabsData.Tabs);

            // Auto-select first tab
            // The ViewGroup pattern automatically syncs this to ViewStackMediator
            SelectItemAt(0);
        }
    }
}
