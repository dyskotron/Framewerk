using System.Collections.Generic;

namespace FramewerkDemo.NetworkingDemo.Model
{
    public class NetworkDemoModel : INetworkDemoModel
    {
        public bool IsHost { get; set; }
        public bool IsClient { get; set; }
        public bool IsConnected { get; set; }
        public List<string> ChatLog { get; private set; }
        public List<DiscoveredHost> DiscoveredHosts { get; private set; }
        public int ConnectedClientsCount { get; set; }
        public float LastRoundTripTime { get; set; }

        public NetworkDemoModel()
        {
            ChatLog = new List<string>();
            DiscoveredHosts = new List<DiscoveredHost>();
            IsHost = false;
            IsClient = false;
            IsConnected = false;
            ConnectedClientsCount = 0;
            LastRoundTripTime = 0f;
        }

        public void AddChatMessage(string message)
        {
            ChatLog.Add(message);
        }

        public void ClearChatLog()
        {
            ChatLog.Clear();
        }

        public void AddDiscoveredHost(DiscoveredHost host)
        {
            // Check if host already exists (by address)
            bool exists = false;
            foreach (var existingHost in DiscoveredHosts)
            {
                if (existingHost.Address == host.Address)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                DiscoveredHosts.Add(host);
            }
        }

        public void ClearDiscoveredHosts()
        {
            DiscoveredHosts.Clear();
        }
    }
}
