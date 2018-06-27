using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;

namespace WC.WarningChartWPF
{
    class SingleRandom : Random
    {
        static SingleRandom _Instance;
        public static SingleRandom Instance
        {
            get
            {
                if (_Instance == null) _Instance = new SingleRandom();
                return _Instance;
            }
        }

        private SingleRandom() { }
    }
    public static class BrushSwatch
    {
        static List<Brush> brb = new List<Brush>()
        {
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F2E273")),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F2BC73")),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CAF188")),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#55F1AF")),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#30F1BB")),

            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64F2B6")),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#55F3C7")),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DDF6B3")),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F6ECA5")),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F6D4A5")),

            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F2BC73")),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F2E273")),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CAF188")),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5BDCA6")),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C6FBEC")),
        };

        public static Brush Get()
        {
            SingleRandom r = SingleRandom.Instance;

            return brb[r.Next(brb.Count)];
        }
    }

    public class WarningChartModel : INotifyPropertyChanged
    {
        private string name;
        private string title;
        private int number;
        private List<ICollection<Autodesk.Revit.DB.ElementId>> ids;
        private Brush color;
        private string id;

        // The ID of the WarningModel
        public string ID
        {
            get
            {
                return id;
            }
            set
            {
                if (id != value)
                {
                    id = value;
                    RaisePropertyChanged("ID");
                }
            }
        }
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
                    Title = Utils.Truncate(value, 24);
                    //Color = BrushSwatch.Get();
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
        public List<ICollection<Autodesk.Revit.DB.ElementId>> IDs
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
        //The title is the trimmed version of the name (for legend use)
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                title = value;
                RaisePropertyChanged("Title");
            }
        }
        //Create a color based on swatches
        public Brush Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
                RaisePropertyChanged("Color");
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
