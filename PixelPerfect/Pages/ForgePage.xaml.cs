using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PixelPerfect.Pages
{
    /// <summary>
    /// Логика взаимодействия для ForgePage.xaml
    /// </summary>
    public partial class ForgePage : Page
    {
        private string gamePath, versionsPath;
        private Dictionary<string, ForgeVersion> versions;

        public ObservableCollection<string> cbVersions = new ObservableCollection<string>();
        public ObservableCollection<string> cbForgeVersions = new ObservableCollection<string>();

        public ForgePage()
        {
            InitializeComponent();
        }

        private void installB_Click(object sender, RoutedEventArgs e)
        {
            versionsCB.IsEnabled = false;
            forgeVersionsCB.IsEnabled = false;
            installB.IsEnabled = false;

            installingAttentionL.Content = "Идёт установка " + forgeVersionsCB.SelectedItem.ToString() + ".";
            installingAttentionL.Visibility = Visibility.Visible;
        }

        private void versionsCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (versionsCB.SelectedIndex == -1 || installingAttentionL.Visibility == Visibility.Visible)
                return;

            cbForgeVersions.Clear();
            foreach (string forgeVersion in versions[versionsCB.SelectedItem.ToString()].versions)
                if (!File.Exists(versionsPath + forgeVersion + "\\" + forgeVersion + ".jar"))
                    cbForgeVersions.Add(forgeVersion);

            CollectionViewSource source = Resources["forgeVersionsSource"] as CollectionViewSource;
            source.Source = cbForgeVersions;

            installB.IsEnabled = true;
        }

        public void loadData(string gamePath, Dictionary<string, ForgeVersion> versions)
        {
            if (installingAttentionL.Visibility == Visibility.Visible)
                return;

            this.gamePath = gamePath;
            this.versions = versions;
            versionsPath = gamePath + "\\versions\\";

            cbVersions.Clear();
            foreach (KeyValuePair<string, ForgeVersion> version in versions)
                if (File.Exists(versionsPath + version.Key + "\\" + version.Key + ".jar"))
                    cbVersions.Add(version.Key);

            CollectionViewSource source = Resources["versionsSource"] as CollectionViewSource;
            source.Source = cbVersions;
        }

        public void update()
        {

        }
    }
}
