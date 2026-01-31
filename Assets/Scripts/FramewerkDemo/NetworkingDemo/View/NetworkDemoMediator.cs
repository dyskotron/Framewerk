using Framewerk.Networking;
using Framewerk.Networking.Discovery;
using FramewerkDemo.NetworkingDemo.Messages;
using FramewerkDemo.NetworkingDemo.Model;
using FramewerkDemo.NetworkingDemo.Signals;
using strange.extensions.mediation.impl;
using System.Text;
using UnityEngine;

namespace FramewerkDemo.NetworkingDemo.View
{
    public class NetworkDemoMediator : EventMediator
    {
        [Inject] public NetworkDemoView View { get; set; }
        [Inject] public INetworkDemoModel Model { get; set; }
        [Inject] public NetworkHost NetworkHost { get; set; }
        [Inject] public NetworkDiscovery NetworkDiscovery { get; set; }

        [Inject] public ChatLogUpdatedSignal ChatLogUpdatedSignal { get; set; }
        [Inject] public DiscoveredHostsUpdatedSignal DiscoveredHostsUpdatedSignal { get; set; }
        [Inject] public ConnectionStateChangedSignal ConnectionStateChangedSignal { get; set; }
        [Inject] public RttUpdatedSignal RttUpdatedSignal { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();

            // Wire up button listeners
            View.hostButton.onClick.AddListener(OnHostButtonClicked);
            View.clientButton.onClick.AddListener(OnClientButtonClicked);
            View.connectButton.onClick.AddListener(OnConnectButtonClicked);
            View.sendChatButton.onClick.AddListener(OnSendChatButtonClicked);
            View.sendPingButton.onClick.AddListener(OnSendPingButtonClicked);
            View.disconnectButton.onClick.AddListener(OnDisconnectButtonClicked);

            // Wire up signals
            ChatLogUpdatedSignal.AddListener(OnChatLogUpdated);
            DiscoveredHostsUpdatedSignal.AddListener(OnDiscoveredHostsUpdated);
            ConnectionStateChangedSignal.AddListener(OnConnectionStateChanged);
            RttUpdatedSignal.AddListener(OnRttUpdated);

            // Initial UI state
            UpdateUI();
        }

        public override void OnRemove()
        {
            base.OnRemove();

            View.hostButton.onClick.RemoveListener(OnHostButtonClicked);
            View.clientButton.onClick.RemoveListener(OnClientButtonClicked);
            View.connectButton.onClick.RemoveListener(OnConnectButtonClicked);
            View.sendChatButton.onClick.RemoveListener(OnSendChatButtonClicked);
            View.sendPingButton.onClick.RemoveListener(OnSendPingButtonClicked);
            View.disconnectButton.onClick.RemoveListener(OnDisconnectButtonClicked);

            ChatLogUpdatedSignal.RemoveListener(OnChatLogUpdated);
            DiscoveredHostsUpdatedSignal.RemoveListener(OnDiscoveredHostsUpdated);
            ConnectionStateChangedSignal.RemoveListener(OnConnectionStateChanged);
            RttUpdatedSignal.RemoveListener(OnRttUpdated);
        }

        private void OnHostButtonClicked()
        {
            Model.IsHost = true;
            Model.IsClient = false;

            // Start server
            NetworkHost.StartServer(NetworkConsts.SERVER_PORT);

            // Start broadcasting presence
            string hostName = "NetworkDemo Host";
            byte[] broadcastData = Encoding.UTF8.GetBytes(hostName);
            NetworkDiscovery.StartBroadcasting(
                NetworkConsts.BROADCAST_PORT,
                NetworkConsts.BROADCAST_KEY,
                NetworkConsts.BROADCAST_VERSION,
                NetworkConsts.BROADCAST_SUBVERSION,
                broadcastData,
                broadcastData.Length,
                1000 // Broadcast every 1 second
            );

            Model.IsConnected = true;
            UpdateUI();
        }

        private void OnClientButtonClicked()
        {
            Model.IsHost = false;
            Model.IsClient = true;

            // Start listening for broadcasts
            NetworkDiscovery.StartListening(NetworkConsts.BROADCAST_PORT);

            UpdateUI();
        }

        private void OnConnectButtonClicked()
        {
            if (Model.IsClient && Model.DiscoveredHosts.Count > 0)
            {
                int hostIndex = 0;
                if (!string.IsNullOrEmpty(View.hostIndexInput.text))
                {
                    int.TryParse(View.hostIndexInput.text, out hostIndex);
                }

                if (hostIndex >= 0 && hostIndex < Model.DiscoveredHosts.Count)
                {
                    var host = Model.DiscoveredHosts[hostIndex];
                    NetworkHost.StartClient(host.Address, host.Port);
                    Debug.Log($"[NetworkDemo] Connecting to {host.Address}:{host.Port}");
                }
            }
        }

