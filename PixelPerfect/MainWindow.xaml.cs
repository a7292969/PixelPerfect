using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PixelPerfect.Pages;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace PixelPerfect
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GeneralPage generalPage;
        private Page settingsPage;
        private StatusPage statusPage;
        private Page addProfilePage;
        private Page editProfilePage;

        private JObject settings;

        private string ppPath, configPath;

        public MainWindow()
        {
            InitializeComponent();

            generalPage = new GeneralPage();
            settingsPage = new SettingsPage();
            statusPage = new StatusPage();
            addProfilePage = new AddProfilePage();
            editProfilePage = new EditProfilePage();

            //for (int i = 0; i < 10; i++)
            //{
            //    ProfileItem item = new ProfileItem();
            //    item.Width = double.NaN;
            //    item.Height = 68;
            //    item.MainText = "Срач версия";
            //    item.SubText = "1.10.2";
            //    Thickness margin = item.Margin;
            //    margin.Top = -1;
            //    item.Margin = margin;
            //    item.IconImage = new BitmapImage(new Uri("Images/block_granite.png", UriKind.Relative));

            //    profilesSP.Children.Add(item);
            //}

            // Set values for startup animations
            bottomG.Height = 41;
            playButtonsSP.Opacity = 0.0;

            // ----------------------------------------------------

            ppPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\AppData\\Roaming\\PixelPerfect\\";
            configPath = ppPath + "\\config.json";
            Directory.CreateDirectory(ppPath);

            loadConfig();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            loadSelectedPage();
        }

        private void minecraftImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://minecraft.net");
        }

        private async void addProfileB_Click(object sender, RoutedEventArgs e)
        {
            profilesSV.Visibility = Visibility.Hidden;
            hidePlayBar();
            navigatePage(addProfilePage, false, false);

            Console.WriteLine(await Utils.GetPlayerUUID("MotanGish"));

            Console.WriteLine(Utils.GenerateClientToken());
        }

        private void editProfileB_Click(object sender, RoutedEventArgs e)
        {
            profilesSV.Visibility = Visibility.Hidden;
            hidePlayBar();
            navigatePage(editProfilePage, false, false);
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
            loadGeneralPage();
        }

        private void settingsTB_Clicked(object sender, MouseButtonEventArgs e)
        {
            updateMainToggleButtons(settingsTB);
            loadSettingsPage();
        }

        private void statusTB_Clicked(object sender, MouseButtonEventArgs e)
        {
            updateMainToggleButtons(statusTB);
            loadStatusPage();
        }

        private void showPlayBar()
        {
            playButtonsSP.Visibility = Visibility.Visible;

            DoubleAnimation anim0 = new DoubleAnimation(80, TimeSpan.FromMilliseconds(200));

            QuarticEase easingFunction = new QuarticEase();
            easingFunction.EasingMode = EasingMode.EaseInOut;

            anim0.EasingFunction = easingFunction;
            anim0.Completed += new EventHandler((object sender2, EventArgs e2) =>
            {
                DoubleAnimation anim = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(300));
                anim.EasingFunction = easingFunction;
                playButtonsSP.BeginAnimation(OpacityProperty, anim);

                Thickness margin = frameSV.Margin;
                margin.Bottom = bottomG.Height;
                frameSV.Margin = margin;
            });

            bottomG.BeginAnimation(HeightProperty, anim0);
        }

        private void hidePlayBar()
        {
            int h = 41;

            DoubleAnimation anim0 = new DoubleAnimation(h, TimeSpan.FromMilliseconds(200));

            QuarticEase easingFunction = new QuarticEase();
            easingFunction.EasingMode = EasingMode.EaseInOut;

            bottomG.BeginAnimation(HeightProperty, anim0);

            DoubleAnimation anim = new DoubleAnimation(0.0, TimeSpan.FromMilliseconds(300));
            anim.EasingFunction = easingFunction;
            anim.Completed += new EventHandler((object sender, EventArgs e) =>
            {
                playButtonsSP.Visibility = Visibility.Hidden;
            });

            Thickness margin = frameSV.Margin;
            margin.Bottom = h;
            frameSV.Margin = margin;

            playButtonsSP.BeginAnimation(OpacityProperty, anim);
        }

        private void updateMainToggleButtons(MinecraftToggleButton checkedButton)
        {
            DoubleAnimation anim0 = new DoubleAnimation(generalTB.CheckOpacity, generalTB.Equals(checkedButton) ? 1 : 0, TimeSpan.FromMilliseconds(220));
            DoubleAnimation anim1 = new DoubleAnimation(settingsTB.CheckOpacity, settingsTB.Equals(checkedButton) ? 1 : 0, TimeSpan.FromMilliseconds(220));
            DoubleAnimation anim2 = new DoubleAnimation(statusTB.CheckOpacity, statusTB.Equals(checkedButton) ? 1 : 0, TimeSpan.FromMilliseconds(220));

            generalTB.BeginAnimation(MinecraftToggleButton.checkOpacityProperty, anim0);
            settingsTB.BeginAnimation(MinecraftToggleButton.checkOpacityProperty, anim1);
            statusTB.BeginAnimation(MinecraftToggleButton.checkOpacityProperty, anim2);
        }

        public void loadGeneralPage()
        {
            generalPage.updateNews();
            navigatePage(generalPage, false, true);
            showPlayBar();
        }

        public void loadSettingsPage()
        {
            navigatePage(settingsPage, false, false);
            showPlayBar();
        }

        public void loadStatusPage()
        {
            statusPage.updateStatuses();
            navigatePage(statusPage, false, false);
            showPlayBar();
        }

        public void loadSelectedPage()
        {
            if (generalTB.CheckOpacity == 1)
                loadGeneralPage();
            else if (settingsTB.CheckOpacity == 1)
                loadSettingsPage();

            showPlayBar();
        }

        public void navigatePage(Page page, bool disableScroll, bool stretch)
        {
            Frame frame = new Frame();

            if (stretch)
            {
                frame.HorizontalAlignment = HorizontalAlignment.Stretch;
                frame.VerticalAlignment = VerticalAlignment.Stretch;
            }
            else
            {
                frame.HorizontalAlignment = HorizontalAlignment.Center;
                frame.VerticalAlignment = VerticalAlignment.Center;

                Thickness margin = frame.Margin;
                margin.Top = 20;
                margin.Bottom = 30;
                frame.Margin = margin;
            }

            frame.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            frame.Content = page;

            frameSV.Content = null;
            frameG.Children.Clear();

            if (disableScroll)
                frameG.Children.Add(frame);
            else
                frameSV.Content = frame;
        }

        public void loadConfig()
        {
            if (File.Exists(configPath))
            {
                try
                {
                    settings = JObject.Parse(File.ReadAllText(configPath));
                }
                catch
                {
                    createNewConfig();
                }
            }
            else
            {
                createNewConfig();
            }
        }

        public void createNewConfig()
        {
            if (File.Exists(configPath))
                File.Delete(configPath);

            settings = new JObject();
            settings.Add("gamePath", ppPath + "Minecraft");

            File.WriteAllText(configPath, JsonConvert.SerializeObject(settings));



            //Console.WriteLine(roamingPath);
        }
    }
}
