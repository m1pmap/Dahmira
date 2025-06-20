using Dahmira_DB.DAL.Model;
using Dahmira.Models;
using Dahmira.Services;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Dahmira.Services.Actions;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Dahmira_Log.DAL.Repository;
using System.Diagnostics;

namespace Dahmira.Pages
{
    /// <summary>
    /// Логика взаимодействия для DuplicateSearchPage.xaml
    /// </summary>
    public partial class DuplicateSearchPage : Window
    {
        MainWindow window;
        int selectedColumnIndex = 0;
        List<Dublicate> dublicationsList = new List<Dublicate>();
        public DuplicateSearchPage(MainWindow mainWindow, List<Dublicate> dublicates)
        {
            try
            {
                InitializeComponent();
                window = mainWindow;
                dublicationsList = dublicates;

                dublicate_dataGrid.ItemsSource = dublicationsList;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void removeRowOnPrice_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (selectedColumnIndex != 0 && selectedColumnIndex != 3)
                {
                    string cellValue = string.Empty;
                    Dublicate selectedDublicate = (Dublicate)dublicate_dataGrid.SelectedItem;
                    if (selectedColumnIndex == 1)
                    {
                        cellValue = selectedDublicate.FirstRowIndex;
                    }
                    else
                    {
                        cellValue = selectedDublicate.SecondRowIndex;
                    }

                    if (cellValue == "Удалён")
                    {
                        return;
                    }

                    bool isDeleting = false;
                    int index = Convert.ToInt32(cellValue);

                    foreach (var item in dublicationsList)
                    {
                        bool isDeletingItem = false;
                        if (item.FirstRowIndex == cellValue)
                        {
                            item.FirstRowIndex = "Удалён";
                            item.IsFirstRowDelete = true;
                            isDeletingItem = true;
                        }
                        if (item.SecondRowIndex == cellValue)
                        {
                            item.SecondRowIndex = "Удалён";
                            item.IsSecondRowDelete = true;
                            isDeletingItem = true;
                        }

                        if (isDeletingItem && !isDeleting)
                        {
                            if (!window.materialForDBDeleting.Any(m => m == window.dbItems[index - 1]))
                            {
                                window.materialForDBDeleting.Add(window.dbItems[index - 1]);
                            }
                            window.actions.Push(new DeleteDB_Action(new List<Material> { window.dbItems[index - 1] }, new List<int> { index - 1 }, window));
                            window.dbItems.Remove(window.dbItems[index - 1]);
                            isDeleting = true;
                        }
                    }

                    if (isDeleting)
                    {
                        foreach (var dublicateItem in dublicationsList)
                        {
                            if (dublicateItem.FirstRowIndex != "Удалён")
                            {
                                if (Convert.ToInt32(dublicateItem.FirstRowIndex) > index)
                                {
                                    dublicateItem.FirstRowIndex = (Convert.ToInt32(dublicateItem.FirstRowIndex) - 1).ToString();
                                }
                            }

                            if (dublicateItem.SecondRowIndex != "Удалён")
                            {
                                if (Convert.ToInt32(dublicateItem.SecondRowIndex) > index)
                                {
                                    dublicateItem.SecondRowIndex = (Convert.ToInt32(dublicateItem.SecondRowIndex) - 1).ToString();
                                }
                            }
                        }
                    }
                    selectedColumnIndex = 0;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void dublicate_dataGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var visualHit = e.OriginalSource as FrameworkElement;
                while (visualHit != null && !(visualHit is DataGridCell))
                {
                    visualHit = VisualTreeHelper.GetParent(visualHit) as FrameworkElement;
                }

                if (visualHit is DataGridCell cell)
                {
                    selectedColumnIndex = cell.Column.DisplayIndex;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void refreshDublicates_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                selectedColumnIndex = 0;
                var duplicates = window.dbItems.Select((material, index) => new { material.Article, Index = index })
                                      .GroupBy(x => x.Article)
                                      .Where(g => g.Count() > 1)
                                      .Select(g => new
                                      {
                                          Article = g.Key,
                                          Indices = g.Select(x => x.Index).ToList()
                                      });

                dublicationsList.Clear();
                int counter = 1;
                foreach (var entry in duplicates)
                {
                    if (entry.Indices.Count % 2 == 0)
                    {
                        for (int i = 0; i < entry.Indices.Count; i += 2)
                        {
                            dublicationsList.Add(new Dublicate { Num = counter, FirstRowIndex = (entry.Indices[i] + 1).ToString(), SecondRowIndex = (entry.Indices[i + 1] + 1).ToString(), Article = entry.Article });
                            counter++;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < entry.Indices.Count - 1; i += 2)
                        {
                            dublicationsList.Add(new Dublicate { Num = counter, FirstRowIndex = (entry.Indices[i] + 1).ToString(), SecondRowIndex = (entry.Indices[i + 1] + 1).ToString(), Article = entry.Article });
                            counter++;
                        }
                        dublicationsList.Add(new Dublicate { Num = counter, FirstRowIndex = (entry.Indices[entry.Indices.Count - 2] + 1).ToString(), SecondRowIndex = (entry.Indices[entry.Indices.Count - 1] + 1).ToString(), Article = entry.Article });
                        counter++;
                    }
                }
                dublicate_dataGrid.Items.Refresh();

                if (dublicationsList.Count == 0)
                {
                    MessageBox.Show("Повторы в прайсе не обнаружены");
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                Topmost = true;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                Topmost = false;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void dublicate_dataGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (selectedColumnIndex != 0 && selectedColumnIndex != 3)
                {
                    string cellValue = string.Empty;

                    Dublicate selectedDublicate = (Dublicate)dublicate_dataGrid.SelectedItem;
                    if (selectedColumnIndex == 1)
                    {
                        cellValue = selectedDublicate.FirstRowIndex;
                    }
                    else
                    {
                        cellValue = selectedDublicate.SecondRowIndex;
                    }

                    if (cellValue != "Удалён")
                    {
                        window.dataBaseGrid.SelectedItem = window.dbItems[Convert.ToInt32(cellValue) - 1];
                        window.dataBaseGrid.ScrollIntoView(window.dataBaseGrid.SelectedItem);
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void exit_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == System.Windows.Input.Key.Escape)
                {
                    Close();
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    DragMove();
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }
    }
}
