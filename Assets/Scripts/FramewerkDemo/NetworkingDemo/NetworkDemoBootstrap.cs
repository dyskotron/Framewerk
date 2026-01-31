using strange.extensions.context.impl;
using UnityEngine;

namespace FramewerkDemo.NetworkingDemo
{
    /// <summary>
    /// ContextView for the NetworkingDemo.
    /// Attach this to a GameObject in your NetworkDemo scene.
    /// </summary>
    public class NetworkDemoBootstrap : ContextView
    {
        private NetworkDemoContext _context;

        private void Start()
        {
            _context = new NetworkDemoContext(this);
            _context.Start();
        }

        private void OnApplicationQuit()
        {
            _context?.OnRemove();
        }
    }
}
