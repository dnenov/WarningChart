using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using MaterialDesignColors;
using LiveChartsCore.SkiaSharpView.Painting;

namespace WC.WarningChartWPF
{
    /// <summary>
    /// Interaction logic for WarningChartView.xaml
    /// </summary>
    public partial class WarningChartView : Window, INotifyPropertyChanged
    {
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

        private List<WarningChartModel> _warningModels;
        private List<WarningChartModel> _previousWarningModels;
        private const int pushAmount = 12;
        private int _warningNumber;
        public event Action<String> SeriesSelectedEvent;        
        public bool DocumentChanged { get; internal set; }
        public bool DocumentSwitched { get; internal set; }
        public int WarningNumber
        {
            get
            {
                return _warningNumber;
            }
            set
            {
                if(_warningNumber != value)
                {
                    _warningNumber = value;
                    OnPropertyChanged("WarningNumber");
                }
            }
        }
        public bool? IsCheckedState { get; private set; }
                
        //public static Func<ChartPoint, string> labelPoint = chartPoint =>
        //chartPoint.Participation > 0.05 ?
        //string.Format("{0} {1} ({2:P})", chartPoint.Y, Environment.NewLine, chartPoint.Participation)
        //: "";

        // The series that will be updated (Warnings)
        private ISeries[] _series;

        public ISeries[] Series
        {
            get => _series;
            set
            {
                _series = value;
                OnPropertyChanged(nameof(Series));
            }
        }

        private Tuple<List<WarningChartModel>, List<WarningChartModel>, List<WarningChartModel>> _changes;
        private ObservableCollection<string> _test;
        public ObservableCollection<string> Test
        {
            get { return _test; }
            set
            {
                if (value != _test)
                {
                    _test = value;
                    OnPropertyChanged("Name2");
                }
            }
        }

