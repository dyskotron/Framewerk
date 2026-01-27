using UnityEngine;

namespace Plugins.Framewerk
{
    public partial class ViewConfig : MonoBehaviour
    {
        public Camera Camera3d;
        public Camera UICamera;

        public float UiCameraHeight => UICamera.orthographicSize;
        public float UiCameraWidth => UiCameraHeight / UICamera.pixelHeight * UICamera.pixelWidth;

        public Transform Container3d;
        public Transform UiBottom;
        public Transform UiDefault;
        public Transform Popups;
        public Transform UiOverlay;
    }
}