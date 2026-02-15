using Framewerk.UI.List;

namespace Framewerk.Examples.TabsViewStackExample
{
    /// <summary>
    /// View for tab container - just a list of tabs.
    /// Does NOT contain ViewStack reference - the ViewGroup pattern keeps them separate.
    /// </summary>
    public class TabContainerView : ListView
    {
        // No ContentStack or ContentArea - ViewStack is a separate view in the ViewGroup
    }
}
