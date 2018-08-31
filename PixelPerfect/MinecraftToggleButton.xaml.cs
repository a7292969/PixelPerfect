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
    /// Логика взаимодействия для MinecraftToggleButton.xaml
    /// </summary>
    public partial class MinecraftToggleButton : UserControl
    {
        public MinecraftToggleButton()
        {
            InitializeComponent();
        }

        public string MainText
        {
            get { return (string)GetValue(mainTextProperty); }
            set { SetValue(mainTextProperty, value); }
        }
        public static DependencyProperty mainTextProperty = DependencyProperty.Register("MainText", typeof(string), typeof(MinecraftToggleButton), null);

        public Brush MainTextForeground
        {
            get { return (Brush)GetValue(mainTextForegroundProperty); }
            set { SetValue(mainTextForegroundProperty, value); }
        }
        public static DependencyProperty mainTextForegroundProperty = DependencyProperty.Register("MainTextForeground", typeof(Brush), typeof(MinecraftToggleButton), null);

        public double CheckOpacity
        {
            get { return (double)GetValue(checkOpacityProperty); }
            set { SetValue(checkOpacityProperty, value); }
        }
        public static DependencyProperty checkOpacityProperty = DependencyProperty.Register("CheckOpacity", typeof(double), typeof(MinecraftToggleButton), null);
    }
}
