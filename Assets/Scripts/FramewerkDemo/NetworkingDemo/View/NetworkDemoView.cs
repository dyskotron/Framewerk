using strange.extensions.mediation.impl;
using UnityEngine;
using UnityEngine.UI;

namespace FramewerkDemo.NetworkingDemo.View
{
    public class NetworkDemoView : strange.extensions.mediation.impl.View
    {
        [Header("Role Selection")]
        public Button hostButton;
        public Button clientButton;
        public GameObject roleSelectionPanel;

        [Header("Connection State")]
        public Text connectionStateText;
        public Text connectedClientsText;

        [Header("Discovery (Client)")]
        public GameObject discoveryPanel;
        public Text discoveredHostsText;
        public Button connectButton;
        public InputField hostIndexInput;

        [Header("Chat")]
        public GameObject chatPanel;
        public Text chatLogText;
        public InputField chatInputField;
        public Button sendChatButton;

        [Header("Ping/Pong")]
        public Button sendPingButton;
        public Text rttText;

        [Header("Disconnect")]
        public Button disconnectButton;
    }
}
