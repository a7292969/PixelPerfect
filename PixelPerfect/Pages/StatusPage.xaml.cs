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
    /// Логика взаимодействия для StatusPage.xaml
    /// </summary>
    public partial class StatusPage : Page
    {
        BitmapImage statusGreen, statusYellow, statusRed, statusMessage, statusUpdate;

        public StatusPage()
        {
            InitializeComponent();

            statusGreen = Utils.ImageFromResource("Images/status_green.png");
            statusYellow = Utils.ImageFromResource("Images/status_yellow.png");
            statusRed = Utils.ImageFromResource("Images/status_red.png");
            statusMessage = Utils.ImageFromResource("Images/status_message.png");
            statusUpdate = Utils.ImageFromResource("Images/status_update.png");
        }

        public void setStatus(string status, string message, Image image, Label text)
        {
            switch (status)
            {
                case "green":
                    image.Source = statusGreen;
                    text.Content = "Сервис работает нормально.";
                    break;
                case "yellow":
                    image.Source = statusYellow;
                    text.Content = "Имеются некоторые проблемы.";
                    break;
                case "red":
                    image.Source = statusRed;
                    text.Content = "Сервис временно недоступен.";
                    break;
                case "message":
                    image.Source = statusMessage;
                    text.Content = message;
                    break;
                case "update":
                    image.Source = statusUpdate;
                    text.Content = "Сервис обновляется.";
                    break;
            }
        }

        public void updateStatuses()
        {

        }
    }
}
