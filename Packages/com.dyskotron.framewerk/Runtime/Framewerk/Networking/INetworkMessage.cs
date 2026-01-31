using Framewerk.Networking.Serialization;

namespace Framewerk.Networking
{
    public interface INetworkMessage
    {
        byte Type { get; }
        int SenderConnectionId  { get; }
        void Serialize(NetworkWriter writer);
        void Deserialize(NetworkReader reader);
    }
}
