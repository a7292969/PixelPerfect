using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PixelPerfect
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }

        private void playB_Click(object sender, RoutedEventArgs e)
        {
            profilesSV.Visibility = Visibility.Hidden;
            playButtonsSP.Visibility = Visibility.Hidden;

            DoubleAnimation anim0 = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
            basePlayInfoGrid.BeginAnimation(OpacityProperty, anim0);

            DoubleAnimation anim1 = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            downloadInfoGrid.BeginAnimation(OpacityProperty, anim1);
        }

        private void addProfileB_Click(object sender, RoutedEventArgs e)
        {

        }

        private void profilesB_Click(object sender, RoutedEventArgs e)
        {
            if (profilesSV.Visibility == Visibility.Visible)
                profilesSV.Visibility = Visibility.Hidden;
            else
                profilesSV.Visibility = Visibility.Visible;
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            profilesSV.Visibility = Visibility.Hidden;
        }

        private void generalTB_Clicked(object sender, MouseButtonEventArgs e)
        {
            updateMainToggleButtons(generalTB);
        }

        private void settingsTB_Clicked(object sender, MouseButtonEventArgs e)
        {
            updateMainToggleButtons(settingsTB);
        }

        private void profilesTB_Clicked(object sender, MouseButtonEventArgs e)
        {
            updateMainToggleButtons(profilesTB);
        }

        private void updateMainToggleButtons(MinecraftToggleButton checkedButton)
        {
            DoubleAnimation anim0 = new DoubleAnimation(generalTB.CheckOpacity, generalTB.Equals(checkedButton) ? 1 : 0, TimeSpan.FromMilliseconds(250));
            DoubleAnimation anim1 = new DoubleAnimation(settingsTB.CheckOpacity, settingsTB.Equals(checkedButton) ? 1 : 0, TimeSpan.FromMilliseconds(250));
            DoubleAnimation anim2 = new DoubleAnimation(profilesTB.CheckOpacity, profilesTB.Equals(checkedButton) ? 1 : 0, TimeSpan.FromMilliseconds(250));

            generalTB.BeginAnimation(MinecraftToggleButton.checkOpacityProperty, anim0);
            settingsTB.BeginAnimation(MinecraftToggleButton.checkOpacityProperty, anim1);
            profilesTB.BeginAnimation(MinecraftToggleButton.checkOpacityProperty, anim2);

           
        }
    }
}
