using System.ComponentModel;
using LiveCharts;
using LiveCharts.Wpf;

namespace WC.WarningChartWPF
{
    /// <summary>
    /// Interaction logic for WarningChartToolTip.xaml
    /// </summary>
    public partial class WarningChartToolTip : IChartTooltip
    {
        private TooltipData _data;

        public WarningChartToolTip()
        {
            InitializeComponent();

            //LiveCharts will inject the tooltip data in the Data property
            //your job is only to display this data as required

            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public TooltipData Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged("Data");
            }
        }

        public TooltipSelectionMode? SelectionMode { get; set; }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