        public WarningChartView()
        {
            var initial = new WarningChartModel() { Name = "Initial", Number = 1, IDs = null };
            var initialList = new List<WarningChartModel>() { initial };
            //Series = GroupsByNumberOfWarnings(initialList);

            Test = new ObservableCollection<string>(new string[] { "element1", "element2", "element3" });

            //InitializeMaterialDesign();
            InitializeComponent();

            
            // Places the UI where it needs to go
            this.Loaded += new RoutedEventHandler(MyWindow_Loaded);
            //this.MyCustomLegend.StatusUpdated += new EventHandler(MyEventHandlerFunction_StatusUpdated);

            //IsCheckedState = intToBool(Properties.Settings.Default.IsCheckedState);

            //UpdateInterfaceLayout();

            DataContext = this;
        }
        /***
        private void InitializeMaterialDesign()
        {
            // Create dummy objects to force the MaterialDesign assemblies to be loaded
            // from this assembly, which causes the MaterialDesign assemblies to be searched
            // relative to this assembly's path. Otherwise, the MaterialDesign assemblies
            // are searched relative to Eclipse's path, so they're not found.
            var card = new Card();
            var hue = new Hue("Dummy", Colors.Black, Colors.White);
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

        private void MyEventHandlerFunction_StatusUpdated(object sender, EventArgs e)
        {
            ListBoxItem item = (e as RoutedEventArgs).Source as ListBoxItem;
            SeriesSelectedEvent((string)item.ToolTip); 
        }

        private void LoadSeries()
        {
            if(!DocumentChanged && !DocumentSwitched)
            {
                // Only at the start
                Series = GroupsByNumberOfWarnings(warningModels);
            }
            else if(DocumentSwitched)
            {
                // When a different document is active
                DocumentSwitched = false;
                // Reset the popouts
                foreach (PieSeries series in Series) series.PushOut = 0;
                Series = GroupsByNumberOfWarnings(warningModels);
            }
            else if(DocumentChanged)
            {
                // When some of the warnings have changed
                DocumentChanged = false;
                // Reset the popouts
                foreach (PieSeries series in Series) series.PushOut = 0;
                // Item1 - New
                // Item2 - Deleted
                // Item3 - Changed
                if (_changes == null) return;
                if(_changes.Item1.Count > 0)
                {
                    Series.AddRange(GroupsByNumberOfWarnings(_changes.Item1));
                }
                if (_changes.Item2.Count > 0)
                {
                    foreach (var deleted in _changes.Item2)
                    {
                        var name = deleted.Name;
                        var deletedSeries = Series.Cast<PieSeries>().First(x => x.Tag.Equals(name));  //use Series Tag to identify ...?

                        Series.Remove(deletedSeries);
                    }
                }
                if(_changes.Item3.Count > 0)
                {
                    foreach (var changed in _changes.Item3)
                    {
                        var name = changed.Name;
                        var changedSeries = Series.Cast<PieSeries>().First(x => x.Tag.Equals(name));  //use Series Tag to identify ...?

                        var color = ((PieSeries)changedSeries).Fill;
                        
                        Series.Remove(changedSeries);
                        Series.Add(ChagnedSeries(changed, color));
                    }
                }
            }

            OnPropertyChanged("Series");
        }

        private static PieSeries ChagnedSeries(WarningChartModel content, Brush color)
        {
            var series = new PieSeries
            {
                Values = new ChartValues<WarningChartPoint>
                    {
                        new WarningChartPoint {Number = content.Number, Title = content.Title, Name = content.Name }
                    },
                LabelPoint = labelPoint,
                PushOut = pushAmount,
                Tag = content.Name,
                Fill = color,
                Title = content.Title
            };
            return series;
        }

        private static ISeries[] GroupsByNumberOfWarnings(List<WarningChartModel> content)
        {
            var max = content.OrderByDescending(x => x.Number).First().Name;

            return content.Select(x => new PieSeries<WarningChartPoint>
            {
                Values = new[] { new WarningChartPoint { Number = x.Number, Title = x.Title, Name = x.Name } },
                Name = x.Title,
                //Mapping = (wp, chartPoint) =>
                //{
                //    chartPoint.PrimaryValue = wp.Number;
                //    chartPoint.Label = wp.Title;
                //},
                Pushout = x.Name == max ? 12 : 0,
                DataLabelsSize = 12,
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.ChartCenter,
                Fill = new SolidColorPaint(new SKColor(255, 203, 33)),
                //TooltipLabelFormatter = point => $"{point.Label}: {point.PrimaryValue}"
            }).ToArray();
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
            }
        }

        public bool NoProjectWarnings { get; private set; }

        // If there are no warnings in the current project
        // Collapse the Chart (and Legend)
        // And display a 'No Warning' sign ;)
        internal void NoWarnings()
        {
            if(!NoProjectWarnings)
            {
                NoProjectWarnings = true;
                pieChart.Visibility = Visibility.Collapsed;
                Legend.Visibility = Visibility.Collapsed;
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

        #region Button Events
        // When user clicks on one of the pies
        public void Chart_OnDataClick(object sender, ChartPoint chartpoint)
        {
            var chart = (LiveCharts.Wpf.PieChart)chartpoint.ChartView;

            //clear selected slice.
            foreach (PieSeries series in chart.Series)
                series.PushOut = 0;

            var selectedSeries = (PieSeries)chartpoint.SeriesView;
            selectedSeries.PushOut = pushAmount;
            SeriesSelectedEvent((string)((PieSeries)selectedSeries).Tag);
        }
        // When user clicks on the custom legend
        private void MyCustomLegend_LegendItemSelected(object sender, RoutedEventArgs e)
        {
            var test = (e as SelectedLegendRoutedEventArgs).SelectedItem;
            SeriesSelectedEvent(test);
        }
        // Drag window by clicking on any control
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        // Close
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            SavePosition();
            Close();
        }
        // Collapse
        private void CollapseButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.IsCheckedState = boolToInt(IsCheckedState);
            UpdateInterfaceLayout();
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
        #endregion

        #region Update Layout
        private void UpdateInterfaceLayout()
        {
            // cycle through the three states
            if (IsCheckedState == true)
            {
                // 1: show everything
                IsCheckedState = null;
                pieChart.Visibility = Visibility.Visible;
                Legend.Visibility = Visibility.Visible;
            }
            else if (IsCheckedState == null)
            {
                // 2: hide legend
                IsCheckedState = false;
                pieChart.Visibility = Visibility.Visible;
                Legend.Visibility = Visibility.Collapsed;
            }
            else if (IsCheckedState == false)
            {
                // 3: hide chart
                IsCheckedState = true;
                pieChart.Visibility = Visibility.Collapsed;
                Legend.Visibility = Visibility.Collapsed;
            }
        }
        private void ResumeInterfaceLayout()
        {
            // cycle through the three states
            if (IsCheckedState == true)
            {
                // 1: show everything
                pieChart.Visibility = Visibility.Visible;
                Legend.Visibility = Visibility.Visible;
            }
            else if (IsCheckedState == null)
            {
                // 2: hide legend
                pieChart.Visibility = Visibility.Visible;
                Legend.Visibility = Visibility.Collapsed;
            }
            else if (IsCheckedState == false)
            {
                // 3: hide chart
                pieChart.Visibility = Visibility.Collapsed;
                Legend.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        ***/

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

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }

}
