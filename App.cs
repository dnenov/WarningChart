#region Namespaces
using System;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Reflection;
using System.Windows.Media.Imaging;
#endregion

namespace Archilizer_WarningChart
{
    class App : IExternalApplication
    {
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


            CreatePushButton(ribbonPanel, String.Format("Warning" + Environment.NewLine + "Chart"), thisAssemblyPath, "Archilizer_WarningChart.CommandWarningChart",
                "Displays a Pie Chart representing Project Warnings.", "archilizer_default.png");
            
        }

        private static void CreatePushButton(RibbonPanel ribbonPanel, string name, string path, string command, string tooltip, string icon)
        {
            PushButtonData pbData = new PushButtonData(
                name,
                name,
                path,
                command);

            PushButton pb = ribbonPanel.AddItem(pbData) as PushButton;
            pb.ToolTip = tooltip;
            BitmapImage pb2Image = new BitmapImage(new Uri(String.Format("pack://application:,,,/Archilizer_WarningChart;component/Resources/{0}", icon)));
            pb.LargeImage = pb2Image;
        }
        public Result OnStartup(UIControlledApplication a)
        {
            // Make sure you have to update the plugin
            string version = a.ControlledApplication.VersionNumber;
            if (Int32.Parse(version) < 2020)
            {
                AddRibbonPanel(a);
            }
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}
