using Plugins.Framewerk;
using strange.extensions.context.impl;
using UnityEngine;

namespace Framewerk.Examples.ScreenFsmExample
{
    public class ScreenFsmExampleBootstrap : ContextView
    {
        public ViewConfig viewConfig;

        private ScreenFsmExampleContext _context;

        private void Start()
        {
            if (viewConfig == null)
            {
                Debug.LogError("[ScreenFsmExample] ViewConfig is not assigned! " +
                               "Please assign a ViewConfig component in the inspector.", this);
                return;
            }

            _context = new ScreenFsmExampleContext(this, viewConfig);
            _context.Start();
        }

        private void OnApplicationQuit()
        {
            _context?.OnRemove();
        }
    }
}
