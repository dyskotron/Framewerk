using System;
using Framewerk.Core;
using Framewerk.Managers;
using Framewerk.Managers.Popup;
using Framewerk.Managers.StateMachine;
using Framewerk.Mvcs;
using FramewerkDemo.Examples.ExampleListPanel;
using FramewerkDemo.Examples.ExamplePopup;
using FramewerkDemo.Examples.ExampleVirtualListPanel;
using FramewerkDemo.MainMenu.Model;
using ExampleTabPanelMediator = FramewerkDemo.Examples.ExampleTabPanel.ExampleTabPanelMediator;

namespace FramewerkDemo.Examples
{
    public class ExamplesScreen : AppStateScreen
    {
        [Inject] private IUIManager _uiManager;
        [Inject] private IPopUpManager _popUpManager;
        
        private TopMenuMediator _topMenu;
        private IMediator _example;

        public void InitExample(ExampleId exampleId)
        {
            _topMenu = _uiManager.CreateUIMediator<TopMenuMediator>();
            _topMenu.SetExampleId(exampleId);
            
            switch (exampleId)
            {
                case ExampleId.Popup:
                    _example = _popUpManager.ShowPopUp<ExamplePopupMediator>();
                    break;
                case ExampleId.Tabs:
                    _example = _uiManager.CreateUIMediator<ExampleTabPanelMediator>("TabPanel/");
                    break;
                case ExampleId.List:
                    _example = _uiManager.CreateUIMediator<ExampleListPanelMediator>("ListPanel/");
                    break;
                case ExampleId.VirtualList:
                    _example = _uiManager.CreateUIMediator<ExampleVirtualListPanelMediator>("VirtualListPanel/");
                    break;
                default:
                    throw new ArgumentOutOfRangeException("exampleId", exampleId, null);
            }
        }

        public override void Destroy()
        {
            if(_topMenu != null)
                _topMenu.Destroy();
            
            if(_example != null)
                _example.Destroy();
            
            base.Destroy();
        }
    }
}