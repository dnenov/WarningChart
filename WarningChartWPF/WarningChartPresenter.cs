using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Archilizer_WarningChart.WarningChartWPF
{
    public class WarningChartPresenter
    {
        private Document doc;
        private List<FailureMessage> warnings;
        private List<WarningChartModel> warningModels;        
        private UIApplication uiapp;
        private ExternalEvent exEvent;
        private RequestHandler handler;
        public WarningChartView form;
        internal bool IsClosed;
        
        public WarningChartPresenter(UIApplication uiapp, ExternalEvent exEvent, RequestHandler handler)
        {
            this.uiapp = uiapp;
            this.doc = uiapp.ActiveUIDocument.Document;
            this.exEvent = exEvent;
            this.handler = handler;
            this.LoadData();
        }

        private void LoadData()
        {
            //Get the list of Warnings in the Project
            this.warnings = new List<FailureMessage>(doc.GetWarnings());

            //WOW!! Get the list of WarningModels
            this.warningModels = warnings.GroupBy(x => x.GetDescriptionText())
              //.Where(g => g.Count() > 1)
              .Select(x => new WarningChartModel { Name = x.Key, Number = x.Count(), IDs = x.Select(y => y.GetFailingElements()).ToList() }).ToList();
        }

        internal void DocumentChanged()
        {
            throw new NotImplementedException();
        }

        internal void Close()
        {
            form.Close();

            IsClosed = true;
        }

        internal void Show(WindowHandle hWndRevit)
        {
            form = new WarningChartView();
            System.Windows.Interop.WindowInteropHelper x = new System.Windows.Interop.WindowInteropHelper(form);
            x.Owner = hWndRevit.Handle;
            //x.Owner = Process.GetCurrentProcess().MainWindowHandle;
            form.warningModels = this.warningModels;
            form.Closed += FormClosed;
            form.SeriesSelectedEvent += SeriesSelected;
            form.Show();
            /*
            form = new WarningChartForm();

            form.warningModels = this.warningModels;
            form.Show(hWndRevit);
            */
        }
        // Notify that the form is closed
        private void FormClosed(object sender, EventArgs e)
        {
            // Revit handler stuff
            exEvent.Dispose();
            exEvent = null;
            handler = null;
            // This form is closed
            IsClosed = true;
            // Remove registered events
            form.Closed -= FormClosed;
            form.SeriesSelectedEvent -= SeriesSelected;
        }
        private void SeriesSelected(string name)
        {
            MakeRequest(RequestId.SelectWarnings, warningModels.First(x => x.Name.Equals(name)).IDs);
        }      

        private void MakeRequest(RequestId request, List<ICollection<ElementId>> ids)
        {
            //MessageBox.Show("You are in the Control.Request event.");
            handler.Request.SelectWarnings(ids);
            handler.Request.Make(request);
            exEvent.Raise();
        }
    }

    public class DelegateCommand : ICommand
    {
        private readonly Action _action;

        public DelegateCommand(Action action)
        {
            _action = action;
        }

        public void Execute(object parameter)
        {
            _action();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

#pragma warning disable 67
        public event EventHandler CanExecuteChanged { add { } remove { } }
#pragma warning restore 67
    }

    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
