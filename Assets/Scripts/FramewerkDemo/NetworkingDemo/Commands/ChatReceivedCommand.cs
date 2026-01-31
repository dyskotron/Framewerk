using FramewerkDemo.NetworkingDemo.Messages;
using FramewerkDemo.NetworkingDemo.Model;
using FramewerkDemo.NetworkingDemo.Signals;
using strange.extensions.command.impl;
using UnityEngine;

namespace FramewerkDemo.NetworkingDemo.Commands
{
    public class ChatReceivedCommand : Command
    {
        [Inject] public ChatMessage Message { get; set; }
        [Inject] public INetworkDemoModel Model { get; set; }
        [Inject] public ChatLogUpdatedSignal ChatLogUpdatedSignal { get; set; }

        public override void Execute()
        {
            Debug.Log($"[NetworkDemo] Chat message received from {Message.SenderName}: {Message.Text}");

            string formattedMessage = $"{Message.SenderName}: {Message.Text}";
            Model.AddChatMessage(formattedMessage);
            ChatLogUpdatedSignal.Dispatch();
        }
    }
}
