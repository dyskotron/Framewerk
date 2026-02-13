using Plugins.Framewerk;
using strange.extensions.context.impl;
using UnityEngine;

namespace Framewerk.Examples.TabsViewStackExample
{
    public class TabsViewStackExampleBootstrap : ContextView
    {
        public ViewConfig viewConfig;

        private TabsViewStackExampleContext _context;

        private void Start()
        {
            if (viewConfig == null)
            {
                Debug.LogError("[TabsViewStackExample] ViewConfig is not assigned! " +
                               "Please assign a ViewConfig component in the inspector.", this);
                return;
            }

            _context = new TabsViewStackExampleContext(this, viewConfig);
            _context.Start();
        }

        private void OnApplicationQuit()
        {
            _context?.OnRemove();
        }
    }
}
