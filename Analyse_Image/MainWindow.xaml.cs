using Microsoft.Win32;
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

namespace Analyse_Image
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


        }

        private void CloseAll(object sender, RoutedEventArgs e)
        {
            leftGrid.Children.Clear();
            rightGrid.Children.Clear();
        }

        private void Open(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                b.Content = op.FileName;
                BitmapImage bitmapImage= new BitmapImage(new Uri(op.FileName));
                Image image = new Image();
                image.Source = bitmapImage;
                leftGrid.Children.Clear();
                leftGrid.Children.Add(image);
            }
        }
        private void Save(object sender, RoutedEventArgs e)
        {
            //TODO
        }
    }
}
