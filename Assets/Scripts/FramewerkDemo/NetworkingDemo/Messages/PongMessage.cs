using Framewerk.Networking;
using Framewerk.Networking.Serialization;

namespace FramewerkDemo.NetworkingDemo.Messages
{
    public class PongMessage : INetworkMessage
    {
        public const byte MESSAGE_TYPE = 2;

        public byte Type => MESSAGE_TYPE;
        public int SenderConnectionId { get; set; }
        public float Timestamp { get; set; }

        public PongMessage()
        {
        }

        public PongMessage(int senderConnectionId, float timestamp)
        {
            SenderConnectionId = senderConnectionId;
            Timestamp = timestamp;
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(Timestamp);
        }

        public void Deserialize(NetworkReader reader)
        {
            Timestamp = reader.ReadFloat();
        }
    }
}
