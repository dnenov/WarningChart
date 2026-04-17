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
using System.Collections;
using WC.Helpers;
using System.IO;
using System.Runtime.InteropServices;
#endregion

namespace WC
{

    class App : IExternalApplication
    {
        private static UIControlledApplication MyApplication { get; set; }
        private static Assembly assembly;

        private static object TheInternalDoingPart(UIControlledApplication CApp, string TabName, string PanelName)
        {
            IList ERPs = null;

            ERPs = CApp.GetRibbonPanels(TabName);

            Autodesk.Revit.UI.RibbonPanel NewOrExtgRevitPanel = null;

            foreach (Autodesk.Revit.UI.RibbonPanel Pan in ERPs)
            {
                if (Pan.Name == PanelName)
                {
                    NewOrExtgRevitPanel = Pan;
                    goto FoundSoJumpPastNew;
                }
            }

            Autodesk.Revit.UI.RibbonPanel NewRevitPanel = null;

            NewRevitPanel = CApp.CreateRibbonPanel(TabName, PanelName);

            NewOrExtgRevitPanel = NewRevitPanel;
        FoundSoJumpPastNew:

            return NewOrExtgRevitPanel;
        }
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
        private static string helpFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
             "Autodesk",
             "ApplicationPlugins",
             "Archilizer_Warchart.bundle",
             "Content",
             "Help");
        private static string helpFilePath = Path.Combine(helpFolderPath, "Warchart _ Revit _ Autodesk App Store.html");
        private static string helpFile = new Uri(helpFilePath).AbsoluteUri;
        //static string helpFile = "file:///C:/ProgramData/Autodesk/ApplicationPlugins/Archilizer_Warchart.bundle/Content/Help/Warchart%20_%20Revit%20_%20Autodesk%20App%20Store.html";
        private bool _disabled;

        static void AddRibbonPanel(UIControlledApplication application)
        {
            // Create a custom ribbon panel
            var tabName = "Archilizer";
            var panelName = "Miscellaneous";
            try
            {
                application.CreateRibbonTab(tabName);
            }
            catch (Exception)
            {

            }

            var ribbonPanel = (RibbonPanel)TheInternalDoingPart(application, tabName, panelName);

            // Get dll assembly path
            var thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            assembly = Assembly.GetExecutingAssembly();
            var assemblyVersion = assembly.GetName().Version;

            var ch = new ContextualHelp(ContextualHelpType.Url, @helpFile);

            CreatePushButton(ribbonPanel, String.Format("Warning" + Environment.NewLine + "Chart"), thisAssemblyPath, "WC.CommandWarningChart",
                String.Format("Displays a Pie Chart representing Project Warnings.{0}{0}v{1}", Environment.NewLine, assemblyVersion), "WC.Resources.icon_Warchart.png", ch);
        }

        private static void CreatePushButton(RibbonPanel ribbonPanel, string name, string path, string command, string tooltip, string icon, ContextualHelp ch)
        {
            BitmapIcons bitmapIcons = new BitmapIcons(assembly, icon, MyApplication);

            PushButtonData pbData = new PushButtonData(
                name,
                name,
                path,
                command);

            PushButton pb = ribbonPanel.AddItem(pbData) as PushButton;

            pb.ToolTip = tooltip;
            var largeImage = bitmapIcons.LargeBitmap();
            var smallImage = bitmapIcons.SmallBitmap();
            pb.LargeImage = largeImage;
            pb.Image = smallImage;
            pb.SetContextualHelp(ch);
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetDllDirectory(string lpPathName);

        private static string _addinDirectory;

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (_addinDirectory == null) return null;
            var name = new AssemblyName(args.Name).Name;
            var candidate = Path.Combine(_addinDirectory, name + ".dll");
            if (File.Exists(candidate))
            {
                try { return Assembly.LoadFrom(candidate); }
                catch { return null; }
            }
            return null;
        }

        private static void PreloadNativeDependencies()
        {
            string logPath = null;
            var log = new System.Text.StringBuilder();
            try
            {
                var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (string.IsNullOrEmpty(dir)) return;
                logPath = Path.Combine(dir, "warchart_preload.log");
                log.AppendLine($"[{DateTime.Now:O}] Preload starting in {dir}");

                bool ok = SetDllDirectory(dir);
                log.AppendLine($"SetDllDirectory => {ok}");

                foreach (var name in new[] { "libSkiaSharp.dll", "libHarfBuzzSharp.dll" })
                {
                    var path = Path.Combine(dir, name);
                    if (!File.Exists(path)) { log.AppendLine($"  MISSING: {name}"); continue; }
                    try
                    {
                        var handle = NativeLibrary.Load(path);
                        log.AppendLine($"  Loaded {name} -> 0x{handle.ToInt64():X}");
                    }
                    catch (Exception ex)
                    {
                        log.AppendLine($"  FAILED {name}: {ex.GetType().Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                log.AppendLine($"PreloadNativeDependencies outer exception: {ex}");
            }
            finally
            {
                if (logPath != null)
                {
                    try { File.AppendAllText(logPath, log.ToString()); } catch { }
                }
            }
        }

        public Result OnStartup(UIControlledApplication a)
        {
            _addinDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

            PreloadNativeDependencies();

            ControlledApplication c_app = a.ControlledApplication;
            MyApplication = a;

            // Make sure you have to update the plugin
            string version = a.ControlledApplication.VersionNumber;

            AddRibbonPanel(a);

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
            if (doc.IsFamilyDocument)
            {
                if (!_disabled && _presenter != null)
                {
                    _presenter.Disable();
                    _disabled = true;
                }
                return;
            }
            else
            {
                if (_disabled)
                {
                    _presenter.Enable();
                    _disabled = false;
                }
            }

            if (_document != null && _document.Title != doc.Title)
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

                //pass parent (Revit) thread here
                _presenter.Show(_hWndRevit);
            }
        }
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
