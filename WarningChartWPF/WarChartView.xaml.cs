using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Versioning;
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

namespace WC.WarningChartWPF
{

    /// <summary>
    /// Int to Color Converter
    /// </summary>
    public class IntToColorConverter : IValueConverter
    {
        private static GradientStopCollection grsc = new GradientStopCollection()
        {
            new GradientStop((Color)ColorConverter.ConvertFromString("#00EDF2F4"), 0), // mellow yellow
            new GradientStop((Color)ColorConverter.ConvertFromString("#EDF2F4"), 0.1), // mellow yellow
            new GradientStop((Color)ColorConverter.ConvertFromString("#FFCB21"), 0.5), // anti-flash white
            new GradientStop((Color)ColorConverter.ConvertFromString("#B21A00"), 0.9), //mordant red 19
            new GradientStop((Color)ColorConverter.ConvertFromString("#9E031E"), 1), //heidelberg red
        };

        private static Color GetColorByOffset(GradientStopCollection collection, double offset)
        {
            GradientStop[] stops = collection.OrderBy(x => x.Offset).ToArray();
            if (offset <= 0) return stops[0].Color;
            if (offset >= 1) return stops[stops.Length - 1].Color;
            GradientStop left = stops[0], right = null;
            foreach (GradientStop stop in stops)
            {
                if (stop.Offset >= offset)
                {
                    right = stop;
                    break;
                }
                left = stop;
            }
            offset = Math.Round((offset - left.Offset) / (right.Offset - left.Offset), 2);

            byte a = (byte)((right.Color.A - left.Color.A) * offset + left.Color.A);
            byte r = (byte)((right.Color.R - left.Color.R) * offset + left.Color.R);
            byte g = (byte)((right.Color.G - left.Color.G) * offset + left.Color.G);
            byte b = (byte)((right.Color.B - left.Color.B) * offset + left.Color.B);

            return Color.FromArgb(a, r, g, b);
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int val = System.Convert.ToInt32(value);

            var color = GetColorByOffset(grsc, Remap(val, 0, Properties.Settings.Default.WarningNumber, 0, 1));

            return new SolidColorBrush(color);
        }
        public static float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaction logic for WarChartView.xaml
    /// </summary>
    public partial class WarChartView : Window, INotifyPropertyChanged
    {
        public bool? IsCheckedState { get; private set; }
        public event Action<String> SeriesSelectedEvent;
        public bool DocumentChanged { get; internal set; }
        public bool DocumentSwitched { get; internal set; }

        private List<WarningChartModel> _warningModels;
        private List<WarningChartModel> _previousWarningModels;

        private Tuple<List<WarningChartModel>, List<WarningChartModel>, List<WarningChartModel>> _changes;

        [SupportedOSPlatform("windows7.0")]
        public ObservableCollection<ISeries> Series { get; set; }

        private int _warningNumber;
        public int WarningNumber
        {
            get
            {
                return _warningNumber;
            }
            set
            {
                if (_warningNumber != value)
                {
                    _warningNumber = value;
                    OnPropertyChanged(nameof(WarningNumber));
                }
            }
        }

        public List<WarningChartModel> warningModels
        {
            get
            {
                return _warningModels;
            }
            set
            {
                _previousWarningModels = _warningModels;
                _warningModels = value;
                _changes = Utils.FindChange(_previousWarningModels, _warningModels);

                //LoadSeries();
            }
        }


        public int boolToInt(bool? b)
        {
            switch (b)
            {
                case true: return 1;
                case false: return -1;
                default:
                    return 0;
            }
        }

        [SupportedOSPlatform("windows7.0")]
        public WarChartView()
        {
            InitializeComponent();

            Series = new ObservableCollection<ISeries>
            {
                new PieSeries<int> { Values = new[] { 10 }, Name = "Apples" },
                new PieSeries<int> { Values = new[] { 20 }, Name = "Oranges" },
                new PieSeries<int> { Values = new[] { 30 }, Name = "Bananas" }
            };
            //LoadSeries();

            DataContext = this;
        }

