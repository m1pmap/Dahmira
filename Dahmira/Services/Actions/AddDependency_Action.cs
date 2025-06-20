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
using System.Windows.Media;

namespace Dahmira.Services.Actions
{
    public class AddDependency_Action : IAction
    {
        private MainWindow Window;
        private CalcProduct Product;
        private Dependency Dependency;

        public AddDependency_Action(MainWindow _window, CalcProduct _product, Dependency _dependency)
        {
            Window = _window;
            Product = _product;
            Dependency = _dependency;
        }

        public void Undo()
        {
            try
            {
                ICalcController calcController = new CalcController_Services();

                Product.dependencies.Remove(Dependency);
                Product.RowColor = calcController.ColorToHex(Colors.Transparent);
                Product.RowForegroundColor = calcController.ColorToHex(Colors.Black);

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

                Window.CalcInfo_label.Content = $"Действие отменено. Зависимость удалена.";
                calcController.Refresh(Window.CalcDataGrid, Window.calcItems);
                calcController.ValidateCalcItem(Product);
                Window.DependencyDataGrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }
    }
}
