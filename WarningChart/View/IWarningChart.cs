using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archilizer_WarningChart.WarningChart
{
    public interface IWarningChart 
    {
        List<WarningModel> warningModels { get; set; }
    }
}
