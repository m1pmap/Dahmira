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
    class UpdateDependency_Action : IAction
    {
        private CalcProduct Product;
        private Dependency Dependency;
        private Dependency NewDependency;
        private MainWindow Window;
        public UpdateDependency_Action(CalcProduct _product, Dependency _dependency, Dependency _newDependency, MainWindow _window)
        {
            Product = _product;
            Dependency = _dependency;
            NewDependency = _newDependency;
            Window = _window;
        }

        public void Undo()
        {
            try
            {
                if (!Window.isCalcOpened) { Window.priceCalcButton_Click(Window, new RoutedEventArgs()); }

                NewDependency.IsFirstButtonVisible = Dependency.IsFirstButtonVisible;
                NewDependency.IsSecondButtonVisible = Dependency.IsSecondButtonVisible;
                NewDependency.ProductId = Dependency.ProductId;
                NewDependency.SecondProductId = Dependency.SecondProductId;
                NewDependency.Multiplier = Dependency.Multiplier;
                NewDependency.SecondMultiplier = Dependency.SecondMultiplier;
                NewDependency.ProductName = Dependency.ProductName;
                NewDependency.SecondProductName = Dependency.SecondProductName;
                NewDependency.SelectedType = Dependency.SelectedType;

                int index = Window.calcItems.IndexOf(Product);
                Window.CalcDataGrid.SelectedItem = null;
                Window.CalcDataGrid.SelectedIndex = index;

                index = Product.dependencies.IndexOf(NewDependency);
                Window.DependencyDataGrid.SelectedItem = null;
                Window.DependencyDataGrid.SelectedIndex = index;

                ICalcController calcController = new CalcController_Services();
                calcController.ValidateCalcItem(Product);
                calcController.Refresh(Window.CalcDataGrid, Window.calcItems);

                Window.CalcInfo_label.Content = $"Действие отменено. Редактирование зависмости строки под номером {Product.Num} возвращено.";
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }
    }
}
