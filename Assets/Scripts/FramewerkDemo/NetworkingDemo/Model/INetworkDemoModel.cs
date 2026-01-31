using System.Collections.Generic;

namespace FramewerkDemo.NetworkingDemo.Model
{
    public interface INetworkDemoModel
    {
        bool IsHost { get; set; }
        bool IsClient { get; set; }
        bool IsConnected { get; set; }
        List<string> ChatLog { get; }
        List<DiscoveredHost> DiscoveredHosts { get; }
        int ConnectedClientsCount { get; set; }
        float LastRoundTripTime { get; set; }

        void AddChatMessage(string message);
        void ClearChatLog();
        void AddDiscoveredHost(DiscoveredHost host);
        void ClearDiscoveredHosts();
    }

    public class DiscoveredHost
    {
        public string Address { get; set; }
        public int Port { get; set; }
        public string HostName { get; set; }

        public DiscoveredHost(string address, int port, string hostName)
        {
            Address = address;
            Port = port;
            HostName = hostName;
        }

        public override string ToString()
        {
            return $"{HostName} ({Address}:{Port})";
        }
    }
}
