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
    public class UpdateCalc_Action : IAction
    {
        private CalcProduct Product;
        private CalcProduct NewProduct;
        private MainWindow Window;
        public UpdateCalc_Action(CalcProduct _product, CalcProduct _newProduct, MainWindow _window)
        {
            Product = _product;
            NewProduct = _newProduct;
            Window = _window;
        }

        public void Undo()
        {
            try
            {
                NewProduct.Article = Product.Article;
                NewProduct.Unit = Product.Unit;
                NewProduct.EnglishUnit = Product.EnglishUnit;
                NewProduct.Manufacturer = Product.Manufacturer;
                NewProduct.ProductName = Product.ProductName;
                NewProduct.EnglishProductName = Product.EnglishProductName;
                NewProduct.Cost = Product.Cost;
                NewProduct.Photo = Product.Photo;
                NewProduct.Count = Product.Count;
                NewProduct.Note = Product.Note;


                if (!Window.isCalcOpened) { Window.priceCalcButton_Click(Window, new RoutedEventArgs()); }
                Window.CalcInfo_label.Content = $"Действие отменено. Редактирование строки под номером {NewProduct.Num} возвращено.";

                int index = Window.calcItems.IndexOf(NewProduct);
                Window.CalcDataGrid.SelectedItem = null;
                Window.CalcDataGrid.SelectedIndex = index;

                ICalcController calcController = new CalcController_Services();
                if (NewProduct.ID > 0)
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
