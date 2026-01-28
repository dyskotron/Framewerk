using Plugins.Framewerk;
using strange.extensions.context.impl;

namespace FramewerkDemo
{
    public class FramewerkDemoBootstrap : ContextView
    {
        public ViewConfig viewConfig;

        private FramewerkDemoContext _context;

        private void Start()
        {
            _context = new FramewerkDemoContext(this, viewConfig);
            _context.Start();
        }

        private void OnApplicationQuit()
        {
            _context.OnRemove();
        }
    }
}
