using Dahmira_Log.DAL.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Dahmira.Services.Converters
{
    internal class BoolToLightSkyBlueBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) //Конвертер нулевого значения в пустую строку
        {
            try
            {
                return value is true ? Brushes.LightSkyBlue : Brushes.Red;
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
