using System.Collections.Generic;
using System.Linq;
using Framewerk.Managers;
using Framewerk.Mvcs;
using UnityEngine;
using UnityEngine.UI;

namespace Framewerk.ViewComponents.TabComponent
{
    public interface ITabContainerMediator
    {
        void NextTab();
        void PreviousTab();
        int GetTabsCount();
        void CreateAllTabs();
        int TabIndex { get; set; }
    }

    public abstract class TabContainerMediator<T> : Mediator<T>, ITabContainerMediator where T : TabContainerView
    {
        protected ITabMediator[] CreatedContent;
        protected List<Toggle> Buttons;
        protected List<ITabContainerComponent> _components = new List<ITabContainerComponent>();

        Color _tabTxtColor;
        private int _tabIndex;
        private int _startIndex = -1;
        private bool _skip;

        protected override void Init()
        {
            EnableHidingByCanvas();

            if (View.TabTogglesParent != null)
            {
                Buttons = View.TabTogglesParent.GetComponentsInChildren<Toggle>().ToList();
                _tabTxtColor = (Buttons.Count > 0) ? Buttons[0].colors.normalColor : Color.white;
            }

            InitContent();

            if (_startIndex >= CreatedContent.Length)
                _startIndex = CreatedContent.Length - 1;
            else if (_startIndex < 0)
                _startIndex = 0;

            _tabIndex = -1;

            base.Init();

            OnTabSwitch(_startIndex, true, false);
        }
        
        public ITabMediator CurrentTab
        {
            get
            {
                if (!CreatedContent.Any() || CreatedContent.Count() < _tabIndex) return null;
                return CreatedContent[_tabIndex];
            }
        }

        public int TabIndex
        {
            set
            {
                if (inited)
                {
                    if (value >= 0 && value < CreatedContent.Length)
                    {
                        OnTabSwitch(_tabIndex, false, false);
                        OnTabSwitch(value, true, false);
                    }
                }
                else
                {
                    _startIndex = value;
                }
            }

            get { return _tabIndex; }
        }

        public void NextTab()
        {
            if(GetTabsCount() > TabIndex + 1)
                TabIndex++;
        }

        public void PreviousTab()
        {
            if (TabIndex > 0)
                TabIndex--;
        }

        public override void SetInitData(ViewInitData data)
        {
            var tabData = data as ITabContainerInitData;
            if(tabData != null)
                _startIndex = tabData.TabIndex;

            base.SetInitData(data);
        }

        public virtual int GetTabsCount()
        {
            return View.TabsContentPrefabs.Length;
        }

        public virtual void CreateAllTabs()
        {
            for (var i = 0; i < GetTabsCount(); i++)
            {
                CreateTabContent(i);
            }
        }

        protected virtual void InitContent()
        {
            //setup tab toggles
            if (Buttons != null)
            {
                for (int i = 0; i < Buttons.Count; i++)
                {
                    var toggeEnabled = (GetTabContentPrefab(i) != null);
                    Buttons[i].gameObject.SetActive(toggeEnabled);

                    if (toggeEnabled)
                    {
                        int k = i;
                        AttachToggleListener(Buttons[i].gameObject, (bool val) =>
                        {
                            if(!_skip)
                                OnTabSwitch(k, val, true);
                        });
                    }
                }
            }

            CreatedContent = GetCreatedContentArray();
        }

        public override void Destroy()
        {
            DestroyContent();
            CreatedContent = null;

            foreach (var component in _components)
                component.Destroy();

            _components.Clear();

            base.Destroy();
        }

        protected virtual void OnTabSwitch(int tabIndex, bool value, bool dispatch = true)
        {
            if (value)
            {
                CreateTabContent(tabIndex);

                if (Buttons != null && Buttons.Count > tabIndex)
                {
                    Buttons[tabIndex].gameObject.SetActive(true);

                    _skip = true;
                    Buttons[tabIndex].isOn = true;
                    _skip = false;
                }

                _tabIndex = tabIndex;
            }

            if (Buttons != null && Buttons.Count > tabIndex)
            {
                var toggle = Buttons[tabIndex];
                var colors = toggle.colors;
                colors.normalColor = value ? Color.white : _tabTxtColor;
                toggle.colors = colors;
                toggle.targetGraphic.color = colors.normalColor;
            }

            SetTabActive(tabIndex, value);

            foreach (var component in _components)
                component.OnTabSwitch(tabIndex, value);
        }

        protected void CreateTabContent(int tabIndex)
        {
            //create view and mediator if it does not exist
            if (CreatedContent[tabIndex] == null)
            {
                ITabMediator tab = GetMediatorForContent(GetTabContentPrefab(tabIndex), tabIndex);

                if (tab != null)
                {
                    tab.TabIndex = tabIndex;
                    CreatedContent[tabIndex] = tab;
                    OnTabCreated(tabIndex, tab);
                }
                else
                {
                    Debug.LogErrorFormat(
                        "<color=\"aqua\">{0} : Tab content is null. GetMediatorForContent is probably not returning mediator</color>",
                        this);
                }

            }
            else
            {
                //Just for scrollToTabcomponent which is removing tabs from parent
                CreatedContent[tabIndex].SetParent(View.ContentsParent,true);
                CreatedContent[tabIndex].SetSiblingIndex(tabIndex);
            }
        }
        
        protected virtual void DestroyContent()
        {
            if (CreatedContent == null)
                return;
            
            foreach (ITabMediator tabMediator in CreatedContent)
            {
                if (tabMediator != null)
                    tabMediator.Destroy();
            }
        }

        protected virtual void SetTabActive(int tabIndex, bool value)
        {
            ITabMediator tabContent = CreatedContent[tabIndex];
            if (tabContent != null)
                tabContent.SetActive(value);
        }

        protected virtual void OnTabCreated(int index, ITabMediator tab)
        {
            foreach (var component in _components)
                component.OnTabCreated(index, tab);
        }

        protected abstract ITabMediator GetMediatorForContent(GameObject tabContentView, int tabIndex);

        protected virtual GameObject GetTabContentPrefab(int tabIndex)
        {
            return View.TabsContentPrefabs[tabIndex];
        }

        protected virtual ITabMediator[] GetCreatedContentArray()
        {
            return new ITabMediator[GetTabsCount()];
        }

        protected void AddComponent(ITabContainerComponent component)
        {
            _components.Add(component);
        }
        
        protected virtual void SetTabTitle(int tabIndex, string title)
        {
            if(tabIndex >= Buttons.Count)
                return;

            var text = Buttons[tabIndex].gameObject.GetComponentInChildren<Text>();
            if (text != null)
                text.text = title;
        }
    }
}