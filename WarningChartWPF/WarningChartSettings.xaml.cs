using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WC.WarningChartWPF
{
    /// <summary>
    /// Interaction logic for WarningChartSettings.xaml
    /// </summary>
    public partial class WarningChartSettings : Window
    {
        public WarningChartSettings()
        {
            InitializeComponent();

            txtAnswer.Text = Properties.Settings.Default.WarningNumber.ToString();
        }
        
        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            string result = txtAnswer.Text;
            if(result.Equals("") || !IsTextAllowed(result))
            {
                this.DialogResult = false;
            }
            else
            {
                Properties.Settings.Default.WarningNumber = Int32.Parse(txtAnswer.Text);
                this.DialogResult = true;
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            txtAnswer.SelectAll();
            txtAnswer.Focus();
        }
        // Check if the input text matches the Regex
        private void txtAnswer_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            TextBox box = sender as TextBox;
            if(box.SelectionLength > 0)
            {
                e.Handled = !IsTextAllowed(e.Text);
            }else
            {
                e.Handled = !IsTextAllowed(((TextBox)sender).Text + e.Text);
            }
        }
        // Checks the input against the Regex (only positive integers)
        private static bool IsTextAllowed(string text)
        {
            int result = 0;
            if (Int32.TryParse(text, out result))
            {
                // Your conditions
                if (result > 0 && result < 100000)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
