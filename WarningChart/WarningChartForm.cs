using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LiveCharts; //Core of the library
using LiveCharts.Wpf; //The WPF controls
using LiveCharts.WinForms; //the WinForm wrappers

namespace Archilizer_WarningChart
{
    public partial class WarningChartForm : Form
    {
        public WarningChartForm(Dictionary<string, int> warnings)
        {
            InitializeComponent();

            Func<ChartPoint, string> labelPoint = chartPoint =>
               string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation);

            if(warnings.Count == 0)
            {
                MessageBox.Show("No warnings here.");
                this.DialogResult = DialogResult.Abort;
                this.Close();
            }

            foreach(var w in warnings)
            {
                pieChart1.Series.Add(new PieSeries
                {
                    Title = w.Key,
                    Values = new ChartValues<double> { w.Value },
                    PushOut = 15,
                    DataLabels = true,
                    LabelPoint = labelPoint,
                    Stroke = System.Windows.Media.Brushes.Transparent
                });
            }

            /*
            pieChart1.Series = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "Maria",
                    Values = new ChartValues<double> {3},
                    PushOut = 15,
                    DataLabels = true,
                    LabelPoint = labelPoint
                },
                new PieSeries
                {
                    Title = "Charles",
                    Values = new ChartValues<double> {4},
                    DataLabels = true,
                    LabelPoint = labelPoint
                },
                new PieSeries
                {
                    Title = "Frida",
                    Values = new ChartValues<double> {6},
                    DataLabels = true,
                    LabelPoint = labelPoint
                },
                new PieSeries
                {
                    Title = "Frederic",
                    Values = new ChartValues<double> {2},
                    DataLabels = true,
                    LabelPoint = labelPoint
                }
            };
            */

            foreach(var s in pieChart1.Series)
            {
                (s as Series).Stroke = System.Windows.Media.Brushes.Transparent;
            }

            pieChart1.LegendLocation = LegendLocation.Bottom;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
