using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archilizer_WarningChart.WarningChartWPF
{
    public class WarningChartModel : INotifyPropertyChanged
    {
        private string name;
        private int number;
        private List<ICollection<ElementId>> ids;

        //Name of the Warning
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if(name != value)
                {
                    name = value;
                    RaisePropertyChanged("Name");
                }
            }
        }
        //Number of that Warning in the current Project
        public int Number
        {
            get
            {
                return number;
            }
            set
            {
                if(number != value)
                {
                    number = value;
                    RaisePropertyChanged("Number");
                }
            }
        }
        //IDs associated with this Warning
        public List<ICollection<ElementId>> IDs
        {
            get
            {
                return ids;
            }
            set
            {
                if(ids != value)
                {
                    ids = value;
                    RaisePropertyChanged("IDs");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }          
    }
}
