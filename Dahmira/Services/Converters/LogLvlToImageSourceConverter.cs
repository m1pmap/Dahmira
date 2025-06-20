using Dahmira_Log.DAL.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Dahmira.Services.Converters
{
    public class LogLvlToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string errorLvl = value.ToString().ToLower();

                switch (errorLvl)
                {
                    case "error":
                        {
                            return new BitmapImage(new Uri("pack://application:,,,/resources/images/errorLog.png", UriKind.Absolute));
                        }
                    case "information":
                        {
                            return new BitmapImage(new Uri("pack://application:,,,/resources/images/informationLog.png", UriKind.Absolute));
                        }
                    case "warning":
                        {
                            return new BitmapImage(new Uri("pack://application:,,,/resources/images/warningLog.png", UriKind.Absolute));
                        }
                    case "debug":
                        {
                            return new BitmapImage(new Uri("pack://application:,,,/resources/images/debugLog.png", UriKind.Absolute));
                        }
                    default:
                        {
                            return Binding.DoNothing;
                        }
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
