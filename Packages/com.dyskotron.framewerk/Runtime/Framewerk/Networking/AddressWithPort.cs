namespace Framewerk.Networking
{
    public struct AddressWithPort
    {
        public string Address { get;}
        public int Port { get;}

        public AddressWithPort(string address, int port)
        {
            Address = address;
            Port = port;
        }

        public static bool operator ==(AddressWithPort c1, AddressWithPort c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(AddressWithPort c1, AddressWithPort c2)
        {
            return !c1.Equals(c2);
        }

        public bool Equals(AddressWithPort other)
        {
            return other.Address == Address && other.Port == Port;
        }
    }
}
