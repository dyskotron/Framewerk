using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using strange.extensions.signal.impl;
using UnityEngine;

namespace Framewerk.Networking.Discovery
{
    public class NetworkDiscovery
    {
        public Signal<string, int, byte[]> BroadcastReceivedSignal { get; } = new Signal<string, int, byte[]>();

        private UdpClient _broadcastClient;
        private UdpClient _listenClient;
        private int _broadcastPort;
        private int _key;
        private int _version;
        private int _subversion;
        private byte[] _broadcastData;
        private int _broadcastInterval;
        private float _lastBroadcastTime;
        private bool _isBroadcasting;
        private bool _isListening;

        public NetworkDiscovery()
        {
        }

        public void SetBroadcastCredentials(int key, int version, int subversion)
        {
            _key = key;
            _version = version;
            _subversion = subversion;
        }

        public void StartBroadcasting(int broadcastPort, int key, int version, int subversion, byte[] buffer, int size, int timeoutMs)
        {
            if (_isBroadcasting)
            {
                Debug.LogWarning("NetworkDiscovery: Already broadcasting");
                return;
            }

            _broadcastPort = broadcastPort;
            _key = key;
            _version = version;
            _subversion = subversion;
            _broadcastData = new byte[size];
            Array.Copy(buffer, _broadcastData, size);
            _broadcastInterval = timeoutMs;

            try
            {
                _broadcastClient = new UdpClient();
                _broadcastClient.EnableBroadcast = true;
                _isBroadcasting = true;
                _lastBroadcastTime = Time.time;
                Debug.Log($"NetworkDiscovery: Started broadcasting on port {broadcastPort}");
            }
            catch (Exception e)
            {
                Debug.LogError($"NetworkDiscovery: Failed to start broadcasting: {e.Message}");
            }
        }

        public void StartListening(int listenPort)
        {
            if (_isListening)
            {
                Debug.LogWarning("NetworkDiscovery: Already listening");
                return;
            }

            try
            {
                _listenClient = new UdpClient(listenPort);
                _listenClient.Client.ReceiveTimeout = 100; // Non-blocking with timeout
                _isListening = true;
                Debug.Log($"NetworkDiscovery: Started listening on port {listenPort}");
            }
            catch (Exception e)
            {
                Debug.LogError($"NetworkDiscovery: Failed to start listening: {e.Message}");
            }
        }

        public void Update()
        {
            if (_isBroadcasting)
            {
                UpdateBroadcast();
            }

            if (_isListening)
            {
                UpdateListening();
            }
        }

        private void UpdateBroadcast()
        {
            if (Time.time - _lastBroadcastTime >= _broadcastInterval / 1000f)
            {
                SendBroadcast();
                _lastBroadcastTime = Time.time;
            }
        }

        private void SendBroadcast()
        {
            try
            {
                // Build packet with credentials
                byte[] packet = new byte[12 + _broadcastData.Length];
                BitConverter.GetBytes(_key).CopyTo(packet, 0);
                BitConverter.GetBytes(_version).CopyTo(packet, 4);
                BitConverter.GetBytes(_subversion).CopyTo(packet, 8);
                Array.Copy(_broadcastData, 0, packet, 12, _broadcastData.Length);

                IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, _broadcastPort);
                _broadcastClient.Send(packet, packet.Length, endPoint);
            }
            catch (Exception e)
            {
                Debug.LogError($"NetworkDiscovery: Broadcast send error: {e.Message}");
            }
        }

        private void UpdateListening()
        {
            try
            {
                if (_listenClient.Available > 0)
                {
                    IPEndPoint remoteEndPoint = null;
                    byte[] data = _listenClient.Receive(ref remoteEndPoint);

                    if (data.Length >= 12)
                    {
                        // Validate credentials
                        int receivedKey = BitConverter.ToInt32(data, 0);
                        int receivedVersion = BitConverter.ToInt32(data, 4);
                        int receivedSubversion = BitConverter.ToInt32(data, 8);

                        if (receivedKey == _key && receivedVersion == _version && receivedSubversion == _subversion)
                        {
                            // Extract broadcast data
                            byte[] broadcastData = new byte[data.Length - 12];
                            Array.Copy(data, 12, broadcastData, 0, broadcastData.Length);

                            BroadcastReceivedSignal.Dispatch(remoteEndPoint.Address.ToString(), remoteEndPoint.Port, broadcastData);
                        }
                    }
                }
            }
            catch (SocketException)
            {
                // Timeout or no data available, ignore
            }
            catch (Exception e)
            {
                Debug.LogError($"NetworkDiscovery: Listen error: {e.Message}");
            }
        }

        public void Stop()
        {
            if (_isBroadcasting)
            {
                _broadcastClient?.Close();
                _broadcastClient = null;
                _isBroadcasting = false;
                Debug.Log("NetworkDiscovery: Stopped broadcasting");
            }

            if (_isListening)
            {
                _listenClient?.Close();
                _listenClient = null;
                _isListening = false;
                Debug.Log("NetworkDiscovery: Stopped listening");
            }
        }
    }
}
