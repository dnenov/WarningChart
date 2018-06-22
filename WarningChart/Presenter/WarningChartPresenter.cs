using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Archilizer_WarningChart.WarningChartWPF;
using System.Diagnostics;

namespace Archilizer_WarningChart.WarningChart
{
    public class WarningChartPresenter
    {
        private Document doc;
        private List<FailureMessage> warnings;
        private List<WarningModel> warningModels;
        //public WarningChartForm form;
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
              .Where(g => g.Count() > 1)
              .Select(x => new WarningModel { Name = x.Key, Number = x.Count(), IDs = x.Select(y => y.GetFailingElements()) }).ToList();
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
            form.Show();
            /*
            form = new WarningChartForm();

            form.warningModels = this.warningModels;
            form.Show(hWndRevit);
            */
        }
    }
}
