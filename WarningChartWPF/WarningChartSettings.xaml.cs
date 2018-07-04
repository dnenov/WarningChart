using System;
using System.Windows;

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
            Properties.Settings.Default.WarningNumber = Int32.Parse(txtAnswer.Text);
            this.DialogResult = true;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            txtAnswer.SelectAll();
            txtAnswer.Focus();
        }
    }
}
