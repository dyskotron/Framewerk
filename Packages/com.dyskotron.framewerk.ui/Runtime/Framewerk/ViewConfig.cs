using UnityEngine;

namespace Plugins.Framewerk
{
    public partial class ViewConfig : MonoBehaviour
    {
        [Header("Addressable ID Configuration")]
        [Tooltip("Context prefix ScriptableObject (e.g. 'Examples', 'ListPopupDemo'). Optional.")]
        public ContextPrefix ContextPrefixSO;

        [Tooltip("Custom address pattern resolver. If null, uses default pattern.")]
        public AddressResolverConfig AddressResolver;

        [Header("Cameras")]
        public Camera Camera3d;
        public Camera UICamera;

        public float UiCameraHeight => UICamera.orthographicSize;
        public float UiCameraWidth => UiCameraHeight / UICamera.pixelHeight * UICamera.pixelWidth;

        [Header("UI Containers")]
        public Transform Container3d;
        public Transform UiBottom;
        public Transform UiDefault;
        public Transform Popups;
        public Transform UiOverlay;
    }
}
