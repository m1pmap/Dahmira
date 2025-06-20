using Dahmira_DB.DAL.Interfaces;
using Dahmira_DB.DAL.Model;
using Dahmira_DB.DAL.Repository;
using Dahmira.Interfaces;
using Dahmira.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Dahmira_Log.DAL.Repository;
using System.Diagnostics;

namespace Dahmira.Services.Actions
{
    public class DeleteDB_Action : IAction
    {
        private List<Material> Materials;
        private MainWindow Window;
        private List<int> MaterialsIndex;
        public DeleteDB_Action(List<Material> _material, List<int> _materialIndex, MainWindow _window)
        {
            Materials = _material;
            Window = _window;
            MaterialsIndex = _materialIndex;
        }

        public void Undo()
        {
            try
            {
                if (Materials.Count == 1)
                {
                    Window.PriceInfo_label.Content = $"Действие отменено. Строка под номером {MaterialsIndex[0] + 1} восстановлена.";
                    Window.dbItems.Insert(MaterialsIndex[0], Materials[0]);

                    Window.productsCount_label.Content = $"из {Window.dbItems.Count}";
                    int index = Window.dbItems.IndexOf(Materials[0]);
                    Window.dataBaseGrid.SelectedItem = null;
                    Window.dataBaseGrid.SelectedIndex = index;

                    bool res = Window.materialForDBDeleting.Remove(Materials[0]);
                    if (!res)
                    {
                        Window.materialForDBAdding.Add(Materials[0]);
                    }
                }
                else if (Materials.Count > 1)
                {
                    //Сортировка элементов и индексов в списке, если выбрано несколько элементов
                    var sortedData = MaterialsIndex
                        .Select((value, index) => new { Value = value, Material = Materials[index] })
                        .OrderBy(item => item.Value)
                        .ToList();

                    // Обновляем списки
                    MaterialsIndex = sortedData.Select(item => item.Value).ToList();
                    Materials = sortedData.Select(item => item.Material).ToList();

                    int firstIndex;

                    if (MaterialsIndex[0] < MaterialsIndex[MaterialsIndex.Count - 1])
                    {
                        Materials.Reverse();
                        firstIndex = MaterialsIndex[0];
                    }
                    else
                    {
                        firstIndex = MaterialsIndex[MaterialsIndex.Count - 1];
                    }

                    foreach (var material in Materials)
                    {
                        Window.dbItems.Insert(firstIndex, material);

                        int index = Window.dbItems.IndexOf(Materials[0]);


                        bool res = Window.materialForDBDeleting.Remove(material);
                        if (!res)
                        {
                            Window.materialForDBAdding.Add(material);
                        }
                    }
                }
                //Обновление данных в поиске
                Window.UpdateDataInSearch();

                Window.productsCount_label.Content = "из " + Window.dataBaseGrid.Items.Count.ToString();

                Window.dataBaseGrid.SelectedItem = null;
                Window.dataBaseGrid.SelectedIndex = MaterialsIndex[0];

                if (Window.isCalcOpened)
                {
                    Window.priceCalcButton_Click(Window, new RoutedEventArgs());
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }
    }
}
