namespace Framewerk.Networking.Serialization
{
    public interface INetworkSerializable
    {
        void Serialize(NetworkWriter writer);
        void Deserialize(NetworkReader reader);
    }
}
