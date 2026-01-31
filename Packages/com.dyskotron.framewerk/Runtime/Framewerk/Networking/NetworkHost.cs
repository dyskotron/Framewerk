using System;
using System.Collections.Generic;
using strange.extensions.signal.impl;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using NetworkReader = Framewerk.Networking.Serialization.NetworkReader;
using NetworkWriter = Framewerk.Networking.Serialization.NetworkWriter;

namespace Framewerk.Networking
{
    public class NetworkHost
    {
        public const int BUFFER_SIZE = 1024 * 32;

        public Signal<int, NetworkConnectionState> ConnectionStateSignal { get; } = new Signal<int, NetworkConnectionState>();
        public Signal<string, int, byte[]> BroadcastReceivedSignal { get; } = new Signal<string, int, byte[]>();

        public int HostId { get; private set; }

        private Dictionary<QosType, byte> _channels = new Dictionary<QosType, byte>();

        private NetworkReader _reader = new NetworkReader(new byte[BUFFER_SIZE], 0);
        private NetworkWriter _writer = new NetworkWriter();

        public NetworkHost(QosType channel, int port = 0, int maxDefaultConnections = 32) : this(new[] {channel}, port, maxDefaultConnections)
        {

        }

        public NetworkHost(QosType[] channels, int port = 0, int maxDefaultConnections = 32)
        {
            //init channels
            var config = new ConnectionConfig();
            foreach (var channel in channels)
            {
                var id = config.AddChannel(channel);
                _channels[channel] = id;
            }

            //create host
            var hostTopology = new HostTopology(config, maxDefaultConnections);
            HostId = NetworkTransport.AddHost(hostTopology, port);

            Debug.LogWarning($"<color=\"magenta\">NetworkHost.Constructor() HostId:{HostId}</color>");
        }

        public void Close()
        {
            NetworkTransport.RemoveHost(HostId);
        }

        public void SetBroadCastingCredentials(int key, int version, int subversion)
        {
            //TODO: error handling
            byte error;
            NetworkTransport.SetBroadcastCredentials(HostId, key, version, subversion, out error);
        }

        public AddressWithPort GetConnectionAddress(int connectionId)
        {
            NetworkTransport.GetConnectionInfo(HostId, connectionId, out string address, out int port, out NetworkID network, out NodeID dstNode, out byte error);
            Debug.LogWarning($"<color=\"aqua\">NetworkHost.GetConnectionAddress() : connectionId{connectionId} address{address} port{port} network{network} dstNode{dstNode} </color>");
            return new AddressWithPort(address, port);
        }

        public void StartBroadcasting(int broadcastPort, int key, int version, int subversion, byte[] buffer, int size, int timeout)
        {
            byte error;
            if (!NetworkTransport.StartBroadcastDiscovery(HostId, broadcastPort, key, version, subversion, buffer, size, timeout, out error))
            {
                Debug.LogError("NetworkDiscovery StartBroadcast failed err: " + (NetworkError) error);
            }
        }

        public void StopBroadcastDiscovery()
        {
            NetworkTransport.StopBroadcastDiscovery();
        }

        public void ConnectTo(string address, int port)
        {
            //TODO: error handling
            byte error;
            NetworkTransport.Connect(HostId, address, port, 0, out error);
        }

        public void Disconnect(int connectionId)
        {
            //TODO: error handling
            byte error;
            NetworkTransport.Disconnect(HostId, connectionId, out error);
        }

        public NetworkError SendMessage(INetworkMessage message, QosType type, int connectionId)
        {
            //todo check if channel is available
            var channelId = _channels[type];
            byte error;

            message.Serialize(_writer);

            //Debug.LogWarning($"<color=\"aqua\">NetworkHost.SendMessage() : data - {BitConverter.ToString(_writer.GetWriteBuffer())}</color>");

            NetworkTransport.Send(HostId, connectionId, channelId, _writer.GetWriteBuffer(), _writer.GetWritePos(), out error);
            return (NetworkError) error;
        }

        public INetworkMessage ReadMessage()
        {
            int connectionId;
            int channelId;
            byte[] buffer = new byte[BUFFER_SIZE];
            int receivedSize;
            byte error;

            //todo check error

            var type = NetworkTransport.ReceiveFromHost(HostId, out connectionId, out channelId, buffer, buffer.Length, out receivedSize, out error);
            if (type == NetworkEventType.Nothing)
                return null;

            //Debug.LogWarning($"<color=\"aqua\">NetworkHost.ReadMessage() : data - {BitConverter.ToString(buffer)} </color>");

            return ReadData(type, channelId, connectionId, buffer, receivedSize);
        }

        private INetworkMessage ReadData(NetworkEventType type, int channelId, int connectionId, byte[] buffer, int receivedSize)
        {
            switch (type)
            {
                case NetworkEventType.DataEvent:
                    var message = ProcessDataEvent(connectionId, buffer, receivedSize);
                    message.Deserialize(_reader);
                    return message;
                case NetworkEventType.ConnectEvent:
                    ConnectionStateSignal.Dispatch(connectionId, NetworkConnectionState.Connected);
                    break;
                case NetworkEventType.DisconnectEvent:
                    ConnectionStateSignal.Dispatch(connectionId, NetworkConnectionState.Disconnected);
                    break;
                case NetworkEventType.Nothing:
                    break;
                case NetworkEventType.BroadcastEvent:

                    int port;
                    string address;
                    byte err;
                    NetworkTransport.GetBroadcastConnectionInfo(HostId, out address, out port, out err );
                    NetworkTransport.GetBroadcastConnectionMessage(HostId, buffer, buffer.Length, out receivedSize, out err);
                    BroadcastReceivedSignal.Dispatch(address, port, buffer);
                    break;
            }

            return null;
        }

        protected virtual INetworkMessage CreateMessage(byte typeId, int connectionId)
        {
            Debug.LogWarning($"<color=\"aqua\">NetworkHost.CreateMessage() : Unknown message type {typeId}</color>");
            return null;
        }

        private INetworkMessage ProcessDataEvent(int connectionId, byte[] buffer, int receivedSize)
        {
            _reader.SetBuffer(buffer, 0);
            var typeId = _reader.ReadByte();

            var message = CreateMessage(typeId, connectionId);
            if (message == null)
            {
                Debug.LogError($"<color=\"aqua\">NetworkHost.ProcessDataEvent() : Message type not recognized type:{typeId}</color>");
                return null;
            }

            return message;
        }
    }
}
