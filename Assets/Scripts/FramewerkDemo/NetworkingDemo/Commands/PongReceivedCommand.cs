using FramewerkDemo.NetworkingDemo.Messages;
using FramewerkDemo.NetworkingDemo.Model;
using FramewerkDemo.NetworkingDemo.Signals;
using strange.extensions.command.impl;
using UnityEngine;

namespace FramewerkDemo.NetworkingDemo.Commands
{
    public class PongReceivedCommand : Command
    {
        [Inject] public PongMessage Message { get; set; }
        [Inject] public INetworkDemoModel Model { get; set; }
        [Inject] public RttUpdatedSignal RttUpdatedSignal { get; set; }

        public override void Execute()
        {
            // Calculate round-trip time
            float currentTime = Time.time;
            float rtt = currentTime - Message.Timestamp;

            Debug.Log($"[NetworkDemo] Pong received from connection {Message.SenderConnectionId}, RTT: {rtt * 1000f:F2}ms");

            Model.LastRoundTripTime = rtt;
            RttUpdatedSignal.Dispatch(rtt);
        }
    }
}
