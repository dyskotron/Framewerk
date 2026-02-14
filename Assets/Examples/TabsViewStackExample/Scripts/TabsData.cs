using System.Collections.Generic;
using UnityEngine;

namespace Framewerk.Examples.TabsViewStackExample
{
    /// <summary>
    /// Data for a content panel in the ViewStack.
    /// </summary>
    public class ContentPanelData
    {
        public string Title { get; set; }
        public Color BackgroundColor { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// Container for tabs and their associated content panel definitions.
    /// Passed via mediatorInjects to TabContainerMediator and ViewStackMediator.
    /// </summary>
    public class TabsData
    {
        public List<TabData> Tabs { get; set; }
        public List<ContentPanelData> ContentPanels { get; set; }
    }
}
