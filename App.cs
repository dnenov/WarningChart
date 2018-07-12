#region Namespaces
using System;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Reflection;
using System.Windows.Media.Imaging;
using WC.WarningChartWPF;
using System.Diagnostics;
using System.Windows.Forms;
using Autodesk.Revit.UI.Events;
#endregion

namespace WC
{
    class App : IExternalApplication
    {
        // Windows Revit handle
        static WindowHandle _hWndRevit = null;
        // Class instance
        internal static App thisApp = null;
        // Presenter instance
        private WarningChartPresenter _presenter;
        // Keeps track of the number of Warnings in the current Document
        private int _currentCount;
        // Current Document
        private Document _document;
        // Hardcoded helpfile path
        static string helpFile = "file:///C:/ProgramData/Autodesk/ApplicationPlugins/Archilizer_Warchart.bundle/Content/Help/Warchart%20_%20Revit%20_%20Autodesk%20App%20Store.html";
        private bool _disabled;

        static void AddRibbonPanel(UIControlledApplication application)
        {
            // Create a custom ribbon panel
            String tabName = "Archilizer";
            try
            {
                application.CreateRibbonTab(tabName);
            }
            catch (Exception)
            {

            }
            RibbonPanel ribbonPanel = application.CreateRibbonPanel(tabName, "Miscellaneous");

            // Get dll assembly path
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            ContextualHelp ch = new ContextualHelp(ContextualHelpType.Url, @helpFile);

            CreatePushButton(ribbonPanel, String.Format("Warning" + Environment.NewLine + "Chart"), thisAssemblyPath, "WC.CommandWarningChart",
                "Displays a Pie Chart representing Project Warnings.", "archilizer_warchart.png", ch);            
        }
        private static void CreatePushButton(RibbonPanel ribbonPanel, string name, string path, string command, string tooltip, string icon, ContextualHelp ch)
        {
            PushButtonData pbData = new PushButtonData(
                name,
                name,
                path,
                command);

            PushButton pb = ribbonPanel.AddItem(pbData) as PushButton;
            pb.ToolTip = tooltip;
            pb.SetContextualHelp(ch);
            BitmapImage pb2Image = new BitmapImage(new Uri(String.Format("pack://application:,,,/Archilizer_Warchart;component/Resources/{0}", icon)));
            pb.LargeImage = pb2Image;
        }
        public Result OnStartup(UIControlledApplication a)
        {
            ControlledApplication c_app = a.ControlledApplication;

            // Make sure you have to update the plugin
            string version = a.ControlledApplication.VersionNumber;
            if (Int32.Parse(version) < 2020)
            {
                AddRibbonPanel(a);
            }

            _presenter = null;  // no dialog needed yet; ThermalAsset command will bring it
            thisApp = this;  // static access to this application instance                                                    
            c_app.DocumentChanged   // Document Changed event - whenever it changes, check for your stuff (in this app check if warnings number has changed)
                += new EventHandler<Autodesk.Revit.DB.Events.DocumentChangedEventArgs>(
                    c_app_DocumentChanged);

            a.ViewActivated += new EventHandler<Autodesk.Revit.UI.Events.ViewActivatedEventArgs>(OnViewActivated);


            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication a)
        {
            ControlledApplication c_app = a.ControlledApplication;

            c_app.DocumentChanged
                -= new EventHandler<Autodesk.Revit.DB.Events.DocumentChangedEventArgs>(
                    c_app_DocumentChanged);

            a.ViewActivated -= new EventHandler<Autodesk.Revit.UI.Events.ViewActivatedEventArgs>(OnViewActivated);

            if (_presenter != null)
            {
                _presenter.Close();
            }

            return Result.Succeeded;
        }
        /// <summary>
        /// On Document Switched
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnViewActivated(object sender, ViewActivatedEventArgs e)
        {
            Document doc = e.CurrentActiveView.Document;

            // If the document is a Family Document, disable the UI
            if(doc.IsFamilyDocument)
            {
                if(!_disabled)
                {
                    _presenter.Disable();
                    _disabled = true;
                }
                return;
            }
            else
            {
                if(_disabled)
                {
                    _presenter.Enable();
                    _disabled = false;
                }
            }

            if (_document.Title != doc.Title)
            {
                _document = doc;
                _currentCount = doc.GetWarnings().Count;
                _presenter._Application = new UIApplication(doc.Application);
                _presenter.DocumentSwitched();
            }
        }
        /// <summary>
        /// On document change, update Family Parameters
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void c_app_DocumentChanged(object sender, Autodesk.Revit.DB.Events.DocumentChangedEventArgs e)
        {
            if (_presenter != null)
            {
                if (e.GetDocument().GetWarnings().Count != _currentCount)
                {
                    _currentCount = e.GetDocument().GetWarnings().Count;
                    _presenter.DocumentChanged();
                }
            }
        }
        /// <summary>
        /// De-facto the command is here.
        /// </summary>
        /// <param name="uiapp"></param>
        public void ShowForm(UIApplication uiapp)
        {
            // get the isntance of Revit Thread
            // to pass it to the Windows Form later
            if (null == _hWndRevit)
            {
                Process process
                  = Process.GetCurrentProcess();

                IntPtr h = process.MainWindowHandle;
                _hWndRevit = new WindowHandle(h);
            }

            if (_presenter == null || _presenter.IsClosed)
            {
                //new handler
                RequestHandler handler = new RequestHandler();
                //new event
                ExternalEvent exEvent = ExternalEvent.Create(handler);
                // set current document
                _document = uiapp.ActiveUIDocument.Document;

                // Set the initial number of warnings so we don't detect document change on the first event
                _currentCount = uiapp.ActiveUIDocument.Document.GetWarnings().Count;

                _presenter = new WarningChartPresenter(uiapp, exEvent, handler);

                try
                {
                    //pass parent (Revit) thread here
                    _presenter.Show(_hWndRevit);
                }
                catch(Exception ex)
                {
                    TaskDialog.Show("Error", ex.Message);
                    _presenter.Dispose();
                    _presenter = null;
                }
            }
        }

        //public void WakeFormUp()
        //{
        //    if (_presenter.form != null)
        //    {
        //        _presenter.form.WakeUp();
        //    }
        //}
    }
    /// <summary>
    /// Retrieve Revit Windows thread in order to pass it to the form as it's owner
    /// </summary>
    public class WindowHandle : IWin32Window
    {
        IntPtr _hwnd;

        public WindowHandle(IntPtr h)
        {
            Debug.Assert(IntPtr.Zero != h,
                "expected non-null window handle");

            _hwnd = h;
        }

        public IntPtr Handle
        {
            get
            {
                return _hwnd;
            }
        }
    }

}
