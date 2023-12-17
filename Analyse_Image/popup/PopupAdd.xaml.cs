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
using System.Windows.Shapes;

namespace Analyse_Image.popup
{
    /// <summary>
    /// Logique d'interaction pour PopupAdd.xaml
    /// </summary>
    public partial class PopupAdd : Window
    {
        private String fileName = "";
        private bool isDone = false;

        public PopupAdd()
        {
            InitializeComponent();

        }

        public void Done(object sender, RoutedEventArgs e)
        {
            isDone = true;
            this.Close();
        }

        public void Cancel(object sender, RoutedEventArgs e)
        {
            fileName = "";
            isDone = false;
            this.Close();
        }

        public void Select(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                fileName = op.FileName;
                fileNameTextBox.Text = fileName;
            }
        }

        public bool IsDone { get => isDone; set => isDone = value; }
        public string FileName { get => fileName; set => fileName = value; }
    }
}
