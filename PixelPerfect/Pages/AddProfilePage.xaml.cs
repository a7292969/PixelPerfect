using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PixelPerfect.Pages
{
    /// <summary>
    /// Логика взаимодействия для AddProfilePage.xaml
    /// </summary>
    public partial class AddProfilePage : Page
    {
        MainWindow mw = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

        string latestVersion = "";

        public AddProfilePage()
        {
            InitializeComponent();
        }

        private void saveB_Click(object sender, RoutedEventArgs e)
        {
            string name = nameTB.Text;

            if (mw.isProfileExists(name))
            {
                existsAttentionL.Visibility = Visibility.Visible;
            }
            else
            {
                BitmapImage img = Utils.ImageFromResource("Images\\Blocks\\Furnace.png");
                byte[] bytes = Utils.ImageToBytes(img);

                JObject profile = new JObject();
                profile.Add("icon", Convert.ToBase64String(bytes));
                profile.Add("version", latestVersion);
                profile.Add("custom", false);
                profile.Add("javaArgs", "-Xmx1G -XX:+UnlockExperimentalVMOptions -XX:+UseG1GC -XX:G1NewSizePercent=20 -XX:G1ReservePercent=20 -XX:MaxGCPauseMillis=50 -XX:G1HeapRegionSize=16M");

                mw.setProfile(name, name, profile);

                nameTB.Text = string.Empty;

                mw.playButtonsSP.Visibility = Visibility.Visible;
                mw.updateProfileItems();


                mw.selectProfile(name);
                mw.editSelectedProfile();
            }
        }

        private void cancelB_Click(object sender, RoutedEventArgs e)
        {
            mw.playButtonsSP.Visibility = Visibility.Visible;
            mw.loadSelectedPage();
        }

        private void nameTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            saveB.IsEnabled = !string.IsNullOrWhiteSpace(nameTB.Text);
            existsAttentionL.Visibility = Visibility.Collapsed;
        }

        public void loadVersionManifest(VersionManifest manifest)
        {
            latestVersion = manifest.latestVersion;
        }
    }
}
