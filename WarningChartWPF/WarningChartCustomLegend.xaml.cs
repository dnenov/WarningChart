﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using LiveCharts.Wpf;

namespace WC.WarningChartWPF
{
    public partial class WarningChartCustomLegend : UserControl, IChartLegend
    {
        private List<SeriesViewModel> _series;

        public WarningChartCustomLegend()
        {
            InitializeComponent();

            DataContext = this;
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}