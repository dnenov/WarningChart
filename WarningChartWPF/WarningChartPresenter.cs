using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Autodesk.Revit.ApplicationServices;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

namespace WC.WarningChartWPF
{
    [SupportedOSPlatform("windows7.0")]
    public class WarningChartPresenter : IDisposable
    {
        private UIApplication uiapp;
        private Document doc;
        private List<FailureMessage> warnings;
        private List<WarningChartModel> warningModels;
        private ExternalEvent exEvent;
        private RequestHandler handler;
        public WarChartView form;
        internal bool IsClosed;

        public UIApplication _Application
        {
            get
            {
                return uiapp;
            }
            set
            {
                if (uiapp != value)
                {
                    uiapp = value;
                    _Document = uiapp.ActiveUIDocument.Document;
                }
            }
        }

        public Document _Document
        {
            get
            {
                return doc;
            }
            set
            {
                if (doc != value)
                {
                    doc = value;
                }
            }
        }

        public WarningChartPresenter(UIApplication uiapp, ExternalEvent exEvent, RequestHandler handler)
        {
            this._Application = uiapp;
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
              .Select(x => new WarningChartModel { Name = x.Key, Number = x.Count(), ID = x.Key + x.Count().ToString(), IDs = x.Select(y => y.GetFailingElements()).ToList() }).ToList();
        }

        internal void DocumentChanged()
        {
            // Fetch the new warnings
            LoadData();
            // Update the Form
            form.DocumentChanged = true;
            if (this.warningModels.Count == 0)
            {
                form.NoWarnings();
            }
            else
            {
                form.YesWarnings();
                form.warningModels = this.warningModels;
            }
            form.WarningNumber = this.warnings.Count;
        }

        internal void DocumentSwitched()
        {
            // Fetch the new warnings
            LoadData();
            // Update the Form
            form.DocumentSwitched = true;
            if (this.warningModels.Count == 0)
            {
                form.NoWarnings();
            }
            else
            {
                form.YesWarnings();
                form.warningModels = this.warningModels;
            }
            form.WarningNumber = this.warnings.Count;
        }

        internal void Close()
        {
            form.Close();

            IsClosed = true;
        }

        [SupportedOSPlatform("windows7.0")]
        internal void Show(WindowHandle hWndRevit)
        {
            form = new WarChartView();
            System.Windows.Interop.WindowInteropHelper x = new System.Windows.Interop.WindowInteropHelper(form);
            x.Owner = hWndRevit.Handle;
            form.DocumentChanged = false;
            if (this.warningModels.Count == 0)
            {
                form.NoWarnings();
            }
            else
            {
                form.YesWarnings();
                form.warningModels = this.warningModels;
                form.WarningNumber = this.warnings.Count;
            }
            form.WarningNumber = this.warnings.Count;
            form.Closed += FormClosed;
            form.SeriesSelectedEvent += SeriesSelected;
            form.Show();
        }

        // Notify that the System.Windows.Markup.XamlParseException: ''Provide value on 'System.Windows.Markup.StaticResourceHolder' threw an exception.' Line number '72' and line pform is closed
        private void FormClosed(object sender, EventArgs e)
        {
            form.Dispose();
            this.Dispose();
        }

        private void SeriesSelected(string name)
        {
            try
            {
                MakeRequest(RequestId.SelectWarnings, warningModels.First(x => x.Name.Equals(name)).IDs);
            }
            catch (Exception ex)
            {
                //
            }
        }
        private void MakeRequest(RequestId request, List<ICollection<ElementId>> ids)
        {
            //MessageBox.Show("You are in the Control.Request event.");
            handler.Request.SelectWarnings(ids);
            handler.Request.Make(request);
            exEvent.Raise();
        }

        public void Dispose()
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

        internal void Disable()
        {
            form.IsEnabled = false;
            form.Visibility = System.Windows.Visibility.Hidden;
        }

        internal void Enable()
        {
            form.IsEnabled = true;
            form.Visibility = System.Windows.Visibility.Visible;
        }


        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
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
