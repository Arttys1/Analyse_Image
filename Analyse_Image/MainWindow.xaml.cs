using Analyse_Image.popup;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
                leftImage = new back.Image(bitmapImage);
                DisplayAnImageOnTheLeft(leftImage);
            }
        }
        private void Save(object sender, RoutedEventArgs e)
        {
            if (rightImage != null)
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Filter = "Images|*.png";;
                bool ?ok = dialog.ShowDialog();
                if (ok != null && (bool)ok)
                {
                    rightImage.Save(dialog.FileName);
                }
            }
        }

        private void Switch(object sender, RoutedEventArgs e)
        {
            rightGrid.Children.Clear();
            DisplayAnImageOnTheLeft(rightImage);
            rightImage = null;

        }

        private void GrayScale(object sender, RoutedEventArgs e)
        {
            if(leftImage != null && leftImage.ImageType == back.ImageType.RGB)
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

        public void Add(object sender, RoutedEventArgs e)
        {
            if (leftImage != null)
            {
                PopupAdd popupAdd = new PopupAdd();
                popupAdd.ShowDialog();
                if (popupAdd.IsDone == true)
                {
                    Bitmap bitmap = new Bitmap(popupAdd.FileName);
                    back.Image imageToAdd = new back.Image(bitmap, back.ImageType.RGB);

                    imageToAdd = TranslateStateOfAnImageToTheSameOfAnother(imageToAdd, leftImage);

                    rightImage = leftImage.Add(imageToAdd);
                    DisplayAnImageOnTheRight(rightImage);
                }
            }
        }

        public void Minus(object sender, RoutedEventArgs e)
        {
            if (leftImage != null)
            {
                PopupAdd popupAdd = new PopupAdd();
                popupAdd.ShowDialog();
                if (popupAdd.IsDone == true)
                {
                    Bitmap bitmap = new Bitmap(popupAdd.FileName);
                    back.Image imageToAdd = new back.Image(bitmap, back.ImageType.RGB);

                    imageToAdd = TranslateStateOfAnImageToTheSameOfAnother(imageToAdd, leftImage);

                    rightImage = leftImage.Minus(imageToAdd);
                    DisplayAnImageOnTheRight(rightImage);
                }
            }
        }
        public void Erosion(object sender, RoutedEventArgs e)
        {
            if (leftImage != null && leftImage.ImageType == back.ImageType.BINARY)
            {
                PopupSelectSize popupSelectSize = new PopupSelectSize();
                popupSelectSize.ShowDialog();
                if (popupSelectSize.IsDone == true)
                {
                    int size = popupSelectSize.Value;
                    rightImage = leftImage.Erosion(size);
                    DisplayAnImageOnTheRight(rightImage);
                }
            }
        }
        public void Dilatation(object sender, RoutedEventArgs e)
        {
            if (leftImage != null && leftImage.ImageType == back.ImageType.BINARY)
            {
                PopupSelectSize popupSelectSize = new PopupSelectSize();
                popupSelectSize.ShowDialog();
                if (popupSelectSize.IsDone == true)
                {
                    int size = popupSelectSize.Value;
                    rightImage = leftImage.Dilatation(size);
                    DisplayAnImageOnTheRight(rightImage);
                }
            }
        }
        public void Fermeture(object sender, RoutedEventArgs e)
        {
            if (leftImage != null && leftImage.ImageType == back.ImageType.BINARY)
            {
                PopupSelectSize popupSelectSize = new PopupSelectSize();
                popupSelectSize.ShowDialog();
                if (popupSelectSize.IsDone == true)
                {
                    int size = popupSelectSize.Value;
                    rightImage = leftImage.Fermeture(size);
                    DisplayAnImageOnTheRight(rightImage);
                }
            }
        }
        public void Ouverture(object sender, RoutedEventArgs e)
        {
            if (leftImage != null && leftImage.ImageType == back.ImageType.BINARY)
            {
                PopupSelectSize popupSelectSize = new PopupSelectSize();
                popupSelectSize.ShowDialog();
                if (popupSelectSize.IsDone == true)
                {
                    int size = popupSelectSize.Value;
                    rightImage = leftImage.Ouverture(size);
                    DisplayAnImageOnTheRight(rightImage);
                }
            }
        }

        private void Aminscissement(object sender, RoutedEventArgs e)
        {
            if (leftImage != null && leftImage.ImageType == back.ImageType.BINARY)
            {
                rightImage = leftImage.Aminscissement();
                DisplayAnImageOnTheRight(rightImage);
            }
        }

        private void AminscissementHomotopique(object sender, RoutedEventArgs e)
        {
            if(leftImage != null && leftImage.ImageType == back.ImageType.BINARY)
            {
                rightImage = leftImage.AminscissementHomotopique();
                DisplayAnImageOnTheRight(rightImage);
            }
        }

        public void Lantuejoul(object sender, RoutedEventArgs e)
        {
            if (leftImage != null && leftImage.ImageType == back.ImageType.BINARY)
            {
                back.Image squelette = leftImage.Lantuejoul();
                DisplayAnImageOnTheRight(squelette);
            }
        }

        private back.Image TranslateStateOfAnImageToTheSameOfAnother(back.Image imageToChange, back.Image imageModel)
        {
            back.Image result = imageToChange;
            //translate image state to gray if leftImage is not rgb
            if (imageModel.ImageType != back.ImageType.RGB)
            {
                result = result.ToGrayScale();

                //translate image state to binary if leftImage is binary
                if (imageModel.ImageType == back.ImageType.BINARY)
                {
                    result = result.ToBinaryImage(result.ComputeThreshold());
                }
            }
            return result;
        }

        private void DisplayAnImageOnTheRight(back.Image? imageToDisplay)
        {
            if (imageToDisplay != null)
            {
                System.Windows.Controls.Image image = new System.Windows.Controls.Image();
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
                System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                image.Source = imageToDisplay.GetBitMapImage();
                leftImage = imageToDisplay;
                leftGrid.Children.Clear();
                leftGrid.Children.Add(image);
            }
        }
    }
}
