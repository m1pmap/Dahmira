using Dahmira.Interfaces;
using Dahmira.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using Dahmira_Log.DAL.Repository;
using System.Diagnostics;

namespace Dahmira.Services.Actions
{
    public class DeleteDependency_Action : IAction
    {
        MainWindow Window;
        CalcProduct Product;
        Dependency Dependency;
        int DependencyIndex;

        public DeleteDependency_Action(MainWindow _window, CalcProduct _product, Dependency _dependency, int _dependencyIndex) 
        { 
            Window = _window;
            Product = _product;
            Dependency = _dependency;
            DependencyIndex = _dependencyIndex;
        }

        public void Undo()
        {
            try
            {
                Window.CalcInfo_label.Content = $"Действие отменено. Зависимость восстановлена.";
                ICalcController calcController = new CalcController_Services();

                Product.dependencies.Insert(DependencyIndex, Dependency);
                Product.RowColor = calcController.ColorToHex(Colors.MediumSeaGreen);
                Product.RowForegroundColor = calcController.ColorToHex(Colors.White);

                if (Window.CalcDataGrid.SelectedItem != Product)
                {
                    int index = Window.calcItems.IndexOf(Product);
                    Window.CalcDataGrid.SelectedIndex = index;
                }

                if (!Window.isCalcOpened)
                {
                    Window.priceCalcButton_Click(Window, new RoutedEventArgs());
                }

                if (Window.isAddtoDependency)
                {
                    Window.startStopAddingDependency_button_Click(Window, new RoutedEventArgs());
                }

                calcController.Refresh(Window.CalcDataGrid, Window.calcItems);
                calcController.ValidateCalcItem(Product);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }
    }
}
