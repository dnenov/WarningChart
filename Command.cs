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
        public static string global_message;

        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            try
            {
                App.thisApp.ShowForm(commandData.Application);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message += global_message;
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
