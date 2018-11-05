using Framewerk.ViewUtils;
using UnityEngine;

namespace Framewerk.Mvcs
{
    public interface IView
    {
        GameObject GameObject { get; }
    }

    public class View : MonoBehaviorEnhanced, IView
    {
        public GameObject GameObject {
            get { return gameObject; }
        }
    }
}