        [SupportedOSPlatform("windows7.0")]
        private void LoadSeries()
        {
            var points = new List<WarningChartPoint>
            {
                new WarningChartPoint { Number = 5, Title = "Type A", Name = "Wall Warnings" },
                new WarningChartPoint { Number = 3, Title = "Type B", Name = "Door Warnings" },
                new WarningChartPoint { Number = 8, Title = "Type C", Name = "Misc Warnings" }
            };

            Series = new ObservableCollection<ISeries>(
                 points.Select(point =>
                     new PieSeries<WarningChartPoint>
                     {
                         Values = new[] { point },
                         Name = point.Title,
                         Mapping = (model, index) => new Coordinate(index, model.Number),
                         DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                         DataLabelsSize = 14,
                         DataLabelsFormatter = chartPoint =>
                             $"{chartPoint.Context.Series.Name}: {chartPoint.Coordinate.PrimaryValue}"
                     }
                 )
             );
        }

        // Drag window by clicking on any control
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            WarningChartSettings settings = new WarningChartSettings();
            if (settings.ShowDialog() == true)
            {
                //stupid update
                var holder = WarningNumber;
                WarningNumber = 0;
                WarningNumber = holder;
            }
        }

        // If there are no warnings in the current project
        // Collapse the Chart (and Legend)
        // And display a 'No Warning' sign ;)
        public bool NoProjectWarnings { get; private set; }

        internal void NoWarnings()
        {
            if (!NoProjectWarnings)
            {
                NoProjectWarnings = true;
                //pieChart.Visibility = Visibility.Collapsed;
                //Legend.Visibility = Visibility.Collapsed;
                //lblNoWarnings.Visibility = Visibility.Visible;
                btnToggle.IsEnabled = false;
            }
        }

        internal void YesWarnings()
        {
            if (NoProjectWarnings)
            {
                NoProjectWarnings = false;
                //lblNoWarnings.Visibility = Visibility.Collapsed;
                btnToggle.IsEnabled = true;
                //ResumeInterfaceLayout();
            }
        }


        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            SavePosition();
            Close();
        }

        private void CollapseButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.IsCheckedState = boolToInt(IsCheckedState);
            UpdateInterfaceLayout();
        }

        #region Update Layout
        private void UpdateInterfaceLayout()
        {
            // cycle through the three states
            if (IsCheckedState == true)
            {
                // 1: show everything
                IsCheckedState = null;
                pieChart.Visibility = Visibility.Visible;
                //Legend.Visibility = Visibility.Visible;
            }
            else if (IsCheckedState == null)
            {
                // 2: hide legend
                IsCheckedState = false;
                pieChart.Visibility = Visibility.Visible;
                //Legend.Visibility = Visibility.Collapsed;
            }
            else if (IsCheckedState == false)
            {
                // 3: hide chart
                IsCheckedState = true;
                //pieChart.Visibility = Visibility.Collapsed;
                pieChart.Visibility = Visibility.Visible;
                //Legend.Visibility = Visibility.Collapsed;
            }
        }
        private void ResumeInterfaceLayout()
        {
            // cycle through the three states
            if (IsCheckedState == true)
            {
                // 1: show everything
                pieChart.Visibility = Visibility.Visible;
                //Legend.Visibility = Visibility.Visible;
            }
            else if (IsCheckedState == null)
            {
                // 2: hide legend
                pieChart.Visibility = Visibility.Visible;
                //Legend.Visibility = Visibility.Collapsed;
            }
            else if (IsCheckedState == false)
            {
                // 3: hide chart
                pieChart.Visibility = Visibility.Visible;
                //Legend.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region Location
        // On loaded, move the UI to the up-right corner
        // or load the previous position and size
        private void MyWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            double screenHeight = SystemParameters.FullPrimaryScreenHeight;
            double screenWidth = SystemParameters.FullPrimaryScreenWidth;
            this.Top = 160;
            this.Left = screenWidth - this.Width - 80;
            try
            {
                Rect bounds = Properties.Settings.Default.WindowPosition;
                if (bounds.Top != 0)
                {
                    this.Top = bounds.Top;

                }
                if (bounds.Left != 0)
                {
                    this.Left = bounds.Left;
                }
                // Restore the size only for a manually sized window.
                if (bounds.Width != 0 && bounds.Height != 0)
                {
                    this.SizeToContent = SizeToContent.Manual;
                    this.Width = bounds.Width;
                    this.Height = bounds.Height;
                }
            }
            catch
            {
                MessageBox.Show("No settings stored.");
            }
        }
        // Save the location of the UI (per user)
        private void SavePosition()
        {
            Properties.Settings.Default.WindowPosition = this.RestoreBounds;
            Properties.Settings.Default.Save();
        }
        #endregion

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}

