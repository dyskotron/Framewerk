using System;
using System.Collections.Generic;
using System.Linq;
using Framewerk.Managers;
using UnityEngine;

namespace Framewerk.ViewComponents.TabComponent
{
    public interface IPageTabContainerMediator : ITabContainerMediator
    {
        void SetPageActive(int pageIndex, bool value);
    }

    public abstract class PageTabContainerMediator<T> : TabContainerMediator<T>, IPageTabContainerMediator
        where T : PageTabContainerView
    {
        protected List<ITabMediator[]> CreatedPageContent = new List<ITabMediator[]>();
        protected int _tabsCount;

        private List<Type[]> _mediators;
        private int _pageIndex = 0;
        private int _numPages;
        private bool _omitTabSwitch;

        public int PageIndex
        {
            set
            {
                if (inited)
                {
                    if (value >= 0 && value < CreatedContent.Length)
                    {
                        OnPageSwitch(_pageIndex, false);
                        OnPageSwitch(value, true);
                    }
                }
            }
            get { return _pageIndex; }
        }

        public virtual void NextPage()
        {
            if (View.NumPages > PageIndex + 1)
                PageIndex++;
        }

        public virtual void PreviousPage()
        {
            if (PageIndex > 0)
                PageIndex--;
        }

        public override int GetTabsCount()
        {
            return _tabsCount;
        }

        public void SetPageActive(int pageIndex, bool value)
        {
            var content = CreatedPageContent.ElementAt(_pageIndex);

            foreach (var tabMediator in content)
            {
                if (tabMediator != null)
                    tabMediator.SetActive(value);
            }
        }

        public override void SetInitData(ViewInitData data)
        {
            var tabData = data as IPageTabContainerInitData;
            if (tabData != null)
                _pageIndex = tabData.PageIndex;

            base.SetInitData(data);
        }

        protected override void Init()
        {
            _tabsCount = (int) Math.Floor((float) View.TabsContentPrefabs.Length / View.NumPages);

            base.Init();

            //TODO: hotfix preventing double OnTabSwitch call in init
            _omitTabSwitch = true;
            OnPageSwitch(_pageIndex, true);
            _omitTabSwitch = false;
        }

        protected override ITabMediator[] GetCreatedContentArray()
        {
            while (CreatedPageContent.Count <= _pageIndex)
                CreatedPageContent.Add(new ITabMediator[View.TabsContentPrefabs.Length / View.NumPages]);

            return CreatedPageContent.ElementAt(_pageIndex);
        }

        protected override GameObject GetTabContentPrefab(int tabIndex)
        {
            return View.TabsContentPrefabs[_tabsCount * _pageIndex + tabIndex];
        }

        protected virtual void OnPageSwitch(int pageIndex, bool value)
        {
            if (value)
            {
                _pageIndex = pageIndex;
                InitContent();
                
                if (!_omitTabSwitch)
                    OnTabSwitch(TabIndex, true);
            }
            else
            {
                if (!_omitTabSwitch)
                    OnTabSwitch(TabIndex, false);
            }

            foreach (var component in _components)
            {
                var pageComponent = component as IPageTabContainerComponent;
                if (pageComponent != null)
                    pageComponent.OnPageSwitch(pageIndex, value);
            }
        }

        protected override void DestroyContent()
        {
            if (CreatedPageContent == null)
                return;

            foreach (var tabMediators in CreatedPageContent)
            {
                foreach (var tabMediator in tabMediators)
                {
                    if (tabMediator != null)
                        tabMediator.Destroy();
                }
            }

            CreatedPageContent.Clear();
        }
    }
}