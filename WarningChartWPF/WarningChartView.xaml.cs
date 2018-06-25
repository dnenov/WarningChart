﻿using System;
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
        private List<WarningChartModel> _warningModels;

        public WarningChartView()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(MyWindow_Loaded);

            DataContext = this;
        }

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
                if(bounds.Top != 0)
                {
                    this.Top = bounds.Top;

                }
                if(bounds.Left != 0)
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

        public List<WarningChartModel> warningModels
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
                    //PushOut = 15,
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
            SavePosition();
            Close();
        }

        private void SavePosition()
        {
            Properties.Settings.Default.WindowPosition = this.RestoreBounds;
            Properties.Settings.Default.Save();
        }
    }
}
