using Newtonsoft.Json.Linq;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PixelPerfect.Pages
{
    /// <summary>
    /// Логика взаимодействия для SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        MainWindow mw = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
        System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;

        public SettingsPage()
        {
            InitializeComponent();

            folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.ShowNewFolderButton = true;
        }

        private void selectFolderB_Click(object sender, RoutedEventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pathTB.Text = folderBrowserDialog.SelectedPath;
                JObject settings = mw.getConfig();
                settings["gamePath"] = pathTB.Text;
                mw.setConfig(settings);
                mw.updateGamePath();
            }
        }

        public void loadSettings(JObject settings)
        {
            pathTB.Text = (string)settings["gamePath"];
            widthTB.Text = (string)settings["width"];
            heightTB.Text = (string)settings["height"];
            showSnapshotsCB.IsChecked = (bool)settings["showSnapshots"];
        }

        private void widthTB_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            try
            {
                if (int.Parse(widthTB.Text + e.Text) > 64000)
                    e.Handled = true;
            }
            catch
            {
                e.Handled = true;
            }
        }

        private void heightTB_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            try
            {
                if (int.Parse(heightTB.Text + e.Text) > 64000)
                    e.Handled = true;
            }
            catch
            {
                e.Handled = true;
            }
        }

        private void widthTB_LostFocus(object sender, RoutedEventArgs e)
        {
            int width = int.Parse(widthTB.Text);
            JObject settings = mw.getConfig();
            settings["width"] = width;
            mw.setConfig(settings);
        }

        private void heightTB_LostFocus(object sender, RoutedEventArgs e)
        {
            int height = int.Parse(heightTB.Text);
            JObject settings = mw.getConfig();
            settings["height"] = height;
            mw.setConfig(settings);
        }

        private void showSnapshotsCB_Click(object sender, RoutedEventArgs e)
        {
            JObject settings = mw.getConfig();
            settings["showSnapshots"] = showSnapshotsCB.IsChecked;
            mw.setConfig(settings);
            mw.updateGamePath();
        }
    }
}
