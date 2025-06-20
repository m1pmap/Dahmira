using Dahmira_DB.DAL.Interfaces;
using Dahmira_DB.DAL.Repository;
using Dahmira_DB.DAL.Model;
using Dahmira.Interfaces;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows;
using Dahmira.Models;
using Dahmira_Log.DAL.Repository;
using System.Diagnostics;

namespace Dahmira.Services.Actions
{
    public class AddDB_Action : IAction
    {
        private Material Material;
        private MainWindow Window;
        public AddDB_Action(Material _material, MainWindow _window)
        {
            Material = _material;
            Window = _window;
        }

        public void Undo()
        {
            try
            {
                Window.PriceInfo_label.Content = $"Действие отменено. Строка под номером {Window.dbItems.IndexOf(Material) + 1} удалена из прайса.";
                Window.dbItems.Remove(Material);
                bool res = Window.materialForDBAdding.Remove(Material);
                if (!res)
                {
                    Window.materialForDBDeleting.Add(Material);
                }

                if (Window.isCalcOpened)
                {
                    Window.priceCalcButton_Click(Window, new RoutedEventArgs());
                }

                //Обновление данных в поиске
                Window.UpdateDataInSearch();

                Window.productsCount_label.Content = "из " + Window.dataBaseGrid.Items.Count.ToString();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }
    }
}
