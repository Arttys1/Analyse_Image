using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Logique d'interaction pour PopupSelectSize.xaml
    /// </summary>
    public partial class PopupSelectSize : Window
    {
        private bool isDone = false;
        private int value = 0;

        public PopupSelectSize()
        {
            InitializeComponent();
        }

        private void SliderChangeValue(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ValueTextBox != null)
            {
                double newValue = e.NewValue;
                ValueTextBox.Text = newValue.ToString();
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        public bool IsDone { get => isDone; set => isDone = value; }
        public int Value { get => value; set => this.value = value; }

        private void Done(object sender, RoutedEventArgs e)
        {
            isDone = true;
            value = int.Parse(ValueTextBox.Text);
            Close();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            isDone = false;
            Close();
        }
    }
}
