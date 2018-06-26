using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WC.WPF.ViewModel
{
    public class WCViewModel : INotifyPropertyChanged
    {
        public WCViewModel()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
