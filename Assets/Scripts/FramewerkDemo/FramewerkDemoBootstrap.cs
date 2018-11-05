using Framewerk.ViewUtils;
using UnityEngine;

namespace FramewerkDemo
{
    public class FramewerkDemoBootstrap : MonoBehaviour
    {
        private FramewerkDemoContext _context;
            
        private void Start()
        {
            var viewSettings = gameObject.GetComponent<BaseViewSettings>();
            _context = new FramewerkDemoContext(viewSettings);
        }
    }
}