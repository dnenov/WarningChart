using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WC.WarningChartWPF;

namespace WC
{
    public static class Utils
    {      
        internal static Tuple<List<WarningChartModel>, List<WarningChartModel>, List<WarningChartModel>> FindChange(List<WarningChartModel> previousWarningModels, List<WarningChartModel> warningModels)
        {
            if(previousWarningModels == null)
            {
                return null;
            }
            else
            {
                // new warnings
                var newWarning = warningModels.Where(x => !previousWarningModels.Any(y => y.Name == x.Name)).ToList();

                // no longer exist warnings
                var deletedWarnings = previousWarningModels.Where(x => !warningModels.Any(y => y.Name == x.Name)).ToList();

                // changed warnings (few warnings have been added or removed)
                var changedWarnings = warningModels.Where(x => !previousWarningModels.Any(y => y.ID == x.ID)).Except(newWarning).ToList();

                return (Tuple.Create(newWarning, deletedWarnings, changedWarnings));                
            }
        }

        /// <summary>
        /// truncate string and add '..' at the end
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static string Truncate(string source, int length)
        {
            if (source.Length > length)
            {
                source = source.Substring(0, length);
                return source + "..";
            }
            else
            {
                return source;
            }
        }
        /// <summary>
        /// Check if a string contains unallowed characters
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        internal static bool UnallowedChacarcters(string text)
        {
            var regexItem = new Regex("^[a-zA-Z0-9 _!-]+$");
            bool result = regexItem.IsMatch(text);
            return(result ?  true :  false);
        }
        /// <summary>
        /// Check if it's a Family Document
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        internal static Autodesk.Revit.DB.Document checkDoc(Autodesk.Revit.DB.Document document)
        {
            if (document.IsFamilyDocument)
            {
                return document;
            }
            else
            {
                return null;
            }
        }
    }
}
