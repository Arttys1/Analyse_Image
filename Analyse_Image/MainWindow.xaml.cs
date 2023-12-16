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
        private back.Image? leftImage  = null;
        private back.Image? rightImage = null;

        public MainWindow()
        {
            InitializeComponent();

        }

        private void CloseAll(object sender, RoutedEventArgs e)
        {
            leftGrid.Children.Clear();
            rightGrid.Children.Clear();
            leftImage = null;
            rightImage = null;
        }

        private void Open(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                BitmapImage bitmapImage= new BitmapImage(new Uri(op.FileName));
                leftImage = new back.Image(bitmapImage, back.ImageType.RGB);
                DisplayAnImageOnTheLeft(leftImage);
            }
        }
        private void Save(object sender, RoutedEventArgs e)
        {
            //TODO
            
        }

        private void Switch(object sender, RoutedEventArgs e)
        {
            rightGrid.Children.Clear();
            DisplayAnImageOnTheLeft(rightImage);
            rightImage = null;

        }

        private void GrayScale(object sender, RoutedEventArgs e)
        {
            if(leftImage != null)
            {
                back.Image grayImage = leftImage.ToGrayScale();
                DisplayAnImageOnTheRight(grayImage);
            }
        }

        public void Threshold(object sender, RoutedEventArgs e)
        {
            if(leftImage != null && leftImage.ImageType == back.ImageType.GRAY)
            {
                int threshold = leftImage.ComputeThreshold();
                rightImage = leftImage.ToBinaryImage(threshold);
                DisplayAnImageOnTheRight(rightImage);
            }
        }

        private void DisplayAnImageOnTheRight(back.Image? imageToDisplay)
        {
            if (imageToDisplay != null)
            {
                Image image = new Image();
                image.Source = imageToDisplay.GetBitMapImage();
                rightImage = imageToDisplay;
                rightGrid.Children.Clear();
                rightGrid.Children.Add(image);
            }
        }
        private void DisplayAnImageOnTheLeft(back.Image? imageToDisplay)
        {
            if (imageToDisplay != null)
            {
                Image image = new Image();
                image.Source = imageToDisplay.GetBitMapImage();
                leftImage = imageToDisplay;
                leftGrid.Children.Clear();
                leftGrid.Children.Add(image);
            }
        }
    }
}
