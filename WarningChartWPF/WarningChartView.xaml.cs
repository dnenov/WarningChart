using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Helpers;
using LiveCharts.Wpf;

namespace WC.WarningChartWPF
{
    /// <summary>
    /// Interaction logic for WarningChartView.xaml
    /// </summary>
    public partial class WarningChartView : Window, INotifyPropertyChanged
    {
        private List<WarningChartModel> _warningModels;
        private List<WarningChartModel> _previousWarningModels;
        private const int pushAmount = 8;
        public event Action<String> SeriesSelectedEvent;

        public Func<ChartPoint, string> Formatter { get; set; }

        public static Func<ChartPoint, string> labelPoint = chartPoint =>
            string.Format("{0} {1} ({2:P})", chartPoint.Y, Environment.NewLine, chartPoint.Participation);

        // The series that will be updated (Warnings)
        private SeriesCollection _series;
        private Tuple<List<WarningChartModel>, List<WarningChartModel>, List<WarningChartModel>> _changes;
       

        // The public Series to which we will bind the View 
        public SeriesCollection Series
        {
            get { return _series; }
            set
            {
                _series = value;
                OnPropertyChanged("Series");
            }
        }

        public WarningChartView()
        {

            var initial = new WarningChartModel() { Name = "Initial", Number = 1, IDs = null };
            var initialList = new List<WarningChartModel>() { initial };
            Series = GroupsByNumberOfWarnings(initialList);

            InitializeComponent();

            // Places the UI where it needs to go
            this.Loaded += new RoutedEventHandler(MyWindow_Loaded);
            
            DataContext = this;
        }
        
        private void LoadSeries()
        {
            if(!DocumentChanged)
            {
                Series = GroupsByNumberOfWarnings(warningModels);
            }
            else
            {
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
                //ToolTip = content.Name,
                Fill = color,
                Title = content.Title
            };

            return series;
        }

        private static SeriesCollection GroupsByNumberOfWarnings(List<WarningChartModel> content)
        {
            var max = content.OrderByDescending(x => x.Number).First().Name;

            var series = content
                .Select(x => new PieSeries
                {
                    Values = new ChartValues<WarningChartPoint>
                    {
                        new WarningChartPoint {Number = x.Number, Title = x.Title, Name = x.Name}
                    },
                    LabelPoint = labelPoint,
                    PushOut = x.Name == max ? pushAmount : 0,
                    Tag = x.Name,
                    Title = x.Title
                    //ToolTip = x.Name,
                }).AsSeriesCollection();

            return series;
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
        
        public bool DocumentChanged { get; internal set; }
        
        // When user click on one of the pies
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
        // Drag window by clicking on any control
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        // Close
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SavePosition();
            Close();
        }

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
