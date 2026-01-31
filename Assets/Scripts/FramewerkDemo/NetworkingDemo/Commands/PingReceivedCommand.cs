using Framewerk.Networking;
using FramewerkDemo.NetworkingDemo.Messages;
using strange.extensions.command.impl;
using UnityEngine;

namespace FramewerkDemo.NetworkingDemo.Commands
{
    public class PingReceivedCommand : Command
    {
        [Inject] public PingMessage Message { get; set; }
        [Inject] public NetworkHost NetworkHost { get; set; }

        public override void Execute()
        {
            Debug.Log($"[NetworkDemo] Ping received from connection {Message.SenderConnectionId} with timestamp {Message.Timestamp}");

            // Auto-respond with Pong
            var pongMessage = new PongMessage(0, Message.Timestamp);
            NetworkHost.SendMessage(pongMessage, QosType.Reliable, Message.SenderConnectionId);
        }
    }
}
