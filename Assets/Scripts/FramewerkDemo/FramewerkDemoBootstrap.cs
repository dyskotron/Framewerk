using Plugins.Framewerk;
using strange.extensions.context.impl;
using UnityEngine;

namespace FramewerkDemo
{
    public class FramewerkDemoBootstrap : ContextView
    {
        public ViewConfig viewConfig;

        private FramewerkDemoContext _context;

        private void Start()
        {
            if (viewConfig == null)
            {
                Debug.LogError("[FramewerkDemo] ViewConfig is not assigned on FramewerkDemoBootstrap! " +
                               "Please assign a ViewConfig component in the inspector.", this);
                return;
            }

            _context = new FramewerkDemoContext(this, viewConfig);
            _context.Start();
        }

        private void OnApplicationQuit()
        {
            _context?.OnRemove();
        }
    }
}
