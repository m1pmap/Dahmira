using Dahmira_Log.DAL.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Dahmira.Services.Converters
{
    public class SpaceGroupDotDecimalConverter : IValueConverter
    {
        private static readonly CultureInfo CustomCulture;

        static SpaceGroupDotDecimalConverter()
        {
            CustomCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            CustomCulture.NumberFormat.NumberGroupSeparator = " ";
            CustomCulture.NumberFormat.NumberDecimalSeparator = ".";
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null ||
                   value is double && double.IsNaN((double)value) ||
                   value is int && (int)value <= 0)
                {
                    return "";
                }
                else if (value is float && float.IsNaN((float)value))
                {
                    return "0.00";
                }
                if (value == null) return "";
                if (double.TryParse(value.ToString(), out double number))
                    return number.ToString("#,##0.00", CustomCulture);
                return value;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return value;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return null;
            }
        }
    }
}
