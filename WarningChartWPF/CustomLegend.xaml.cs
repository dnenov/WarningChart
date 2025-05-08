using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView.Painting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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

namespace WC.WarningChartWPF
{
    public class PaintToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorPaint paint)
            {
                var c = paint.Color;
                return new SolidColorBrush(Color.FromArgb(c.Alpha, c.Red, c.Green, c.Blue));
            }

            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class SeriesToNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ISeries<WarningChartPoint> series &&
                series.Values is IEnumerable<WarningChartPoint> points)
            {
                return points.FirstOrDefault()?.Number ?? 0;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    /// <summary>
    /// Interaction logic for CustomLegend.xaml
    /// </summary>
    public partial class CustomLegend : UserControl
    {
        public event EventHandler<string> LegendItemSelected;

        public CustomLegend()
        {
            InitializeComponent();
        }

        public void Show(IEnumerable<ISeries> series)
        {
            DataContext = new { Series = series.ToList() };
        }

        public void Hide()
        {
            DataContext = null;
        }

        private void LegendItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ISeries series)
            {
                var name = series.Name;
                LegendItemSelected?.Invoke(this, name);
            }
        }
    }
}
