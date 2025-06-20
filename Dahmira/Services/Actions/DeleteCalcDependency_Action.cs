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
    public class DeleteCalcDependency_Action : IAction
    {
        CalcProduct OldProduct;
        CalcProduct NewProduct;
        MainWindow Window;
        public DeleteCalcDependency_Action(CalcProduct _oldProduct, CalcProduct _newProduct, MainWindow _window)
        {
            OldProduct = _oldProduct;
            NewProduct = _newProduct;
            Window = _window;
        }

        public void Undo()
        {
            try
            {
                Window.CalcInfo_label.Content = $"Действие отменено. Зависимости строки {NewProduct.Num} восстановлены.";

                NewProduct.isDependency = OldProduct.isDependency;
                NewProduct.dependencies = OldProduct.dependencies;
                NewProduct.Count = OldProduct.Count;

                if (!Window.isCalcOpened)
                {
                    Window.priceCalcButton_Click(Window, new RoutedEventArgs());
                }

                if (Window.CalcDataGrid.SelectedItem != NewProduct)
                {
                    int index = Window.calcItems.IndexOf(NewProduct);
                    Window.CalcDataGrid.SelectedItem = null;
                    Window.CalcDataGrid.SelectedIndex = index;
                }
                Window.DependencyImage.Visibility = Visibility.Hidden;
                Window.DependencyDataGrid.Visibility = Visibility.Visible;
                Window.DependencyButtons.Visibility = Visibility.Visible;
                Window.DependencyDataGrid.ItemsSource = NewProduct.dependencies;

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
