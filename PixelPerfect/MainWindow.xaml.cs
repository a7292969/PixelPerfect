using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PixelPerfect.Pages;
using System;
using System.Collections.Generic;
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
        private AddProfilePage addProfilePage;
        private EditProfilePage editProfilePage;

        private JObject settings;
        private string ppPath, configPath;

        private VersionManifest versionManifest;

        private BitmapImage grassIcon, craftingTableIcon;
        private string grassIconData, craftingTableIconData;

        public static string RELEASE_VERSION_NAME = "Последний выпуск";
        public static string SNAPSHOT_VERSION_NAME = "Предварительная версия";

        public MainWindow()
        {
            InitializeComponent();

            Console.WriteLine(Utils.computeFileHash("D:\\test"));

            generalPage = new GeneralPage();
            settingsPage = new SettingsPage();
            statusPage = new StatusPage();
            addProfilePage = new AddProfilePage();
            editProfilePage = new EditProfilePage();

            // Set values for startup animations
            bottomG.Height = 41;
            playButtonsSP.Opacity = 0.0;

            // ----------------------------------------------------
            grassIcon = Utils.ImageFromResource("Images\\Blocks\\Grass.png");
            craftingTableIcon = Utils.ImageFromResource("Images\\Blocks\\Crafting_Table.png");

            grassIconData = Convert.ToBase64String(Utils.ImageToBytes(grassIcon));
            craftingTableIconData = Convert.ToBase64String(Utils.ImageToBytes(craftingTableIcon));

            ppPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\AppData\\Roaming\\PixelPerfect\\";
            configPath = ppPath + "\\config.json";
            Directory.CreateDirectory(ppPath);

            loadConfig();

            versionManifest = Utils.GetMCVersions();

            addProfilePage.loadVersionManifest(versionManifest);
            editProfilePage.updateVersions(versionManifest);
            updateProfileItems();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            loadSelectedPage();
        }

        private void minecraftImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://minecraft.net");
        }

        private void addProfileB_Click(object sender, RoutedEventArgs e)
        {
            profilesSV.Visibility = Visibility.Hidden;
            hidePlayBar();
            navigatePage(addProfilePage, false, false);
        }

        private void editProfileB_Click(object sender, RoutedEventArgs e)
        {
            string selectedProfile = settings["selectedProfile"].ToString();
            editProfilePage.load(selectedProfile, getProfile(selectedProfile));

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

            downloadPB.Value = 0;


            JObject selectedProfile = getProfile((string)settings["selectedProfile"]);
            string name = (string)selectedProfile["name"];
            string version = (string)selectedProfile["version"];
            string gamePath = (string)settings["gamePath"];
            string path = (bool)selectedProfile["custom"] == true ? gamePath + "\\" + name + "\\" : gamePath + "\\";

            startGame(version, path);
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
            else if (statusTB.CheckOpacity == 1)
                loadStatusPage();

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

        public void clearProfileItems()
        {
            profilesSP.Children.Clear();
        }

        public void addProfileItem(string mainText, string subText, BitmapImage icon)
        {
            ProfileItem item = new ProfileItem();
            item.Width = double.NaN;
            item.Height = 68;
            item.MainText = mainText;
            item.SubText = subText;
            Thickness margin = item.Margin;
            margin.Top = profilesSP.Children.Count == 0 ? 0 : -1;
            item.Margin = margin;
            item.IconImage = icon;
            item.MouseUp += new MouseButtonEventHandler((object sender, MouseButtonEventArgs e) =>
            {
                ProfileItem i = (ProfileItem)sender;
                settings["selectedProfile"] = i.MainText;
                selectedProfileNameL.Content = i.MainText + " - " + i.SubText;
                saveConfig();
            });

            profilesSP.Children.Add(item);

            // Check for selected
            string selectedProfile = settings["selectedProfile"].ToString();
            if (selectedProfile == mainText)
                selectedProfileNameL.Content = item.MainText + " - " + item.SubText;
        }

        public void updateProfileItems()
        {
            clearProfileItems();

            if (versionManifest != null)
            {
                addProfileItem(RELEASE_VERSION_NAME, versionManifest.latestVersion, grassIcon);
                addProfileItem(SNAPSHOT_VERSION_NAME, versionManifest.latestSnapshot, craftingTableIcon);
            }

            JObject profiles = (JObject)settings["profiles"];

            foreach (JProperty property in profiles.Properties())
            { 
                JObject profile = (JObject)property.Value;
                string name = property.Name;

                Console.WriteLine(name);

                if (name != RELEASE_VERSION_NAME && name != SNAPSHOT_VERSION_NAME)
                {
                    string version = (string)profiles[name]["version"];
                    BitmapImage icon = Utils.BytesToImage(Convert.FromBase64String(profiles[name]["icon"].ToString()));
                    addProfileItem(name, version, icon);
                }
            }
        }

        public void loadConfig()
        {
            if (File.Exists(configPath))
                try
                {
                    settings = JObject.Parse(File.ReadAllText(configPath));
                }
                catch { createNewConfig(); }
            else createNewConfig();
        }

        public void saveConfig()
        {
            if (File.Exists(configPath))
                File.Delete(configPath);

            File.WriteAllText(configPath, JToken.Parse(JsonConvert.SerializeObject(settings)).ToString(Formatting.Indented));
        }

        public void createNewConfig()
        {
            settings = new JObject();
            settings["gamePath"] = ppPath + "Minecraft";
            settings["profiles"] = new JObject();
            settings["selectedProfile"] = RELEASE_VERSION_NAME;

            saveConfig();
        }

        public JObject getProfile(string name)
        {
            JObject o;

            if (name == RELEASE_VERSION_NAME)
            {
                o = new JObject();
                o["name"] = name;
                o["icon"] = grassIconData;
                o["version"] = versionManifest.latestVersion;
                o["custom"] = false;
                o["javaArgs"] = "";
            }
            else if (name == SNAPSHOT_VERSION_NAME)
            {
                o = new JObject();
                o["name"] = name;
                o["icon"] = craftingTableIconData;
                o["version"] = versionManifest.latestSnapshot;
                o["custom"] = false;
                o["javaArgs"] = "";
            }
            else
            {
                o = (JObject)settings["profiles"][name];
            }

            return o;
        }

        public void setProfile(string oldName, string newName, JObject profile)
        {
            if (oldName != newName)
            {
                if (oldName == (string)settings["selectedProfile"])
                    settings["selectedProfile"] = newName;

                settings["profiles"][oldName].Parent.Remove();
            }

            settings["profiles"][newName] = profile;
            saveConfig();
        }

        public void deleteProfile(string name)
        {
            settings["profiles"][name].Parent.Remove();

            if (name == (string)settings["selectedProfile"])
                settings["selectedProfile"] = RELEASE_VERSION_NAME;

            saveConfig();
        }

        public bool isProfileExists(string name)
        {
            return settings["profiles"][name] != null || name == RELEASE_VERSION_NAME || name == SNAPSHOT_VERSION_NAME;
        }

        public async void downloadVersion(string version)
        {
            List<FileToDownload> files = await Utils.GetFilesForDownload(version, (string)settings["gamePath"], versionManifest);

            FileDownloader d = new FileDownloader(files);
            d.OnCompleted += new FileDownloader.OnCompletedEventHandler((object sender) =>
            {
                Console.WriteLine("DOWN LOADED !!!!!");
            });
            d.Start();
            
            
        }

        public void startGame(string version, string path)
        {
            downloadVersion(version);
        }
    }
}
