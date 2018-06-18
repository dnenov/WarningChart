#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Linq;
#endregion

namespace Archilizer_WarningChart
{
    [Transaction(TransactionMode.Manual)]
    public class CommandWarningChart : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            List<FailureMessage> warnings = new List<FailureMessage>(doc.GetWarnings());

            Dictionary<string, int> warningMap = new Dictionary<string, int>();

            warningMap = warnings.GroupBy(x => x.GetDescriptionText())
              .Where(g => g.Count() > 1)
              .ToDictionary(x => x.Key, y => y.Count());
            
            using (WarningChartForm form = new WarningChartForm(warningMap))
            {
                var result = form.ShowDialog();

                if(result == System.Windows.Forms.DialogResult.OK)
                {
                    return Result.Succeeded;
                }
                else
                {
                    return Result.Failed;
                }
            }
        }
    }
}
