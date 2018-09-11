using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace PixelPerfect.Pages
{
    /// <summary>
    /// Логика взаимодействия для ForgePage.xaml
    /// </summary>
    public partial class ForgePage : Page
    {
        MainWindow mw = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

        private string gamePath, versionsPath;
        private Dictionary<string, ForgeVersion> versions;

        public ObservableCollection<string> cbVersions = new ObservableCollection<string>();
        public ObservableCollection<string> cbForgeVersions = new ObservableCollection<string>();

        public ForgePage()
        {
            InitializeComponent();
        }

        private async void installB_Click(object sender, RoutedEventArgs e)
        {
            versionsCB.IsEnabled = forgeVersionsCB.IsEnabled = installB.IsEnabled = false;

            string selectedVersion = versionsCB.SelectedItem.ToString();
            string selectedForgeVersion = forgeVersionsCB.SelectedItem.ToString();

            installingAttentionL.Content = "Идёт установка " + selectedForgeVersion + ".";
            installingAttentionL.Visibility = Visibility.Visible;

            await Utils.installForge(selectedVersion, selectedForgeVersion, gamePath);

            installingAttentionL.Content = "Forge версии " + selectedVersion + "-" + selectedForgeVersion + " успешно установлен.";
            versionsCB.IsEnabled = forgeVersionsCB.IsEnabled = installB.IsEnabled = true;

            mw.updateGamePath();

            versionsCB.SelectedItem = selectedVersion;
            forgeVersionsCB.SelectedIndex = 0;
        }

        private void versionsCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (versionsCB.SelectedIndex == -1 || !versionsCB.IsEnabled)
                return;

            string selectedVersion = versionsCB.SelectedItem.ToString();

            cbForgeVersions.Clear();
            foreach (string forgeVersion in versions[selectedVersion].versions)
                if (!File.Exists(versionsPath + selectedVersion + "-" + forgeVersion + "\\" + selectedVersion + "-" + forgeVersion + ".json"))
                    cbForgeVersions.Add(forgeVersion);

            CollectionViewSource source = Resources["forgeVersionsSource"] as CollectionViewSource;
            source.Source = cbForgeVersions;

            installB.IsEnabled = true;
            forgeVersionsCB.SelectedIndex = 0;
        }

        public void loadData(string gamePath, Dictionary<string, ForgeVersion> versions)
        {
            if (!versionsCB.IsEnabled)
                return;

            this.gamePath = gamePath;
            this.versions = versions;
            versionsPath = gamePath + "\\versions\\";

            cbForgeVersions.Clear();
            cbVersions.Clear();
            foreach (KeyValuePair<string, ForgeVersion> version in versions)
                if (File.Exists(versionsPath + version.Key + "\\" + version.Key + ".jar"))
                    cbVersions.Add(version.Key);

            CollectionViewSource source = Resources["versionsSource"] as CollectionViewSource;
            source.Source = cbVersions;
        }
    }
}
