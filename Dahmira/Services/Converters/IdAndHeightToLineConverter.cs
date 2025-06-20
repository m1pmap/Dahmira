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
using System.Windows.Media;

namespace Dahmira.Services.Converters
{
    public class IdAndHeightToLineConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values.Length < 2 || !(values[0] is int id) || !(values[1] is double height))
                    return null;

                var geometry = new GeometryGroup();

                if (id == 0)
                {
                    geometry.Children.Add(new LineGeometry(new Point(0, 0), new Point(0, height + 10)));
                }
                else if (id > 0)
                {
                    geometry.Children.Add(new LineGeometry(new Point(0, 0), new Point(0, height + 10)));
                    geometry.Children.Add(new LineGeometry(new Point(22, 0), new Point(22, height + 10)));
                }

                return geometry;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
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
