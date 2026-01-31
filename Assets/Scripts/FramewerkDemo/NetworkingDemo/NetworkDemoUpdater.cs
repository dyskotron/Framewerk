using Framewerk.Networking;
using Framewerk.Networking.Discovery;
using Framewerk.Networking.StrangeIntegration;
using FramewerkDemo.NetworkingDemo.Model;
using FramewerkDemo.NetworkingDemo.Signals;
using System.Text;
using UnityEngine;

namespace FramewerkDemo.NetworkingDemo
{
    /// <summary>
    /// MonoBehaviour that polls NetworkHost.ReadMessage() and NetworkDiscovery.Update() each frame.
    /// Dispatches NetworkMessageReceivedSignal for incoming messages and updates the model with discovered hosts.
    /// </summary>
    public class NetworkDemoUpdater : MonoBehaviour
    {
        [Inject] public NetworkHost NetworkHost { get; set; }
        [Inject] public NetworkDiscovery NetworkDiscovery { get; set; }
        [Inject] public NetworkMessageReceivedSignal NetworkMessageReceivedSignal { get; set; }
        [Inject] public INetworkDemoModel Model { get; set; }
        [Inject] public DiscoveredHostsUpdatedSignal DiscoveredHostsUpdatedSignal { get; set; }

        private void Update()
        {
            // Update NetworkDiscovery (handles broadcast and listening)
            if (NetworkDiscovery != null)
            {
                NetworkDiscovery.Update();
            }

            // Poll for incoming messages
            if (NetworkHost != null)
            {
                INetworkMessage message;
                while ((message = NetworkHost.ReadMessage()) != null)
                {
                    // Dispatch to NetworkCommandBinder
                    if (NetworkMessageReceivedSignal != null)
                    {
                        NetworkMessageReceivedSignal.Dispatch(message);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            // Clean up networking when this object is destroyed
            if (NetworkHost != null)
            {
                NetworkHost.Close();
            }

            if (NetworkDiscovery != null)
            {
                NetworkDiscovery.Stop();
            }
        }

        // Called by injection after dependencies are set
        [PostConstruct]
        public void PostConstruct()
        {
            // Subscribe to discovery signal
            if (NetworkDiscovery != null)
            {
                NetworkDiscovery.BroadcastReceivedSignal.AddListener(OnBroadcastReceived);
            }
        }

        private void OnBroadcastReceived(string address, int port, byte[] data)
        {
            // Extract host name from broadcast data
            string hostName = "Unknown Host";
            if (data.Length > 0)
            {
                hostName = Encoding.UTF8.GetString(data);
            }

            var discoveredHost = new DiscoveredHost(address, NetworkConsts.SERVER_PORT, hostName);
            Model.AddDiscoveredHost(discoveredHost);
            DiscoveredHostsUpdatedSignal.Dispatch();

            Debug.Log($"[NetworkDemo] Discovered host: {hostName} at {address}:{port}");
        }
    }
}
