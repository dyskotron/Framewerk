using Framewerk.Networking;
using Framewerk.Networking.Discovery;
using FramewerkDemo.NetworkingDemo.Messages;
using FramewerkDemo.NetworkingDemo.Model;
using FramewerkDemo.NetworkingDemo.Signals;
using strange.extensions.command.impl;
using System.Text;
using UnityEngine;

namespace FramewerkDemo.NetworkingDemo
{
    /// <summary>
    /// StartCommand for the NetworkingDemo.
    /// Initializes the NetworkHost and NetworkDiscovery.
    /// </summary>
    public class NetworkDemoStartCommand : Command
    {
        [Inject] public NetworkHost NetworkHost { get; set; }
        [Inject] public NetworkDiscovery NetworkDiscovery { get; set; }
        [Inject] public INetworkDemoModel Model { get; set; }
        [Inject] public ConnectionStateChangedSignal ConnectionStateChangedSignal { get; set; }

        public override void Execute()
        {
            Debug.Log("[NetworkDemo] Starting Networking Demo...");

            // Register message types with NetworkHost using custom factories
            NetworkHost.RegisterMessage(PingMessage.MESSAGE_TYPE, (connectionId) =>
            {
                var msg = new PingMessage();
                msg.SenderConnectionId = connectionId;
                return msg;
            });

            NetworkHost.RegisterMessage(PongMessage.MESSAGE_TYPE, (connectionId) =>
            {
                var msg = new PongMessage();
                msg.SenderConnectionId = connectionId;
                return msg;
            });

            NetworkHost.RegisterMessage(ChatMessage.MESSAGE_TYPE, (connectionId) =>
            {
                var msg = new ChatMessage();
                msg.SenderConnectionId = connectionId;
                return msg;
            });

            // Set up NetworkDiscovery credentials
            NetworkDiscovery.SetBroadcastCredentials(
                NetworkConsts.BROADCAST_KEY,
                NetworkConsts.BROADCAST_VERSION,
                NetworkConsts.BROADCAST_SUBVERSION
            );

            // Subscribe to connection state changes
            NetworkHost.ConnectionStateSignal.AddListener(OnConnectionStateChanged);

            Debug.Log("[NetworkDemo] Networking Demo initialized. Select Host or Client role to begin.");
        }

        private void OnConnectionStateChanged(int connectionId, NetworkConnectionState state)
        {
            ConnectionStateChangedSignal.Dispatch(connectionId, state);
        }
    }
}
