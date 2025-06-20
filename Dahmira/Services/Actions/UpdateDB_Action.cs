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
    public class UpdateDB_Action : IAction
    {
        private Material Material;
        private Material NewMaterial;
        private MainWindow Window;
        public UpdateDB_Action(Material _material, Material _newMaterial, MainWindow _window)
        {
            Material = _material;
            NewMaterial = _newMaterial;
            Window = _window;
        }

        public void Undo()
        {
            try
            {
                NewMaterial.Article = Material.Article;
                NewMaterial.Unit = Material.Unit;
                NewMaterial.Manufacturer = Material.Manufacturer;
                NewMaterial.Type = Material.Type;
                NewMaterial.ProductName = Material.ProductName;
                NewMaterial.EnglishProductName = Material.EnglishProductName;
                NewMaterial.EnglishUnit = Material.EnglishUnit;
                NewMaterial.Cost = Material.Cost;
                NewMaterial.Photo = Material.Photo;
                NewMaterial.LastCostUpdate = Material.LastCostUpdate;

                bool res = Window.materialForDBUpdating.Remove(NewMaterial);
                if (!res) { Window.materialForDBUpdating.Add(NewMaterial); }

                if (Window.isCalcOpened) { Window.priceCalcButton_Click(Window, new RoutedEventArgs()); }

                Window.dataBaseGrid.SelectedItem = null;
                Window.dataBaseGrid.SelectedIndex = Window.dbItems.IndexOf(NewMaterial);
                Window.PriceInfo_label.Content = $"Действие отменено. Редактирование строки под номером {Window.dbItems.IndexOf(NewMaterial) + 1} возвращено.";

                //Возвращение картинки и в расчёте
                List<CalcProduct> foundedCalcProducts = Window.calcItems.Where(i => i.Article == Material.Article).ToList();
                if (foundedCalcProducts != null)
                {
                    foreach (CalcProduct product in foundedCalcProducts)
                    {
                        product.Photo = Material.Photo;
                    }

                    int index = Window.calcItems.IndexOf(Window.CalcDataGrid.SelectedItem as CalcProduct);
                    Window.CalcDataGrid.SelectedItem = null;
                    Window.CalcDataGrid.SelectedIndex = index;

                    IDbController dbController = new DbController_Service();
                    dbController.ValidateDbItem(NewMaterial);
                }
                Window.UpdateDataInSearch();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }
    }
}
