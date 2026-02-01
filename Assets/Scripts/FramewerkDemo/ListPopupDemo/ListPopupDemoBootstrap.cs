using Plugins.Framewerk;
using strange.extensions.context.impl;
using UnityEngine;

namespace FramewerkDemo.ListPopupDemo
{
    public class ListPopupDemoBootstrap : ContextView
    {
        public ViewConfig viewConfig;

        private ListPopupDemoContext _context;

        private void Start()
        {
            if (viewConfig == null)
            {
                Debug.LogError("[ListPopupDemo] ViewConfig is not assigned on ListPopupDemoBootstrap! " +
                               "Please assign a ViewConfig component in the inspector.", this);
                return;
            }

            _context = new ListPopupDemoContext(this, viewConfig);
            _context.Start();
        }

        private void OnApplicationQuit()
        {
            _context?.OnRemove();
        }
    }
}
