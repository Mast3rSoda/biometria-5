using System;
using System.Collections.Generic;
using System.Drawing;
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

namespace biometria_5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MainImg.Source = new Bitmap("../../../apple.png")
                .ToSource();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            MainImg.Source = new Bitmap("../../../apple.png")
                .ToSource();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            MainImg.Source = Algorithm
                .Apply(new Bitmap("../../../apple.png"), (int)maxValue.Value , (int)minValue.Value)
                .ToSource();
        }

    }
}
