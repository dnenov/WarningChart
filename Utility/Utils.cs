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
        private static DisplayUnitType dut;
        public const double METERS_IN_FEET = 0.3048;

        public static void Init(Document doc)
        {
            dut = doc.GetUnits().GetFormatOptions(UnitType.UT_Length).DisplayUnits;
        }
        /// <summary>
        /// forward conversion of project to unit values
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static double convertValueTO(double p)
        {
            switch (dut)
            {
                case DisplayUnitType.DUT_METERS:
                    return p * METERS_IN_FEET;
                case DisplayUnitType.DUT_CENTIMETERS:
                    return p * METERS_IN_FEET * 100;
                case DisplayUnitType.DUT_DECIMAL_FEET:
                    return p;
                case DisplayUnitType.DUT_DECIMAL_INCHES:
                    return p * 12;
                case DisplayUnitType.DUT_METERS_CENTIMETERS:
                    return p * METERS_IN_FEET;
                case DisplayUnitType.DUT_MILLIMETERS:
                    return p * METERS_IN_FEET * 1000;
                case DisplayUnitType.DUT_DECIMETERS:
                    return p * METERS_IN_FEET * 10;
            }
            return p;
        }
        /// <summary>
        /// reverse the unit transformation to project units
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static double convertValueFROM(double p)
        {
            switch (dut)
            {
                case DisplayUnitType.DUT_METERS:
                    return p / METERS_IN_FEET;
                case DisplayUnitType.DUT_CENTIMETERS:
                    return p / METERS_IN_FEET / 100;
                case DisplayUnitType.DUT_DECIMAL_FEET:
                    return p;
                case DisplayUnitType.DUT_DECIMAL_INCHES:
                    return p / 12;
                case DisplayUnitType.DUT_METERS_CENTIMETERS:
                    return p / METERS_IN_FEET;
                case DisplayUnitType.DUT_MILLIMETERS:
                    return p / METERS_IN_FEET / 1000;
                case DisplayUnitType.DUT_DECIMETERS:
                    return p / METERS_IN_FEET / 10;
            }
            return p;
        }
        /// <summary>
        /// check if we support user units
        /// </summary>
        /// <returns></returns>
        public static Boolean _goUnits()
        {
            if (dut.Equals(DisplayUnitType.DUT_FEET_FRACTIONAL_INCHES)) return false;
            if (dut.Equals(DisplayUnitType.DUT_FRACTIONAL_INCHES)) return false;
            return true;
        }

        internal static WarningChartModel FindChange(List<WarningChartModel> previousWarningModels, List<WarningChartModel> warningModels)
        {
            if(previousWarningModels == null)
            {
                return null;
            }
            else
            {
                WarningChartModel change = null;
                List<WarningChartModel> popList = new List<WarningChartModel>(previousWarningModels);
                foreach (var x1 in previousWarningModels)
                {
                    foreach(var x2 in warningModels)
                    {
                        if(x1.Name.Equals(x2.Name))
                        {
                            //they are the same warning
                            if(x1.Number != x2.Number)
                            {
                                //number of warning changed, so x2 is the change
                                change = x2;
                                return x2;
                            }
                            popList.Remove(x1);
                            break;
                        }
                    }
                }
                if(popList.Count > 0)
                {
                    popList[0].IDs.Clear();
                    return popList[0];
                }
                /*
                var withoutFirst = previousWarningModels.Except(warningModels, new WCModelComparer());
                if(!withoutFirst.Any())
                {
                    withoutFirst = previousWarningModels.Except(warningModels);
                }
                var withFirst = withoutFirst.First();

                return withFirst;
                */
                return null;
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
