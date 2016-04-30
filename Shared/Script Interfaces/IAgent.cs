namespace Shared
{
    public interface IAgent
    {
        bool OnAgentKicked(Packet packet, Client client);
        bool OnAgentConnected(Packet packet, Client client);
        bool OnAgentDisconnected(Packet packet, Client client);
        bool OnAgentPacketReceived(Packet packet, Client client);
    }
}