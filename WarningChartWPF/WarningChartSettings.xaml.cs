using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace WC.WarningChartWPF
{
    /// <summary>
    /// Interaction logic for WarningChartSettings.xaml
    /// </summary>
    public partial class WarningChartSettings : Window
    {
        // Only allow positive integers
        private static readonly Regex _regex = new Regex("[^0-9]+");

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
            e.Handled = !IsTextAllowed(e.Text);
        }
        // Checks the input against the Regex (only positive integers)
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }
    }
}
