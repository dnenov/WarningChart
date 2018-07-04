using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using LiveCharts.Wpf;
using System.Windows.Data;
using System;
using LiveCharts;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace WC.WarningChartWPF
{
    public class ReverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((SeriesCollection)value).Reverse();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public partial class WarningChartCustomLegend : UserControl, IChartLegend
    {
        private List<SeriesViewModel> _series;
        public event EventHandler StatusUpdated;

        public WarningChartCustomLegend()
        {
            InitializeComponent();

            //this.DataContext = this;
        }

        public List<SeriesViewModel> Series
        {
            get { return _series; }
            set
            {
                _series = value;
                OnPropertyChanged("Series");
            }
        }
        private void ListBox_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //start

        public static readonly RoutedEvent LegendItemSelectedEvent = EventManager.RegisterRoutedEvent("TabItemSelected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(WarningChartCustomLegend));

        public event RoutedEventHandler LegendItemSelected
        {
            add { AddHandler(LegendItemSelectedEvent, value); }
            remove { RemoveHandler(LegendItemSelectedEvent, value); }
        }

        void RaiseLegendItemSelectedEvent(string selectedItem)
        {
            SelectedLegendRoutedEventArgs newEventArgs = new SelectedLegendRoutedEventArgs(LegendItemSelectedEvent, selectedItem);
            RaiseEvent(newEventArgs);
        }

        //end


        private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {
            ListBoxItem item = e.Source as ListBoxItem;
            var pieSeries = (PieSeries) item.Content;
            var chartPoint = (WarningChartPoint) pieSeries.ActualValues[0];
            var name = chartPoint.Name;

            RaiseLegendItemSelectedEvent(name);
        }
    }
    public class SelectedLegendRoutedEventArgs : RoutedEventArgs
    {
        private readonly string selectedItem;

        public SelectedLegendRoutedEventArgs(RoutedEvent routedEvent,
                                          string selectedItem)
            : base(routedEvent)
        {
            this.selectedItem = selectedItem;
        }

        public string SelectedItem
        {
            get
            {
                return selectedItem;
            }
        }
    }
}