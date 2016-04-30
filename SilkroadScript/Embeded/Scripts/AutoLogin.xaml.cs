using System;
using System.Linq;
using System.Windows.Controls;
using LiteDB;
using Shared;

public partial class AutoLogin : Page, IScript, IClient, IGateway, IAgent, IContent, IDatabase<AutoLogin.AutoLoginData>
{
    public class AutoLoginData
    {
        public string ID { set; get; }
        public string Password { set; get; }
        public string CharName { set; get; }
        public string ServerName { set; get; }
        public bool IsEnable { set; get; }
    }

    public AutoLogin()
    {
        InitializeComponent();
        UserID.TextChanged += (sender, args) => { _userId = UserID.Text; };
        Password.PasswordChanged += (sender, args) => { _password = Password.Password; };
        ServerName.TextChanged += (sender, args) => { _servName = ServerName.Text; };
        CharName.TextChanged += (sender, args) => { _charName = CharName.Text; };
        AutoLoginEnable.Checked += (sender, args) => { _isEnable = AutoLoginEnable.IsChecked != null && AutoLoginEnable.IsChecked.Value; };
        AutoLoginEnable.Unchecked += (sender, args) => { _isEnable = AutoLoginEnable.IsChecked != null && AutoLoginEnable.IsChecked.Value; };
    }

    void IScript.Loaded()
    {

        AutoLoginData user;
        if ((user = Database.FindAll().FirstOrDefault()) == null) return;
        UserID.Text = user.ID;
        Password.Password = user.Password;
        ServerName.Text = user.ServerName;
        CharName.Text = user.CharName;
        AutoLoginEnable.IsChecked = user.IsEnable;
    }

    string IScript.Name => "Auto Login";

    int IScript.Version => 0;

    string IScript.Description => "Auto login for game";

    object IContent.Content => Content;

    public LiteCollection<AutoLoginData> Database { get; set; }

    private string _userId;
    private string _password;
    private string _charName;
    private string _servName;
    private bool _isEnable;
    private bool _isLoginSend;
    private bool _isLogined;

    public bool OnClientPacketReceived(Packet packet, Client client)
    {
        //Console.WriteLine(Utility.PacketDebug(_isLogined ? typeof(OpCodes.AgentClient) : typeof(OpCodes.GatewayClient), packet));
        if (!_isEnable && _isLogined) { return false; }
        switch (packet.Opcode)
        {
            case (ushort)OpCodes.AgentClient.Login:
                var pack = new Packet((ushort)OpCodes.AgentClient.Login);
                pack.WriteUInt(packet.ReadUInt());
                packet.ReadUInt();
                pack.WriteAscii(_userId.ToLowerInvariant());
                pack.WriteAscii(_password.ToLowerInvariant());
                pack.WriteByteArray(packet.ReadByteArray(7));
                pack.Lock();
                client.SendFromClient(pack);
                return true;
        }
        return false;
    }

    public bool OnGatewayPacketReceived(Packet packet, Client client)
    {
        if (!_isEnable && _isLogined) { return false; }
        //Console.WriteLine(Utility.PacketDebug(typeof(OpCodes.GatewayServer), packet));
        Packet pack;
        switch ((OpCodes.GatewayServer)packet.Opcode)
        {
            #region OpCodes.GatewayServer.ServerList
            case OpCodes.GatewayServer.ServerList:
                if (!_isLoginSend && !string.IsNullOrEmpty(_servName.ToUpperInvariant()))
                {
                    pack = new Packet((ushort)OpCodes.GatewayClient.Login, true);
                    pack.WriteByte(client.DivisionServer.Locale);
                    pack.WriteAscii(_userId.ToLowerInvariant());
                    pack.WriteAscii(_password.ToLowerInvariant());
                    pack.WriteUShort(client.Servers.First(serv => serv.Name.ToUpperInvariant().Contains(_servName.ToUpperInvariant())).Id);
                    pack.Lock();
                    _isLoginSend = true;
                    client.SendFromClient(pack);
                }
                break;
            #endregion

            #region OpCodes.GatewayServer.LoginReply
            case OpCodes.GatewayServer.LoginReply:
                _isLoginSend = false;
                if (packet.ReadByte() == 1)
                {
                    if (!Database.Exists(data => data.ID == _userId.ToLowerInvariant()))
                    {
                        Database.Insert(new AutoLoginData
                        {
                            CharName = _charName,
                            IsEnable = _isEnable,
                            Password = _password,
                            ServerName = _servName,
                            ID = _userId
                        });
                    }
                }
                else
                {
                    switch (packet.ReadByte())
                    {
                        case 1:
                            var currentTry = packet.ReadUInt();
                            var maxTry = packet.ReadUInt();
                            Console.WriteLine("The password is wrong, please try again (" + maxTry + "/" + currentTry + ")");
                            break;
                        case 2:
                            if (packet.ReadByte() == 1)
                            {
                                var reason = packet.ReadAscii();
                                Console.WriteLine("This user is blocked - " + reason);
                            }
                            break;
                        case 3:
                            Console.WriteLine("The user is allready logged in");
                            break;
                        case 5:
                            Console.WriteLine("The server is full!");
                            break;
                        case 7:
                            Console.WriteLine("Error while logging in: [C7]");
                            break;
                    }
                }
                break;
            #endregion

            #region OpCodes.GatewayServer.Captcha
            case OpCodes.GatewayServer.Captcha:
                //client.SendFromServer(packet);
                pack = new Packet((ushort)OpCodes.GatewayClient.Captcha);
                pack.WriteAscii("1");
                pack.Lock();
                client.SendFromClient(pack);
                return true;
                #endregion

        }
        return false;
    }

    public bool OnAgentPacketReceived(Packet packet, Client client)
    {
        if (!_isEnable && _isLogined) { return false; }
        //Console.WriteLine(Utility.PacketDebug(typeof(OpCodes.AgentServer), packet));
        switch ((OpCodes.AgentServer)packet.Opcode)
        {
            #region OpCodes.AgentServer.CharacterListing
            case OpCodes.AgentServer.CharacterListing:
                if (!string.IsNullOrEmpty(_charName))
                {
                    var pack = new Packet((ushort)OpCodes.AgentClient.SelectCharacter);
                    pack.WriteAscii(
                        client.CharacterListings.First(cht => cht.Name.ToUpperInvariant().Contains(_charName.ToUpperInvariant())).Name);
                    pack.Lock();
                    client.SendFromClient(pack);
                    _isLogined = true;
                    return true;
                }
                break;
                #endregion
        }
        return false;
    }

    #region Connect/Kicked/Disconnect
    public bool OnClientConnected(Packet packet, Client client)
    {
        return false;
    }
    public bool OnClientDisconnected(Packet packet, Client client)
    {
        _isLoginSend = false;
        return false;
    }


    public bool OnGatewayKicked(Packet packet, Client client)
    {
        return false;
    }

    public bool OnGatewayConnected(Packet packet, Client client)
    {
        return false;
    }

    public bool OnGatewayDisconnected(Packet packet, Client client)
    {
        return false;
    }


    public bool OnAgentKicked(Packet packet, Client client)
    {
        _isLoginSend = false;
        _isLogined = false;
        return false;
    }

    public bool OnAgentConnected(Packet packet, Client client)
    {
        _isLoginSend = false;
        return false;
    }

    public bool OnAgentDisconnected(Packet packet, Client client)
    {
        return false;
    }
    #endregion


}