        private void OnSendChatButtonClicked()
        {
            if (string.IsNullOrEmpty(View.chatInputField.text))
                return;

            string senderName = Model.IsHost ? "Host" : "Client";
            var chatMessage = new ChatMessage(0, senderName, View.chatInputField.text);

            // Add to local chat log
            Model.AddChatMessage($"{senderName}: {View.chatInputField.text}");
            ChatLogUpdatedSignal.Dispatch();

            // Send to remote
            if (Model.IsHost)
            {
                // Send to all connected clients (would need to track connection IDs)
                // For simplicity, we'll broadcast to connection ID 1 (first client)
                NetworkHost.SendMessage(chatMessage, QosType.Reliable, 1);
            }
            else if (Model.IsClient && Model.IsConnected)
            {
                NetworkHost.SendMessage(chatMessage, QosType.Reliable, 0);
            }

            View.chatInputField.text = "";
        }

        private void OnSendPingButtonClicked()
        {
            if (!Model.IsConnected)
                return;

            var pingMessage = new PingMessage(0, Time.time);

            if (Model.IsHost)
            {
                NetworkHost.SendMessage(pingMessage, QosType.Reliable, 1);
            }
            else if (Model.IsClient)
            {
                NetworkHost.SendMessage(pingMessage, QosType.Reliable, 0);
            }

            Debug.Log($"[NetworkDemo] Ping sent at {Time.time}");
        }

        private void OnDisconnectButtonClicked()
        {
            NetworkHost.Close();
            NetworkDiscovery.Stop();
            Model.IsHost = false;
            Model.IsClient = false;
            Model.IsConnected = false;
            Model.ConnectedClientsCount = 0;
            Model.ClearChatLog();
            Model.ClearDiscoveredHosts();
            UpdateUI();
        }

        private void OnChatLogUpdated()
        {
            UpdateChatLog();
        }

        private void OnDiscoveredHostsUpdated()
        {
            UpdateDiscoveredHosts();
        }

        private void OnConnectionStateChanged(int connectionId, NetworkConnectionState state)
        {
            if (state == NetworkConnectionState.Connected)
            {
                Model.IsConnected = true;
                if (Model.IsHost)
                {
                    Model.ConnectedClientsCount++;
                }
            }
            else if (state == NetworkConnectionState.Disconnected)
            {
                if (Model.IsHost)
                {
                    Model.ConnectedClientsCount--;
                }
                else
                {
                    Model.IsConnected = false;
                }
            }

            UpdateUI();
        }

        private void OnRttUpdated(float rtt)
        {
            View.rttText.text = $"RTT: {rtt * 1000f:F2}ms";
        }

        private void UpdateUI()
        {
            // Role selection visibility
            View.roleSelectionPanel.SetActive(!Model.IsHost && !Model.IsClient);

            // Discovery panel (client only)
            View.discoveryPanel.SetActive(Model.IsClient && !Model.IsConnected);

            // Chat panel (when connected)
            View.chatPanel.SetActive(Model.IsConnected);

            // Connection state
            if (Model.IsHost)
            {
                View.connectionStateText.text = Model.IsConnected ? "Host: Running" : "Host: Starting...";
                View.connectedClientsText.text = $"Connected Clients: {Model.ConnectedClientsCount}";
            }
            else if (Model.IsClient)
            {
                View.connectionStateText.text = Model.IsConnected ? "Client: Connected" : "Client: Discovering...";
                View.connectedClientsText.text = "";
            }
            else
            {
                View.connectionStateText.text = "Select Role";
                View.connectedClientsText.text = "";
            }

            // Disconnect button
            View.disconnectButton.gameObject.SetActive(Model.IsHost || Model.IsClient);

            UpdateChatLog();
            UpdateDiscoveredHosts();
        }

        private void UpdateChatLog()
        {
            var sb = new StringBuilder();
            foreach (var message in Model.ChatLog)
            {
                sb.AppendLine(message);
            }
            View.chatLogText.text = sb.ToString();
        }

        private void UpdateDiscoveredHosts()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Discovered Hosts:");
            for (int i = 0; i < Model.DiscoveredHosts.Count; i++)
            {
                sb.AppendLine($"{i}: {Model.DiscoveredHosts[i]}");
            }
            View.discoveredHostsText.text = sb.ToString();
        }
    }
}
