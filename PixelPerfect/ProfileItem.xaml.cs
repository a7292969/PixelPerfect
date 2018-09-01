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

namespace PixelPerfect
{
    /// <summary>
    /// Логика взаимодействия для ProfileItem.xaml
    /// </summary>
    public partial class ProfileItem : UserControl
    {
        public ProfileItem()
        {
            InitializeComponent();
        }

       

        public string MainText
        {
            get { return (string)GetValue(mainTextProperty); }
            set { SetValue(mainTextProperty, value); }
        }
        public static DependencyProperty mainTextProperty = DependencyProperty.Register("MainText", typeof(string), typeof(ProfileItem), null);

        public string SubText
        {
            get { return (string)GetValue(subTextProperty); }
            set { SetValue(subTextProperty, value); }
        }
        public static DependencyProperty subTextProperty = DependencyProperty.Register("SubText", typeof(string), typeof(ProfileItem), null);

        public ImageSource IconImage
        {
            get { return (ImageSource)GetValue(iconImageProperty); }
            set { SetValue(iconImageProperty, value); }
        }
        public static DependencyProperty iconImageProperty = DependencyProperty.Register("IconImage", typeof(ImageSource), typeof(ProfileItem), null);
    }
}
