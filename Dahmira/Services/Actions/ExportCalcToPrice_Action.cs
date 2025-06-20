using Dahmira_DB.DAL.Model;
using Dahmira.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Dahmira_Log.DAL.Repository;
using System.Diagnostics;

namespace Dahmira.Services.Actions
{
    public class ExportCalcToPrice_Action : IAction
    {
        List<Material> DBNewItems;
        MainWindow Window;
        public ExportCalcToPrice_Action(List<Material> _dbNewItems, MainWindow window)
        {
            DBNewItems = _dbNewItems;
            Window = window;
        }

        public void Undo()
        {
            try
            {
                foreach (var item in DBNewItems)
                {
                    Window.dbItems.Remove(item);
                    bool res = Window.materialForDBAdding.Remove(item);
                    if (!res)
                    {
                        Window.materialForDBDeleting.Add(item);
                    }
                }

                if (Window.isCalcOpened)
                {
                    Window.priceCalcButton_Click(Window, new RoutedEventArgs());
                }

                Window.PriceInfo_label.Content = "Действие отменено. Cтроки, добавленные с расчёта удалены в прайсе.";
                Window.productsCount_label.Content = $"из {Window.dbItems.Count}";
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }
    }
}
