using Framewerk.Networking;
using Framewerk.Networking.Serialization;

namespace FramewerkDemo.NetworkingDemo.Messages
{
    public class ChatMessage : INetworkMessage
    {
        public const byte MESSAGE_TYPE = 3;

        public byte Type => MESSAGE_TYPE;
        public int SenderConnectionId { get; set; }
        public string SenderName { get; set; }
        public string Text { get; set; }

        public ChatMessage()
        {
        }

        public ChatMessage(int senderConnectionId, string senderName, string text)
        {
            SenderConnectionId = senderConnectionId;
            SenderName = senderName;
            Text = text;
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(SenderName);
            writer.Write(Text);
        }

        public void Deserialize(NetworkReader reader)
        {
            SenderName = reader.ReadString();
            Text = reader.ReadString();
        }
    }
}
