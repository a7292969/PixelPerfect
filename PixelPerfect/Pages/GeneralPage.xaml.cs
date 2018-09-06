using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PixelPerfect.Pages
{
    /// <summary>
    /// Логика взаимодействия для GeneralPage.xaml
    /// </summary>
    public partial class GeneralPage : Page
    {
        public GeneralPage()
        {
            InitializeComponent();
        }

        public void addNews(string title, string text, string url, ImageSource image)
        {
            DropShadowEffect dropShadowEffect = new DropShadowEffect();
            dropShadowEffect.BlurRadius = 5;
            dropShadowEffect.ShadowDepth = 0;
            dropShadowEffect.RenderingBias = RenderingBias.Quality;

            Grid grid = new Grid();
            grid.HorizontalAlignment = HorizontalAlignment.Center;
            grid.VerticalAlignment = VerticalAlignment.Top;
            grid.Cursor = Cursors.Hand;
            grid.MouseUp += new MouseButtonEventHandler((object sender, MouseButtonEventArgs e) =>
            {
                Process.Start(url);
            });

            Image img = new Image();
            img.Source = image;


            Label titleL = new Label();
            titleL.HorizontalAlignment = HorizontalAlignment.Left;
            titleL.VerticalAlignment = VerticalAlignment.Top;

            Thickness margin = titleL.Margin;
            margin.Left = 20;
            margin.Top = 20;

            titleL.Margin = margin;
            titleL.FontFamily = new FontFamily("Source Sans Pro Semibold");
            titleL.FontSize = 22;
            titleL.Effect = dropShadowEffect;
            titleL.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            titleL.Content = title;


            Label textL = new Label();
            textL.HorizontalAlignment = HorizontalAlignment.Left;
            textL.VerticalAlignment = VerticalAlignment.Top;

            margin = textL.Margin;
            margin.Left = 21;
            margin.Top = 48;

            textL.Margin = margin;
            textL.FontFamily = new FontFamily("Source Sans Pro Semibold");
            textL.FontSize = 18;
            textL.Effect = dropShadowEffect;
            textL.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            textL.Content = text;



            grid.Children.Add(img);
            grid.Children.Add(titleL);
            grid.Children.Add(textL);

            newsSP.Children.Add(grid);
        }

        public async void updateNews()
        {
            string response = await Connector.GetAsync("http://launchermeta.mojang.com/mc/news.json");

            if (response == "-1")
                return;

            JObject obj = JObject.Parse(response);
            JArray entries = (JArray)obj["entries"];

            newsSP.Children.Clear();

            foreach (JObject o in entries)
            {
                string tag = (string)o["tags"][0];

                if (tag != "demo")
                {
                    string url = (string)o["content"]["en-us"]["action"];
                    string imgPath = (string)o["content"]["en-us"]["image"];
                    string title = (string)o["content"]["en-us"]["title"];
                    string text = (string)o["content"]["en-us"]["text"];

                    BitmapImage bitmap = new BitmapImage(new Uri(imgPath, UriKind.Absolute));
                    addNews(title, text, url, bitmap);
                }
            }
        }
    }
}
