using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace GunboundImageCreator.App.Converters
{
    public class IntToBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.</param><param name="targetType">The type of the binding target property.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var num = (int)value;

                if (num > 0)
                {
                    return true;
                }
            }

            return false;

            //if (value == null) return false;
            //PropertyInfo propertyInfo = value.GetType().GetProperty("Count");
            //if (propertyInfo != null)
            //{
            //    int count = (int)propertyInfo.GetValue(value, null);
            //    return count > 0;
            //}
            //if (!(value is bool || value is bool?)) return true;
            //return value;
        }

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value that is produced by the binding target.</param><param name="targetType">The type to convert to.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return 1;
            }

            return 0;
        }
    }
}
