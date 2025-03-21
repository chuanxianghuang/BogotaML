using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddIn.ExtensionsUtils
{
    public static class RevitParameterExtensions
    {
        /// <summary>
        /// 获取Revit界面上显示对应单位的值
        /// </summary>
        /// <param name="parameter">Revit参数对象</param>
        /// <returns>转换后的值</returns>
        public static double GetFromInternalValue(this Parameter parameter)
        {
            if (parameter == null|| parameter.StorageType != StorageType.Double)
            {
                return 0;
            }
            var paramValue = parameter.AsDouble();
            return ConvertFromInternalUnits(paramValue, parameter);
        }

        /// <summary>
        /// 获取例如界面显示：平方米，转成Revit内部单位的值
        /// </summary>
        /// <param name="parameter">Revit参数对象</param>
        /// <param name="fromInternalValue">从内部单位转换的值</param>
        /// <returns>转换后的内部单位值</returns>
        public static double GetToInternalValue(this Parameter parameter, double fromInternalValue)
        {
            if (parameter == null || parameter.StorageType != StorageType.Double)
            {
                return 0;
            }

            return ConvertToInternalUnits(fromInternalValue, parameter);
        }

        /// <summary>
        /// 将界面上显示对应单位的值，设置到Revit参数中
        /// </summary>
        /// <param name="parameter">Revit参数对象</param>
        /// <param name="displayValue">界面上显示的值</param>
        public static void SetDisplayValueToParameter(this Parameter parameter, double displayValue)
        {
            if (parameter == null || parameter.StorageType != StorageType.Double)
            {
                return;
            }

            double internalValue = ConvertToInternalUnits(displayValue, parameter);
            parameter.Set(internalValue);
        }

        /// <summary>
        /// 将内部单位的值转换为显示单位的值
        /// </summary>
        /// <param name="value">内部单位的值</param>
        /// <param name="parameter">Revit参数对象</param>
        /// <returns>转换后的显示单位值</returns>
        private static double ConvertFromInternalUnits(double value, Parameter parameter)
        {
#if (REVIT2020)
            return UnitUtils.ConvertFromInternalUnits(value, parameter.DisplayUnitType);
#else
            return UnitUtils.ConvertFromInternalUnits(value, parameter.GetUnitTypeId());
#endif
        }

        /// <summary>
        /// 将显示单位的值转换为内部单位的值
        /// </summary>
        /// <param name="value">显示单位的值</param>
        /// <param name="parameter">Revit参数对象</param>
        /// <returns>转换后的内部单位值</returns>
        private static double ConvertToInternalUnits(double value, Parameter parameter)
        {
#if (REVIT2020)
            return UnitUtils.ConvertToInternalUnits(value, parameter.DisplayUnitType);
#else
            return UnitUtils.ConvertToInternalUnits(value, parameter.GetUnitTypeId());
#endif
        }
    }
}
