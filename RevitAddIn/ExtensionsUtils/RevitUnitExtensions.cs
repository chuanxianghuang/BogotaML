using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddIn.ExtensionsUtils
{
    public static class RevitUnitExtensions
    {
        /// <summary>
        /// millimetres to meter
        /// </summary>
        /// <param name="mmValue"></param>
        /// <returns></returns>
        public static double MmToM(this int mmValue)
        {
#if (REVIT2020)
            {
            return UnitUtils.Convert(mmValue, DisplayUnitType.DUT_MILLIMETERS, DisplayUnitType.DUT_METERS);
            }
#else
            return UnitUtils.Convert(mmValue, UnitTypeId.Millimeters, UnitTypeId.Meters);
#endif

        }

        /// <summary>
        /// millimetres to meter
        /// </summary>
        /// <param name="mmValue"></param>
        /// <returns></returns>
        public static double MmToM(this double mmValue)
        {
#if (REVIT2020)
            {
                return UnitUtils.Convert(mmValue, DisplayUnitType.DUT_MILLIMETERS, DisplayUnitType.DUT_METERS);
            }
#else
            return UnitUtils.Convert(mmValue, UnitTypeId.Millimeters, UnitTypeId.Meters);
#endif

        }

        /// <summary>
        /// millimetres to feet
        /// </summary>
        /// <param name="mmValue"></param>
        /// <returns></returns>
        public static double MmToFeet(this int mmValue)
        {
#if (REVIT2020)
            {
                return UnitUtils.ConvertToInternalUnits(mmValue, DisplayUnitType.DUT_MILLIMETERS);
            }
#else
            return UnitUtils.ConvertToInternalUnits(mmValue, UnitTypeId.Millimeters);
#endif

        }

        /// <summary>
        ///  int feetValue to mm
        /// </summary>
        /// <param name="feetValue">int feetValue</param>
        /// <returns>double mm vlaue</returns>
        public static double FeetToMM(this int feetValue)
        {
#if (REVIT2020)
            {
                return UnitUtils.ConvertFromInternalUnits(feetValue, DisplayUnitType.DUT_MILLIMETERS);
            }
#else
            return UnitUtils.ConvertFromInternalUnits(feetValue, UnitTypeId.Millimeters);
#endif

        }
    }
}
