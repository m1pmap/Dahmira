using Dahmira_DB.DAL.Model; 
using Dahmira.Interfaces;
using Dahmira.Models;
using Dahmira.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
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
    /// Логика взаимодействия для ExportPriceListToExcel.xaml
    /// </summary>
    public partial class ExportPriceListToExcel : Window
    {
        //Сервесы
        private IFolderPath pathFolderController = new FolderPath_Services();           //Сервис для работы с проводником
        private IFileImporter fileImporter = new FileImporter_Services();               //Сервис для Экспорта данных

        //Формы
        public MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;    //Основная форма


        public ExportPriceListToExcel()
        {
            try
            {
                InitializeComponent();

                //Заносим в комбо бокс производителей
                Manufacturer_CB.ItemsSource = CountryManager.Instance.allManufacturers;
                patch_tb.Text = mainWindow.settings.PathExport_Price;
                patch_tb.IsEnabled = false;
                export_one_rb.IsChecked = true;

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


        //Кликаем по RadioButton
        private void TypeExport_RB_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                // Отображение выбранного варианта
                var radioButton = sender as RadioButton;
                if (radioButton != null)
                {

                    switch (radioButton.Tag)
                    {
                        //Выгружаем конктретного производителя
                        case "1":
                            Manufacturer_CB.Text = "Не выбран";
                            Manufacturer_CB.IsEnabled = true;
                            break;

                        //Выгружаем всех производителей
                        case "2":
                            Manufacturer_CB.Text = "Все";
                            Manufacturer_CB.IsEnabled = false;
                            break;
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


        //Экспорт бд
        private void Export_DB_bt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Конекретного производителя
                if (export_one_rb.IsChecked == true)
                {
                    //Смотрим пустой ли combobox
                    if (Manufacturer_CB.Text == "" || Manufacturer_CB.Text == null)
                    {
                        MessageBox.Show("Выберите производителя!");
                    }
                    else
                    {
                        //Отбираем конкретного производителя
                        var dbItems_Manufacturer = new ObservableCollection<Material>(mainWindow.dbItems.Where(w => w.Manufacturer == Manufacturer_CB.Text).ToList());

                        //Провепяем найден ли такой производитель
                        if (dbItems_Manufacturer.Count == 0)
                        {
                            MessageBox.Show("Производитель не найден!");
                        }
                        else
                        {
                            DirectoryInfo dirInfo = Directory.CreateDirectory(patch_tb.Text + "//" + Manufacturer_CB.Text);
                            string path = dirInfo.FullName;
                            //Эскмпортируем в Excel
                            if (!fileImporter.ExportPriseToExcel_DB(Manufacturer_CB.Text, mainWindow.settings, dbItems_Manufacturer, path + "//" + Manufacturer_CB.Text + ".xlsx"))
                            {
                                mainWindow.PriceInfo_label.Content = "Не удалось выгрузить данные по поставщику " + Manufacturer_CB.Text + " из прайса.";
                                MessageBox.Show("Не удалось выгрузить данные по поставщику " + Manufacturer_CB.Text + " из прайса.");
                            }
                            else
                            {
                                //Экспортируем картинки
                                if (!fileImporter.ExportPhotoToJPG_DB(dbItems_Manufacturer, path))
                                {
                                    mainWindow.PriceInfo_label.Content = "Excel файл выгружен без ошибок, но не удалось выгрузить картинки из прайса по поставщику " + Manufacturer_CB.Text + ".";
                                    MessageBox.Show("Excel файл выгружен без ошибок, но не удалось выгрузить картинки из прайса по поставщику " + Manufacturer_CB.Text + ".");
                                }
                                else
                                {
                                    mainWindow.PriceInfo_label.Content = "Весь прайс выгружен в Excel без ошибок. Все картинки выгружены без ошибок.";
                                    MessageBox.Show("Весь прайс выгружен в Excel без ошибок. \nВсе картинки выгружены без ошибок.");
                                    this.Close();
                                }
                            }
                        }
                    }
                }


                //Всех производителей (весть прайс)
                if (export_all_rb.IsChecked == true)
                {
                    DirectoryInfo dirInfo = Directory.CreateDirectory(patch_tb.Text + "//Полный прайс");
                    string path = dirInfo.FullName;
                    //Эскмпортируем в Excel
                    if (!fileImporter.ExportPriseToExcel_DB("all", mainWindow.settings, mainWindow.dbItems, path + "//Полный прайс.xlsx"))
                    {
                        mainWindow.PriceInfo_label.Content = "Не удалось выгрузить все данные из прайса.";
                        MessageBox.Show("Не удалось выгрузить все данные из прайса.");
                    }
                    else
                    {
                        //Экспортируем картинки
                        if (!fileImporter.ExportPhotoToJPG_DB(mainWindow.dbItems, path))
                        {
                            mainWindow.PriceInfo_label.Content = "Excel файл выгружен без ошибок, но не удалось выгрузить все картинки из прайса.";
                            MessageBox.Show("Excel файл выгружен без ошибок, но не удалось выгрузить все картинки из прайса.");
                        }
                        else
                        {
                            mainWindow.PriceInfo_label.Content = "Весь прайс выгружен в Excel без ошибок. Все картинки выгружены без ошибок.";
                            MessageBox.Show("Весь прайс выгружен в Excel без ошибок. \nВсе картинки выгружены без ошибок.");
                            this.Close();
                        }
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


        //Добавление пути к паке
        private void GetPach_bt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                pathFolderController.SelectedFolder(patch_tb);

                mainWindow.settings.PathExport_Price = patch_tb.Text;

                fileImporter.ExportSettingsOnFile(mainWindow);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }


        //Удаление пути к папке
        private void DeletePach_bt_Click(object sender, RoutedEventArgs e) 
        {
            try
            {
                patch_tb.Text = "C:\\";
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void ExportManufacturer_label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (export_one_rb.IsChecked == false)
                {
                    export_one_rb.IsChecked = true;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void exportAllManufacturers_label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (export_all_rb.IsChecked == false)
                {
                    export_all_rb.IsChecked = true;
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
