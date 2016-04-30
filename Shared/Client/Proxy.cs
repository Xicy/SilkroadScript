using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using Shared.Core.Network;
using Shared.Structs;

namespace Shared
{
    public partial class Client
    {
        public void LoadEvents(IScript script)
        {
            var client = script as IClient;
            var agent = script as IAgent;
            var gateway = script as IGateway;

            if (client != null)
            {
                ClientConnected += client.OnClientConnected;
                ClientDisconnected += client.OnClientDisconnected;
                ClientPacketReceived += client.OnClientPacketReceived;
            }

            if (agent != null)
            {
                AgentConnected += agent.OnAgentConnected;
                AgentKicked += agent.OnAgentKicked;
                AgentDisconnected += agent.OnAgentDisconnected;
                AgentPacketReceived += agent.OnAgentPacketReceived;
            }

            if (gateway != null)
            {
                GatewayConnected += gateway.OnGatewayConnected;
                GatewayKicked += gateway.OnGatewayKicked;
                GatewayDisconnected += gateway.OnGatewayDisconnected;
                GatewayPacketReceived += gateway.OnGatewayPacketReceived;
            }
        }

        private void Start(ushort localPort, string ip)
        {
            //Store parameters
            _mLocalPort = localPort;
            _mGatewayIp = ip;
            _mGatewayPort = Data.GatewayPort;
            //Create ClientSocket instance and link the Events
            _mClientSocket = new Core.Network.Client();
            _mClientSocket.Connected += ClientSocket_Connected;
            _mClientSocket.Disconnected += ClientSocket_Disconnected;
            _mClientSocket.PacketReceived += ClientSocket_PacketReceived;
            //_mClientSocket.PacketSend += o => { Console.WriteLine("P->C:" + o.Opcode.ToString("X4") + " Data:" + o.ToString()); };

            //Listen for Client        
            _mClientSocket.Listen(localPort);

            //Create GatewayServer instance and link the Events
            _mGatewaySocket = new Server();
            _mGatewaySocket.Connected += GatewaySocket_Connected;
            _mGatewaySocket.Disconnected += GatewaySocket_Disconnected;
            _mGatewaySocket.Kicked += GatewaySocket_Kicked;
            _mGatewaySocket.PacketReceived += GatewaySocket_PacketReceived;
            //_mGatewaySocket.PacketSend += o => { Console.WriteLine("P->G:" + o.Opcode.ToString("X4") + " Data:" + o.ToString()); };

            //Create AgentSocket instance and link the Events
            _mAgentSocket = new Server();
            _mAgentSocket.Connected += AgentSocket_Connected;
            _mAgentSocket.Disconnected += AgentSocket_Disconnected;
            _mAgentSocket.Kicked += AgentSocket_Kicked;
            _mAgentSocket.PacketReceived += AgentSocket_PacketReceived;
            //_mAgentSocket.PacketSend += o => { Console.WriteLine("P->A:" + o.Opcode.ToString("X4") + " Data:" + o.ToString()); };
        }

        private ushort GetFreePort(ushort start = 1, ushort end = ushort.MaxValue, ushort step = 1)
        {
            var usedPorts = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Select(p => p.Port).ToList();
            for (var port = start; port < end; port += step)
            {
                if (usedPorts.Contains(port)) continue;
                return port;
            }
            return 0;
        }

        #region EventArgs
        public delegate bool AllEventHandler(Packet packet, Client client);

        public event AllEventHandler ClientConnected;
        public event AllEventHandler ClientDisconnected;
        public event AllEventHandler ClientPacketReceived;

        public event AllEventHandler AgentConnected;
        public event AllEventHandler AgentKicked;
        public event AllEventHandler AgentDisconnected;
        public event AllEventHandler AgentPacketReceived;

        public event AllEventHandler GatewayConnected;
        public event AllEventHandler GatewayKicked;
        public event AllEventHandler GatewayDisconnected;
        public event AllEventHandler GatewayPacketReceived;

        private bool InvokeMethodWithSkip(AllEventHandler _event, Packet packet)
        {
            var bl = new List<bool>();
            if (_event == null) { return false; }
            foreach (var cea in _event.GetInvocationList().Cast<AllEventHandler>())
            {
                packet?.SeekRead(0, SeekOrigin.Begin);
                bl.Add(cea.Invoke(packet, this));
            }
            return bl.Any(b => b);
        }
        #endregion

        #region Members
        private ushort _mLocalPort;
        private string _mGatewayIp;
        private ushort _mGatewayPort;

        private Core.Network.Client _mClientSocket;
        private Server _mGatewaySocket;
        private Server _mAgentSocket;
        private string _agentServerIp;
        private ushort _agentServerPort;
        private bool _doAgentServerConnect;
        private bool _isConnectedToAgentServer;
        #endregion

        #region ClientSocket EventHandlers

        private void ClientSocket_PacketReceived(Packet packet)
        {
            if (InvokeMethodWithSkip(ClientPacketReceived, packet)) return;//Skip
            SendFromClient(packet);
        }

        private void ClientSocket_Disconnected()
        {
            if (_isConnectedToAgentServer)
            {
                _isConnectedToAgentServer = false;
                _doAgentServerConnect = false;
                _mAgentSocket.Disconnect();
            }
            else
            {
                _mGatewaySocket.Disconnect();
            }
            InvokeMethodWithSkip(ClientDisconnected, null);
        }

        private void ClientSocket_Connected()
        {
            if (_doAgentServerConnect)
            {
                _mAgentSocket.Connect(_agentServerIp, _agentServerPort);
                _mGatewaySocket.Disconnect();
            }
            else
            {
                _mGatewaySocket.Connect(_mGatewayIp, _mGatewayPort);
            }
            InvokeMethodWithSkip(ClientConnected, null);
        }

