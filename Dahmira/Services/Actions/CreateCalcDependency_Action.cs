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
    public class CreateCalcDependency_Action : IAction
    {
        CalcProduct Product;
        MainWindow Window;
        public CreateCalcDependency_Action(CalcProduct _product, MainWindow _window) 
        {
            Product = _product;
            Window = _window;
        }

        public void Undo()
        {
            try
            {
                Window.CalcInfo_label.Content = $"Действие отменено. Зависимость строки под номером {Product.Num} удалена.";
                Product.isDependency = false;
                Product.dependencies.Clear();
                Product.Count = "1";

                if (!Window.isCalcOpened)
                {
                    Window.priceCalcButton_Click(Window, new RoutedEventArgs());
                }

                if (Window.CalcDataGrid.SelectedItem != Product)
                {
                    int index = Window.calcItems.IndexOf(Product);
                    Window.CalcDataGrid.SelectedItem = null;
                    Window.CalcDataGrid.SelectedIndex = index;
                }
                Window.DependencyImage.Visibility = Visibility.Visible;
                Window.DependencyDataGrid.Visibility = Visibility.Hidden;
                Window.DependencyButtons.Visibility = Visibility.Hidden;
                Window.DependencyDataGrid.ItemsSource = null;

                ICalcController calcController = new CalcController_Services();
                calcController.ValidateCalcItem(Product);
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
