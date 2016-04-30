namespace Shared
{
    public interface IClient
    {
        bool OnClientConnected(Packet packet, Client client);
        bool OnClientDisconnected(Packet packet, Client client);
        bool OnClientPacketReceived(Packet packet, Client client);
    }
}