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
    internal class NullToEmptyStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) //Конвертер нулевого значения в пустую строку
        {
            try
            {
                // Проверяем, является ли значение null
                if (value == null ||
                   value is double && double.IsNaN((double)value) ||
                   value is int && (int)value <= 0 ||
                   value is double && (double)value <= 0)
                {
                    return "";
                }
                else
                {
                    return value;
                }
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
                if (value is string strValue && string.IsNullOrEmpty(strValue))
                {
                    // Возвращаем 0 для числовых типов или null для других типов
                    if (targetType == typeof(int))
                    {
                        return 0; // Возвращаем 0 для int
                    }
                    else if (targetType == typeof(double))
                    {
                        return 0.0; // Возвращаем 0.0 для double
                    }
                    else if (targetType == typeof(string))
                    {
                        return null; // Возвращаем null для string
                    }
                    // Добавьте дополнительные типы по мере необходимости
                }

                // Если значение не пустое, возвращаем его как есть
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
