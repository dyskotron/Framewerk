using System.Collections.Generic;
using Framewerk.Mvcs;

namespace FramewerkDemo.MainMenu.Model
{
    public enum ExampleId
    {
        Popup,
        Tabs,
        List,
        VirtualList
        
        
    }
    public interface IMenuModel
    {
        List<MenuDataProvider> GetMenuData();
        MenuDataProvider GetMenuItemById(ExampleId id);
    }
    
    public class MenuModel : Actor, IMenuModel
    {
        private Dictionary<ExampleId, string> _titles;
        private List<MenuDataProvider> _providers;

        public MenuModel()
        {
            _titles = new Dictionary<ExampleId, string>();
            _titles.Add(ExampleId.Popup, "Simple Popup example");
            _titles.Add(ExampleId.Tabs, "Tab Container example");
            _titles.Add(ExampleId.List, "List example");
            _titles.Add(ExampleId.VirtualList, "Virtual List example");
            
            _providers = new List<MenuDataProvider>();
            _providers.Add(CreateDataProvider(ExampleId.Popup));
            _providers.Add(CreateDataProvider(ExampleId.Tabs));
            _providers.Add(CreateDataProvider(ExampleId.List));
            _providers.Add(CreateDataProvider(ExampleId.VirtualList));
        }
        
        public List<MenuDataProvider> GetMenuData()
        {
            return _providers;
        }

        public MenuDataProvider GetMenuItemById(ExampleId id)
        {
            return CreateDataProvider(id);
        }

        private MenuDataProvider CreateDataProvider(ExampleId exampleId)
        {
            return new MenuDataProvider(exampleId, _titles[exampleId]);
        }
    }
}