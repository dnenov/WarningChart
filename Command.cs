#region Namespaces
using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
#endregion

namespace WC
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
                var sb = new System.Text.StringBuilder();
                sb.AppendLine(ex.GetType().FullName + ": " + ex.Message);
                var inner = ex.InnerException;
                int depth = 0;
                while (inner != null && depth < 5)
                {
                    sb.AppendLine("--- Inner [" + depth + "] " + inner.GetType().FullName + ": " + inner.Message);
                    inner = inner.InnerException;
                    depth++;
                }
                sb.AppendLine("--- Stack ---");
                sb.AppendLine(ex.StackTrace);
                message = sb.ToString();
                return Result.Failed;
            }
        }
    }
}
