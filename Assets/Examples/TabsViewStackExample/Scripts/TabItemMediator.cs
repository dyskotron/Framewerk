using Framewerk.UI.List;

namespace Framewerk.Examples.TabsViewStackExample
{
    public class TabItemMediator : ListItemMediator<TabItemView, TabData>
    {
        public override void SetData(TabData dataProvider, int index)
        {
            base.SetData(dataProvider, index);
            
            if (dataProvider != null && View.Label != null)
            {
                View.Label.text = dataProvider.Title;
            }
        }
    }
}
