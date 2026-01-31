using System;
using System.Collections.Generic;
using strange.extensions.signal.impl;
using UnityEngine;
using Mirror;
using NetworkReader = Framewerk.Networking.Serialization.NetworkReader;
using NetworkWriter = Framewerk.Networking.Serialization.NetworkWriter;

namespace Framewerk.Networking
{
    // Keep QosType enum for backwards compatibility with existing code
    public enum QosType
    {
        Reliable = 0,
        Unreliable = 1
    }

    public enum NetworkError
    {
        Ok = 0,
        WrongConnection,
        Timeout,
        BadMessage,
        NoResources
    }

    public class NetworkHost
    {
        public const int BUFFER_SIZE = 1024 * 32;

        public Signal<int, NetworkConnectionState> ConnectionStateSignal { get; } = new Signal<int, NetworkConnectionState>();

        public int HostId { get; private set; }

        private Transport _transport;
        private Dictionary<byte, Func<int, INetworkMessage>> _messageFactories = new Dictionary<byte, Func<int, INetworkMessage>>();
        private Queue<INetworkMessage> _messageQueue = new Queue<INetworkMessage>();
        private bool _isServer = false;
        private bool _isClient = false;
        private int _nextConnectionId = 1;
        private Dictionary<int, int> _mirrorToConnectionId = new Dictionary<int, int>();
        private Dictionary<int, int> _connectionIdToMirror = new Dictionary<int, int>();

        private Serialization.NetworkReader _reader = new Serialization.NetworkReader(new byte[BUFFER_SIZE], 0);
        private Serialization.NetworkWriter _writer = new Serialization.NetworkWriter();

        public NetworkHost(QosType channel, int port = 0, int maxDefaultConnections = 32) : this(new[] {channel}, port, maxDefaultConnections)
        {

        }

        public NetworkHost(QosType[] channels, int port = 0, int maxDefaultConnections = 32)
        {
            _transport = Transport.active;
            if (_transport == null)
            {
                Debug.LogError("NetworkHost: No active Mirror Transport found!");
                return;
            }

            // Set up Mirror transport callbacks
            _transport.OnServerConnected = OnServerConnected;
            _transport.OnServerDataReceived = OnServerDataReceived;
            _transport.OnServerDisconnected = OnServerDisconnected;
            _transport.OnClientConnected = OnClientConnected;
            _transport.OnClientDataReceived = OnClientDataReceived;
            _transport.OnClientDisconnected = OnClientDisconnected;

            HostId = UnityEngine.Random.Range(1000, 9999);
            Debug.LogWarning($"<color=\"magenta\">NetworkHost.Constructor() HostId:{HostId}</color>");
        }

        public void StartServer(int port)
        {
            if (_transport != null)
            {
                _transport.ServerStart();
                _isServer = true;
                Debug.Log($"NetworkHost: Server started on port {port}");
            }
        }

        public void StartClient(string address, int port)
        {
            if (_transport != null)
            {
                _transport.ClientConnect(address);
                _isClient = true;
                Debug.Log($"NetworkHost: Client connecting to {address}:{port}");
            }
        }

        public void Close()
        {
            if (_isServer && _transport != null)
            {
                _transport.ServerStop();
            }
            if (_isClient && _transport != null)
            {
                _transport.ClientDisconnect();
            }
            _isServer = false;
            _isClient = false;
        }

        public void RegisterMessage<T>(byte typeId) where T : INetworkMessage, new()
        {
            _messageFactories[typeId] = (connectionId) =>
            {
                var msg = new T();
                // Assuming T has a constructor that sets SenderConnectionId
                return msg;
            };
        }

        public void RegisterMessage(byte typeId, Func<int, INetworkMessage> factory)
        {
            _messageFactories[typeId] = factory;
        }

        public AddressWithPort GetConnectionAddress(int connectionId)
        {
            // Mirror doesn't expose connection addresses easily
            // Return placeholder for now
            Debug.LogWarning($"<color=\"aqua\">NetworkHost.GetConnectionAddress() : connectionId{connectionId}</color>");
            return new AddressWithPort("unknown", 0);
        }

