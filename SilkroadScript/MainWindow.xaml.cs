using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Shared;
using Shared.Core;
using Shared.Structs;
using Shared.Structs.Data;

namespace SilkroadScript
{
    public partial class MainWindow
    {
        public static Client Client;

        public void AddPage(IScript page)
        {
            TabControl.Items.Add(new TabItem { Header = page.Name, Content = ((IContent)page).Content });
        }

        public MainWindow()
        {
            Client = new Client();

            ScriptEngine.LoadAllScripts(Client);
            InitializeComponent();
            ScriptEngine.ScriptsWithContents.ForEach(AddPage);

            Show();
            if (Data.Load("", ""))
            {
                GamePath.Text = Data.SilkroadPath;
                Divisions.ItemsSource = Data.DivisionServers;
                Divisions.SelectedIndex = 0;
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            Client.StartGame((DivisionServer)Divisions.SelectedItem, (string)IPlist.SelectedItem);
        }

        private void FindGame_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Filter = "Silkroad Client|sro_client.exe";
            ofd.Title = "Find Game Path";
            if (!ofd.ShowDialog(this).Value) return;
            GamePath.Text = Path.GetDirectoryName(ofd.FileName);
            Data.Load(GamePath.Text, "169841");
            Divisions.ItemsSource = Data.DivisionServers;
            Divisions.SelectedIndex = 0;
        }

        private void Divisions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IPlist.ItemsSource = ((DivisionServer)Divisions.SelectedItem).LoginServers;
            IPlist.SelectedIndex = 0;
        }

    }
}
