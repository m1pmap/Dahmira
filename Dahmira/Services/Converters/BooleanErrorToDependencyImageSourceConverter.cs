using Dahmira_Log.DAL.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Dahmira.Services.Converters
{
    internal class BooleanErrorToDependencyImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) //Конвертер нулевого значения в пустую строку
        {
            try
            {
                if (value is bool booleanValue)
                {
                    string imagePath = booleanValue
                        ? "pack://application:,,,/resources/images/errorDependency.png"
                        : "pack://application:,,,/resources/images/dependency.png";

                    return new BitmapImage(new Uri(imagePath, UriKind.Absolute));
                }

                return Binding.DoNothing;
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
