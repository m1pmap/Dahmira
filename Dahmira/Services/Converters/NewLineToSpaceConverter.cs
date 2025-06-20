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
    public class NewLineToSpaceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is string str)
                {
                    // Заменяем все возможные варианты переноса строки на пробел
                    return str
                        .Replace("\r\n", " ")  // CR+LF (Windows)
                        .Replace("\n", " ")    // LF (Unix/macOS)
                        .Replace("\r", " ")    // CR (старые Mac)
                        .Replace("\v", " ")    // Вертикальный таб
                        .Replace("\u2028", " ") // Line Separator (Unicode)
                        .Replace("\u2029", " "); // Paragraph Separator (Unicode)
                }
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
                throw new NotImplementedException();
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
