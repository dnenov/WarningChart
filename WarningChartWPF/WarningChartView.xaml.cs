using System;
using System.Collections.Generic;
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
using Archilizer_WarningChart.WarningChart;
using LiveCharts;
using LiveCharts.Wpf;

namespace Archilizer_WarningChart.WarningChartWPF
{
    /// <summary>
    /// Interaction logic for WarningChartView.xaml
    /// </summary>
    public partial class WarningChartView : Window
    {
        private List<WarningModel> _warningModels;

        public WarningChartView()
        {
            InitializeComponent();
            
            DataContext = this;
        }

        public List<WarningModel> warningModels
        {
            get
            {
                return _warningModels;
            }
            set
            {
                _warningModels = value;

                SetUpChart();
            }
        }

        public Func<ChartPoint, string> PointLabel { get; set; }

        private void SetUpChart()
        {
            if (_warningModels.Count == 0)
            {
                MessageBox.Show("No warnings here.");
                Close();
            }

            Func<ChartPoint, string> labelPoint = chartPoint =>
                string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation);

            LiveCharts.SeriesCollection series = new LiveCharts.SeriesCollection();

            foreach (var w in _warningModels)
            {
                PieSeries ps = new PieSeries
                {
                    Title = w.Name,
                    Values = new ChartValues<double> { w.Number },
                    PushOut = 15,
                    DataLabels = true,
                    LabelPoint = labelPoint,
                    Stroke = System.Windows.Media.Brushes.Transparent
                };
                series.Add(ps);
            }
            pieChart.Series = series;
            pieChart.Update();
        }

        private void Chart_OnDataClick(object sender, ChartPoint chartpoint)
        {
            var chart = (LiveCharts.Wpf.PieChart)chartpoint.ChartView;

            //clear selected slice.
            foreach (PieSeries series in chart.Series)
                series.PushOut = 0;

            var selectedSeries = (PieSeries)chartpoint.SeriesView;
            selectedSeries.PushOut = 8;
        }
        // Drag window by clicking on any control
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
