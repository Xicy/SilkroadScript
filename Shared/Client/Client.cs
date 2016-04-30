using System.Collections.Generic;
using Shared.Core;
using Shared.Structs;
using Shared.Structs.Data;

namespace Shared
{
    public partial class Client
    {
        public SynchronizedList<ServerList> Servers { private set; get; }
        public SynchronizedList<CharacterListing> CharacterListings { private set; get; }
        public DivisionServer DivisionServer { private set; get; }
        public ushort GamePort { private set; get; }

        public Client()
        {
            Servers = new List<ServerList>();
            CharacterListings = new List<CharacterListing>();
        }

        public void StartGame(DivisionServer divisionserver, string ip)
        {
            DivisionServer = divisionserver;
            GamePort = GetFreePort(15770);
            Start(GamePort,ip);
            Loader.Inject(Data.SilkroadPath, GamePort, DivisionServer.Locale);
        }

        public void SendFromClient(Packet packet)
        {
            //Console.WriteLine(BasicController.PacketDebug(_isConnectedToAgentServer ? "AC" : "GC", packet));
            if (_isConnectedToAgentServer) { _mAgentSocket.Send(packet); } else { _mGatewaySocket.Send(packet); }
        }

        public void SendFromServer(Packet packet)
        {
            _mClientSocket?.Send(packet);
            //Console.WriteLine(BasicController.PacketDebug(_isConnectedToAgentServer ? "AS" : "GS", packet));
        }
    }
}