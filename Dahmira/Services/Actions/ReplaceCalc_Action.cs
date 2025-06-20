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

namespace Dahmira.Services.Actions
{
    public class ReplaceCalc_Action : IAction
    {
        private CalcProduct OldProduct;
        private CalcProduct NewProduct;
        private MainWindow Window;
        public ReplaceCalc_Action(CalcProduct _oldProduct, CalcProduct _newProduct, MainWindow _window)
        {
            OldProduct = _oldProduct;
            NewProduct = _newProduct;
            Window = _window;
        }

        public void Undo()
        {
            try
            {
                NewProduct.ID = OldProduct.ID;
                NewProduct.Num = OldProduct.Num;
                NewProduct.Manufacturer = OldProduct.Manufacturer;
                NewProduct.ProductName = OldProduct.ProductName;
                NewProduct.EnglishProductName = OldProduct.EnglishProductName;
                NewProduct.Article = OldProduct.Article;
                NewProduct.Unit = OldProduct.Unit;
                NewProduct.Photo = OldProduct.Photo;
                NewProduct.RealCost = OldProduct.RealCost;
                NewProduct.Cost = OldProduct.Cost;
                NewProduct.Count = OldProduct.Count;
                NewProduct.TotalCost = OldProduct.TotalCost;
                NewProduct.ID_Art = OldProduct.ID_Art;
                NewProduct.Note = OldProduct.Note;
                NewProduct.RowColor = OldProduct.RowColor;
                NewProduct.RowForegroundColor = OldProduct.RowForegroundColor;
                NewProduct.isDependency = OldProduct.isDependency;
                NewProduct.dependencies = OldProduct.dependencies;

                if (!Window.isCalcOpened)
                {
                    Window.priceCalcButton_Click(Window, new RoutedEventArgs());
                }

                Window.CalcInfo_label.Content = $"Действие отменено. Замена строки под номером {NewProduct.Num} отменена.";
                if (Window.CalcDataGrid.SelectedItem != NewProduct)
                {
                    Window.CalcDataGrid.SelectedItems.Clear();
                    int index = Window.calcItems.IndexOf(NewProduct);
                    Window.CalcDataGrid.SelectedIndex = index;
                }

                if (!Window.isCalcOpened)
                {
                    Window.priceCalcButton_Click(Window, new RoutedEventArgs());
                }
                ICalcController calcController = new CalcController_Services();
                calcController.ValidateCalcItem(NewProduct);
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
