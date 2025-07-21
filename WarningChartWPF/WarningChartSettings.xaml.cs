using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;

namespace WC.WarningChartWPF
{
    /// <summary>
    /// Interaction logic for WarningChartSettings.xaml
    /// </summary>
    public partial class WarningChartSettings : Window
    {
        private string[] customColors;
        private Button[] colorButtons;

        public WarningChartSettings()
        {
            InitializeComponent();

            txtAnswer.Text = Properties.Settings.Default.WarningNumber.ToString();

            // Initialize color buttons array
            colorButtons = new Button[] { colorButton1, colorButton2, colorButton3, colorButton4, colorButton5 };

            // Load current color scheme
            LoadColorScheme();
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            string result = txtAnswer.Text;
            if (result.Equals("") || !IsTextAllowed(result))
            {
                this.DialogResult = false;
            }
            else
            {
                Properties.Settings.Default.WarningNumber = Int32.Parse(txtAnswer.Text);

                // Save custom color scheme
                Properties.Settings.Default.CustomColorScheme = string.Join(",", customColors);
                Properties.Settings.Default.Save();

                this.DialogResult = true;
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            // Remove focus from textbox to avoid blue highlight
            this.Focus();
        }
        // Check if the input text matches the Regex
        private void txtAnswer_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            TextBox box = sender as TextBox;
            if (box.SelectionLength > 0)
            {
                e.Handled = !IsTextAllowed(e.Text);
            }
            else
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

        private void LoadColorScheme()
        {
            try
            {
                string colorScheme = Properties.Settings.Default.CustomColorScheme;
                if (!string.IsNullOrEmpty(colorScheme))
                {
                    customColors = colorScheme.Split(',').Select(c => c.Trim()).ToArray();
                }
                else
                {
                    // Default colors
                    customColors = new string[] { "#F2ECD5", "#EDF2F4", "#FFCB21", "#B21A00", "#9E031E" };
                }

                // Update button backgrounds to show current colors
                for (int i = 0; i < colorButtons.Length && i < customColors.Length; i++)
                {
                    var color = (Color)ColorConverter.ConvertFromString(customColors[i]);
                    colorButtons[i].Background = new SolidColorBrush(color);
                }
            }
            catch
            {
                // Fallback to default colors if parsing fails
                customColors = new string[] { "#F2ECD5", "#EDF2F4", "#FFCB21", "#B21A00", "#9E031E" };
                ResetColorButtons();
            }
        }

                private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag.ToString(), out int colorIndex))
            {
                try
                {
                    // Use our modern WPF color picker
                    var currentColor = (Color)ColorConverter.ConvertFromString(customColors[colorIndex]);
                    var dialog = new ColorPickerDialog(currentColor)
                    {
                        Owner = this
                    };
                    
                    if (dialog.ShowDialog() == true)
                    {
                        var newColor = dialog.SelectedColor;
                        string hexColor = $"#{newColor.A:X2}{newColor.R:X2}{newColor.G:X2}{newColor.B:X2}";
                        
                        // Update the color in our array
                        customColors[colorIndex] = hexColor;
                        
                        // Update button background
                        button.Background = new SolidColorBrush(newColor);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error selecting color: {ex.Message}", "Color Selection Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void btnResetColors_Click(object sender, RoutedEventArgs e)
        {
            // Reset to default color scheme
            customColors = new string[] { "#F2ECD5", "#EDF2F4", "#FFCB21", "#B21A00", "#9E031E" };
            ResetColorButtons();
        }

        private void ResetColorButtons()
        {
            for (int i = 0; i < colorButtons.Length && i < customColors.Length; i++)
            {
                var color = (Color)ColorConverter.ConvertFromString(customColors[i]);
                colorButtons[i].Background = new SolidColorBrush(color);
            }
        }

        private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left && e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
            {
                try
                {
                    this.DragMove();
                }
                catch (InvalidOperationException)
                {
                    // DragMove failed - ignore silently
                }
            }
        }
    }
}
