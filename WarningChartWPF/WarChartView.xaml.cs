using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView.WPF;
using SkiaSharp;
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
    [SupportedOSPlatform("windows7.0")]
    public partial class WarChartView : Window, INotifyPropertyChanged, IDisposable
    {
        public event Action<String> SeriesSelectedEvent;
        public bool? IsCheckedState { get; private set; }
        public bool DocumentChanged { get; internal set; }
        public bool DocumentSwitched { get; internal set; }

        private List<WarningChartModel> _warningModels;
        private List<WarningChartModel> _previousWarningModels;
        private const int pushAmount = 12;

        private Tuple<List<WarningChartModel>, List<WarningChartModel>, List<WarningChartModel>> _changes;

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

                LoadSeries();

                pieChart.DataPointerDown += PieChartOnDataPointerDown;


                // Hook up legend click handler
                legend.LegendItemSelected += (s, seriesName) =>
                {
                    SeriesSelectedEvent?.Invoke(seriesName);

                    // Reset pushouts
                    foreach (var series in Series)
                    {
                        if (series is PieSeries<WarningChartPoint> pie)
                            pie.Pushout = 0;
                    }

                    // Apply pushout to selected
                    var selectedSeries = Series.FirstOrDefault(x => x.Name == seriesName) as PieSeries<WarningChartPoint>;
                    if (selectedSeries != null)
                    {
                        selectedSeries.Pushout = pushAmount;
                    }
                };
            }
        }

        private void PieChartOnDataPointerDown(IChartView chart, IEnumerable<ChartPoint> points)
        {
            var point = points.FirstOrDefault();
            if (point == null) return;

            if (point.Context.DataSource is WarningChartPoint data)
            {
                // Do something with your data
                string name = data.Name;
                SeriesSelectedEvent?.Invoke(name);
            }

            // Reset pushouts
            foreach (var series in Series)
            {
                if (series is PieSeries<WarningChartPoint> pie)
                    pie.Pushout = 0;
            }

            // Apply pushout to selected
            if (point.Context.Series is PieSeries<WarningChartPoint> selectedPie)
            {
                selectedPie.Pushout = pushAmount;
            }
        }

        public bool? intToBool(int i)
        {
            switch (i)
            {
                case 0: return null;
                case 1: return true;
                default:
                    return false;
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

        /// <summary>
        /// Color library
        /// </summary>
        public static readonly SKColor[] CustomColors = new[]
        {
            SKColor.Parse("#FFCB21"),
            SKColor.Parse("#8D99AE"),
            SKColor.Parse("#EDF2F4"),
            SKColor.Parse("#EF233C"),
            SKColor.Parse("#9E031E"),
            SKColor.Parse("#FFDECA"),
            SKColor.Parse("#9891BA"),
            SKColor.Parse("#EFE2FF"),
            SKColor.Parse("#F93E18"),
            SKColor.Parse("#B21A00"),
        };

        public WarChartView()
        {
            var initial = new WarningChartModel() { Name = "Initial", Number = 1, IDs = null };
            var initialList = new List<WarningChartModel>() { initial };

            Series = GroupsByNumberOfWarnings(initialList);

            InitializeComponent();

            this.Loaded += new RoutedEventHandler(MyWindow_Loaded);
            IsCheckedState = intToBool(Properties.Settings.Default.IsCheckedState);

            UpdateInterfaceLayout();

            DataContext = this;
        }

        public void LoadSeries()
        {
            if (!DocumentChanged && !DocumentSwitched)
            {
                if (warningModels == null) return;

                // Only at the start
                Series = GroupsByNumberOfWarnings(warningModels);
            }
            else if (DocumentSwitched)
            {
                // When a different document is active
                DocumentSwitched = false;
                // Reset the popouts
                foreach (var series in Series)
                {
                    if (series is PieSeries<int> pie)
                    {
                        pie.Pushout = 0; // Reset push-out
                    }
                }
                Series = GroupsByNumberOfWarnings(warningModels);
            }
            else if (DocumentChanged)
            {
                // When some of the warnings have changed
                DocumentChanged = false;
                // Reset the popouts
                foreach (var series in Series)
                {
                    if (series is PieSeries<int> pie)
                    {
                        pie.Pushout = 0; // Reset push-out
                    }
                }
                // Item1 - New
                // Item2 - Deleted
                // Item3 - Changed
                if (_changes == null) return;
                if (_changes.Item1.Count > 0)
                {
                    foreach (var item in GroupsByNumberOfWarnings(_changes.Item1))
                    {
                        Series.Add(item);
                    }
                }
                if (_changes.Item2.Count > 0)
                {
                    foreach (var deleted in _changes.Item2)
                    {
                        var name = deleted.Name;
                        var deletedSeries = Series.Cast<PieSeries<int>>().First(x => x.Tag?.Equals(name) == true); //use Series Tag to identify ...?

                        Series.Remove(deletedSeries);
                    }
                }
                if (_changes.Item3.Count > 0)
                {
                    foreach (var changed in _changes.Item3)
                    {
                        var name = changed.Name;
                        var changedSeries = Series.Cast<PieSeries<int>>().First(x => x.Tag?.Equals(name) == true);  //use Series Tag to identify ...?

                        if (changedSeries is PieSeries<int> pie)
                        {
                            var color = pie.Fill;
                            // do something with color
                            Series.Remove(changedSeries);
                            Series.Add(ChagnedSeries(changed, color));
                        }

                    }
                }
            }

            OnPropertyChanged("Series");
        }

        private static ObservableCollection<ISeries> GroupsByNumberOfWarnings(List<WarningChartModel> content)
        {
            var series = new ObservableCollection<ISeries>();
            if (!content.Any()) return series;

            var total = content.Sum(x => x.Number);
            var max = content.OrderByDescending(x => x.Number).First().Name;

            return new ObservableCollection<ISeries>(
                content
                .OrderByDescending(x => x.Number)
                .Select((x, i) => new PieSeries<WarningChartPoint>
                {
                    Values = new[] { new WarningChartPoint { Number = x.Number, Title = x.Title, Name = x.Name } },
                    Name = x.Name,
                    Mapping = (model, index) => new Coordinate(index, model.Number),
                    Pushout = x.Name == max ? pushAmount : 0,
                    Stroke = null,
                    DataLabelsSize = 12,
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                    Fill = new SolidColorPaint(CustomColors[i % CustomColors.Length]),
                    DataLabelsFormatter = point =>
                    {
                        var value = point.Coordinate.PrimaryValue;
                        var percent = value / total;
                        return percent > 0.05
                            ? string.Format("{0} {1} ({2:P})", value, Environment.NewLine, percent)
                            : string.Empty;
                    }
                }).ToArray()
            );
        }

        private static PieSeries<WarningChartPoint> ChagnedSeries(WarningChartModel content, IPaint<SkiaSharpDrawingContext> color)
        {
            return new PieSeries<WarningChartPoint>
            {
                Values = new[]
                {
                    new WarningChartPoint
                    {
                        Number = content.Number,
                        Title = content.Title,
                        Name = content.Name
                    }
                },
                Mapping = (model, index) => new Coordinate(index, model.Number),
                Pushout = pushAmount,
                Tag = content.Name,
                Fill = color,
                Name = content.Title
            };
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
                pieChart.Visibility = Visibility.Collapsed;
                legend.Visibility = Visibility.Collapsed;
                lblNoWarnings.Visibility = Visibility.Visible;
                btnToggle.IsEnabled = false;
            }
        }

        internal void YesWarnings()
        {
            if (NoProjectWarnings)
            {
                NoProjectWarnings = false;
                lblNoWarnings.Visibility = Visibility.Collapsed;
                btnToggle.IsEnabled = true;
                ResumeInterfaceLayout();
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
                legend.Visibility = Visibility.Visible;

                splitterColumn.Width = new GridLength(5);
                legendColumn.Width = GridLength.Auto;
            }
            else if (IsCheckedState == null)
            {
                // 2: hide legend
                IsCheckedState = false;
                pieChart.Visibility = Visibility.Visible;
                legend.Visibility = Visibility.Collapsed;

                splitterColumn.Width = new GridLength(0);
                legendColumn.Width = new GridLength(0);
            }
            else if (IsCheckedState == false)
            {
                // 3: hide chart
                IsCheckedState = true;
                pieChart.Visibility = Visibility.Collapsed;
                legend.Visibility = Visibility.Collapsed;

                splitterColumn.Width = new GridLength(0);
                legendColumn.Width = new GridLength(0);
            }
        }

        private void ResumeInterfaceLayout()
        {
            // cycle through the three states
            if (IsCheckedState == true)
            {
                // 1: show everything
                pieChart.Visibility = Visibility.Visible;
                legend.Visibility = Visibility.Visible;

                splitterColumn.Width = new GridLength(5);
                legendColumn.Width = GridLength.Auto;
            }
            else if (IsCheckedState == null)
            {
                // 2: hide legend
                pieChart.Visibility = Visibility.Visible;
                legend.Visibility = Visibility.Collapsed;

                splitterColumn.Width = new GridLength(0);
                legendColumn.Width = new GridLength(0);
            }
            else if (IsCheckedState == false)
            {
                // 3: hide chart
                pieChart.Visibility = Visibility.Visible;
                legend.Visibility = Visibility.Collapsed;

                splitterColumn.Width = new GridLength(0);
                legendColumn.Width = new GridLength(0);
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

                // Set legend grid column width
                if (Properties.Settings.Default.LegendWidth != 0)
                {
                    legendColumn.Width = new GridLength(Properties.Settings.Default.LegendWidth);
                }
                else
                {
                    legendColumn.Width = new GridLength(200);
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
            Properties.Settings.Default.LegendWidth = legendColumn.Width.Value;
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

        #region Dispose
        public void Dispose()
        {
            pieChart.DataPointerDown -= PieChartOnDataPointerDown;
        }
        #endregion
    }
}

