using Dahmira.Interfaces;
using Dahmira.Models;
using Dahmira.Services;
using OfficeOpenXml.Drawing.Style.Coloring;
using System;
using System.CodeDom;
using System.Collections.Generic;
using Dahmira_Log.DAL.Repository;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Dahmira_DB.DAL;
using Microsoft.Win32;
using System.Net.Mail;
using System.Net;
using Dahmira_DB.DAL.Repository;
using System.Diagnostics;

namespace Dahmira.Pages
{
    /// <summary>
    /// Логика взаимодействия для SimpleSettings.xaml
    /// </summary>
    public partial class SimpleSettings : Window
    {
        private ObservableCollection<ColorItem> colors { get; set; }

        private IFolderPath pathFolderController = new FolderPath_Services();
        private IFileImporter fileImporter = new FileImporter_Services();
        private SettingsParameters settings;
        private MainWindow mainWindow;

        bool isCountryDataGridSelected = false;
        bool isManufacturerWithoutSelected = false;
        bool isManufacturerWithCountrySelected = false;
        public SimpleSettings(SettingsParameters s, MainWindow window)
        {
            try
            {
                InitializeComponent();

                mainWindow = window;
                //Заполнение данными CountryDataGrid
                CountryDataGrid.ItemsSource = CountryManager.Instance.priceManager.countries;
                colors = new ObservableCollection<ColorItem>
            {
                new ColorItem("Красный", System.Drawing.Color.Red),
                new ColorItem ("Зелёный", System.Drawing.Color.LightGreen),
                new ColorItem ("Светло-оранжевый", System.Drawing.Color.NavajoWhite),
                new ColorItem ("Синий", System.Drawing.Color.Blue),
                new ColorItem ("Жёлтый", System.Drawing.Color.LightYellow),
                new ColorItem ("Прозрачный", System.Drawing.Color.Transparent)
            };
                //Цвета для comboBox, отвечающие за настройку Excel
                ExcelTitleColors_comboBox.ItemsSource = colors;
                ExcelCategoryColors_comboBox.ItemsSource = colors;
                ExcelChapterColors_comboBox.ItemsSource = colors;
                ExcelDataColors_comboBox.ItemsSource = colors;
                ExcelPhotoBackgroundColors_comboBox.ItemsSource = colors;
                ExcelNotesColors_comboBox.ItemsSource = colors;
                ExcelNumberColors_comboBox.ItemsSource = colors;

                settings = s; //Получение настроек

                //Отображение даты последней модификации прйса на сервере
                DateTime lastModifyPriceDate = fileImporter.GetFileLastModified(settings, "/Dahmira/data_price_test/db/Dahmira_DB_beta.bak");
                LastDateUpdatePrice_label.Content = "Последняя версия прайса: " + lastModifyPriceDate.ToString("dd.MM.yyyy");

                lastDataUpdate.Content = "ПОСЛЕДНЕЕ ОБНОВЛЕНИЕ: " + CountryManager.Instance.priceManager.lastUpdated.ToString();

                //Установка значений настроек
                //Общие настройки
                theme_comboBox.SelectedIndex = settings.Theme;
                //IsNotificationsWithSound_checkBox.IsChecked = settings.IsNotificationsWithSound;
                //CheckingIntervalFromMail_textBox.Text = settings.CheckingIntervalFromMail.ToString();
                PriceFolderPath_textBox.Text = settings.PriceFolderPath;

                if (settings.IsAdministrator)
                {
                    AdministratingStatus_label.Content = "вы Администратор";
                    AdministratingStatus_label.Foreground = new SolidColorBrush(Colors.MediumSeaGreen);
                    hideShowButtonColumn.Width = new GridLength(30, GridUnitType.Pixel);
                    withoutCountryColumn.Width = new GridLength(0, GridUnitType.Pixel);
                    settings.IsAdministrator = true;
                    CountryDataGrid.IsReadOnly = false;

                    takeAdministratorRules.IsChecked = true;

                    ftpUsername_textBox.Width = 271;
                    ftpPassword_label.Visibility = Visibility.Visible;
                    ftpURL_textBox.IsReadOnly = false;
                    ftpUsername_textBox.IsReadOnly = false;
                }
                else
                {
                    AdministratingStatus_label.Content = "вы не обладаете правами Администратора";
                    AdministratingStatus_label.Foreground = new SolidColorBrush(Colors.OrangeRed);
                    hideShowButtonColumn.Width = new GridLength(0, GridUnitType.Star);
                    withoutCountryColumn.Width = new GridLength(0, GridUnitType.Pixel);
                    settings.IsAdministrator = false;
                    CountryDataGrid.IsReadOnly = true;

                    takeAdministratorRules.IsChecked = false;

                    ftpUsername_textBox.Width = 676;
                    ftpPassword_label.Visibility = Visibility.Hidden;
                    ftpURL_textBox.IsReadOnly = true;
                    ftpUsername_textBox.IsReadOnly = true;

                }

                ftpURL_textBox.Text = settings.url_praise;
                ftpUsername_textBox.Text = settings.ftpUsername;
                ftpPassword_passwordBox.Text = settings.ftpPassword;

                if (mainWindow.isFullProductNames)
                {
                    fullProductNameView_checkBox.IsChecked = true;
                }

                //Вывод данных в Excel
                ComboBoxItemCompare(ExcelTitleColors_comboBox, settings.ExcelTitleColor);
                ComboBoxItemCompare(ExcelCategoryColors_comboBox, settings.ExcelCategoryColor);
                ComboBoxItemCompare(ExcelChapterColors_comboBox, settings.ExcelChapterColor);
                ComboBoxItemCompare(ExcelDataColors_comboBox, settings.ExcelDataColor);
                ComboBoxItemCompare(ExcelPhotoBackgroundColors_comboBox, settings.ExcelPhotoBackgroundColor);
                ComboBoxItemCompare(ExcelNotesColors_comboBox, settings.ExcelNotesColor);
                ComboBoxItemCompare(ExcelNumberColors_comboBox, settings.ExcelNumberColor);
                IsInsertExcelPicture_textBox.IsChecked = settings.IsInsertExcelPicture;
                IsInsertExcelCategory_checkBox.IsChecked = settings.IsInsertExcelCategory;
                IsInserDepartmentRequestWithCalc_textBox.IsChecked = settings.isDepartmentRequestExportWithCalc;
                ExcelImageWidth.Text = settings.ExcelPhotoWidth.ToString();
                ExcelImageHeight.Text = settings.ExcelPhotoHeight.ToString();
                FullCostType_comboBox.SelectedItem = FullCostType_comboBox.Items
                    .Cast<ComboBoxItem>()
                    .FirstOrDefault(i => i.Content.ToString() == settings.FullCostType);

                //Пути сохранения
                ExcelImportFolderPath_textBox.Text = settings.ExcelFolderPath;
                CalcImportFolderPath_textBox.Text = settings.CalcFolderPath;

                //Отображение id элемента в расчёте
                if (settings.isIdOnCalcVisible)
                {
                    displayIdOnCalc.IsChecked = true;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void ComboBoxItemCompare(ComboBox comboBox, ColorItem item)
        {
            try
            {
                comboBox.SelectedItem = comboBox.Items
                    .Cast<ColorItem>()
                    .FirstOrDefault(i => i.Name == item.Name);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void CountryDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e) //Отображение поставщиков у выбранной страны
        {
            try
            {
                isCountryDataGridSelected = true;
                isManufacturerWithCountrySelected = false;
                isManufacturerWithoutSelected = false;
                if (CountryDataGrid.SelectedIndex != CountryDataGrid.Items.Count - 1)
                {
                    Country selectedItem = (Country)CountryDataGrid.SelectedItem;
                    if (selectedItem != null)
                    {
                        ManufacturerDataGrid.ItemsSource = selectedItem.manufacturers;

                        List<Manufacturer> list = CountryManager.Instance.allManufacturers.Except(selectedItem.manufacturers).ToList();
                        WithoutCountryManufacturersDataGrid.ItemsSource = new ObservableCollection<Manufacturer>(list);
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

        private void showHide_button_Click(object sender, RoutedEventArgs e) //Показ и скрытие дополнительного dataGrid с производителями без страны
        {
            try
            {
                GridLength length = new GridLength(20, GridUnitType.Star);
                if (withoutCountryColumn.Width == length) //Если дополнительный dataGrid показан
                {
                    withoutCountryColumn.Width = new GridLength(0, GridUnitType.Star); //Скрытие dataGrid
                    showHide_button.RenderTransform = new RotateTransform(360); //Разворот кнопки
                    showHide_button.ToolTip = "Развернуть";
                }
                else
                {
                    withoutCountryColumn.Width = length; //Разворот dataGrid
                    showHide_button.RenderTransform = new RotateTransform(180);
                    showHide_button.ToolTip = "Скрыть";
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void WithoutCountryManufacturersDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Country selectedCountry = (Country)CountryDataGrid.SelectedItem;
                if(selectedCountry != null)
                {
                    Manufacturer selectedWithoutCountryManufacturer = (Manufacturer)WithoutCountryManufacturersDataGrid.SelectedItem;
                    //Добавление поставщика в местные поставщики выбранной страны
                    selectedCountry.manufacturers.Add(selectedWithoutCountryManufacturer);
                    //Удаление из поставщиков, которые не имеют страны
                    var ManufacturerWithoutCountryItemSource = WithoutCountryManufacturersDataGrid.ItemsSource as ObservableCollection<Manufacturer>;
                    ManufacturerWithoutCountryItemSource.Remove(selectedWithoutCountryManufacturer);
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

            
            }
        }

        private void ManufacturerDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if(settings.IsAdministrator)
                {
                    Country selectedCountry = (Country)CountryDataGrid.SelectedItem;
                    if(selectedCountry != null)
                    {
                        Manufacturer selectedManufacturer = (Manufacturer)ManufacturerDataGrid.SelectedItem;
                        if(selectedManufacturer != null)
                        {
                            //Добавление поставщика в те, что не имеют страны
                            var ManufacturerWithoutCountryItemSource = WithoutCountryManufacturersDataGrid.ItemsSource as ObservableCollection<Manufacturer>;
                            ManufacturerWithoutCountryItemSource.Add(selectedManufacturer);
                            //Удаление поставщика из местных поставщиков выбранной страны
                            selectedCountry.manufacturers.Remove(selectedManufacturer);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

            }
        }

        private void CountryDataGrid_CurrentCellChanged(object sender, EventArgs e) //Когда заканчивается редактирование текущей ячейки
        {
            try
            { 
            isCountryDataGridSelected = true;
            isManufacturerWithCountrySelected = false;
            isManufacturerWithoutSelected = false;
                Country selectedItem = (Country)CountryDataGrid.SelectedItem;

                if (selectedItem != null)
                {
                    if (selectedItem.discount > 100)
                    {
                        selectedItem.discount = 100;
                    }
                    if (selectedItem.discount < 0)
                    {
                        selectedItem.discount = 0;
                    }
                    if (selectedItem.coefficient == 1)
                    {
                        selectedItem.discount = 0;
                    }

                    //Обновляем DataGrid после завершения текущего цикла событий
                    CountryDataGrid.CommitEdit();
                    CountryDataGrid.Dispatcher.BeginInvoke(new Action(() => { CountryDataGrid.Items.Refresh(); }));
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

            }
        }

        private void AdministratorPasswordCheck_button_Click(object sender, RoutedEventArgs e) //Проверка введённого пароля администратора
        {
            try
            {
                if (AdministratorPassword_passwordBox.Password == "Administrator2024") //Если пароль верный
                {
                    AdministratingStatus_label.Content = "вы Администратор";
                    AdministratingStatus_label.Foreground = new SolidColorBrush(Colors.MediumSeaGreen);
                    hideShowButtonColumn.Width = new GridLength(30, GridUnitType.Pixel);
                    withoutCountryColumn.Width = new GridLength(0, GridUnitType.Pixel);
                    settings.IsAdministrator = true;
                    CountryDataGrid.IsReadOnly = false;
                    MyTimer timer = new MyTimer(1800, TimerAction);
                    timer.Start(); // Запускаем таймер

                    takeAdministratorRules.IsChecked = true;

                    ftpUsername_textBox.Width = 271;
                    ftpPassword_label.Visibility = Visibility.Visible;
                    ftpURL_textBox.IsReadOnly = false;
                    ftpUsername_textBox.IsReadOnly = false;
                }
                else
                {
                    MessageBox.Show("Пароль введён неверно!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    AdministratingStatus_label.Content = "вы не обладаете правами Администратора";
                    AdministratingStatus_label.Foreground = new SolidColorBrush(Colors.OrangeRed);
                    hideShowButtonColumn.Width = new GridLength(0, GridUnitType.Star);
                    withoutCountryColumn.Width = new GridLength(0, GridUnitType.Pixel);
                    settings.IsAdministrator = false;
                    CountryDataGrid.IsReadOnly = true;

                    takeAdministratorRules.IsChecked = false;

                    ftpUsername_textBox.Width = 676;
                    ftpPassword_label.Visibility = Visibility.Hidden;
                    ftpURL_textBox.IsReadOnly = true;
                    ftpUsername_textBox.IsReadOnly = true;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void TimerAction()
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Пароль введён неверно!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    AdministratingStatus_label.Content = "вы не обладаете правами Администратора";
                    AdministratingStatus_label.Foreground = new SolidColorBrush(Colors.OrangeRed);
                    hideShowButtonColumn.Width = new GridLength(0, GridUnitType.Star);
                    withoutCountryColumn.Width = new GridLength(0, GridUnitType.Pixel);
                    settings.IsAdministrator = false;
                    CountryDataGrid.IsReadOnly = true;

                    takeAdministratorRules.IsChecked = false;

                    ftpUsername_textBox.Width = 676;
                    ftpPassword_label.Visibility = Visibility.Hidden;
                    ftpURL_textBox.IsReadOnly = true;
                    ftpUsername_textBox.IsReadOnly = true;
                });
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void AddPriceFolderPath_button_Click(object sender, RoutedEventArgs e)  //Добавление пути к прайсу
        {
            try
            {
                pathFolderController.SelectedFolderPathToTextBox(PriceFolderPath_textBox);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void DeletePriceFolderPath_button_Click(object sender, RoutedEventArgs e) //Удаление пути к прайсу
        {
            try
            {
                PriceFolderPath_textBox.Text = "D:\\";
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void ExcelImportFolderPath_button_Click(object sender, RoutedEventArgs e) //Добавление пути к Excel
        {
            try
            {
                pathFolderController.SelectedFolderPathToTextBox(ExcelImportFolderPath_textBox);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void DeleteExcelImportFolderPath_button_Click(object sender, RoutedEventArgs e) //Удаление пути к Excel
        {
            try
            {
                ExcelImportFolderPath_textBox.Text = "D:\\";
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void CalcImportFolderPath_button_Click(object sender, RoutedEventArgs e) //Добавление пути к расчётке
        {
            try
            {
                pathFolderController.SelectedFolderPathToTextBox(CalcImportFolderPath_textBox);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void DeleteCalcImportFolderPath_button_Click(object sender, RoutedEventArgs e) //Удаление пути к расчётке
        {
            try
            {
                CalcImportFolderPath_textBox.Text = "D:\\";
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                //Общие настройки
                settings.Theme = theme_comboBox.SelectedIndex;
                //settings.IsNotificationsWithSound = IsNotificationsWithSound_checkBox.IsChecked;
                //settings.CheckingIntervalFromMail = Convert.ToDouble(CheckingIntervalFromMail_textBox.Text);
                settings.PriceFolderPath = PriceFolderPath_textBox.Text;

                //Вывод данных в Excel
                settings.ExcelTitleColor = (ColorItem)ExcelTitleColors_comboBox.SelectedItem;
                settings.ExcelChapterColor = (ColorItem)ExcelChapterColors_comboBox.SelectedItem;
                settings.ExcelDataColor = (ColorItem)ExcelDataColors_comboBox.SelectedItem;
                settings.ExcelNotesColor = (ColorItem)ExcelNotesColors_comboBox.SelectedItem;
                settings.ExcelPhotoBackgroundColor = (ColorItem)ExcelPhotoBackgroundColors_comboBox.SelectedItem;
                settings.ExcelNumberColor = (ColorItem)ExcelNumberColors_comboBox.SelectedItem;
                settings.IsInsertExcelPicture = IsInsertExcelPicture_textBox.IsChecked;
                settings.IsInsertExcelCategory = (bool)IsInsertExcelCategory_checkBox.IsChecked;
                settings.isDepartmentRequestExportWithCalc = IsInserDepartmentRequestWithCalc_textBox.IsChecked;
                settings.ExcelPhotoWidth = Convert.ToInt32(ExcelImageWidth.Text);
                settings.ExcelPhotoHeight = Convert.ToInt32(ExcelImageHeight.Text);
                settings.FullCostType = FullCostType_comboBox.Text;

                //Пути сохранения
                settings.ExcelFolderPath = ExcelImportFolderPath_textBox.Text;
                settings.CalcFolderPath = CalcImportFolderPath_textBox.Text;

                mainWindow.settings = settings;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void SaveCountries_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CountryManager.Instance.priceManager.lastUpdated = DateTime.Now;
                IFileImporter fileImporter = new FileImporter_Services();
                fileImporter.ExportCountriesToFTP(settings);
                lastDataUpdate.Content = "ПОСЛЕДНЕЕ ОБНОВЛЕНИЕ: " + CountryManager.Instance.priceManager.lastUpdated.ToString();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void CountryDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                if (e.Column.Header.ToString() == "Коэффициент" || e.Column.Header.ToString() == "Скидка, %")
                {
                    string newTex = ((TextBox)e.EditingElement).Text;


                    if (string.IsNullOrWhiteSpace(newTex)) //Если текст пустой
                    {
                        ((TextBox)e.EditingElement).Text = "0";
                    }
                    else
                    {
                        bool oneSymbol = false; //Флаг для отслеживания наличия точки
                        string validNumber = ""; //Строка для хранения валидного числа

                        for (int i = 0; i < newTex.Length; i++)
                        {
                            char currentChar = newTex[i];

                            if (char.IsDigit(currentChar)) //Если символ цифра
                            {
                                validNumber += currentChar; //Добавление символа
                            }
                            else if (currentChar == '.' && !oneSymbol) //Если символ точка и её ещё не было
                            {
                                validNumber += currentChar; //Добавление точки
                                oneSymbol = true;
                            }
                        }

                        // Если число пустое или состоит только из точки
                        if (string.IsNullOrEmpty(validNumber) || validNumber == ".")
                        {
                            ((TextBox)e.EditingElement).Text = "0";
                        }
                        else
                        {
                            ((TextBox)e.EditingElement).Text = validNumber;
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

        private void HandleDeleteAction()
        {
            try
            {
                Country selectedItem = (Country)CountryDataGrid.SelectedItem;
                if (selectedItem != null)
                {
                    MessageBoxResult res = MessageBox.Show("Вы точно хотите удалить выбранного поставщика?", "", MessageBoxButton.YesNo);
                    if (res == MessageBoxResult.Yes)
                    {
                        CountryManager.Instance.priceManager.countries.Remove(selectedItem);
                        ManufacturerDataGrid.ItemsSource = null;
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

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Delete)
                {
                    if (CountryDataGrid.SelectedIndex < (CountryDataGrid.Items.Count - 1))
                    {
                        if (MyTabControl.SelectedItem is TabItem selectedTab)
                        {
                            if (selectedTab.Header.ToString() == "Менеджер цен")
                            {
                                if (!settings.IsAdministrator)
                                {
                                    e.Handled = true;
                                }
                                else
                                {
                                    if (isCountryDataGridSelected)
                                    {
                                        HandleDeleteAction();
                                    }
                                    else if (isManufacturerWithCountrySelected)
                                    {
                                        ManufacturerDataGrid_MouseDoubleClick(sender, new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left));
                                    }
                                    e.Handled = true;
                                }
                            }
                        }
                    }
                }

                if (e.Key == Key.Escape)
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

        private void fullProductNameView_checkBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var column in mainWindow.dataBaseGrid.Columns)
                {
                    if (column.Header.ToString() == "Наименование на английском" || column.Header.ToString() == "Ед. измерения на английском") // Укажите заголовок или индекс колонки
                    {
                        column.Visibility = Visibility.Visible; // Скрываем колонку
                    }
                    if (column.Header.ToString() == "Наименование" || column.Header.ToString() == "Ед. измерения") // Укажите заголовок или индекс колонки
                    {
                        column.Visibility = Visibility.Visible; // Скрываем колонку
                    }
                }
                foreach (var column in mainWindow.CalcDataGrid.Columns)
                {
                    if (column.Header.ToString() == "Наименование на английском" || column.Header.ToString() == "Ед. измерения на английском") // Укажите заголовок или индекс колонки
                    {
                        column.Visibility = Visibility.Visible; // Скрываем колонку
                    }
                    if (column.Header.ToString() == "Наименование" || column.Header.ToString() == "Ед. измерения") // Укажите заголовок или индекс колонки
                    {
                        column.Visibility = Visibility.Visible; // Скрываем колонку
                    }
                }

                mainWindow.isFullProductNames = true;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void fullProductNameView_checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var column in mainWindow.dataBaseGrid.Columns)
                {
                    if (settings.isEnglishNameVisible)
                    {
                        if (column.Header.ToString() == "Наименование на английском" || column.Header.ToString() == "Ед. измерения на английском") // Укажите заголовок или индекс колонки
                        {
                            column.Visibility = Visibility.Visible; // Скрываем колонку
                        }
                        if (column.Header.ToString() == "Наименование" || column.Header.ToString() == "Ед. измерения") // Укажите заголовок или индекс колонки
                        {
                            column.Visibility = Visibility.Collapsed; // Скрываем колонку
                        }
                    }
                    else
                    {
                        if (column.Header.ToString() == "Наименование на английском" || column.Header.ToString() == "Ед. измерения на английском") // Укажите заголовок или индекс колонки
                        {
                            column.Visibility = Visibility.Collapsed; // Скрываем колонку
                        }
                        if (column.Header.ToString() == "Наименование" || column.Header.ToString() == "Ед. измерения") // Укажите заголовок или индекс колонки
                        {
                            column.Visibility = Visibility.Visible; // Скрываем колонку
                        }
                    }
                }

                foreach (var column in mainWindow.CalcDataGrid.Columns)
                {
                    if (settings.isEnglishNameVisible)
                    {
                        if (column.Header.ToString() == "Наименование на английском" || column.Header.ToString() == "Ед. измерения на английском") // Укажите заголовок или индекс колонки
                        {
                            column.Visibility = Visibility.Visible; // Скрываем колонку
                        }
                        if (column.Header.ToString() == "Наименование" || column.Header.ToString() == "Ед. измерения") // Укажите заголовок или индекс колонки
                        {
                            column.Visibility = Visibility.Collapsed; // Скрываем колонку
                        }
                    }
                    else
                    {
                        if (column.Header.ToString() == "Наименование на английском" || column.Header.ToString() == "Ед. измерения на английском") // Укажите заголовок или индекс колонки
                        {
                            column.Visibility = Visibility.Collapsed; // Скрываем колонку
                        }
                        if (column.Header.ToString() == "Наименование" || column.Header.ToString() == "Ед. измерения") // Укажите заголовок или индекс колонки
                        {
                            column.Visibility = Visibility.Visible; // Скрываем колонку
                        }
                    }
                }

                mainWindow.isFullProductNames = false;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void displayIdOnCalc_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var column in mainWindow.CalcDataGrid.Columns)
                {
                    if (column.Header.ToString() == "ID")
                    {
                        column.Visibility = Visibility.Visible;
                    }
                    settings.isIdOnCalcVisible = true;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void displayIdOnCalc_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var column in mainWindow.CalcDataGrid.Columns)
                {
                    if (column.Header.ToString() == "ID")
                    {
                        column.Visibility = Visibility.Collapsed;
                    }
                    settings.isIdOnCalcVisible = false;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void checkMail_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string mailTo = mail_textBox.Text;
                if (!IsValidEmail(mailTo))
                {
                    MessageBox.Show("Введён неверный формат почты.");
                    return;
                }
                string mailFrom = "dok_koks@mail.ru";
                string pass = "TKZB28r34gSTNVmY7DeW";
                string subject = "Подтверждение почты";
                string code = GenerateAlphaNumericCode(6);
                string text = $"Ваш код подтверждения: {code}";

                IFileImporter importer = new FileImporter_Services();

                if (importer.SendEmail(mailFrom, pass, mailTo, subject, text))
                {
                    WriteMailCodePage writeMailCodePage = new WriteMailCodePage(code, mailTo);
                    writeMailCodePage.Owner = this;
                    writeMailCodePage.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        static string GenerateAlphaNumericCode(int length)
        {
            try
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                Random random = new Random();
                return new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return null;
            }
        }

        static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return false;
            }
        }

        private void exportDataBaseOnFtp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (settings.IsAdministrator)
                {
                    ProgressBarPage progressBarPage = new ProgressBarPage(mainWindow, "exportDataBaseOnFTP");
                    progressBarPage.ShowDialog();

                    string date = DateTime.Now.ToString("dd.MM.yyyy");
                    LastDateUpdatePrice_label.Content = $"Последняя версия прайса: {date}";
                    string lastModifyPriceUser = "Аноним";
                    mainWindow.LastPriceUpdateDate_Label.Content = $"Последнее обновление прайса на сервере: {date} - {lastModifyPriceUser}";
                }
                else
                {
                    MessageBox.Show("Для этого действия необходимо обладать правами Администратора.", "Недостаточно прав", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Log_Repository log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

               
            }


        
        }

        private void exportTemplatesOnFtp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (settings.IsAdministrator)
                {
                    ProgressBarPage progressBarPage = new ProgressBarPage(mainWindow, "exportTemplatesOnFTP");
                    progressBarPage.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Для этого действия необходимо обладать правами Администратора.", "Недостаточно прав", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }

        }

        private void takeAdministratorRules_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                AdministratingStatus_label.Content = "вы Администратор";
                AdministratingStatus_label.Foreground = new SolidColorBrush(Colors.MediumSeaGreen);
                hideShowButtonColumn.Width = new GridLength(30, GridUnitType.Pixel);
                withoutCountryColumn.Width = new GridLength(0, GridUnitType.Pixel);
                settings.IsAdministrator = true;
                CountryDataGrid.IsReadOnly = false;

                AdministratorPassword_passwordBox.Password = string.Empty;

                ftpUsername_textBox.Width = 271;
                ftpPassword_label.Visibility = Visibility.Visible;
                ftpURL_textBox.IsReadOnly = false;
                ftpUsername_textBox.IsReadOnly = false;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void takeAdministratorRules_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                AdministratingStatus_label.Content = "вы не обладаете правами Администратора";
                AdministratingStatus_label.Foreground = new SolidColorBrush(Colors.OrangeRed);
                hideShowButtonColumn.Width = new GridLength(0, GridUnitType.Star);
                withoutCountryColumn.Width = new GridLength(0, GridUnitType.Pixel);
                settings.IsAdministrator = false;
                CountryDataGrid.IsReadOnly = true;

                AdministratorPassword_passwordBox.Password = string.Empty;

                ftpUsername_textBox.Width = 676;
                ftpPassword_label.Visibility = Visibility.Hidden;
                ftpURL_textBox.IsReadOnly = true;
                ftpUsername_textBox.IsReadOnly = true;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void saveFTPSettings_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(ftpURL_textBox.Text))
                {
                    settings.url_praise = ftpURL_textBox.Text;
                }
                if (!string.IsNullOrEmpty(ftpUsername_textBox.Text))
                {
                    settings.ftpUsername = ftpUsername_textBox.Text;
                }
                if (!string.IsNullOrEmpty(ftpPassword_passwordBox.Text))
                {
                    settings.ftpPassword = ftpPassword_passwordBox.Text;
                }

                ftpURL_textBox.Text = string.Empty;
                ftpUsername_textBox.Text = string.Empty;
                ftpPassword_passwordBox.Text = string.Empty;

                MessageBox.Show("Данные успешно сохранены");
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

        private void MyTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.OriginalSource is TabControl)
                {
                    TabItem tabItem = ((TabControl)sender).SelectedItem as TabItem;
                    if (tabItem != null)
                    {
                        switch (tabItem.Header)
                        {
                            case "Общие":
                                {
                                    mainSettingsPlaceWidth.Width = new GridLength(80, GridUnitType.Star);
                                    break;
                                }
                            case "Менеджер цен":
                            case "Пути сохранения":
                                {
                                    mainSettingsPlaceWidth.Width = new GridLength(855, GridUnitType.Star);
                                    break;
                                }
                            case "Вывод данных":
                            case "Для разработчика":
                                {
                                    mainSettingsPlaceWidth.Width = new GridLength(55, GridUnitType.Star);
                                    break;
                                }
                        }
                    }

                    exit_button.Focus();
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void IsInserDepartmentRequestWithCalc_textBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (mainWindow.isDepartmentRequesComplete)
                {
                    mainWindow.openDepartmentRequesPage_button.Background = new SolidColorBrush(Colors.MediumSeaGreen);
                }
                else
                {
                    mainWindow.openDepartmentRequesPage_button.Background = new SolidColorBrush(Colors.Coral);
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void IsInserDepartmentRequestWithCalc_textBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                mainWindow.openDepartmentRequesPage_button.Background = new SolidColorBrush(Colors.Gray);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void ManufacturerDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                isCountryDataGridSelected = false;
                isManufacturerWithCountrySelected = true;
                isManufacturerWithoutSelected = false;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void WithoutCountryManufacturersDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                isCountryDataGridSelected = false;
                isManufacturerWithCountrySelected = false;
                isManufacturerWithoutSelected = true;
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
