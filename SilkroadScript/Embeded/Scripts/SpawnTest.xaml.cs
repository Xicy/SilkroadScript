using System.Windows.Controls;
using System.Windows.Threading;
using Shared;

namespace SilkroadScript.Embeded.Scripts
{
    /// <summary>
    /// Interaction logic for SpawnTest.xaml
    /// </summary>
    public partial class SpawnTest : Page, IScript, IContent, IAgent
    {
        public SpawnTest()
        {
            InitializeComponent();
        }

        string IScript.Name => "Spawn Test";
        int IScript.Version => 0;
        string IScript.Description => "Spawn Test";

        void IScript.Loaded()
        {

        }

        public bool OnAgentKicked(Packet packet, Client client)
        {
            return false;
        }

        public bool OnAgentConnected(Packet packet, Client client)
        {
            playes.InvokeIfRequired(() => playes.ItemsSource = client.Players, DispatcherPriority.Background);
            monsters.InvokeIfRequired(() => monsters.ItemsSource = client.Monsters, DispatcherPriority.Background);
            pets.InvokeIfRequired(() => pets.ItemsSource = client.Pets, DispatcherPriority.Background);
            items.InvokeIfRequired(() => items.ItemsSource = client.Items, DispatcherPriority.Background);
            npcs.InvokeIfRequired(() => npcs.ItemsSource = client.NPCs, DispatcherPriority.Background);
            return false;
        }

        public bool OnAgentDisconnected(Packet packet, Client client)
        {
            return false;
        }

        public bool OnAgentPacketReceived(Packet packet, Client client)
        {
            switch ((OpCodes.AgentServer)packet.Opcode)
            {
                case OpCodes.AgentServer.GroupSpawnEnd:
                    playes.InvokeIfRequired(playes.Items.Refresh,DispatcherPriority.Background);
                    items.InvokeIfRequired(items.Items.Refresh, DispatcherPriority.Background);
                    monsters.InvokeIfRequired(monsters.Items.Refresh, DispatcherPriority.Background);
                    pets.InvokeIfRequired(pets.Items.Refresh, DispatcherPriority.Background);
                    npcs.InvokeIfRequired(npcs.Items.Refresh, DispatcherPriority.Background);
                    break;
            }
            return false;
        }

    }
}
