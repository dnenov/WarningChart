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
            ListBoxItem item = e.Source as ListBoxItem;
            if (this.StatusUpdated != null)
                this.StatusUpdated(this, new EventArgs());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}