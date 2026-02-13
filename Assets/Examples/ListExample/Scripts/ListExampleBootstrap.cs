using Plugins.Framewerk;
using strange.extensions.context.impl;
using UnityEngine;

namespace Framewerk.Examples.ListExample
{
    public class ListExampleBootstrap : ContextView
    {
        public ViewConfig viewConfig;

        private ListExampleContext _context;

        private void Start()
        {
            if (viewConfig == null)
            {
                Debug.LogError("[ListExample] ViewConfig is not assigned on ListExampleBootstrap! " +
                               "Please assign a ViewConfig component in the inspector.", this);
                return;
            }

            _context = new ListExampleContext(this, viewConfig);
            _context.Start();
        }

        private void OnApplicationQuit()
        {
            _context?.OnRemove();
        }
    }
}
