using System;
using System.Collections.Generic;
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

            Button button = new Button();
            button.Width = 48;
            button.Height = 48;
            button.Style = (Style)res["SelectIconButton"];
            button.Background = new ImageBrush(new BitmapImage(new Uri("Images/Blocks/Mycelium.png", UriKind.Relative)));
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

        }
    }
}
