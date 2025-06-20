using Dahmira_DB.DAL.Model;
using Dahmira.Interfaces;
using Dahmira.Models;
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
    public class AddCalc_Action : IAction
    {
        private List<CalcProduct> Products;
        private MainWindow Window;
        public AddCalc_Action(List<CalcProduct> _products, MainWindow _window)
        {
            Products = _products;
            Window = _window;
        }

        public void Undo()
        {
            try
            {
                if (Products.Count == 1)
                {
                    string text = string.Empty;

                    if (Products[0].ID == -1)
                        text = $"Категория с именем {Products[0].Manufacturer} удалена из расчёта.";
                    else if (Products[0].ID == 0)
                        text = $"Раздел с именем {Products[0].Manufacturer} удалён из расчёта.";
                    else
                        text = $"Строка под номером {Products[0].Num} удалена из расчёта.";

                    Window.CalcInfo_label.Content = $"Действие отменено. {text}";
                    Window.calcItems.Remove(Products[0]);
                }
                else if (Products.Count > 1)
                {
                    foreach (CalcProduct product in Products)
                    {
                        Window.calcItems.Remove(product);
                    }

                    Window.CalcInfo_label.Content = $"Действие отменено. Добавленные строки удалены.";
                }

                if (!Window.isCalcOpened)
                {
                    Window.priceCalcButton_Click(Window, new RoutedEventArgs());
                }

                ICalcController calcController = new CalcController_Services();
                calcController.Refresh(Window.CalcDataGrid, Window.calcItems);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }
    }
}
