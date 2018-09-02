using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Логика взаимодействия для EditProfilePage.xaml
    /// </summary>
    public partial class EditProfilePage : Page
    {
        ResourceDictionary res = (ResourceDictionary)Application.LoadComponent(new Uri("styles.xaml", UriKind.Relative));
        MainWindow mw = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

        OpenFileDialog openFileDialog;

        private string[] defaultIconNames =
        {
            "Bedrock", "Bookshelf", "Brick", "Chest", "Clay", "Coal_Block", "Coal_Ore", "Cobblestone",
            "Crafting_Table", "Diamond_Ore", "Dirt", "Dirt_Podzol", "Dirt_Snow", "Emerald_Block", "Emerald_Ore", "End_Stone",
            "Farmland", "Furnace", "Furnace_On", "Glass", "Glowstone", "Gold_Block", "Gold_Ore", "Grass",
            "Gravel", "Hardened_Clay", "Ice_Packed", "Iron_Block", "Iron_Ore", "Lapis_Ore", "Leaves_Birch", "Leaves_Jungle",
            "Leaves_Oak", "Leaves_Spruce", "Log_Acacia", "Log_Birch", "Log_DarkOak", "Log_Jungle", "Log_Oak", "Log_Spruce",
            "Mycelium"
        };

        public EditProfilePage()
        {
            InitializeComponent();

            openFileDialog = new OpenFileDialog();
            openFileDialog.CheckFileExists = true;
            openFileDialog.DefaultExt = ".png";
            openFileDialog.Filter = "PNG Image (*.png)|*.png";
            openFileDialog.FileOk += openFileDialog_FileOk;

            foreach (string icon in defaultIconNames)
            {
                Button button = new Button();
                button.Width = 50;
                button.Height = 50;

                Thickness margin = button.Margin;
                margin.Left = 4;
                margin.Top = 4;

                button.Margin = margin;
                button.Style = (Style)res["MCIconItem"];
                button.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,/Images/Blocks/" + icon + ".png", UriKind.Absolute)));
                button.Click += selectedIconFromDefaults;

                iconsWP.Children.Add(button);
            }
        }

        private void saveB_Click(object sender, RoutedEventArgs e)
        {
            mw.playButtonsSP.Visibility = Visibility.Visible;
            mw.loadSelectedPage();
        }

        private void cancelB_Click(object sender, RoutedEventArgs e)
        {
            mw.playButtonsSP.Visibility = Visibility.Visible;
            mw.loadSelectedPage();
        }

        private void selectIconB_Click(object sender, RoutedEventArgs e)
        {
            iconSelecionG.Visibility = Visibility.Visible;
        }

        private void cancelIconSelectB_Click(object sender, RoutedEventArgs e)
        {
            iconSelecionG.Visibility = Visibility.Collapsed;
        }

        private void selectIconFileB_Click(object sender, RoutedEventArgs e)
        {
            openFileDialog.ShowDialog();
        }

        private void selectedIconFromDefaults(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ImageBrush image = (ImageBrush)button.Background;

            selectIconB.Background = image;
            iconSelecionG.Visibility = Visibility.Collapsed;
        }

        private void openFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            BitmapImage image = new BitmapImage(new Uri(openFileDialog.FileName));

            if (image.Width == image.Height)
            {
                selectIconB.Background = new ImageBrush(image);
                imageAttentionL.Visibility = Visibility.Collapsed;
                iconSelecionG.Visibility = Visibility.Collapsed;
            }
            else
            {
                imageAttentionL.Visibility = Visibility.Visible;
            }

            iconSelecionG.Visibility = Visibility.Collapsed;
        }
    }
}
