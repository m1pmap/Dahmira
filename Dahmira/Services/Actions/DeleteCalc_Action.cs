using Dahmira.Interfaces;
using Dahmira.Models;
using Dahmira_Log.DAL.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static Azure.Core.HttpHeader;
using static OfficeOpenXml.ExcelErrorValue;

namespace Dahmira.Services.Actions
{
    public class DeleteCalc_Action : IAction
    {
        private List<CalcProduct> Products;
        private MainWindow Window;
        private List<int> ProductsIndex;
        public DeleteCalc_Action(List<CalcProduct> _products, List<int> _productsIndex, MainWindow _window)
        {
            Products = _products;
            Window = _window;
            ProductsIndex = _productsIndex;
        }
        public void Undo()
        {
            try
            {
                if (ProductsIndex.Count == 1)
                {
                    string text = string.Empty;

                    if (Products[0].ID == -1)
                        text = $"Категория с именем {Products[0].Manufacturer} восстановлена.";
                    else if (Products[0].ID == 0)
                        text = $"Раздел с именем {Products[0].Manufacturer} восстановлен.";
                    else
                        text = $"Строка под номером {Products[0].Num} восстановлена.";

                    Window.CalcInfo_label.Content = $"Действие отменено. {text}";
                    Window.calcItems.Insert(ProductsIndex[0], Products[0]);
                }
                else if (ProductsIndex.Count > 1)
                {
                    //Сортировка элементов и индексов в списке, если выбрано несколько элементов
                    var sortedData = ProductsIndex
                        .Select((value, index) => new { Value = value, Material = Products[index] })
                        .OrderBy(item => item.Value)
                        .ToList();

                    // Обновляем списки
                    ProductsIndex = sortedData.Select(item => item.Value).ToList();
                    Products = sortedData.Select(item => item.Material).ToList();

                    Products.Reverse();

                    foreach (var product in Products)
                    {
                        Window.calcItems.Insert(ProductsIndex[0], product);
                    }

                    Window.CalcInfo_label.Content = $"Действие отменено. Удалённые строки восстановлены.";
                }

                if (!Window.isCalcOpened)
                {
                    Window.priceCalcButton_Click(Window, new RoutedEventArgs());
                }

                Window.CalcDataGrid.SelectedItem = null;
                Window.CalcDataGrid.SelectedIndex = ProductsIndex[0];

                ICalcController calcController = new CalcController_Services();
                calcController.Refresh(Window.CalcDataGrid, Window.calcItems);

                Clipboard.Clear();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }
    }
}