        #endregion

        #region GatewaySocket EventHandlers

        private void GatewaySocket_PacketReceived(Packet packet)
        {
            switch ((OpCodes.GatewayServer)packet.Opcode)
            {
                #region OpCodes.GatewayServer.ServerList
                case OpCodes.GatewayServer.ServerList:
                    Servers.Clear();
                    while (packet.ReadByte() != 0)
                    {
                        packet.ReadByte();//server ID
                        packet.ReadAscii();//Server Name
                    }

                    while (packet.ReadByte() == 1)
                    {
                        var serverId = packet.ReadShort();
                        var serverName = packet.ReadAscii();
                        var currentCapacity = packet.ReadShort();
                        var maxCapacity = packet.ReadShort();
                        var status = packet.ReadByte();
                        Servers.Add(new ServerList(serverId, serverName, currentCapacity, maxCapacity, status));
                    }
                    break;
                #endregion

                #region OpCodes.GatewayServer.LoginReply
                case OpCodes.GatewayServer.LoginReply:
                    if (packet.ReadByte() == 1)
                    {
                        var sessionId = packet.ReadUInt();
                        _agentServerIp = packet.ReadAscii();
                        _agentServerPort = packet.ReadUShort();
                        _doAgentServerConnect = true;

                        var response = new Packet((ushort)OpCodes.GatewayServer.LoginReply, true);
                        response.WriteByte(1);
                        response.WriteUInt(sessionId);
                        response.WriteAscii("127.0.0.1");
                        response.WriteUShort(_mLocalPort);
                        packet = response;
                        packet.Lock();
                    }
                    break;
                    #endregion
            }
            if (InvokeMethodWithSkip(GatewayPacketReceived, packet)) return; //Skip
            SendFromServer(packet);
        }

        private void GatewaySocket_Kicked()
        {
            _mGatewaySocket.Disconnect();
            InvokeMethodWithSkip(GatewayKicked, null);
        }

        private void GatewaySocket_Disconnected()
        {
            InvokeMethodWithSkip(GatewayDisconnected, null);
        }

        private void GatewaySocket_Connected(string ip, ushort port)
        {
            InvokeMethodWithSkip(GatewayConnected, null);
        }

        #endregion

        #region AgentSocket EventHandlers

        private void AgentSocket_PacketReceived(Packet packet)
        {
            switch ((OpCodes.AgentServer)packet.Opcode)
            {
                #region OpCodes.AgentServer.CharacterListing
                case OpCodes.AgentServer.CharacterListing:
                    if (packet.ReadByte() == 2)
                    {
                        if (packet.ReadByte() == 1)
                        {
                            CharacterListings.Clear();
                            var charCount = packet.ReadByte();
                            for (var i = 0; i < charCount; i++)
                            {
                                var modelId = packet.ReadInt();
                                var name = packet.ReadAscii();
                                var volume = packet.ReadByte();
                                var level = packet.ReadByte();
                                var exp = packet.ReadLong();
                                var strength = packet.ReadUShort();
                                var intelligence = packet.ReadUShort();
                                var statPoints = packet.ReadUShort();
                                var hp = packet.ReadUInt();
                                var mp = packet.ReadUInt();
                                var isInDeletion = false;

                                if (packet.ReadByte() == 1)
                                {
                                    isInDeletion = true;
                                    packet.ReadInt();//TICKS ...
                                }
                                //packet.ReadByte();
                                //packet.ReadByte();
                                //packet.ReadByte();
                                packet.ReadByteArray(3);

                                var itemCount = packet.ReadByte();
                                /*for (var iItem = 1; iItem <= itemCount; iItem++)
                                {
                                    packet.ReadUInt(); //Item ID
                                    packet.ReadByte(); //item Plus
                                }*/
                                packet.ReadByteArray(5 * itemCount);

                                byte avatarCount = packet.ReadByte();
                                /*for (var iAvatar = 1; iAvatar <= avatarCount; iAvatar++)
                                {
                                    packet.ReadUInt(); //Item ID
                                    packet.ReadByte(); //Item plus
                                }*/
                                packet.ReadByteArray(5 * avatarCount);

                                CharacterListings.Add(new CharacterListing(modelId, name, volume, level, exp, strength, intelligence, statPoints, hp, mp, isInDeletion));
                            }
                        }
                    }
                    break;
                #endregion
                case OpCodes.AgentServer.GroupSpawnBegin:
                    DecodeGroupSpawnStart(packet);
                    break;
                case OpCodes.AgentServer.GroupeSpawn:
                    DecodeGroupSpawn(packet);
                    break;
                case OpCodes.AgentServer.GroupSpawnEnd:
                    DecodeGroupSpawnEnd(packet);
                    break;
                case OpCodes.AgentServer.SingleSpawn:
                    DecodeSingleSpawn(packet);
                    break;
                case OpCodes.AgentServer.SingleDespawn:
                    DecodeSingleDespawn(packet);
                    break;
            }
            if (InvokeMethodWithSkip(AgentPacketReceived, packet)) return; //Skip
            SendFromServer(packet);
        }

        private void AgentSocket_Kicked()
        {
            _mAgentSocket.Disconnect();
            InvokeMethodWithSkip(AgentKicked, null);
        }

        private void AgentSocket_Disconnected()
        {
            _isConnectedToAgentServer = false;
            _doAgentServerConnect = false;
            _mClientSocket?.Shutdown();
            InvokeMethodWithSkip(AgentDisconnected, null);
        }

        private void AgentSocket_Connected(string ip, ushort port)
        {
            _isConnectedToAgentServer = true;
            InvokeMethodWithSkip(AgentConnected, null);
        }

        #endregion
    }
}