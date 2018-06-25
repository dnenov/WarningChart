using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archilizer_WarningChart.WarningChartWPF
{
    public class WarningChartModel
    {
        //Name of the Warning
        public string Name { get; set; }
        //Number of that Warning in the current Project
        public int Number { get; set; }
        //IDs associated with this Warning
        public IEnumerable<ICollection<ElementId>> IDs { get; set; }
    }
}