        public void ConnectTo(string address, int port)
        {
            StartClient(address, port);
        }

        public void Disconnect(int connectionId)
        {
            if (_isServer && _transport != null && _connectionIdToMirror.ContainsKey(connectionId))
            {
                _transport.ServerDisconnect(_connectionIdToMirror[connectionId]);
            }
        }

        public NetworkError SendMessage(INetworkMessage message, QosType type, int connectionId)
        {
            _writer.Begin();
            _writer.Write(message.Type);
            message.Serialize(_writer);
            _writer.End();

            var segment = new ArraySegment<byte>(_writer.GetWriteBuffer(), 0, _writer.GetWritePos());
            int channel = (int)type;

            try
            {
                if (_isServer && _transport != null && _connectionIdToMirror.ContainsKey(connectionId))
                {
                    _transport.ServerSend(_connectionIdToMirror[connectionId], segment, channel);
                }
                else if (_isClient && _transport != null)
                {
                    _transport.ClientSend(segment, channel);
                }
                return NetworkError.Ok;
            }
            catch (Exception e)
            {
                Debug.LogError($"NetworkHost.SendMessage error: {e.Message}");
                return NetworkError.BadMessage;
            }
        }

        public INetworkMessage ReadMessage()
        {
            if (_messageQueue.Count > 0)
            {
                return _messageQueue.Dequeue();
            }
            return null;
        }

        // Mirror Transport Callbacks
        private void OnServerConnected(int mirrorConnectionId)
        {
            int connectionId = _nextConnectionId++;
            _mirrorToConnectionId[mirrorConnectionId] = connectionId;
            _connectionIdToMirror[connectionId] = mirrorConnectionId;
            ConnectionStateSignal.Dispatch(connectionId, NetworkConnectionState.Connected);
        }

        private void OnServerDataReceived(int mirrorConnectionId, ArraySegment<byte> data, int channel)
        {
            if (_mirrorToConnectionId.ContainsKey(mirrorConnectionId))
            {
                ProcessIncomingData(_mirrorToConnectionId[mirrorConnectionId], data);
            }
        }

        private void OnServerDisconnected(int mirrorConnectionId)
        {
            if (_mirrorToConnectionId.ContainsKey(mirrorConnectionId))
            {
                int connectionId = _mirrorToConnectionId[mirrorConnectionId];
                ConnectionStateSignal.Dispatch(connectionId, NetworkConnectionState.Disconnected);
                _connectionIdToMirror.Remove(connectionId);
                _mirrorToConnectionId.Remove(mirrorConnectionId);
            }
        }

        private void OnClientConnected()
        {
            int connectionId = 0; // Client always uses connection ID 0
            ConnectionStateSignal.Dispatch(connectionId, NetworkConnectionState.Connected);
        }

        private void OnClientDataReceived(ArraySegment<byte> data, int channel)
        {
            ProcessIncomingData(0, data);
        }

        private void OnClientDisconnected()
        {
            ConnectionStateSignal.Dispatch(0, NetworkConnectionState.Disconnected);
        }

        private void ProcessIncomingData(int connectionId, ArraySegment<byte> data)
        {
            byte[] buffer = new byte[data.Count];
            Array.Copy(data.Array, data.Offset, buffer, 0, data.Count);

            _reader.SetBuffer(buffer, 0);
            var typeId = _reader.ReadByte();

            if (_messageFactories.ContainsKey(typeId))
            {
                var message = _messageFactories[typeId](connectionId);
                if (message != null)
                {
                    message.Deserialize(_reader);
                    _messageQueue.Enqueue(message);
                }
            }
            else
            {
                Debug.LogError($"<color=\"aqua\">NetworkHost.ProcessIncomingData() : Message type not recognized type:{typeId}</color>");
            }
        }

        protected virtual INetworkMessage CreateMessage(byte typeId, int connectionId)
        {
            if (_messageFactories.ContainsKey(typeId))
            {
                return _messageFactories[typeId](connectionId);
            }
            Debug.LogWarning($"<color=\"aqua\">NetworkHost.CreateMessage() : Unknown message type {typeId}</color>");
            return null;
        }
    }
}
