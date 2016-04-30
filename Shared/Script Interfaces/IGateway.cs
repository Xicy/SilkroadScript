namespace Shared
{
    public interface IGateway
    {
        bool OnGatewayKicked(Packet packet, Client client);
        bool OnGatewayConnected(Packet packet, Client client);
        bool OnGatewayDisconnected(Packet packet, Client client);
        bool OnGatewayPacketReceived(Packet packet, Client client);
    }
}