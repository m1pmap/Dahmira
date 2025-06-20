using Dahmira_DB.DAL.Model;
using Dahmira.Models;
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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Dahmira_Log.DAL.Repository;
using System.Diagnostics;

namespace Dahmira.Pages
{
    /// <summary>
    /// Логика взаимодействия для ChangePriceForm.xaml
    /// </summary>
    public partial class ChangePriceForm : Window
    {
        MainWindow mainWindow;
        public ChangePriceForm(MainWindow window)
        {
            try
            {
                InitializeComponent();
                mainWindow = window;

                //Заносим в комбо бокс производителей
                Manufacturer_CB.ItemsSource = CountryManager.Instance.allManufacturers;

                //Задаём изначально производителя
                Material selectedItem = (Material)mainWindow.dataBaseGrid.SelectedItem;
                if (selectedItem != null)
                {
                    Manufacturer_CB.Text = selectedItem.Manufacturer;
                }
                else
                {
                    Manufacturer_CB.Text = "не выбран";
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

               // return null;
            }
        }

        private void ChangePrice_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var changeItems = mainWindow.dbItems.Where(m => m.Manufacturer == Manufacturer_CB.Text).ToList();

                double percent = double.Parse(discount_tb.Text);

                foreach (var item in changeItems)
                {
                    double percentPrice = percent * item.Cost / 100;
                    item.Cost = (float)Math.Round(item.Cost + percentPrice, 2);
                    item.LastCostUpdate = DateTime.Now.ToString("dd.MM.yyyy");
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
