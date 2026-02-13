using Plugins.Framewerk;
using strange.extensions.context.impl;
using UnityEngine;

namespace Framewerk.Examples.PopupExample
{
    public class PopupExampleBootstrap : ContextView
    {
        public ViewConfig viewConfig;

        private PopupExampleContext _context;

        private void Start()
        {
            if (viewConfig == null)
            {
                Debug.LogError("[PopupExample] ViewConfig is not assigned on PopupExampleBootstrap! " +
                               "Please assign a ViewConfig component in the inspector.", this);
                return;
            }

            _context = new PopupExampleContext(this, viewConfig);
            _context.Start();
        }

        private void OnApplicationQuit()
        {
            _context?.OnRemove();
        }
    }
}
