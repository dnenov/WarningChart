using System.Collections.Generic;
using WC.WarningChartWPF;

namespace WC
{
    internal class WCModelComparer : IEqualityComparer<WarningChartModel>
    {
        public int GetHashCode(WarningChartModel co)
        {
            if (co == null)
            {
                return 0;
            }
            return co.Number.GetHashCode();
        }

        public bool Equals(WarningChartModel x1, WarningChartModel x2)
        {
            if (object.ReferenceEquals(x1, x2))
            {
                return true;
            }
            if (object.ReferenceEquals(x1, null) ||
                object.ReferenceEquals(x2, null))
            {
                return false;
            }
            return x1.Number == x2.Number;
        }
    }
}