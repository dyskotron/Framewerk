using Framewerk.Core;
using Framewerk.Managers;
using Framewerk.Managers.StateMachine;
using Framewerk.ViewComponents.ListComponent;
using FramewerkDemo.MainMenu.Model;

namespace FramewerkDemo.MainMenu
{
    public class MenuScreen : AppStateScreen
    {
        private const string MENU_UI_ROOT = "Menu/";
        
        [Inject] private IMenuModel _menuModel;
        [Inject] private IUIManager _uiManager;
        [Inject] private IEventDispatcher _eventDispatcher;

        private MenuPanelMediator _menuPanelList;
        
        
        public override void In()
        {
            base.In();

            _menuPanelList = _uiManager.CreateUIMediator<MenuPanelMediator>(MENU_UI_ROOT);
            _menuPanelList.SetData(_menuModel.GetMenuData());
            _menuPanelList.EventBus.AddListener<ListItemClickedEvent>(MenuItemClickedHandler);
        }

        public override void Out()
        {
            _menuPanelList.Destroy();
            base.Out();
        }

        private void MenuItemClickedHandler(ListItemClickedEvent e)
        {
            var item = _menuPanelList.GetDataproviderAt(e.ItemIndex);
            _eventDispatcher.DispatchEvent(new MenuItemSelectedEvent(item.ItemIdId));
        }
    }
}