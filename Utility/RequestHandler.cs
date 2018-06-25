using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Archilizer_WarningChart
{
    /// <summary>
    /// A class with methods to execute requests made by the dialog user.
    /// </summary>
    /// 
    public class RequestHandler : IExternalEventHandler
    {
        // delegate that will expect a family parameter to act upon
        private delegate void FamilyOperation(FamilyManager fm, FamilyParameter fp);
        // The value of the latest request made by the modeless form 
        private Request m_request = new Request();
                
        /// <summary>
        /// A public property to access the current request value
        /// </summary>
        public Request Request
        {
            get { return m_request; }
        }

        /// <summary>
        ///   A method to identify this External Event Handler
        /// </summary>
        public String GetName()
        {
            return "Warning Chart";
        }

        public void Execute(UIApplication uiapp)
        {
            try
            {
                switch (Request.Take())
                {
                    case RequestId.None:
                        {
                            return;
                        }
                    case RequestId.SelectWarnings:
                        {
                            ExecuteMakeSelection(uiapp, "Select Warnings", Request.GetIDs());
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
            finally
            {
                //App.thisApp.WakeFormUp();
            }

            return;
        }
         /// <summary>
         /// Executes on restore all values
         /// </summary>
         /// <param name="uiapp"></param>
         /// <param name="text"></param>
         /// <param name="values"></param>
        private void ExecuteMakeSelection(UIApplication uiapp, String text, List<ICollection<ElementId>> ids)
        {
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            if (doc.IsFamilyDocument)
            {
                CommandWarningChart.global_message =
                  "Please run this command in a normal document.";
                TaskDialog.Show("Message", CommandWarningChart.global_message);
            }

            if ((uidoc != null))
            {
                List<ElementId> idsToSelect = new List<ElementId>();

                foreach (var warning in ids)
                {
                    foreach(var id in warning)
                    {
                        idsToSelect.Add(id);
                    }
                }

                using (Transaction trans = new Transaction(uidoc.Document))
                {
                    if (trans.Start(text) == TransactionStatus.Started)
                    {
                        uidoc.Selection.SetElementIds(idsToSelect);
                        doc.Regenerate();
                        trans.Commit();
                        uidoc.RefreshActiveView();
                    }
                }
            }
        }        
    }
}
