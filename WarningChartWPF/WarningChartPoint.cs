using LiveCharts;
using LiveCharts.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WC.WarningChartWPF
{
    public class WarningChartPoint
    {
        static WarningChartPoint()
        {
            //In this case we are plotting our own point to have 
            //more control over the current plot
            //configuring a custom type is quite simple

            //first we define a mapper
            var mapper = Mappers.Pie<WarningChartPoint>()
                .Value(x => x.Number);//use the value property in the plot

            //then we save the mapper globally, there are many ways
            //so configure a series, for more info see:
            //https://lvcharts.net/App/examples/v1/wpf/Types%20and%20Configuration
            Charting.For<WarningChartPoint>(mapper);
        }

        /// <summary>
        /// Gets or sets the value to plot
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public double Value
        {
            get { return Number; }
        }

        /// <summary>
        /// Gets or sets the content, all the values that represent this point
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public int Number { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title { get; set; }
    }
}
