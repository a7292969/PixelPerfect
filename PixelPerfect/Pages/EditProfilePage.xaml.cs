﻿using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

        private string oldName, selectedIconPath, iconData;

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

        private void nameTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            existsAttentionL.Visibility = Visibility.Collapsed;
        }

        private void saveB_Click(object sender, RoutedEventArgs e)
        {
            string name = nameTB.Text;

            Console.WriteLine(name + " " + oldName);

            if (name != oldName && mw.isProfileExists(name))
            {
                existsAttentionL.Visibility = Visibility.Visible;
            }
            else
            {
                JObject profile = new JObject();

                if (string.IsNullOrWhiteSpace(selectedIconPath))
                    profile.Add("icon", iconData);
                else
                {
                    BitmapImage img = new BitmapImage(new Uri(selectedIconPath, UriKind.Absolute));
                    byte[] bytes = Utils.ImageToBytes(img);
                    string newIconData = Convert.ToBase64String(bytes);
                    profile.Add("icon", newIconData);
                }
                
                profile.Add("version", versionsCB.SelectedItem.ToString());
                profile.Add("custom", false);
                profile.Add("javaArgs", javaParamsTB.Text);

                mw.setProfile(oldName, name, profile);

                mw.playButtonsSP.Visibility = Visibility.Visible;
                mw.loadSelectedPage();
                mw.updateProfileItems();
            }
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

            selectedIconPath = ((ImageBrush)selectIconB.Background).ImageSource.ToString();
        }

        private void openFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            BitmapImage image = new BitmapImage(new Uri(openFileDialog.FileName));

            if (image.Width == image.Height)
            {
                selectIconB.Background = new ImageBrush(image);
                imageAttentionL.Visibility = Visibility.Collapsed;
                iconSelecionG.Visibility = Visibility.Collapsed;
                selectedIconPath = new Uri(((ImageBrush)selectIconB.Background).ImageSource.ToString()).AbsolutePath;
            }
            else
            {
                imageAttentionL.Visibility = Visibility.Visible;
            }

            iconSelecionG.Visibility = Visibility.Collapsed;
        }


        public void updateVersions(VersionManifest manifest)
        {
            versionsCB.Items.Clear();

            foreach (KeyValuePair<string, MCVersion> version in manifest.versions)
                versionsCB.Items.Add(version.Key);

            versionsCB.SelectedIndex = 0;
        }
     
        public void load(string name, JObject profile)
        {
            oldName = name;

            nameTB.Text = name;
            versionsCB.SelectedValue = profile["version"].ToString();
            javaParamsTB.Text = profile["javaArgs"].ToString();

            if (string.IsNullOrWhiteSpace(javaParamsTB.Text))
                javaParamsTB.Text = "-Xmx1G -XX:+UnlockExperimentalVMOptions -XX:+UseG1GC -XX:G1NewSizePercent=20 -XX:G1ReservePercent=20 -XX:MaxGCPauseMillis=50 -XX:G1HeapRegionSize=16M";

            iconData = profile["icon"].ToString();

            BitmapImage img = Utils.BytesToImage(Convert.FromBase64String(iconData));
            selectIconB.Background = new ImageBrush(img);

            selectIconB.IsEnabled = !(name == "Последний выпуск" || name == "Предварительная версия");
            nameTB.IsEnabled = !(name == "Последний выпуск" || name == "Предварительная версия");
            versionsCB.IsEnabled = !(name == "Последний выпуск" || name == "Предварительная версия");
        }
    }
}
