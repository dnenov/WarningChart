using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WC.WarningChart
{
    public interface IWarningChart 
    {
        List<WarningModel> warningModels { get; set; }
    }
}
