using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WC.WarningChartWPF
{
    public partial class ColorPickerDialog : Window
    {
        public Color SelectedColor { get; private set; }

        private static readonly Color[] ColorPalette = new Color[]
        {
            // Row 1 - Grayscale and whites
            Colors.White, Colors.LightGray, Colors.Silver, Colors.Gray, 
            Colors.DarkGray, Colors.DimGray, Colors.Black, Color.FromRgb(40, 40, 40),
            
            // Row 2 - Reds
            Color.FromRgb(255, 235, 235), Color.FromRgb(255, 205, 210), Color.FromRgb(239, 83, 80), Color.FromRgb(244, 67, 54),
            Color.FromRgb(229, 57, 53), Color.FromRgb(198, 40, 40), Color.FromRgb(183, 28, 28), Color.FromRgb(136, 14, 79),
            
            // Row 3 - Blues
            Color.FromRgb(227, 242, 253), Color.FromRgb(144, 202, 249), Color.FromRgb(66, 165, 245), Color.FromRgb(33, 150, 243),
            Color.FromRgb(30, 136, 229), Color.FromRgb(25, 118, 210), Color.FromRgb(21, 101, 192), Color.FromRgb(13, 71, 161),
            
            // Row 4 - Greens, Yellows, Oranges
            Color.FromRgb(200, 230, 201), Color.FromRgb(129, 199, 132), Color.FromRgb(102, 187, 106), Color.FromRgb(76, 175, 80),
            Color.FromRgb(255, 235, 59), Color.FromRgb(255, 193, 7), Color.FromRgb(255, 152, 0), Color.FromRgb(255, 87, 34)
        };

        public ColorPickerDialog()
        {
            InitializeComponent();
            InitializeColorPalette();
        }

        public ColorPickerDialog(Color initialColor) : this()
        {
            SetSelectedColor(initialColor);
        }

        private void InitializeColorPalette()
        {
            for (int i = 0; i < ColorPalette.Length; i++)
            {
                var color = ColorPalette[i];
                var button = CreateColorButton(color);
                colorGrid.Children.Add(button);
            }
        }

        private Button CreateColorButton(Color color)
        {
            var button = new Button
            {
                Background = new SolidColorBrush(color),
                BorderBrush = new SolidColorBrush(Colors.LightGray),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(2),
                Tag = color
            };

            // Create button style
            var style = new Style(typeof(Button));
            var template = new ControlTemplate(typeof(Button));
            
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetBinding(Border.BackgroundProperty, 
                new System.Windows.Data.Binding("Background") { RelativeSource = System.Windows.Data.RelativeSource.TemplatedParent });
            borderFactory.SetBinding(Border.BorderBrushProperty, 
                new System.Windows.Data.Binding("BorderBrush") { RelativeSource = System.Windows.Data.RelativeSource.TemplatedParent });
            borderFactory.SetBinding(Border.BorderThicknessProperty, 
                new System.Windows.Data.Binding("BorderThickness") { RelativeSource = System.Windows.Data.RelativeSource.TemplatedParent });
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));
            
            template.VisualTree = borderFactory;

            // Hover effect
            var hoverTrigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            hoverTrigger.Setters.Add(new Setter(Button.BorderBrushProperty, Brushes.DarkGray));
            hoverTrigger.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(2)));
            template.Triggers.Add(hoverTrigger);

            style.Setters.Add(new Setter(Button.TemplateProperty, template));
            button.Style = style;

            button.Click += ColorButton_Click;
            return button;
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Color color)
            {
                SetSelectedColor(color);
            }
        }

        private void SetSelectedColor(Color color)
        {
            SelectedColor = color;
            selectedColorPreview.Background = new SolidColorBrush(color);
            
            // Temporarily remove event handler to avoid recursion
            selectedColorText.TextChanged -= SelectedColorText_TextChanged;
            selectedColorText.Text = $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
            selectedColorText.TextChanged += SelectedColorText_TextChanged;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
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

        private void SelectedColorText_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox textBox)
            {
                try
                {
                    string text = textBox.Text.Trim();
                    
                    // Add # if missing
                    if (!text.StartsWith("#"))
                    {
                        text = "#" + text;
                    }
                    
                    // Validate hex color format
                    if (text.Length >= 7 && text.Length <= 9) // #RGB, #ARGB, #RRGGBB, or #AARRGGBB
                    {
                        var color = (Color)ColorConverter.ConvertFromString(text);
                        
                        // Update preview without triggering text change event
                        selectedColorPreview.Background = new SolidColorBrush(color);
                        SelectedColor = color;
                        
                        // Reset border to normal
                        textBox.BorderBrush = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC));
                    }
                    else
                    {
                        // Invalid format - show red border
                        textBox.BorderBrush = new SolidColorBrush(Colors.Red);
                    }
                }
                catch (FormatException)
                {
                    // Invalid hex color - show red border
                    textBox.BorderBrush = new SolidColorBrush(Colors.Red);
                }
                catch (Exception)
                {
                    // Any other error - show red border
                    textBox.BorderBrush = new SolidColorBrush(Colors.Red);
                }
            }
        }

        private void SelectedColorPreview_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                // Get owner window handle
                var windowInterop = new System.Windows.Interop.WindowInteropHelper(this);
                
                // Open native Windows color picker
                var selectedColor = NativeColorPicker.ShowColorPicker(SelectedColor, windowInterop.Handle);
                
                if (selectedColor.HasValue)
                {
                    SetSelectedColor(selectedColor.Value);
                }
            }
        }
    }
} 