using Plugins.Framewerk;
using strange.extensions.context.impl;
using UnityEngine;

namespace SomeSpace
{
    public class ExfoliatestBootstrap : ContextView
    {
        public ViewConfig viewConfig;

        private ExfoliatestContext _context;

        private void Start()
        {
            if (viewConfig == null)
            {
                Debug.LogError("[Exfoliatest] ViewConfig is not assigned on ExfoliatestBootstrap! " +
                               "Please assign a ViewConfig component in the inspector.", this);
                return;
            }

            _context = new ExfoliatestContext(this, viewConfig);
            _context.Start();
        }

        private void OnApplicationQuit()
        {
            _context?.OnRemove();
        }
    }
}
