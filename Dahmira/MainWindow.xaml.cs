using Dahmira.Interfaces;
using Dahmira.Models;
using Dahmira.Pages;
using Dahmira.Services;
using Dahmira.Services.Actions;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Text.Json;
using Dahmira_DB.DAL;
using Dahmira_Log.DAL;
using Dahmira_DB.DAL.Repository;
using Material = Dahmira_DB.DAL.Model.Material;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Windows.Controls.Primitives;
using Path = System.IO.Path;
using HealthPassport.Services;
using HealthPassport.Interfaces;
using System.Text;
using System;
using System.Windows.Media.Media3D;
using System.Windows.Documents;
using Dahmira_Log.DAL.Repository;

namespace Dahmira
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public SettingsParameters settings { get; set; } = new SettingsParameters();

        private IProductImageUpdating ImageUpdater;
        private ICalcController CalcController;
        private IDbController DbController;
        private ByteArrayToImageSourceConverter_Services converter;
        private IFileImporter fileImporter;
        private IShaderEffects shaderEffectsService;
        public bool isCalcSaved;
        public ObservableCollection<Dependency> dependencies;

        Material_Repository repository;
        public List<string> ComboBoxValues { get; set; }

        int oldCurrentProductIndex;

        public ObservableCollection<Material> dbItems;
        public ObservableCollection<CalcProduct> calcItems;

        public bool isAddtoDependency;
        private CalcProduct selectItemForDependencies;
        public bool isCalcOpened;

        public HashSet<Material> materialForDBAdding;
        public HashSet<Material> materialForDBUpdating;
        public HashSet<Material> materialForDBDeleting;

        public bool isCalculationNeed;
        public bool isDependencySelected;
        public bool isFullProductNames;
        public bool isAddingSecondDependencyPosition;
        public bool isAddingFirstDependencyPosition;

        private Material MaterialBeginEdit;
        private CalcProduct CalcProductBeginEdit;
        private Dependency DependencyBeginEdit;

        public Stack<IAction> actions;

        public DepartmentRequest lastDepartmentRequest;
        public bool isDepartmentRequesComplete;

        public bool isSortBDSave; //Флаг, если true - была сортировка бд и необходимо пресохранить всю бд.

        public Action<string, int>? OnProgressUpdate;


        public MainWindow()
        {
            InitializeComponent();
        }


        //Прогрузка в фоне и прогресс
        public async Task InitializeWithProgress()
        {
            await Task.Delay(200);
            OnProgressUpdate?.Invoke("🧬 Инициализация загрузочного контекста приложения…", 1);
            await Task.Delay(150);
            OnProgressUpdate?.Invoke("🧵 Создание основных потоков исполнения…", 2);
            await Task.Delay(250);
            OnProgressUpdate?.Invoke("🧱 Резервирование памяти под статические блоки данных…", 3);
            await Task.Delay(150);
            OnProgressUpdate?.Invoke("📦 Импорт зависимостей и подключение сборок…", 4);
            await Task.Delay(350);
            OnProgressUpdate?.Invoke("⛓️ Привязка интерфейсов к реализациям…", 5);
            await Task.Delay(150);
            OnProgressUpdate?.Invoke("🧪 Валидация конфигурационных параметров…", 6);
            await Task.Delay(250);
            OnProgressUpdate?.Invoke("🧱 Инициализация слоёв DAL, BLL, UI…", 7);
            await Task.Delay(150);
            OnProgressUpdate?.Invoke("🧬 Проверка совместимости версий компонентов…", 8);
            await Task.Delay(250);
            OnProgressUpdate?.Invoke("🔌 Инициализация сторонних интеграций и API-клиентов…", 9);
            await Task.Delay(150);
            OnProgressUpdate?.Invoke("🧾 Проверка прав доступа и политик безопасности…", 10);
            await Task.Delay(350);

            //Подгружаем логи
            OnProgressUpdate?.Invoke("Проверка БД с логами…", 12);
            await Task.Delay(350);
            try
            {
                Dahmira_Log.DAL.ApplicationContext.EnsureLogDbCreated();
                OnProgressUpdate?.Invoke("Логи успешно загружены…", 14);
                await Task.Delay(350);

                Log_Repository logs = new Log_Repository();
                OnProgressUpdate?.Invoke("Очищаем старые логи…", 15);
                await Task.Delay(300);
                logs.DeleteOld();
            }
            catch
            {
                OnProgressUpdate?.Invoke("Не удалось создать БД с логами…", 14);
                await Task.Delay(500);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }


            OnProgressUpdate?.Invoke("Инициализация переменных...", 17);
            await Task.Delay(600);
            try
            {
                ImageUpdater = new ProductImageUpdating_Services();//Для работы с загрузкой/удалением/сохранением картинки в файл/буфер
                CalcController = new CalcController_Services();//Для работы с обновлением/добавлением в расчётку
                DbController = new DbController_Service();//Для работы с бд
                converter = new ByteArrayToImageSourceConverter_Services(); //???Для Конвертации изображения в массив байтов и обратно???
                fileImporter = new FileImporter_Services(); //Для импорта изображений
                shaderEffectsService = new ShaderEffects_Service();
                isCalcSaved = true; //Указывает на то сохранена ли сейчас расчётка
                dependencies = new ObservableCollection<Dependency>(); //Зависимости для товара

                repository = new Material_Repository(); //Для работы с БД
                ComboBoxValues = new List<string> { "*", "+", "-", "/" };

                oldCurrentProductIndex = 0; //Прошлый выбранный элемент в dataBaseGrid

                dbItems = new(); //Элементы в БД
                calcItems = new ObservableCollection<CalcProduct>(); //Элементы в расчётке
                

                isAddtoDependency = false; //Указывает на то идёт ли сейчас добавление в расчётку

                isCalcOpened = false;

                materialForDBAdding = new HashSet<Material>();
                materialForDBUpdating = new HashSet<Material>();
                materialForDBDeleting = new HashSet<Material>();

                isCalculationNeed = true;
                isDependencySelected = false;
                isFullProductNames = false;
                isAddingSecondDependencyPosition = false;
                isAddingFirstDependencyPosition = false;

                MaterialBeginEdit = null;
                CalcProductBeginEdit = null;

                actions = new Stack<IAction>();

                lastDepartmentRequest = new();
                isDepartmentRequesComplete = false;


                isSortBDSave = false; //Флаг, если true - была сортировка бд и необходимо пресохранить всю бд.
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                OnProgressUpdate?.Invoke("Ошибка: " + ex.Message, 17);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";


                return;
            }


            OnProgressUpdate?.Invoke("Загрузка конфигурации...", 25);
            await Task.Delay(500);
            try
            {
                //dataBaseGrid.ItemsSource = dbItems;
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                //Указываем в реестре, что формат dah открывается через программу Dahmira + указываем путь программы для дальнейшего запуска файла в реестре
                string extension = "dah"; // Укажите ваше расширение
                string progId = "Dahmira";
                string applicationPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                FileAssociationHelper.RegisterFileAssociation(extension, progId, applicationPath);

                fileImporter.ImportSettingsFromFile(this);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                OnProgressUpdate?.Invoke("Ошибка: " + ex.Message, 25);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";

                return;
            }


            OnProgressUpdate?.Invoke("Подключение к FTP...", 40);
            await Task.Delay(500);
            try
            {
                //Отображение даты последнего обновления прайса на сервере
                DateTime lastModifyPriceDate = fileImporter.GetFileLastModified(settings, "/Dahmira/data_price_test/db/Dahmira_DB_beta.bak");
                string lastModifyPriceUser = "Аноним";

                if (lastModifyPriceDate != DateTime.MinValue)
                {
                    LastPriceUpdateDate_Label.Content = $"Последнее обновление прайса на сервере: {lastModifyPriceDate.ToString("dd.MM.yyyy")} - {lastModifyPriceUser}";
                }
                else
                {
                    LastPriceUpdateDate_Label.Content = "Невозможно узнать данные, из-за отсутствия соединения с сервером";
                }

                calcItems.Add(new CalcProduct { Count = settings.FullCostType, TotalCost = 0, ID = -50 });

                this.Title = $"Dahmira       {settings.Price}";
                this.WindowState = WindowState.Maximized; // Разворачиваем окно на весь экран

                fileImporter.ImportCountriesFromFTP(settings);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Warning", new StackTrace(), "noneUser", ex);

                OnProgressUpdate?.Invoke("Ошибка: " + ex.Message, 40);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";

                return;
            }


            OnProgressUpdate?.Invoke("Подключение к базе данных...", 55);
            await Task.Delay(500);
            try
            {
                ConnectionString_Global.Value = settings.Price;

                allCountries_comboBox.ItemsSource = CountryManager.Instance.priceManager.countries;
                CalcDataGrid.ItemsSource = calcItems;
                DependencyDataGrid.ItemsSource = dependencies;
                isCalcSaved = true;

                if (settings.isDepartmentRequestExportWithCalc == false)
                {
                    openDepartmentRequesPage_button.Background = new SolidColorBrush(Colors.Gray);
                }


                if (!File.Exists(ConnectionString_Global.Value))
                {
                    MessageBox.Show("Указанная База Данных не была найдена. Приложение будет открыто без Базы Данных");
                    this.Title += " (Прайс не найден)";
                }
                else
                {
                    try
                    {
                        dbItems = repository.Get_AllMaterials();
                    }
                    catch (Exception ex)
                    {
                        var log = new Log_Repository();
                        log.Add("Error", new StackTrace(), "noneUser", ex);

                        MessageBox.Show($"Ошибка при чтении данных из БД, возможно вы используете старую БД (проверьте миграции и поля): {ex.Message}");
                        dbItems = new ObservableCollection<Material>();
                    }

                    dataBaseGrid.ItemsSource = dbItems;

                    productsCount_label.Content = "из " + dataBaseGrid.Items.Count.ToString(); //Отображение количества товаров

                    List<string> firstColumnValues = dataBaseGrid.ItemsSource
                                                     .Cast<Material>()
                                                     .Select(item => item.Manufacturer)
                                                     .Distinct()
                                                     .ToList();
                    foreach (string item in firstColumnValues)
                    {
                        CountryManager.Instance.allManufacturers.Add(new Manufacturer { name = item });
                    }

                    //Добавление ItemSource компонентам
                    Manufacturer_comboBox.ItemsSource = CountryManager.Instance.allManufacturers;
                    ProductName_comboBox.ItemsSource = dbItems;
                    List<string> types = dataBaseGrid.ItemsSource.Cast<Material>().Select(i => i.Type).Distinct().ToList();
                    Type_comboBox.ItemsSource = types;
                    Article_comboBox.ItemsSource = dbItems;
                    Cost_comboBox.ItemsSource = dbItems;


                    DbController.ValidateDb(dbItems);
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                OnProgressUpdate?.Invoke("Ошибка: " + ex.Message, 55);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";


                return;
            }





            OnProgressUpdate?.Invoke("Инициализация интерфейса...", 80);
            await Task.Delay(500);
            try
            {
                DataContext = this;
                if (settings.isEnglishNameVisible)
                {
                    toggleButton.IsChecked = true;
                }

                //Создание анимации для уведомления об отсутствии расчёта
                DoubleAnimation moveAnimation = new DoubleAnimation
                {
                    From = 0, //Начальная позиция (текущая позиция)
                    To = 150, //Конечная позиция (1000 пикселей вправо)
                    Duration = new Duration(TimeSpan.FromSeconds(5)), //Длительность анимации
                    AutoReverse = true, //Включаем автоматическое обратное движение
                    RepeatBehavior = RepeatBehavior.Forever //Повторяем бесконечно
                };

                labelTransform.BeginAnimation(TranslateTransform.XProperty, moveAnimation);

            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                OnProgressUpdate?.Invoke("Ошибка: " + ex.Message, 80);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";


                return;
            }






            OnProgressUpdate?.Invoke("Готово!", 100);
            await Task.Delay(500);
        }


        private void addGrid_Button_Click(object sender, RoutedEventArgs e) //Смена функционала меню
        {
            try
            {
                addGrid.Visibility = Visibility.Visible;
                searchGrid.Visibility = Visibility.Hidden;
                searchGrid_Button.Background = new SolidColorBrush(Colors.LightGray);
                addGrid_Button.Background = new SolidColorBrush(Colors.MediumSeaGreen);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                OnProgressUpdate?.Invoke("Ошибка: " + ex.Message, 80);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";



            }
        }

        private void searchGrid_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                searchGrid.Visibility = Visibility.Visible;
                addGrid.Visibility = Visibility.Hidden;
                searchGrid_Button.Background = new SolidColorBrush(Colors.Coral);
                addGrid_Button.Background = new SolidColorBrush(Colors.LightGray);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                OnProgressUpdate?.Invoke("Ошибка: " + ex.Message, 80);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void uploadFromFile_button_Click(object sender, RoutedEventArgs e) //Загрузка картинки из файла
        {
            try
            {
                bool imageIsEdit = ImageUpdater.UploadImageFromFile(productImage, this); //Загрузка картинки

                if (imageIsEdit) //Если картинку загрузили
                {
                    PriceInfo_label.Content = "Картинка загружена из файла.";
                    //Изменение картинки в dataBaseGrid
                    var selectedItem = (Material)dataBaseGrid.SelectedItem;
                    Material oldMaterial = repository.CloneMaterial(selectedItem);

                    byte[] photo = converter.ConvertFromComponentImageToByteArray(productImage);
                    selectedItem.Photo = photo;

                    materialForDBUpdating.Add(selectedItem);
                    actions.Push(new UpdateDB_Action(oldMaterial, selectedItem, this));
                    //Изменение фото в расчётке
                    List<CalcProduct> foundedCalcProducts = calcItems.Where(i => i.Article == selectedItem.Article).ToList();
                    if(foundedCalcProducts != null) 
                    {
                        foreach (CalcProduct product in foundedCalcProducts)
                        {
                            product.Photo = photo;
                        }
                        CalcDataGrid.Items.Refresh();
                    }
                }
                else
                {
                    PriceInfo_label.Content = "Картинка не была загружена из файла.";
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void deletePhoto_button_Click(object sender, RoutedEventArgs e) //Удаление картинки
        {
            try
            {
                ImageUpdater.DeleteImage(productImage);
                var selectedItem = (Material)dataBaseGrid.SelectedItem;
                Material oldMaterial = repository.CloneMaterial(selectedItem);
                byte[] photo = converter.ConvertFromFileImageToByteArray(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/without_image_database.png"));
                selectedItem.Photo = photo;

                materialForDBUpdating.Add(selectedItem);
                actions.Push(new UpdateDB_Action(oldMaterial, selectedItem, this));
                //Изменение фото в расчётке
                List<CalcProduct> foundedCalcProducts = calcItems.Where(i => i.Article == selectedItem.Article).ToList();
                if (foundedCalcProducts != null)
                {
                    foreach (CalcProduct product in foundedCalcProducts)
                    {
                        product.Photo = photo;
                    }
                    CalcDataGrid.Items.Refresh();
                }
                PriceInfo_label.Content = "Картинка удалена.";
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void uploadFromClipboard_Click(object sender, RoutedEventArgs e) //Загрузка картинки из буфера
        {
            try
            {
                int imageIsEdit = ImageUpdater.UploadImageFromClipboard(productImage, this); //Загрузка картинки
                switch (imageIsEdit)
                {
                    case 1:
                        {
                            PriceInfo_label.Content = "Картинка загружена из буфера.";
                            //Изменение картинки в dataBaseGrid
                            var selectedItem = (Material)dataBaseGrid.SelectedItem;
                            Material oldMaterial = repository.CloneMaterial(selectedItem);
                            byte[] photo = converter.ConvertFromComponentImageToByteArray(productImage);
                            selectedItem.Photo = photo;

                            materialForDBUpdating.Add(selectedItem);
                            actions.Push(new UpdateDB_Action(oldMaterial, selectedItem, this));
                            //Изменение фото в расчётке
                            List<CalcProduct> foundedCalcProducts = calcItems.Where(i => i.Article == selectedItem.Article).ToList();
                            if (foundedCalcProducts != null)
                            {
                                foreach (CalcProduct product in foundedCalcProducts)
                                {
                                    product.Photo = photo;
                                }
                                CalcDataGrid.Items.Refresh();
                            }
                            break;
                        }
                    case 0:
                        {
                            PriceInfo_label.Content = "Картинка не была загружена из буфера.";
                            break;
                        }
                    case -1:
                        {
                            WarningFlashing("В буфера нет картинки!", WarningBorder, WarningLabel, Colors.OrangeRed, 2.5);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void downloadToClipboard_button_Click(object sender, RoutedEventArgs e) //Сохранение картинки в буфер
        {
            try
            {
                PriceInfo_label.Content = "Картинка загружена в буфер.";
                ImageUpdater.DownloadImageToClipboard(productImage);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void downloadToFile_button_Click(object sender, RoutedEventArgs e) //Сохранение картинки в файл
        {
            try
            {
                bool imageIsDownload = ImageUpdater.DownloadImageToFile(productImage);
                if (imageIsDownload)
                {
                    PriceInfo_label.Content = "Картинка загружена в файл.";
                }
                else
                {
                    PriceInfo_label.Content = "Картинка не была загружена в файл.";
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void uploadFromFileAdd_button_Click(object sender, RoutedEventArgs e) //Загрузка картинки из файла (Меню добавления)
        {
            try
            {
                ImageUpdater.UploadImageFromFile(addedProductImage, this);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }

        private void uploadFromClipboardAdd_button_Click(object sender, RoutedEventArgs e) //Загрузка картинки из буфера обмена (Меню добавления)
        {
            try
            {
                ImageUpdater.UploadImageFromClipboard(addedProductImage, this);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void deleteAdd_button_Click(object sender, RoutedEventArgs e) //Удаление картинки (Меню добавления)
        {
            try
            {
                ImageUpdater.DeleteImage(addedProductImage);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        public static string GetImageHash(byte[] imageBytes)
        {
            try
            {
                using (var sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(imageBytes);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                //if (isCalcOpened)
                //    CalcInfo_label.Content = "Возникла уведомление, зайдите в журнал логов для получения большей информации.";
                //else
                //    PriceInfo_label.Content = "Возникла уведомление, зайдите в журнал логов для получения большей информации.";

                return ""; 
            }
        }

        private void dataBaseGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) //При изменении выделенной строки
        {
            try
            {
                //Отображение индекса выделенной строки
                productNum_textBox.Text = (dataBaseGrid.SelectedIndex + 1).ToString();

                //Отображении информации о текущем выделенном элементе
                Material selectedItem = (Material)dataBaseGrid.SelectedItem; //Получение текущего выделенного элемента
                if (selectedItem != null)
                {
                    ManufacturerInformation_textBox.Text = selectedItem.Manufacturer;
                    if (settings.isEnglishNameVisible)
                    {
                        ProductNameInformation_textBox.Text = selectedItem.EnglishProductName;
                        UnitInformation_textBox.Text = selectedItem.EnglishUnit;
                    }
                    else
                    {
                        ProductNameInformation_textBox.Text = selectedItem.ProductName;
                        UnitInformation_textBox.Text = selectedItem.Unit;
                    }
                    ArticleInformation_textBox.Text = selectedItem.Article;
                    CostInformation_textBox.Text = selectedItem.Cost.ToString();
                    TypeInformation_textBox.Text = selectedItem.Type;

                   
                        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/without_image_database.png");
                        var fileImageBytes = converter.ConvertFromFileImageToByteArray(filePath);
                        //var fileImageBytes = converter.ConvertFromFileImageToByteArray("without_image_database.png");

                        if (BitConverter.ToString(fileImageBytes) == BitConverter.ToString(selectedItem.Photo)) //Если нет фотографии
                        {
                           
                            string path1 = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/without_picture.png");
                            CalcProductImage.Source = new BitmapImage(new Uri(path1, UriKind.Absolute)); //Обнуление картинки, так как у раздела не может быть картинки
                          
                            productImage.Source = new BitmapImage(new Uri(path1));
                        }
                        else
                        {
                            // Вызов метода Convert для преобразования массива байтов в BitmapImage
                            var converter = new ByteArrayToImageSourceConverter_Services();
                            productImage.Source = (BitmapImage)converter.Convert(selectedItem.Photo, typeof(BitmapImage), null, CultureInfo.CurrentCulture);
                        }
                    

                   
                }
                //Отображение меню с поиском и информацией в случае, если она была скрыта
                if (isCalcOpened)
                {
                    priceCalcButton_Click(sender, e);
                }
                else
                {
                    searchGrid_Button_Click(sender, e);
                }

                oldCurrentProductIndex = dataBaseGrid.SelectedIndex;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";

                //MessageBox.Show("Хмм, А вот и ошибочка - 2. Найди меня) " +ex.Message);
            }

        }

        private void productNum_textBox_KeyDown(object sender, KeyEventArgs e) //Переход к строке, введённой в textBox
        {
            try
            {
                if (e.Key == Key.Enter) //Если нажат Enter
                {
                    int index = Convert.ToInt32(productNum_textBox.Text) - 1;

                    if (index > dataBaseGrid.Items.Count) //Если индекс выше допустимого
                    {
                        throw new Exception("Выход за предел количества элементов");
                    }

                    dataBaseGrid.SelectedIndex = index;
                    oldCurrentProductIndex = index;

                    dataBaseGrid.Focus();
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Warning", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникло уведомление, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникло уведомление, зайдите в журнал логов для получения большей информации.";

                dataBaseGrid.SelectedIndex = oldCurrentProductIndex;
                productNum_textBox.Text = (oldCurrentProductIndex + 1).ToString();
                dataBaseGrid.Focus();
            }
        }

        private void addToPrice_button_Click(object sender, RoutedEventArgs e) //Добавление в прайс новой строки
        {
            try
            {
                //проверка на незаполненные поля
                if (string.IsNullOrWhiteSpace(newManufacturer_textBox.Text) ||
                    string.IsNullOrWhiteSpace(newProductName_textBox.Text) ||
                    string.IsNullOrWhiteSpace(newEnglishProductName_textBox.Text) ||
                    string.IsNullOrWhiteSpace(newArticle_textBox.Text) ||
                    string.IsNullOrWhiteSpace(newUnit_textBox.Text) ||
                    string.IsNullOrWhiteSpace(newEnglishUnit_textBox.Text) ||
                    string.IsNullOrWhiteSpace(newCost_textBox.Text))
                {
                    WarningFlashing("Не все поля заполнены!", WarningBorder, WarningLabel, Colors.OrangeRed, 2.5);
                    PriceInfo_label.Content = "Новая позиция в прайс не добавлена. Пользователь заполнил не все поля.";
                    return;
                }

                //Составление нового элемента прайса
                Material newMaterial = new Material
                {
                    Manufacturer = newManufacturer_textBox.Text,
                    ProductName = newProductName_textBox.Text,
                    EnglishProductName = newEnglishProductName_textBox.Text,
                    Article = newArticle_textBox.Text,
                    Unit = newUnit_textBox.Text,
                    EnglishUnit = newEnglishUnit_textBox.Text,
                    Cost = float.Parse(newCost_textBox.Text),
                    LastCostUpdate = DateTime.Now.ToString("dd.MM.yyyy"),
                    Type = newType_textBox.Text
                };

                //if (addedProductImage.Source.ToString() != "pack://application:,,,/resources/images/without_picture.png") //Если картинка изменилась
                if (addedProductImage.Source.ToString() != Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/without_picture.png")) //Если картинка изменилась
                {
                    //Конвертация из Image в массив байтов
                    newMaterial.Photo = converter.ConvertFromComponentImageToByteArray(addedProductImage);
                }
                if(newMaterial.Photo == null)
                {
                    newMaterial.Photo = converter.ConvertFromFileImageToByteArray(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/without_image_database.png"));
                }

                //Добавление
                materialForDBAdding.Add(newMaterial);

                dbItems.Add(newMaterial);
                PriceInfo_label.Content = "Добавлена новая позиция в прайс. Порядковый номер: " + dataBaseGrid.Items.Count.ToString();
                productsCount_label.Content = "из " + dataBaseGrid.Items.Count.ToString();
                actions.Push(new AddDB_Action(newMaterial, this));

                //Проверка на нового производителя
                bool isNewManufacturer = !CountryManager.Instance.allManufacturers
                                         .Any(manufacturerItem => manufacturerItem.name == newMaterial.Manufacturer);
                if (isNewManufacturer)
                {
                    CountryManager.Instance.allManufacturers.Add(new Manufacturer { name = newMaterial.Manufacturer });
                }
                //Очистка строк и картинки от прошлого добавленного элемента
                newManufacturer_textBox.Clear();
                newProductName_textBox.Clear();
                newEnglishProductName_textBox.Clear();
                newArticle_textBox.Clear();
                newUnit_textBox.Clear();
                newEnglishUnit_textBox.Clear();
                newCost_textBox.Clear();
                addedProductImage.Source = new BitmapImage(new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/without_picture.png")));

                //Обновление данных в поиске
                UpdateDataInSearch();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Warning", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникло уведомление, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникло уведомление, зайдите в журнал логов для получения большей информации.";

                WarningFlashing("Формат введённых данных неверен!", WarningBorder, WarningLabel, Colors.OrangeRed, 2.5);
                PriceInfo_label.Content = "Новая позиция в прайс не добавлена. Пользователь ввёл неверный формат данных.";
            }
        }

        private void deleteSelectedProduct_button_Click(object sender, RoutedEventArgs e) //Удаление выделенного элемента прайса
        {
            try
            {
                var res = MessageBox.Show("Вы точно хотите удалить выбранные элементы?", "", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    var items = dataBaseGrid.SelectedItems;
                    //Создаем список для хранения выделенных элементов нужного типа
                    List<Material> selectedItems = new List<Material>();
                    List<Material> materials = new List<Material>();
                    List<int> materialsIndex = new List<int>();

                    for (int i = items.Count - 1; i >= 0; i--)
                    {
                        Material product = items[i] as Material;
                        materialForDBDeleting.Add(product);
                        materials.Add(product);
                        materialsIndex.Add(dbItems.IndexOf(product));
                        dbItems.Remove(product);
                    }
                    PriceInfo_label.Content = "Выбранные товары удалены.";
                    actions.Push(new DeleteDB_Action(materials, materialsIndex, this));


                    //Обновление данных в поиске
                    UpdateDataInSearch();

                    productsCount_label.Content = "из " + dataBaseGrid.Items.Count.ToString();
                }
            }

            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void deleteSelectedManufacturerProducts_button_Click(object sender, RoutedEventArgs e) //Удаление всех товаров выделенного производителя
        {
            try
            {
                var res = MessageBox.Show("Вы точно хотите удалить выбранного производителя?", "", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    Material selectedItem = (Material)dataBaseGrid.SelectedItem;
                    List<Material> materials = new List<Material>();
                    List<int> materialsIndex = new List<int>();
                    //запрос (Если среди производителей бд есть те, что равны с выделенным)
                    IEnumerable<Material> dataForRemove = dataBaseGrid.Items.OfType<Material>()
                                                                          .Where(item => item.Manufacturer == selectedItem.Manufacturer);

                    foreach (var item in dataForRemove.Cast<Material>().ToArray())
                    {
                        materialForDBDeleting.Add(item);
                        materials.Add(item);
                        materialsIndex.Add(dbItems.IndexOf(item));
                        dbItems.Remove(item);

                        PriceInfo_label.Content = "Выбранный производитель удалён.";
                    }
                    actions.Push(new DeleteDB_Action(materials, materialsIndex, this));

                    productsCount_label.Content = "из " + dataBaseGrid.Items.Count.ToString();

                    //Обновление данных в поиске
                    UpdateDataInSearch();

                    productsCount_label.Content = "из " + dataBaseGrid.Items.Count.ToString();
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void Information_textBox_LostFocus(object sender, RoutedEventArgs e) //Обновление информации в выбранном элементе dataBaseGrid при потере фокуса на TextBox
        {
            try
            {
                var selectedItem = (Material)dataBaseGrid.SelectedItem;
                Material oldMaterial = repository.CloneMaterial(selectedItem);

                float newPrice = float.Parse(CostInformation_textBox.Text);

                if (newPrice != selectedItem.Cost)
                {
                    selectedItem.LastCostUpdate = DateTime.Now.ToString("dd.MM.yyyy");
                }

                if (settings.isEnglishNameVisible)
                {
                    selectedItem.EnglishUnit = UnitInformation_textBox.Text;
                    selectedItem.EnglishProductName = ProductNameInformation_textBox.Text;
                }
                else
                {
                    selectedItem.Unit = UnitInformation_textBox.Text;
                    selectedItem.ProductName = ProductNameInformation_textBox.Text;
                }

                selectedItem.Manufacturer = ManufacturerInformation_textBox.Text;
                selectedItem.Article = ArticleInformation_textBox.Text;
                selectedItem.Cost = newPrice; 
                selectedItem.Type = TypeInformation_textBox.Text; 


                materialForDBUpdating.Add(selectedItem);
                actions.Push(new UpdateDB_Action(oldMaterial, selectedItem, this));
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }


        private void simpleSettings_menuItem_Click(object sender, RoutedEventArgs e) //Открытие настроек
        {
            try
            {
                SimpleSettings simpleSettings = new SimpleSettings(settings, this);
                simpleSettings.Owner = this;
                shaderEffectsService.ApplyBlurEffect(this, 10);
                simpleSettings.ShowDialog();
                shaderEffectsService.ClearEffect(this);

                calcItems[calcItems.Count - 1].Count = settings.FullCostType;
                CalcDataGrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }


        //Открытие Шаблонов
        private void templates_menuItem_Click(object sender, RoutedEventArgs e) 
        {
            try
            {
                TemplatesForm templatesForm = new TemplatesForm();
                templatesForm.Owner = this;
                templatesForm.Show();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }

        }


        public void priceCalcButton_Click(object sender, RoutedEventArgs e) //Переход на прайс и расчётку
        {
            try
            {
                if (CalcDataGrid_Grid.Visibility == Visibility.Hidden) //Если открыт прайс
                {
                    if (DependencyDataGrid.SelectedItem != null)
                    {
                        isDependencySelected = true;
                    }
                    priceCalcButton.Content = "РАСЧЁТ->ПРАЙС";

                    CulcGrid_Grid.Visibility = Visibility.Visible;
                    CalcDataGrid_Grid.Visibility = Visibility.Visible;

                    addGrid.Visibility = Visibility.Hidden;
                    searchGrid.Visibility = Visibility.Hidden;
                    DataBaseGrid_Grid.Visibility = Visibility.Hidden;

                    priceCalcButton.Background = new SolidColorBrush(Colors.LightGreen);
                    addGrid_Button.Visibility = Visibility.Hidden;
                    searchGrid_Button.Visibility = Visibility.Hidden;
                    isCalcOpened = true;
                }
                else //Если открыта расчётка
                {
                    isDependencySelected = false;

                    priceCalcButton.Content = "ПРАЙС->РАСЧЁТ";

                    searchGrid.Visibility = Visibility.Visible;
                    DataBaseGrid_Grid.Visibility = Visibility.Visible;

                    CulcGrid_Grid.Visibility = Visibility.Hidden;
                    CalcDataGrid_Grid.Visibility = Visibility.Hidden;

                    priceCalcButton.Background = new SolidColorBrush(Colors.LightPink);
                    addGrid_Button.Visibility = Visibility.Visible;
                    searchGrid_Button.Visibility = Visibility.Visible;
                    searchGrid_Button.Background = new SolidColorBrush(Colors.Coral);
                    addGrid_Button.Background = new SolidColorBrush(Colors.LightGray);

                    isCalcOpened = false;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void dataBaseGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) //Добавление в расчётку при двойном нажатии на элемент 
        {
            try
            {
                if (e.ChangedButton == MouseButton.Right)
                {
                    if (CalcDataGrid.SelectedIndex == -1)
                    {
                        WarningFlashing("Выберите строку!", WarningBorder, WarningLabel, Colors.OrangeRed, 2.5);
                        PriceInfo_label.Content = "Новая позиция в прайс не добавлена. Пользователь не выбрал поле.";
                        return;
                    }

                    bool isAddedWell = CalcController.AddToCalc(dataBaseGrid, CalcDataGrid, this, position: "UnderSelect");
                    if (!isAddedWell)
                    {
                        WarningFlashing("Для началa создайте раздел!", WarningBorder, WarningLabel, Colors.OrangeRed, 2.5);
                        PriceInfo_label.Content = "Новая позиция в прайс не добавлена. Пользователь не добавил раздел.";
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }
        private CancellationTokenSource _cancellationTokenSource;

        public async void WarningFlashing(string content, System.Windows.Controls.Border border, Label label, Color color, double interval)
        {
            try
            {
                // Отменяем предыдущую задачу, если она существует
                _cancellationTokenSource?.Cancel();

                // Создаем новый токен отмены
                _cancellationTokenSource = new CancellationTokenSource();
                var token = _cancellationTokenSource.Token;

                label.Content = content;
                CalcController.ObjectFlashing(border, Colors.Transparent, color, interval);

                try
                {
                    await Task.Delay(Convert.ToInt32(700 * interval * 3), token); // Передаем токен в Task.Delay
                    border.Background = new SolidColorBrush(Colors.Transparent);
                }
                catch (TaskCanceledException)
                { }
                catch (Exception ex)
                {
                    var log = new Log_Repository();
                    log.Add("Error", new StackTrace(), "noneUser", ex);

                    if (isCalcOpened)
                        CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                    else
                        PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";

                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void CalcdataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e) //При смене текущего выделенного элемента расчётки
        {
            try
            {
                CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem;
                if (selectedItem != null)
                {
                    isDependencySelected = false;
                    if (CalcDataGrid.SelectedIndex != calcItems.Count - 1)
                    {
                        if (selectedItem != null)
                        {
                            if (!isAddtoDependency)
                            {
                                if (selectedItem.ID == -1)
                                {

                                    string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/without_picture.png");
                                    CalcProductImage.Source = new BitmapImage(new Uri(path, UriKind.Absolute)); //Обнуление картинки, так как у раздела не может быть картинки

                                    DependencyImage.Visibility = Visibility.Visible;
                                    DependencyDataGrid.Visibility = Visibility.Hidden;
                                    DependencyButtons.Visibility = Visibility.Hidden;
                                    CalcController.Refresh(CalcDataGrid, calcItems);
                                    return;
                                }

                                if (selectedItem.isDependency == true)
                                {
                                    DependencyDataGrid.ItemsSource = selectedItem.dependencies;
                                    DependencyDataGrid.SelectedIndex = -1;
                                    DependencyImage.Visibility = Visibility.Hidden;
                                    DependencyDataGrid.Visibility = Visibility.Visible;
                                    DependencyButtons.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    DependencyImage.Visibility = Visibility.Visible;
                                    DependencyDataGrid.Visibility = Visibility.Hidden;
                                    DependencyButtons.Visibility = Visibility.Hidden;
                                }
                            }
                            else
                            {
                                if (isAddtoDependency && !isAddingSecondDependencyPosition && !isAddingFirstDependencyPosition)
                                {
                                    if (!selectItemForDependencies.dependencies.Any(i => i.ProductId == selectedItem.ID))
                                    {
                                        if (selectedItem != null && selectItemForDependencies != selectedItem && selectedItem.ID > 0) //Если элемент выбран и это не текущий для добавления
                                        {
                                            //Добавление
                                            Dependency newDependency = new Dependency { ProductId = selectedItem.ID, ProductName = selectedItem.ProductName, SelectedType = "*", SecondMultiplier = 1, IsFirstButtonVisible = false };
                                            selectItemForDependencies.dependencies.Add(newDependency);
                                            CalcController.ActivateNeedCalculation(this);
                                            actions.Push(new AddDependency_Action(this, selectItemForDependencies, newDependency));
                                            isCalcSaved = false;
                                            DependencyDataGrid.SelectedItem = newDependency;
                                            CalcInfo_label.Content = "Зависимость успешно добавлена.";
                                            CalcController.Refresh(CalcDataGrid, calcItems);
                                            CalcController.ValidateCalcItem(selectItemForDependencies);
                                            return;
                                        }
                                    }
                                }

                                if (isAddingFirstDependencyPosition)
                                {
                                    isAddingFirstDependencyPosition = false;
                                    isAddtoDependency = false;
                                    selectItemForDependencies.RowColor = CalcController.ColorToHex(Colors.White);
                                    selectItemForDependencies.RowForegroundColor = CalcController.ColorToHex(Colors.Black);

                                    if (!selectItemForDependencies.dependencies.Any(i => i.SecondProductId == selectedItem.ID))
                                    {
                                        Dependency selectedDependency = (Dependency)DependencyDataGrid.SelectedItem;
                                        if (selectedItem != null && selectItemForDependencies != selectedItem && selectedItem.ID > 0 && selectedItem.ID != selectedDependency.SecondProductId)
                                        {
                                            selectedDependency.ProductId = selectedItem.ID;
                                            selectedDependency.ProductName = selectedItem.ProductName;
                                            CalcDataGrid.SelectedItem = selectItemForDependencies;
                                            selectedDependency.IsFirstButtonVisible = false;
                                            selectedDependency.Multiplier = -1;
                                            CalcController.Refresh(CalcDataGrid, calcItems);
                                            CalcController.ValidateCalcItem(selectItemForDependencies);
                                            CalcController.ActivateNeedCalculation(this);
                                            actions.Push(new UpdateDependency_Action(selectItemForDependencies, DependencyBeginEdit, selectedDependency, this));
                                        }
                                        else
                                        {
                                            if (selectedItem == selectItemForDependencies)
                                            {
                                                selectedDependency.IsFirstButtonVisible = true;
                                                CalcDataGrid.SelectedItem = selectedItem;
                                            }
                                            else
                                            {
                                                CalcInfo_label.Content = "Невозможно применить зависимость к данной строке.";
                                                DependencyImage.Visibility = Visibility.Visible;
                                                DependencyDataGrid.Visibility = Visibility.Hidden;
                                                DependencyButtons.Visibility = Visibility.Hidden;
                                            }
                                        }
                                    }

                                    DependencyDataGrid.Items.Refresh();
                                }

                                if (isAddingSecondDependencyPosition)
                                {
                                    isAddingSecondDependencyPosition = false;
                                    isAddtoDependency = false;
                                    selectItemForDependencies.RowColor = CalcController.ColorToHex(Colors.White);
                                    selectItemForDependencies.RowForegroundColor = CalcController.ColorToHex(Colors.Black);

                                    if (!selectItemForDependencies.dependencies.Any(i => i.SecondProductId == selectedItem.ID))
                                    {
                                        Dependency selectedDependency = (Dependency)DependencyDataGrid.SelectedItem;
                                        if (selectedItem != null && selectItemForDependencies != selectedItem && selectedItem.ID > 0 && selectedItem.ID != selectedDependency.ProductId)
                                        {
                                            selectedDependency.SecondProductId = selectedItem.ID;
                                            selectedDependency.SecondProductName = selectedItem.ProductName;
                                            CalcDataGrid.SelectedItem = selectItemForDependencies;
                                            selectedDependency.IsSecondButtonVisible = false;
                                            selectedDependency.SecondMultiplier = -1;
                                            CalcController.Refresh(CalcDataGrid, calcItems);
                                            CalcController.ValidateCalcItem(selectItemForDependencies);
                                            CalcController.ActivateNeedCalculation(this);
                                            actions.Push(new UpdateDependency_Action(selectItemForDependencies, DependencyBeginEdit, selectedDependency, this));
                                        }
                                        else
                                        {
                                            if (selectedItem == selectItemForDependencies)
                                            {
                                                selectedDependency.IsSecondButtonVisible = true;
                                                CalcDataGrid.SelectedItem = selectedItem;
                                            }
                                            else
                                            {
                                                CalcInfo_label.Content = "Невозможно применить зависимость к данной строке.";
                                                DependencyImage.Visibility = Visibility.Visible;
                                                DependencyDataGrid.Visibility = Visibility.Hidden;
                                                DependencyButtons.Visibility = Visibility.Hidden;
                                            }
                                        }
                                    }

                                    DependencyDataGrid.Items.Refresh();
                                }
                            }

                            if (selectedItem.ProductName == string.Empty) //Если нажат раздел
                            {
                                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/without_picture.png");
                                CalcProductImage.Source = new BitmapImage(new Uri(path, UriKind.Absolute)); //Обнуление картинки, так как у раздела не может быть картинки

                            }
                            else
                            {
                                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/without_picture.png");


                                var fileImageBytes = converter.ConvertFromFileImageToByteArray(path);
                                if (BitConverter.ToString(fileImageBytes) == BitConverter.ToString(selectedItem.Photo)) //Если нет фотографии
                                {
                                    CalcProductImage.Source = new BitmapImage(new Uri(path, UriKind.Absolute)); //Обнуление картинки, так как у раздела не может быть картинки
                                }
                                else
                                {
                                    // Вызов метода Convert для преобразования массива байтов в BitmapImage
                                    var converter = new ByteArrayToImageSourceConverter_Services();
                                    CalcProductImage.Source = (BitmapImage)converter.Convert(selectedItem.Photo, typeof(BitmapImage), null, CultureInfo.CurrentCulture);
                                }
                            }
                        }
                    }
                    CalcController.Refresh(CalcDataGrid, calcItems);
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";

                //MessageBox.Show("Хмм, А вот и ошибочка - 3. Найди меня) " + ex.Message);
            }
        }

        private void CalcDeleteSelectedProduct_button_Click(object sender, RoutedEventArgs e) //Удаление выбранного товара из расчётки
        {
            try
            {
                bool isRemoved = false;
                var items = CalcDataGrid.SelectedItems;

                List<CalcProduct> products = new List<CalcProduct>();
                List<int> productsIndex = new List<int>();

                if (items.Count == calcItems.Count)
                {
                    for (int i = 0; i < calcItems.Count - 1; i++)
                    {
                        products.Add(calcItems[i].Clone());
                        products[i].RowColor = calcItems[i].RowColor;
                        products[i].RowForegroundColor = calcItems[i].RowForegroundColor;
                        productsIndex.Add(i);
                    }

                    actions.Push(new DeleteCalc_Action(products, productsIndex, this));

                    calcItems.Clear();
                    calcItems.Add(new CalcProduct { Count = settings.FullCostType, TotalCost = 0, ID = -50 });
                    CalcDataGrid.SelectedItem = calcItems[0];
                    CalcInfo_label.Content = "Все элементы расчёта успешно удалены.";
                    CalcController.ActivateNeedCalculation(this);
                    return;
                }

                List<CalcProduct> selectedItems = new List<CalcProduct>();

                foreach (var item in items)
                {
                    CalcProduct product = item as CalcProduct;

                    if (calcItems.IndexOf(product) != calcItems.Count - 1) //если не последний элемент
                    {
                        if (product.ID < 1) // если удаляется раздел или категория
                        {
                            int index = calcItems.IndexOf(product);
                            //Если скрыто, то удаляется всё содержимое, если нет, то проверяется есть ли внутри элементы. Если элементов нет - удаляется
                            if (product.hideButtonContext == "+")
                            {
                                selectedItems.Add(product);
                                for (int i = index + 1; i < calcItems.Count - 1; i++)
                                {
                                    selectedItems.Add(calcItems[i]);
                                    if ((product.ID == 0 && calcItems[i + 1].ID < 1) || (product.ID == -1 && calcItems[i + 1].ID == -1))
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (index != 0)
                                {
                                    if (product.ID == 0)
                                    {
                                        if (calcItems[index + 1].ID < 1 || calcItems[index - 1].ID > -1)
                                        {
                                            selectedItems.Add(product);
                                        }
                                    }

                                    if (product.ID == -1)
                                    {
                                        selectedItems.Add(product);
                                    }
                                }
                                else
                                {
                                    if (product.ID == -1)
                                    {
                                        MessageBoxResult res = MessageBox.Show("Желаете удалить все категории в расчёте?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Information);

                                        if (res == MessageBoxResult.Yes)
                                        {
                                            selectedItems.AddRange(calcItems.Where(item => item.ID == -1));
                                        }
                                    }

                                    if (product.ID == 0 && calcItems[index + 1].ID < 1)
                                    {
                                        selectedItems.Add(product);
                                    }
                                }
                            }
                        }
                        else
                        {
                            selectedItems.Add(product);
                        }
                    }
                }



                HashSet<CalcProduct> removedItems = new HashSet<CalcProduct>();

                foreach (var calcItem in calcItems)
                {
                    foreach (var item in selectedItems)
                    {
                        foreach (var dependency in calcItem.dependencies)
                        {
                            if (dependency.ProductId == item.ID && !selectedItems.Any(c => c.ID == calcItem.ID) || dependency.SecondProductId == item.ID && !selectedItems.Any(c => c.ID == calcItem.ID))
                            {
                                removedItems.Add(item);
                                break;
                            }
                        }
                    }
                }

                // Удаление элементов вне циклов
                foreach (var item in removedItems)
                {
                    selectedItems.Remove(item);
                }

                foreach (var item in removedItems)
                {
                    CalcController.Refresh(CalcDataGrid, calcItems);

                    foreach (var calcItem in calcItems)
                    {
                        if (calcItem.ID != 0)
                        {
                            calcItem.RowColor = CalcController.ColorToHex(Colors.Transparent);
                            calcItem.RowForegroundColor = CalcController.ColorToHex(Colors.Black);
                            foreach (var dependency in calcItem.dependencies)
                            {
                                if (dependency.ProductId == item.ID || dependency.SecondProductId == item.ID)
                                {
                                    calcItem.RowColor = CalcController.ColorToHex(Colors.LightGray);
                                }
                            }
                        }
                    }

                    MessageBoxResult res = MessageBox.Show($"Вы уверены что хотите удалить товар, находящийся в зависимости:" +
                        $"\nНомер: {item.Num}" +
                        $"\nПроизводитель: {item.Manufacturer}" +
                        $"\nНаименование: {item.ProductName}" +
                        $"\nАртикул: {item.Article}",
                        "Товар находится в зависимости", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (res == MessageBoxResult.Yes)
                    {
                        // Удаляем элемент из calcItem
                        selectedItems.Add(item);
                        isRemoved = true;

                        foreach (var calcItem in calcItems)
                        {
                            Dependency removedDependency = calcItem.dependencies.FirstOrDefault(d => d.ProductId == item.ID);
                            if (removedDependency != null)
                            {
                                removedDependency.IsFirstButtonVisible = true;
                                removedDependency.ProductName = "";
                                removedDependency.ProductId = -2;
                                removedDependency.Multiplier = 1;
                            }

                            Dependency removedSecondDependency = calcItem.dependencies.FirstOrDefault(d => d.SecondProductId == item.ID);
                            if (removedSecondDependency != null)
                            {
                                removedSecondDependency.IsSecondButtonVisible = true;
                                removedSecondDependency.SecondProductName = "";
                                removedSecondDependency.SecondProductId = -2;
                                removedSecondDependency.SecondMultiplier = 1;
                            }

                            calcItem.dependencies.Remove(calcItem.dependencies.FirstOrDefault(d => d.ProductName == "" && d.SecondProductName == ""));

                        }

                        CalcController.Refresh(CalcDataGrid, calcItems);
                        CalcController.ValidateCalcItems(calcItems);
                    }
                }

                foreach (var item in selectedItems)
                {
                    products.Add(item);
                    productsIndex.Add(calcItems.IndexOf(item));
                    calcItems.Remove(item);
                    isRemoved = true;
                }

                if (isRemoved)
                {
                    actions.Push(new DeleteCalc_Action(products, productsIndex, this));
                    CalcInfo_label.Content = "Выбранные товары удалены.";
                    isCalcSaved = false;
                    DependencyImage.Visibility = Visibility.Visible;
                    DependencyDataGrid.Visibility = Visibility.Hidden;
                    DependencyButtons.Visibility = Visibility.Hidden;
                    string withoutImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/without_picture.png");
                    CalcProductImage.Source = new BitmapImage(new Uri(withoutImagePath, UriKind.Absolute));
                    CalcController.ActivateNeedCalculation(this);
                }
                else
                {
                    CalcInfo_label.Content = "Выбранные товары не удалены.";
                }

                CalcController.Refresh(CalcDataGrid, calcItems);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }

        }

        private void allCountries_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) //Изменение ценников в зависимости от местных поставщиков выбранной страны
        {
            try
            {
                CalcController.ActivateNeedCalculation(this);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void CalcRefresh_button_Click(object sender, RoutedEventArgs e) //Обновление расчётки
        {
            try
            {
                CalcController.Refresh(CalcDataGrid, calcItems); //Обновление
                if (CalcController.CheckingDifferencesWithDB(CalcDataGrid, this))
                {
                    MessageBox.Show("Соответствие с Прайсом не нарушено.");
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void CalcUploadFromFile_Click(object sender, RoutedEventArgs e) //Загрузка картинки из файла в элемент расчётки
        {
            try
            {
                CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem;
                if (selectedItem != null)
                {
                    CalcProduct oldCalcProduct = selectedItem.Clone();
                    if (selectedItem != null && selectedItem.ProductName != string.Empty)
                    {
                        bool imageIsEdit = ImageUpdater.UploadImageFromFile(CalcProductImage, this); //Загрузка картинки

                        if (imageIsEdit) //Если картинку загрузили
                        {
                            CalcInfo_label.Content = "Картинка загружена из файла.";
                            //Изменение картинки в calcDataGrid
                            int index = CalcDataGrid.SelectedIndex;
                            calcItems[index].Photo = converter.ConvertFromComponentImageToByteArray(CalcProductImage);
                            actions.Push(new UpdateCalc_Action(oldCalcProduct, selectedItem, this));
                            CalcDataGrid.Items.Refresh();
                            isCalcSaved = false;
                        }
                        else
                        {
                            CalcInfo_label.Content = "Картинка не была загружена из файла.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void CalcDeleteImage_Click(object sender, RoutedEventArgs e) //Удаление картинки элемента расчётки
        {
            try
            {
                CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem;
                if (selectedItem != null)
                {
                    CalcProduct oldCalcProduct = selectedItem.Clone();
                    if (selectedItem != null && selectedItem.ProductName != string.Empty)
                    {
                        ImageUpdater.DeleteImage(CalcProductImage);

                        int index = CalcDataGrid.SelectedIndex;
                        calcItems[index].Photo = converter.ConvertFromFileImageToByteArray(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/without_image_database.png"));
                        actions.Push(new UpdateCalc_Action(oldCalcProduct, selectedItem, this));
                        CalcDataGrid.Items.Refresh();
                        isCalcSaved = false;
                        CalcInfo_label.Content = "Картинка удалена.";
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void CalcDownloadToFile_Click(object sender, RoutedEventArgs e) //Сохранение картинки в файл из элемента расчётки
        {
            try
            {
                bool isImageDownload = ImageUpdater.DownloadImageToFile(CalcProductImage);
                if (isImageDownload)
                {
                    CalcInfo_label.Content = "Картинка загружена в файла.";
                }
                else
                {
                    CalcInfo_label.Content = "Картинка не была загружена в файла.";
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void CalcUploadFromClipboard_Click(object sender, RoutedEventArgs e) //Загрузка картинки из буфера в элемент расчётки
        {
            try
            {
                CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem;
                if (selectedItem != null)
                {
                    CalcProduct oldCalcProduct = selectedItem.Clone();
                    if (selectedItem != null && selectedItem.ProductName != string.Empty)
                    {
                        int imageIsEdit = ImageUpdater.UploadImageFromClipboard(CalcProductImage, this); //Загрузка картинки

                        if (imageIsEdit == 1) //Если картинку загрузили
                        {
                            CalcInfo_label.Content = "Картинка загружена из буфера.";
                            //Изменение картинки в dataBaseGrid
                            int index = CalcDataGrid.SelectedIndex;
                            calcItems[index].Photo = converter.ConvertFromComponentImageToByteArray(CalcProductImage);
                            actions.Push(new UpdateCalc_Action(oldCalcProduct, selectedItem, this));
                            CalcDataGrid.Items.Refresh();
                            isCalcSaved = false;
                        }
                        else
                        {
                            CalcInfo_label.Content = "Картинка не была загружена из буфера. В буфере нет картинки.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void CalcDownloadToClipboard_Click(object sender, RoutedEventArgs e) //Сохранение картинки в буфер из элемента расчётки
        {
            try
            {
                CalcInfo_label.Content = "Картинка загружена в буфер.";
                ImageUpdater.DownloadImageToClipboard(CalcProductImage);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }

        }

        private void AddToCalc_button_Click(object sender, RoutedEventArgs e) //Добавление выделенного элемента DAtaBaseGrid в расчётку
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CountProductToAdd_textBox.Text) || Convert.ToInt32(CountProductToAdd_textBox.Text) == 0) //Если количество не указано
                {
                    WarningFlashing("Количество не указано!", WarningBorder, WarningLabel, Colors.OrangeRed, 2.5);
                    PriceInfo_label.Content = "Новая позиция в прайс не добавлена. Пользователь не указал количество.";
                    return;
                }
                string count = CountProductToAdd_textBox.Text; //Получение количества
                bool isAddedWell = CalcController.AddToCalc(dataBaseGrid, CalcDataGrid, this, count); //Добавление
                if (!isAddedWell)
                {
                    WarningFlashing("Для началa создайте раздел!", WarningBorder, WarningLabel, Colors.OrangeRed, 2.5);
                    PriceInfo_label.Content = "Новая позиция в прайс не добавлена. Пользователь не добавил раздел.";
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void AddToCalcUnderSelectedRow_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CountProductToAdd_textBox.Text) || Convert.ToInt32(CountProductToAdd_textBox.Text) == 0) //Если количество не указано
                {
                    WarningFlashing("Количество не указано!", WarningBorder, WarningLabel, Colors.OrangeRed, 2.5);
                    PriceInfo_label.Content = "Новая позиция в прайс не добавлена. Пользователь не указал количество.";
                    return;
                }
                string count = CountProductToAdd_textBox.Text; //Получение количества
                bool isAddedWell = CalcController.AddToCalc(dataBaseGrid, CalcDataGrid, this, count, "UnderSelect"); //Добавление
                if (!isAddedWell)
                {
                    WarningFlashing("Для началa создайте раздел!", WarningBorder, WarningLabel, Colors.OrangeRed, 2.5);
                    PriceInfo_label.Content = "Новая позиция в прайс не добавлена. Пользователь не добавил раздел.";
                }
                else if (CalcDataGrid.SelectedIndex == -1)
                {
                    WarningFlashing("Выберите строку!", WarningBorder, WarningLabel, Colors.OrangeRed, 2.5);
                    PriceInfo_label.Content = "Новая позиция в прайс не добавлена. Пользователь не выбрал строку.";
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }

        }

        private void ReplaceCalc_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CountProductToAdd_textBox.Text) || Convert.ToInt32(CountProductToAdd_textBox.Text) == 0) //Если количество не указано
                {
                    WarningFlashing("Количество не указано!", WarningBorder, WarningLabel, Colors.OrangeRed, 2.5);
                    PriceInfo_label.Content = "Позиция в прайсе не изменена. Пользователь не указал количество.";
                    return;
                }
                string count = CountProductToAdd_textBox.Text; //Получение количества
                bool isAddedWell = CalcController.AddToCalc(dataBaseGrid, CalcDataGrid, this, count, "Replace"); //Замена
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void CalcChapter_button_Click(object sender, RoutedEventArgs e) //Создание раздела
        {
            try
            {
                int selectedIndex = CalcDataGrid.SelectedIndex; //Индекс текущего выделенного элемента
                string chapterName = chapterName_textBox.Text;



                //Создание раздела
                CalcProduct chapter = new CalcProduct
                {
                    ID = 0,
                    Manufacturer = chapterName,
                    Cost = double.NaN,
                    TotalCost = double.NaN,
                    isHidingButton = true,
                    RowColor = CalcController.ColorToHex(Color.FromRgb(223, 242, 253)),
                    RowForegroundColor = CalcController.ColorToHex(Colors.Black)
                };
                if (CalcDataGrid.SelectedItem != null)
                {
                    if (calcItems[selectedIndex].hideButtonContext == "+" && calcItems[selectedIndex].ID == -1)
                    {
                        chapter.isVisible = false;
                    }
                }

                actions.Push(new AddCalc_Action(new List<CalcProduct> { chapter }, this));
                //Добавление
                if (selectedIndex == -1)
                {
                    selectedIndex = calcItems.Count - 1;
                    CalcInfo_label.Content = "Раздел добавлен в начало.";
                    calcItems.Insert(selectedIndex, chapter);
                    CalcDataGrid.SelectedItem = chapter;
                    isCalcSaved = false;
                    return;
                }

                //Если добавление идёт не в конец
                if (CalcDataGrid.SelectedIndex != calcItems.Count - 1)
                {
                    CalcProduct selectedItem = calcItems[selectedIndex];
                    //Если 
                    if (selectedItem.isHidingButton == true && selectedItem.hideButtonContext == "+" && selectedItem.ID == 0)
                    {
                        for (int i = selectedIndex + 1; i < calcItems.Count; i++)
                        {
                            if (calcItems[i].ID < 1)
                            {
                                selectedIndex = i;
                                break;
                            }
                        }
                    }
                    else
                    {
                        selectedIndex++;
                    }
                    CalcInfo_label.Content = $"Раздел добавлен под строкой {selectedIndex}.";
                }
                else
                {
                    for (int i = selectedIndex - 1; i >= 0; i--)
                    {
                        if (calcItems[i].ID == -1 && calcItems[i].isHidingButton && calcItems[i].hideButtonContext == "+")
                        {
                            chapter.isVisible = false;
                            break;
                        }
                    }
                    if (chapter.isVisible)
                    {
                        CalcInfo_label.Content = "Раздел добавлен в конец.";
                    }
                    else
                    {
                        CalcInfo_label.Content = "Раздел добавлен внутрь категории.";
                    }
                }

                calcItems.Insert(selectedIndex, chapter);
                if (chapter.isVisible)
                {
                    CalcDataGrid.SelectedItem = chapter;
                }
                isCalcSaved = false;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void CalcCategory_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isCategoryAdded = false;
                int selectedIndex = CalcDataGrid.SelectedIndex; //Индекс текущего выделенного элемента
                string chapterName = chapterName_textBox.Text;

                //Создание категории
                CalcProduct category = new CalcProduct
                {
                    ID = -1,
                    Manufacturer = chapterName,
                    Cost = double.NaN,
                    TotalCost = double.NaN,
                    isHidingButton = true,
                    RowForegroundColor = CalcController.ColorToHex(Colors.Black),
                    RowColor = CalcController.ColorToHex(Color.FromRgb(254, 241, 230)),
                };

                if (selectedIndex == 0)
                {
                    calcItems.Insert(selectedIndex, category);
                    CalcInfo_label.Content = "Категория добавлена в начало.";
                }
                else if (selectedIndex == -1)
                {
                    selectedIndex = calcItems.Count - 1;
                    if (calcItems.Count == 1)
                    {
                        CalcInfo_label.Content = "Категория добавлена в начало.";
                    }
                    else
                    {
                        CalcInfo_label.Content = "Категория добавлена в конец.";
                    }
                    calcItems.Insert(selectedIndex, category);
                }
                else if (selectedIndex == calcItems.Count - 1) //если выбрано ИТОГО то добавляем как прелпоследнюю строку
                {
                    calcItems.Insert(selectedIndex, category);
                    CalcInfo_label.Content = "Категория добавлена в конец.";
                }
                else if (selectedIndex != calcItems.Count - 2) //если не последняя строчка
                {
                    // если выбран раздел, то добавляем категорию над ним
                    if (calcItems[selectedIndex].ID > 0)
                    {
                        if (calcItems[selectedIndex + 1].ID != 0 && calcItems[selectedIndex + 1].ID != -1 && calcItems[selectedIndex].ID != -1) //если выбран не раздел, и следующим элементом идёт не раздел или категория, то запрещаем добавление
                        {
                            CalcInfo_label.Content = $"Категория не может быть создана, выберите раздел над которым хотите создать категорию.";
                            return;
                        }
                    }
                    else if (calcItems[selectedIndex].ID == -1)
                    {
                        if (calcItems[selectedIndex].hideButtonContext == "+")
                        {
                            for (int i = selectedIndex + 1; i < calcItems.Count - 1; i++)
                            {
                                if (calcItems[i + 1].ID == -1 || i == calcItems.Count - 2)
                                {
                                    selectedIndex = i;
                                    break;
                                }
                            }
                        }
                    }
                    else selectedIndex--;

                    CalcInfo_label.Content = $"Категория добавлена под строкой {selectedIndex}.";
                    calcItems.Insert(selectedIndex + 1, category);
                }
                else
                {
                    CalcInfo_label.Content = $"Категория добавлена под строкой {selectedIndex}.";
                    calcItems.Insert(selectedIndex + 1, category);
                }

                if (calcItems[0].ID != -1)
                {
                    CalcProduct newCategory = category.Clone();
                    newCategory.Manufacturer = "Категория";

                    calcItems.Insert(0, newCategory);
                }

                CalcDataGrid.SelectedItem = category;
                isCalcSaved = false;

                actions.Push(new AddCalc_Action(new List<CalcProduct> { category }, this));
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void CalcToExcel_button_Click(object sender, RoutedEventArgs e) //Экспорт в Excel
        {
            try
            {
                if (calcItems.Any(item => item.ID > 0))
                {
                    if (isCalculationNeed)
                    {
                        WarningFlashing("Для начала произведите расчёт", CalcWarningBorder, CalcWarningLabel, Colors.OrangeRed, 2.5);
                        CalcInfo_label.Content = "Расчёт не сохранён в Excel. Необходимо произвести расчёт.";
                        return;
                    }
                    else if (calcItems.Any(item => item.RowColor == CalcController.ColorToHex(Colors.OrangeRed)))
                    {
                        WarningFlashing("Несоответствие с прайсом", CalcWarningBorder, CalcWarningLabel, Colors.OrangeRed, 2.5);
                        CalcInfo_label.Content = "Расчёт не сохранён в Excel. Присутствует несоответствие с прайсом.";
                        return;
                    }
                    else if (!CalcController.IsCalcValid(calcItems, settings))
                    {
                        WarningFlashing("Исправьте некорректные поля", CalcWarningBorder, CalcWarningLabel, Colors.OrangeRed, 2.5);
                        CalcInfo_label.Content = "Расчёт не сохранён в Excel. Присутствуют ошибки.";
                        return;
                    }
                    else if (calcItems[calcItems.Count - 2].ID == 0 && calcItems[calcItems.Count - 2].ProductName == string.Empty)
                    {
                        WarningFlashing("Раздел находиться в конце", CalcWarningBorder, CalcWarningLabel, Colors.OrangeRed, 2.5);
                        CalcInfo_label.Content = "Расчёт не сохранён в Excel. Раздел находится в конце расчёта.";
                    }
                    else if (settings.isDepartmentRequestExportWithCalc == true && !isDepartmentRequesComplete)
                    {
                        WarningFlashing("Заявка ТО не заполнена", CalcWarningBorder, CalcWarningLabel, Colors.OrangeRed, 2.5);
                        CalcInfo_label.Content = "Расчёт не сохранён в Excel. Заявка Технического отдела не заполнена.";
                    }
                    else
                    {
                        fileImporter.ExportToExcel(this);
                    }
                }
                else
                {
                    WarningFlashing("Расчёт пустой", CalcWarningBorder, CalcWarningLabel, Colors.OrangeRed, 2.5);
                    CalcInfo_label.Content = "Расчёт не сохранён. Необходимо добавить содержимое расчёта.";
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void CalcToNewSheetExcel_button_Click(object sender, RoutedEventArgs e) //Экспорт в Excel как новый лист
        {
            try
            {
                if (calcItems.Any(item => item.ID > 0))
                {
                    if (isCalculationNeed)
                    {
                        WarningFlashing("Для начала произведите расчёт", CalcWarningBorder, CalcWarningLabel, Colors.OrangeRed, 2.5);
                        CalcInfo_label.Content = "Расчёт не сохранён. Необходимо произвести расчёт.";
                        return;
                    }
                    else if (calcItems.Any(item => item.RowColor == CalcController.ColorToHex(Colors.OrangeRed)))
                    {
                        WarningFlashing("Несоответствие с прайсом", CalcWarningBorder, CalcWarningLabel, Colors.OrangeRed, 2.5);
                        CalcInfo_label.Content = "Расчёт не сохранён в Excel. Присутствует несоответствие с прайсом.";
                        return;
                    }
                    else if (!CalcController.IsCalcValid(calcItems, settings))
                    {
                        WarningFlashing("Исправьте некорректные поля", CalcWarningBorder, CalcWarningLabel, Colors.OrangeRed, 2.5);
                        CalcInfo_label.Content = "Расчёт не сохранён в Excel. Присутствуют ошибки.";
                        return;
                    }
                    else if (calcItems[calcItems.Count - 2].ID == 0 && calcItems[calcItems.Count - 2].ProductName == string.Empty)
                    {
                        WarningFlashing("Раздел не может находиться в конце расчёта", CalcWarningBorder, CalcWarningLabel, Colors.OrangeRed, 2.5);
                        CalcInfo_label.Content = "Расчёт не сохранён. Раздел находится в конце расчёта.";
                    }
                    else
                    {
                        fileImporter.ExportToExcelAsNewSheet(this);
                    }
                }
                else
                {
                    WarningFlashing("Расчёт пустой", CalcWarningBorder, CalcWarningLabel, Colors.OrangeRed, 2.5);
                    CalcInfo_label.Content = "Расчёт не сохранён. Необходимо добавить содержимое расчёта.";
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void saveCaalc_menuItem_Click(object sender, RoutedEventArgs e) //Сохранение расчётки
        {
            try
            {
                if (calcItems.Count > 1)
                {
                    if (isCalculationNeed)
                    {
                        WarningFlashing("Для начала произведите расчёт", CalcWarningBorder, CalcWarningLabel, Colors.OrangeRed, 2.5);
                        CalcInfo_label.Content = "Расчёт не сохранён. Необходимо произвести расчёт.";
                        return;
                    }
                    else if (calcItems[calcItems.Count - 2].ID == 0 && calcItems[calcItems.Count - 2].ProductName == string.Empty)
                    {
                        WarningFlashing("Раздел не может находиться в конце расчёта", CalcWarningBorder, CalcWarningLabel, Colors.OrangeRed, 2.5);
                        CalcInfo_label.Content = "Расчёт не сохранён. Раздел находится в конце расчёта.";
                    }
                    else
                    {
                        CalcDataGrid.SelectedItem = null;

                        if (string.IsNullOrEmpty(calcFilePath))
                        {
                            saveCaalcAs_menuItem_Click(sender, e);
                        }
                        else
                        {
                            var options = new JsonSerializerOptions
                            {
                                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                            };

                            string jsonString = JsonSerializer.Serialize(calcItems, options);

                            CalcPath_label.Content = $"Имя файла расчёта: {calcFilePath}";
                            File.WriteAllText(calcFilePath, jsonString);
                            CalcInfo_label.Content = $"Расчёт успешно сохранён по пути: {calcFilePath}";
                        }

                        isCalcSaved = true;
                        isCalculationNeed = false;
                    }
                }
                else
                {
                    WarningFlashing("Расчёт пустой", CalcWarningBorder, CalcWarningLabel, Colors.OrangeRed, 2.5);
                    CalcInfo_label.Content = "Расчёт не сохранён. Необходимо добавить содержимое расчёта.";
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        //Путь открытого файла в расчёте
        public string calcFilePath = string.Empty;

        private void saveCaalcAs_menuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (calcItems.Count > 1)
                {
                    if (isCalculationNeed)
                    {
                        WarningFlashing("Для начала произведите расчёт", CalcWarningBorder, CalcWarningLabel, Colors.OrangeRed, 2.5);
                        CalcInfo_label.Content = "Расчёт не сохранён. Необходимо произвести расчёт.";
                        return;
                    }
                    else if (calcItems[calcItems.Count - 2].ID == 0 && calcItems[calcItems.Count - 2].ProductName == string.Empty)
                    {
                        WarningFlashing("Раздел не может находиться в конце расчёта", CalcWarningBorder, CalcWarningLabel, Colors.OrangeRed, 2.5);
                        CalcInfo_label.Content = "Расчёт не сохранён. Раздел находится в конце расчёта.";
                    }
                    else
                    {
                        fileImporter.ExportCalcToFile(this);
                    }
                }
                else
                {
                    WarningFlashing("Расчёт пустой", CalcWarningBorder, CalcWarningLabel, Colors.OrangeRed, 2.5);
                    CalcInfo_label.Content = "Расчёт не сохранён. Необходимо добавить содержимое расчёта.";
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        //Сохранение расчётки в шаблонах
        public bool saveTemplatesCalc(string patch) 
        {
            try
            {
                if (calcItems.Count > 2)
                {
                    if (isCalculationNeed)
                    {
                        MessageBoxResult res = MessageBox.Show("Для начала произведите расчёт", "Информационное", MessageBoxButton.OK, MessageBoxImage.Information);
                        return false;
                    }
                    else
                    {
                        CalcDataGrid.SelectedItem = null;
                        fileImporter.ExportCalcToTemlates(this, patch);
                        isCalcSaved = true;
                        CalcController.ActivateNeedCalculation(this);

                        return true;
                    }
                }
                else
                {
                    if (calcItems.Count > 1)
                    {
                        MessageBoxResult res = MessageBox.Show("Расчет не может состоять только из одного раздела или категории!", "Нечего сохранять!", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBoxResult res = MessageBox.Show("Сначало создайте расчет!", "Нечего сохранять!", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";

                return false;
            }
        }


        private void openCalc_menuItem_Click(object sender, RoutedEventArgs e) //Открытие расчётки из файла
        {
            try
            {
                CalcDataGrid.SelectedItem = null;
                try
                {
                    if (isCalcSaved == false) //Если расчётка не сохранена
                    {
                        MessageBoxResult res = MessageBox.Show("Сохранить расчёт?", "", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);
                        if (res == MessageBoxResult.Yes)
                        {
                            saveCaalc_menuItem_Click(sender, e);
                            if (!isCalculationNeed)
                            {
                                CalcDataGrid.SelectedItem = null;
                            }
                            return;
                        }
                        if (res == MessageBoxResult.Cancel)
                        {
                            return;
                        }
                    }
                    fileImporter.ImportCalcFromFile(this);
                    isCalcSaved = true;
                    DependencyDataGrid.ItemsSource = dependencies; //Обнуление зависимостей

                    string path1 = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/without_picture.png");
                    CalcProductImage.Source = new BitmapImage(new Uri(path1, UriKind.Absolute)); //Обнуление картинки, так как у раздела не может быть картинки

                    CalcController.ClearBackgroundsColors(this);
                    calcItems[calcItems.Count - 1].Count = settings.FullCostType;
                    CalcController.Refresh(CalcDataGrid, calcItems);
                    isCalculationNeed = true;
                }
                //Если файл невероно формата
                catch (Exception ex)
                {
                    var log = new Log_Repository();
                    log.Add("Warning", new StackTrace(), "noneUser", ex);

                    MessageBox.Show($"Запущен файл, не соответсвующий нужному типу или поврежден.");
                    calcItems.Clear();
                    isCalcSaved = true;
                    DependencyDataGrid.ItemsSource = dependencies; //Обнуление зависимостей

                    string path1 = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/without_picture.png");
                    CalcProductImage.Source = new BitmapImage(new Uri(path1, UriKind.Absolute)); //Обнуление картинки, так как у раздела не может быть картинки
                    CalcController.ClearBackgroundsColors(this);
                    calcItems.Add(new CalcProduct { Manufacturer = "Основной расчёт", ID = -1, isHidingButton = true, RowColor = CalcController.ColorToHex(Color.FromRgb(254, 241, 230)) });
                    calcItems.Add(new CalcProduct { Count = settings.FullCostType, TotalCost = 0, ID = -50 });
                    CalcController.Refresh(CalcDataGrid, calcItems);
                    isCalculationNeed = true;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";

                //MessageBox.Show("Хмм, А вот и ошибочка - 4. Найди меня) " + ex.Message);
            }
        }

        //Открытие расчётки из файла из другой формы
        public void openCalc_Templates_Click(string path) 
        {
            try
            {
                CalcDataGrid.SelectedItem = null;

                isCalcOpened = true;

                //Проверяем на невреный файл или поврежденный
                try
                {
                    //Альтернативная десериализацтя JSON
                    fileImporter.ImportCalcFromFile_StartDUH(path, this);
                    isCalcSaved = true;
                    DependencyDataGrid.ItemsSource = dependencies; //Обнуление зависимостей

                    string path1 = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/without_picture.png");
                    CalcProductImage.Source = new BitmapImage(new Uri(path1, UriKind.Absolute)); //Обнуление картинки, так как у раздела не может быть картинки

                    CalcController.ClearBackgroundsColors(this);
                    calcItems[calcItems.Count - 1].Count = settings.FullCostType;
                    CalcController.Refresh(CalcDataGrid, calcItems);
                    CalcController.ActivateNeedCalculation(this);
                }
                catch (Exception ex)
                {
                    var log = new Log_Repository();
                    log.Add("Error", new StackTrace(), "noneUser", ex);

                    MessageBox.Show($"Запущен файл: {path} - не соответсвует нужному типу или поврежден.");
                    calcItems.Clear();
                    isCalcSaved = true;
                    DependencyDataGrid.ItemsSource = dependencies; //Обнуление зависимостей

                    string path1 = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/without_picture.png");
                    CalcProductImage.Source = new BitmapImage(new Uri(path1, UriKind.Absolute)); //Обнуление картинки, так как у раздела не может быть картинки

                    CalcController.ClearBackgroundsColors(this);
                    calcItems.Add(new CalcProduct());
                    calcItems[0].Count = settings.FullCostType;
                    CalcController.Refresh(CalcDataGrid, calcItems);
                    isCalculationNeed = true;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }
        public void openCalcTest(string path) //Открытие расчётки из файла dah (двойной клик по нему)
        {

            try
            {
                //Открываем сразу расчетку
                if (DependencyDataGrid.SelectedItem != null)
                {
                    isDependencySelected = true;
                }
                priceCalcButton.Content = "РАСЧЁТ->ПРАЙС";

                CulcGrid_Grid.Visibility = Visibility.Visible;
                CalcDataGrid_Grid.Visibility = Visibility.Visible;

                addGrid.Visibility = Visibility.Hidden;
                searchGrid.Visibility = Visibility.Hidden;
                DataBaseGrid_Grid.Visibility = Visibility.Hidden;

                priceCalcButton.Background = new SolidColorBrush(Colors.LightGreen);
                addGrid_Button.Visibility = Visibility.Hidden;
                searchGrid_Button.Visibility = Visibility.Hidden;

                isCalcOpened = true;

                //Проверяем на невреный файл или поврежденный
                try
                {
                    //Альтернативная десериализацтя JSON
                    fileImporter.ImportCalcFromFile_StartDUH(path, this);
                    isCalcSaved = true;
                    DependencyDataGrid.ItemsSource = dependencies; //Обнуление зависимостей

                    string path1 = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/without_picture.png");
                    CalcProductImage.Source = new BitmapImage(new Uri(path1, UriKind.Absolute)); //Обнуление картинки, так как у раздела не может быть картинки
                                                                                                 //CalcProductImage.Source = new BitmapImage(new Uri("resources/images/without_picture.png", UriKind.Relative));


                    CalcController.ClearBackgroundsColors(this);
                    calcItems[calcItems.Count - 1].Count = settings.FullCostType;

                    CalcController.Refresh(CalcDataGrid, calcItems);
                    isCalculationNeed = false;
                }
                catch (Exception ex)
                {
                    var log = new Log_Repository();
                    log.Add("Error", new StackTrace(), "noneUser", ex);

                    MessageBox.Show($"Запущен файл: {path} - не соответсвует нужному типу или поврежден.");
                    calcItems.Clear();
                    isCalcSaved = true;
                    DependencyDataGrid.ItemsSource = dependencies; //Обнуление зависимостей

                    string path1 = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/without_picture.png");
                    CalcProductImage.Source = new BitmapImage(new Uri(path1, UriKind.Absolute)); //Обнуление картинки, так как у раздела не может быть картинки
                                                                                                 //CalcProductImage.Source = new BitmapImage(new Uri("resources/images/without_picture.png", UriKind.Relative));


                    CalcController.ClearBackgroundsColors(this);
                    calcItems.Add(new CalcProduct());
                    calcItems[0].Count = settings.FullCostType;
                    CalcController.Refresh(CalcDataGrid, calcItems);
                    isCalculationNeed = true;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }


        private void newCalc_menuItem_Click(object sender, RoutedEventArgs e) //Создание новой расчётки
        {
            try
            {
                bool isCalcSavedNow = false;
                CalcDataGrid.SelectedItem = null;
                if (calcItems.Count <= 1)
                {
                    return;
                }
                if (isCalcSaved == false) //Если расчётка не сохранена
                {
                    MessageBoxResult dres = MessageBox.Show("Сохранить расчёт?", "", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);
                    if (dres == MessageBoxResult.Yes)
                    {
                        saveCaalc_menuItem_Click(sender, e);
                        if (!isCalculationNeed)
                        {
                            isCalcSaved = true;
                            CalcDataGrid.SelectedItem = null;
                        }
                        return;
                    }
                    else if (dres == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                    else
                    {
                        isCalcSaved = true;
                        isCalcSavedNow = true;
                    }
                }

                MessageBoxResult res;
                if (isCalcSavedNow)
                {
                    res = MessageBoxResult.OK;
                }
                else
                {
                    res = MessageBox.Show("Создать новый расчёт?", "", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                }

                if (res == MessageBoxResult.OK)
                {
                    calcItems.Clear();
                    //calcItems.Add(new CalcProduct { Manufacturer = "Основной расчёт", ID = -1, isHidingButton = true, RowColor = CalcController.ColorToHex(Color.FromRgb(254, 241, 230)) });
                    calcItems.Add(new CalcProduct { Count = settings.FullCostType, TotalCost = 0, ID = -50 });
                    CalcDataGrid.SelectedItem = calcItems[0];

                    CalcPath_label.Content = "Имя файла расчёта: - ";
                    isCalculationNeed = false;
                    CalcInfo_label.Content = "Новый расчёт успешно создан.";
                }
                else
                {
                    CalcInfo_label.Content = "Новый расчёт не был создан.";
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void deleteDependency_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem; //Текущий элемент

                if (isAddtoDependency)
                {
                    selectedItem = selectItemForDependencies;
                }

                if (selectedItem != null)
                {
                    Dependency selectDependency = (Dependency)DependencyDataGrid.SelectedItem; //Текущая выбранная зависимость
                    if (selectDependency != null)
                    {
                        //Удаление
                        actions.Push(new DeleteDependency_Action(this, selectedItem, selectDependency, selectedItem.dependencies.IndexOf(selectDependency)));
                        selectedItem.dependencies.Remove(selectDependency);
                        isDependencySelected = false;
                        if (isAddtoDependency)
                        {
                            startStopAddingDependency_button_Click(this, e);
                        }

                        CalcDataGrid.SelectedItem = null;
                        CalcDataGrid.SelectedIndex = calcItems.IndexOf(selectedItem);
                        CalcController.Refresh(CalcDataGrid, calcItems);
                        CalcInfo_label.Content = "Выбранная зависимость удалена.";
                        CalcController.ActivateNeedCalculation(this);
                    }
                    CalcController.ValidateCalcItem(selectedItem);
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        bool articleSuppressEvent = false;
        bool costSuppressEvent = false;
        bool productNameSuppressEvent = false;
        bool typeSuppressEvent = false;
        bool manufacturerSuppressEvent = false;

        bool isManufacturerMainFilter = false;
        bool isTypeMainFilter = false;
        bool isNoneMainFilter = false;

        private void Manufacturer_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (isNoneMainFilter)
                {
                    manufacturerSuppressEvent = true;
                }

                if (!isManufacturerMainFilter && !isTypeMainFilter)
                {
                    isManufacturerMainFilter = true;
                }

                if (isManufacturerMainFilter)
                {
                    var ArticleTextBox = Article_comboBox.Template.FindName("PART_EditableTextBox", Article_comboBox) as TextBox;
                    var ProductNameTextBox = ProductName_comboBox.Template.FindName("PART_EditableTextBox", ProductName_comboBox) as TextBox;
                    var CostTextBox = Cost_comboBox.Template.FindName("PART_EditableTextBox", Cost_comboBox) as TextBox;
                    var TypeTextBox = Type_comboBox.Template.FindName("PART_EditableTextBox", Type_comboBox) as TextBox;
                    if (ArticleTextBox.IsFocused || ProductNameTextBox.IsFocused || TypeTextBox.IsFocused || CostTextBox.IsFocused)
                    {
                        return;
                    }
                    else
                    {
                        Manufacturer manufacturerItem = (Manufacturer)Manufacturer_comboBox.SelectedItem;
                        if (manufacturerItem != null)
                        {
                            productNameSuppressEvent = true;
                            articleSuppressEvent = true;
                            costSuppressEvent = true;
                            typeSuppressEvent = true;
                            var selectedItems = dbItems.Where(item => item.Manufacturer == manufacturerItem.name).ToList();

                            ProductName_comboBox.ItemsSource = selectedItems;
                            Article_comboBox.ItemsSource = selectedItems;
                            List<string> types = selectedItems.Select(i => i.Type).Distinct().ToList();
                            Type_comboBox.ItemsSource = types;
                            Cost_comboBox.ItemsSource = selectedItems;

                            ProductName_comboBox.SelectedItem = selectedItems[0];
                            Article_comboBox.SelectedItem = selectedItems[0];
                            Cost_comboBox.SelectedItem = selectedItems[0];
                            dataBaseGrid.SelectedItem = selectedItems[0];
                            Type_comboBox.SelectedItem = types[0];


                            dataBaseGrid.ScrollIntoView(selectedItems[0]);
                        }
                    }
                }
                else
                {
                    var ProductNameTextBox = ProductName_comboBox.Template.FindName("PART_EditableTextBox", ProductName_comboBox) as TextBox;
                    var ArticleTextBox = Article_comboBox.Template.FindName("PART_EditableTextBox", Article_comboBox) as TextBox;
                    var TypeTextBox = Type_comboBox.Template.FindName("PART_EditableTextBox", Type_comboBox) as TextBox;
                    var CostTextBox = Cost_comboBox.Template.FindName("PART_EditableTextBox", Cost_comboBox) as TextBox;
                    if (ArticleTextBox.IsFocused || ProductNameTextBox.IsFocused || TypeTextBox.IsFocused || CostTextBox.IsFocused)
                    {
                        return;
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            Material selectedItem = dbItems.FirstOrDefault(i => i.Type == Type_comboBox.Text && i.Manufacturer == Manufacturer_comboBox.Text); // (Material)Type_comboBox.SelectedItem;
                            if (selectedItem != null)
                            {
                                ProductName_comboBox.SelectedItem = selectedItem;
                                Article_comboBox.SelectedItem = selectedItem;
                                Cost_comboBox.SelectedItem = selectedItem;
                                dataBaseGrid.SelectedItem = selectedItem;

                                productNameSuppressEvent = true;
                                articleSuppressEvent = true;
                                costSuppressEvent = true;
                                manufacturerSuppressEvent = true;

                                dataBaseGrid.ScrollIntoView(selectedItem);
                            }
                        }), DispatcherPriority.Background);
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        //bool isTypeSelection
        private void Type_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (isNoneMainFilter)
                {
                    typeSuppressEvent = true;
                    //isManufacturerMainFilter = false;
                    //isTypeMainFilter = false;
                }

                if (!isManufacturerMainFilter && !isTypeMainFilter)
                {
                    isTypeMainFilter = true;
                }

                if (isManufacturerMainFilter)
                {
                    var ProductNameTextBox = ProductName_comboBox.Template.FindName("PART_EditableTextBox", ProductName_comboBox) as TextBox;
                    var ArticleTextBox = Article_comboBox.Template.FindName("PART_EditableTextBox", Article_comboBox) as TextBox;
                    var ManufacturerTextBox = Manufacturer_comboBox.Template.FindName("PART_EditableTextBox", Manufacturer_comboBox) as TextBox;
                    var CostTextBox = Cost_comboBox.Template.FindName("PART_EditableTextBox", Cost_comboBox) as TextBox;
                    if (ArticleTextBox.IsFocused || ProductNameTextBox.IsFocused || ManufacturerTextBox.IsFocused || CostTextBox.IsFocused)
                    {
                        return;
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            Material selectedItem = dbItems.FirstOrDefault(i => i.Type == Type_comboBox.Text && i.Manufacturer == Manufacturer_comboBox.Text); // (Material)Type_comboBox.SelectedItem;
                            if (selectedItem != null)
                            {
                                ProductName_comboBox.SelectedItem = selectedItem;
                                Article_comboBox.SelectedItem = selectedItem;
                                Cost_comboBox.SelectedItem = selectedItem;
                                dataBaseGrid.SelectedItem = selectedItem;

                                productNameSuppressEvent = true;
                                articleSuppressEvent = true;
                                costSuppressEvent = true;
                                typeSuppressEvent = true;

                                dataBaseGrid.ScrollIntoView(selectedItem);
                            }
                        }), DispatcherPriority.Background);
                    }
                }
                else
                {
                    var ArticleTextBox = Article_comboBox.Template.FindName("PART_EditableTextBox", Article_comboBox) as TextBox;
                    var ProductNameTextBox = ProductName_comboBox.Template.FindName("PART_EditableTextBox", ProductName_comboBox) as TextBox;
                    var ManufacturerTextBox = Manufacturer_comboBox.Template.FindName("PART_EditableTextBox", Manufacturer_comboBox) as TextBox;
                    var CostTextBox = Cost_comboBox.Template.FindName("PART_EditableTextBox", Cost_comboBox) as TextBox;
                    if (ArticleTextBox.IsFocused || ProductNameTextBox.IsFocused || ManufacturerTextBox.IsFocused || CostTextBox.IsFocused)
                    {
                        return;
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            string selectedType = Type_comboBox.Text;
                            if (!string.IsNullOrWhiteSpace(selectedType))
                            {
                                productNameSuppressEvent = true;
                                articleSuppressEvent = true;
                                costSuppressEvent = true;
                                manufacturerSuppressEvent = true;
                                var selectedItems = dbItems.Where(item => item.Type == selectedType).ToList();
                                if (selectedItems.Count > 0)
                                {
                                    List<Manufacturer> manufacturers = CountryManager.Instance.allManufacturers
                                                    .Where(m => selectedItems.Select(si => si.Manufacturer).Contains(m.name))
                                                    .Distinct()
                                                    .ToList();
                                    Manufacturer_comboBox.ItemsSource = manufacturers;
                                    ProductName_comboBox.ItemsSource = selectedItems;
                                    Article_comboBox.ItemsSource = selectedItems;
                                    Cost_comboBox.ItemsSource = selectedItems;

                                    Manufacturer_comboBox.SelectedIndex = 0;
                                    ProductName_comboBox.SelectedItem = selectedItems[0];
                                    Article_comboBox.SelectedItem = selectedItems[0];
                                    Cost_comboBox.SelectedItem = selectedItems[0];
                                    dataBaseGrid.SelectedItem = selectedItems[0];

                                    dataBaseGrid.ScrollIntoView(selectedItems[0]);
                                }
                            }
                        }), DispatcherPriority.Background);
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void ProductName_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //if (isNoneMainFilter)
                //{
                //    productNameSuppressEvent = true;
                //}

                //if (isNoneMainFilter)
                //{
                //    var ArticleTextBox = Article_comboBox.Template.FindName("PART_EditableTextBox", Article_comboBox) as TextBox;
                //    var TypeTextBox = Article_comboBox.Template.FindName("PART_EditableTextBox", Type_comboBox) as TextBox;
                //    var CostTextBox = Cost_comboBox.Template.FindName("PART_EditableTextBox", Cost_comboBox) as TextBox;
                //    if (ArticleTextBox.IsFocused || TypeTextBox.IsFocused || CostTextBox.IsFocused)
                //    {
                //        return;
                //    }
                //    else
                //    {
                //        Material selectedItem = (Material)ProductName_comboBox.SelectedItem;
                //        if (selectedItem != null && selectedItem != dataBaseGrid.SelectedItem)
                //        {
                //            productNameSuppressEvent = true;
                //            articleSuppressEvent = true;
                //            costSuppressEvent = true;
                //            typeSuppressEvent = true;
                //            Type_comboBox.SelectedItem = Type_comboBox.Items.Cast<string>().FirstOrDefault(i => i == selectedItem.Type);

                //            //manufacturerSuppressEvent = true;
                //            var selectManufacturer = CountryManager.Instance.allManufacturers.First(item => item.name == selectedItem.Manufacturer);
                //            Manufacturer_comboBox.SelectedItem = selectManufacturer;

                //            Article_comboBox.SelectedItem = selectedItem;
                //            Cost_comboBox.SelectedItem = selectedItem;

                //            dataBaseGrid.SelectedItem = selectedItem;
                //            dataBaseGrid.ScrollIntoView(selectedItem);
                //        }
                //    }
                //}
                //else
                //{
                var ArticleTextBox = Article_comboBox.Template.FindName("PART_EditableTextBox", Article_comboBox) as TextBox;
                var TypeTextBox = Article_comboBox.Template.FindName("PART_EditableTextBox", Type_comboBox) as TextBox;
                var CostTextBox = Cost_comboBox.Template.FindName("PART_EditableTextBox", Cost_comboBox) as TextBox;
                if (ArticleTextBox.IsFocused || TypeTextBox.IsFocused || CostTextBox.IsFocused)
                {
                    return;
                }
                else
                {
                    Material selectedItem = (Material)ProductName_comboBox.SelectedItem;
                    if (selectedItem != null && selectedItem != dataBaseGrid.SelectedItem)
                    {
                        productNameSuppressEvent = true;
                        articleSuppressEvent = true;
                        costSuppressEvent = true;

                        if (isManufacturerMainFilter)
                        {
                            typeSuppressEvent = true;
                            Type_comboBox.SelectedItem = Type_comboBox.Items.Cast<string>().FirstOrDefault(i => i == selectedItem.Type);
                        }
                        else
                        {
                            manufacturerSuppressEvent = true;
                            var selectManufacturer = CountryManager.Instance.allManufacturers.First(item => item.name == selectedItem.Manufacturer);
                            Manufacturer_comboBox.SelectedItem = selectManufacturer;
                        }

                        Article_comboBox.SelectedItem = selectedItem;
                        Cost_comboBox.SelectedItem = selectedItem;

                        dataBaseGrid.SelectedItem = selectedItem;
                        dataBaseGrid.ScrollIntoView(selectedItem);
                    }
                }
                //}
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void Article_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {

                //if (isNoneMainFilter)
                //{
                //    return;
                //}

                var ProductTextBox = ProductName_comboBox.Template.FindName("PART_EditableTextBox", ProductName_comboBox) as TextBox;
                var TypeTextBox = Article_comboBox.Template.FindName("PART_EditableTextBox", Type_comboBox) as TextBox;
                var CostTextBox = Cost_comboBox.Template.FindName("PART_EditableTextBox", Cost_comboBox) as TextBox;
                if (ProductTextBox.IsFocused || TypeTextBox.IsFocused || CostTextBox.IsFocused)
                {
                    return;
                }
                else
                {
                    Material selectedItem = (Material)Article_comboBox.SelectedItem;
                    if (selectedItem != null && selectedItem != dataBaseGrid.SelectedItem)
                    {
                        productNameSuppressEvent = true;
                        articleSuppressEvent = true;
                        costSuppressEvent = true;

                        if (isManufacturerMainFilter)
                        {
                            typeSuppressEvent = true;
                            if (Type_comboBox.Items.Count > 0)
                            {
                                Type_comboBox.SelectedItem = Type_comboBox.Items.Cast<string>().FirstOrDefault(i => i == selectedItem.Type);
                            }
                        }
                        else
                        {
                            manufacturerSuppressEvent = true;
                            var selectManufacturer = CountryManager.Instance.allManufacturers.First(item => item.name == selectedItem.Manufacturer);
                            Manufacturer_comboBox.SelectedItem = selectManufacturer;
                        }

                        ProductName_comboBox.SelectedItem = selectedItem;
                        Cost_comboBox.SelectedItem = selectedItem;

                        dataBaseGrid.SelectedItem = selectedItem;
                        dataBaseGrid.ScrollIntoView(selectedItem);
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void Cost_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //if (isNoneMainFilter)
                //{
                //    return;
                //}

                var ArticleTextBox = Article_comboBox.Template.FindName("PART_EditableTextBox", Article_comboBox) as TextBox;
                var ProductTextBox = ProductName_comboBox.Template.FindName("PART_EditableTextBox", ProductName_comboBox) as TextBox;
                var TypeTextBox = Type_comboBox.Template.FindName("PART_EditableTextBox", Type_comboBox) as TextBox;
                if (ArticleTextBox.IsFocused || ProductTextBox.IsFocused || TypeTextBox.IsFocused)
                {
                    return;
                }
                else
                {
                    Material selectedItem = (Material)Cost_comboBox.SelectedItem;
                    if (selectedItem != null && selectedItem != dataBaseGrid.SelectedItem)
                    {
                        productNameSuppressEvent = true;
                        articleSuppressEvent = true;
                        costSuppressEvent = true;

                        if (isManufacturerMainFilter)
                        {
                            typeSuppressEvent = true;
                            Type_comboBox.SelectedItem = Type_comboBox.Items.Cast<string>().FirstOrDefault(i => i == selectedItem.Type);
                        }
                        else
                        {
                            manufacturerSuppressEvent = true;
                            var selectManufacturer = CountryManager.Instance.allManufacturers.First(item => item.name == selectedItem.Manufacturer);
                            Manufacturer_comboBox.SelectedItem = selectManufacturer;
                        }

                        Article_comboBox.SelectedItem = selectedItem;
                        ProductName_comboBox.SelectedItem = selectedItem;

                        dataBaseGrid.SelectedItem = selectedItem;
                        dataBaseGrid.ScrollIntoView(selectedItem);
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void uploadDataBase_menuItem_Click(object sender, RoutedEventArgs e) //Загрузка выбранной БД с пк
        {
            try
            {
                //Открытие диалогового окна
                OpenFileDialog file = new OpenFileDialog();

                //Параметры для открытия
                file.Title = "Путь к локальной DB";

                if (Directory.Exists(settings.PriceFolderPath))
                {
                    file.InitialDirectory = settings.PriceFolderPath;

                }

                file.Filter = "MDF File|*.mdf";
                file.RestoreDirectory = true;

                if (file.ShowDialog() == true) //Если файл выбран
                {
                    ConnectionString_Global.Value = file.FileName;
                    settings.PriceFolderPath = System.IO.Path.GetDirectoryName(file.FileName);
                    settings.Price = file.FileName;

                    //Запускаем новое экземпляр приложения
                    string exePath = AppDomain.CurrentDomain.BaseDirectory + "Dahmira.exe";
                    Process.Start(exePath);

                    //Закрываем текущее приложение
                    System.Windows.Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void downloadDataBaseFromFtp_menuItem_Click(object sender, RoutedEventArgs e) //Загрузка БД с сервера
        {
            try
            {
                if (!isCalcSaved)
                {
                    MessageBoxResult res = MessageBox.Show("В Расчёт были внесены изменения. Желаете сохранить изменения?", "Изменения в Расчёте", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);
                    if (res == MessageBoxResult.Yes)
                    {
                        saveCaalc_menuItem_Click(e, new RoutedEventArgs());
                        return;
                    }
                    if (res == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }

                ConnectionString_Global.Value = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db", "Dahmira_DB_beta.mdf");

                settings.Price = ConnectionString_Global.Value;
                ProgressBarPage progressBarPage = new ProgressBarPage(this, "downloadDataBase");
                progressBarPage.Owner = this;
                shaderEffectsService.ApplyBlurEffect(this, 20);
                progressBarPage.ShowDialog();
                shaderEffectsService.ClearEffect(this);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        //private int IsCellEditing(DataGrid dataGrid)
        //{
        //    if (dataGrid.CurrentCell.Column == null)
        //    {
        //        return 2;
        //    }
        //    if (dataGrid.SelectedItem != null)
        //    {
        //        if (dataGrid.CurrentCell != null)
        //        {
        //            var cell = dataGrid.CurrentCell.Column.GetCellContent(dataGrid.CurrentCell.Item).Parent;
        //            if (cell != null)
        //            {
        //                var sd = (DataGridCell)cell;
        //                bool value = (bool)sd.Tag;
        //                if (value) { return 0; }
        //            }
        //        }
        //    }
        //    return 1;
        //}

        private void CalcDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                var items = CalcDataGrid.SelectedItems;

                var options = new JsonSerializerOptions
                {
                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                };

                if (e.Key == Key.Delete && !chapterName_textBox.IsFocused) // Проверяем, нажата ли клавиша Delete
                {
                    if (isDependencySelected)
                    {
                        deleteDependency_button_Click(sender, e);
                        e.Handled = true;
                    }
                    else
                    {
                        if (isCalcOpened && !IsDataGridCellEditing(CalcDataGrid))
                        {
                            CalcDeleteSelectedProduct_button_Click(sender, e);
                            e.Handled = true;
                        }

                        if (!isCalcOpened && !IsDataGridCellEditing(dataBaseGrid) && isDbFocused)
                        {
                            deleteSelectedProduct_button_Click(sender, e);
                            e.Handled = true;
                        }
                    }
                }

                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if (e.Key == Key.C && !IsDataGridCellEditing(CalcDataGrid) && isCalcOpened)
                    {
                        CopyCalc_Click(this, e);
                    }
                    else if (e.Key == Key.X && !IsDataGridCellEditing(CalcDataGrid) && CalcDataGrid.SelectedItems.Count > 0 && isCalcOpened)
                    {
                        // Создаем список для хранения выделенных элементов нужного типа
                        List<CalcProduct> selectedItems = new List<CalcProduct>();

                        List<CalcProduct> products = new List<CalcProduct>();
                        List<int> productsIndex = new List<int>();

                        // Перебираем выделенные элементы и добавляем их в список
                        foreach (var item in items)
                        {
                            CalcProduct product = (CalcProduct)item;
                            if (product.ID != -50)
                            {
                                selectedItems.Add(product);
                                if (product.ID < 1)
                                {
                                    int index = calcItems.IndexOf(product);
                                    if (calcItems[index + 1].ID != product.ID)
                                    {
                                        for (int i = index + 1; i < calcItems.Count - 1; i++)
                                        {
                                            selectedItems.Add(calcItems[i]);
                                            if ((product.ID == 0 && calcItems[i + 1].ID < 1) || (product.ID == -1 && calcItems[i + 1].ID <= -1))
                                            {
                                                break;
                                            }
                                        }
                                    }

                                }
                            }
                        }
                        // Копируем и удаляем выделенные элементы
                        var itemsToCopy = new List<CalcProduct>();

                        foreach (var item in selectedItems)
                        {
                            if (item is CalcProduct product)
                            {
                                products.Add(product);
                                productsIndex.Add(calcItems.IndexOf(product));
                            }
                        }

                        foreach (var item in selectedItems)
                        {
                            if (item is CalcProduct product)
                            {
                                itemsToCopy.Add(product.Clone());
                                calcItems.Remove(item);
                            }
                        }

                        if (products.Count > 0 && productsIndex.Count > 0)
                        {
                            actions.Push(new DeleteCalc_Action(products, productsIndex, this));
                        }

                        string json = JsonSerializer.Serialize(itemsToCopy, options);
                        Clipboard.SetText(json); // Сохраняем в буфер обмена
                        CalcController.Refresh(CalcDataGrid, calcItems);
                        CalcController.ActivateNeedCalculation(this);
                        //e.Handled = true; // Указываем, что событие обработано
                    }
                    else if (e.Key == Key.V && isCalcOpened)
                    {
                        if (Clipboard.ContainsText())
                        {
                            if (IsDataGridCellEditing(CalcDataGrid))
                            {
                                string clipboardText = Clipboard.GetText();
                                if (clipboardText.Trim().StartsWith("[") && clipboardText.Trim().EndsWith("]"))
                                {
                                    e.Handled = true;
                                }
                            }
                            else
                            {
                                if (isCalcOpened) //Если открыта расчётка то вставляем в расчётку
                                {
                                    PasteCalc_Click(sender, e);
                                }
                            }
                        }
                    }
                    else if (e.Key == Key.Tab)
                    {
                        priceCalcButton_Click(sender, e);
                    }
                    else if (e.Key == Key.A)
                    {
                        if (isCalcOpened && !IsDataGridCellEditing(CalcDataGrid))
                        {
                            CalcDataGrid.SelectAll();
                            e.Handled = true;
                        }

                        if (!isCalcOpened && !IsDataGridCellEditing(dataBaseGrid))
                        {
                            dataBaseGrid.SelectAll();
                            e.Handled = true;
                        }
                    }
                    else if (e.Key == Key.F && !isCalcOpened)
                    {
                        e.Handled = true;
                        FastSearch_button_Click(sender, e);
                    }
                    else if (e.Key == Key.O && isCalcOpened)
                    {
                        openCalc_menuItem_Click(sender, e);
                        e.Handled = true;
                    }
                    else if (e.Key == Key.Up && isCalcOpened)
                    {
                        MoveUp_button_Click(sender, e);
                        e.Handled = true;
                        return;
                    }
                    else if (e.Key == Key.Down && isCalcOpened)
                    {
                        MoveDown_button_Click(sender, e);
                        e.Handled = true;
                        return;
                    }
                    else if (e.Key == Key.S && isCalcOpened)
                    {
                        saveCaalc_menuItem_Click(sender, e);
                    }
                    else if (e.Key == Key.Z)
                    {
                        if (actions.Count > 0)
                        {
                            IAction lastAction = actions.Pop();
                            lastAction.Undo();
                        }
                        else
                        {
                            string text = "Достигнут максимум. Больше нечего отменять.";
                            if (isCalcOpened)
                            {
                                CalcInfo_label.Content = text;
                            }
                            else
                            {
                                PriceInfo_label.Content = text;
                            }
                        }
                    }
                    else if (e.Key == Key.N && isCalcOpened)
                    {
                        newCalc_menuItem_Click(sender, e);
                    }
                    else if (e.Key == Key.Enter)
                    {
                        if (CalcDataGrid.SelectedIndex != -1)
                        {
                            var selectedItem = CalcDataGrid.SelectedItem;
                            if (selectedItem != null)
                            {
                                // Получаем визуальный контейнер строки
                                var row = CalcDataGrid.ItemContainerGenerator.ContainerFromItem(selectedItem) as DataGridRow;
                                if (row != null)
                                {
                                    // Ищем кнопку в визуальном дереве строки
                                    var button = FindVisualChild<Button>(row);
                                    if (button != null)
                                    {
                                        button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                                    }
                                }
                            }
                        }
                    }
                }

                if (e.Key == Key.Up && isCalcOpened)
                {
                    int selectedIndex = CalcDataGrid.SelectedIndex;
                    if (selectedIndex > 0)
                    {
                        for (int i = selectedIndex - 1; i >= 0; i--)
                        {
                            if (calcItems[i].isVisible == true)
                            {
                                CalcDataGrid.SelectedIndex = i;
                                break;
                            }
                        }
                    }
                }

                if (e.Key == Key.Down && isCalcOpened)
                {
                    int selectedIndex = CalcDataGrid.SelectedIndex;
                    if (selectedIndex < calcItems.Count)
                    {
                        for (int i = selectedIndex + 1; i < calcItems.Count; i++)
                        {
                            if (calcItems[i].isVisible == true)
                            {
                                CalcDataGrid.SelectedIndex = i;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            try
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    if (child is T result)
                        return result;
                    var descendant = FindVisualChild<T>(child);
                    if (descendant != null)
                        return descendant;
                }
                return null;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";


                return null;
            }
        }

        private void ExportCalcToPrice_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isAdded = false;
                var items = CalcDataGrid.SelectedItems;
                bool isAllItemsAdded = true;

                List<CalcProduct> selectedItems = new List<CalcProduct>();
                List<Material> dbNewItems = new List<Material>();

                foreach (var item in items)
                {
                    if (item is CalcProduct product)
                    {
                        if (product.IsCellCorrects[3])
                            selectedItems.Add(product);
                        else
                        {
                            isAllItemsAdded = false;
                            if (items.Count == 1)
                            {
                                isAdded = true;
                            }
                        }
                    }
                }

                foreach (var item in selectedItems)
                {
                    if (item.ID > 0)
                    {
                        Material newMaterial = new Material
                        {
                            Manufacturer = item.Manufacturer,
                            Type = item.Type,
                            ProductName = item.ProductName,
                            EnglishProductName = item.EnglishProductName,
                            Unit = item.Unit,
                            EnglishUnit = item.EnglishUnit,
                            Article = item.Article,
                            Photo = item.Photo,
                            Cost = (float)item.Cost,
                            LastCostUpdate = DateTime.Now.ToString("dd.MM.yyyy")
                        };

                        dbNewItems.Add(newMaterial);
                        dbItems.Add(newMaterial);
                        DbController.ValidateDbItem(newMaterial);

                        materialForDBAdding.Add(newMaterial);
                        isAdded = true;
                    }
                }

                if (isAdded)
                {
                    actions.Push(new ExportCalcToPrice_Action(dbNewItems, this));
                    if (!isAllItemsAdded)
                    {
                        CalcInfo_label.Content = "Не все выбранные строки строки перенесены в прайс.";
                        MessageBox.Show("Незаполненные строки не были перенесены. Проверьте корректность ввода Артикула.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        CalcInfo_label.Content = "Выбранные строки успешно перенесены в прайс.";
                    }
                    productsCount_label.Content = "из " + dataBaseGrid.Items.Count.ToString();

                    //Обновление данных в поиске
                    UpdateDataInSearch();
                    CalcController.ActivateNeedCalculation(this);
                }
                else
                {
                    CalcInfo_label.Content = "Выбранные строки не были перенесены в прайс.";
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private async void saveDBChanges_button_ClickAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                if (isSortBDSave)
                {
                    var materials = dbItems; // твой список новых материалов

                    //Обснуляем таблицу и записываем новые данные
                    await Task.Run(() =>
                    {
                        repository.ReplaceAllMaterials(materials, (current, total) =>
                        {
                            string progressText = $"Сохраняется {current} из {total}";

                            // Обновим Label из фонового потока — через Dispatcher
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                PriceInfo_label.Content = progressText;
                            });
                        });
                    });

                    //Считываем данные заного, чтобы обносить ID 
                    await Task.Run(() =>
                    {
                        var loadedMaterials = repository.Get_AllMaterialsWithProgress((current, total) =>
                        {
                            string text = $"Считывание {current} из {total}";

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                PriceInfo_label.Content = text;
                            });
                        });

                        // Обновляем коллекцию в UI-потоке
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            dbItems.Clear();
                        });

                        int count = loadedMaterials.Count;
                        for (int i = 0; i < count; i++)
                        {
                            var item = loadedMaterials[i];

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                dbItems.Add(item);
                                PriceInfo_label.Content = $"Обновляем ID {i + 1} из {count}";
                            });
                        }

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            PriceInfo_label.Content = "Данные успешно обновлены.";
                        });
                    });


                    materialForDBAdding.Clear();
                    materialForDBUpdating.Clear();
                    materialForDBDeleting.Clear();


                    PriceInfo_label.Content = "Изменения в прайс внесены успешно.";


                    isSortBDSave = false;
                }
                else
                {
                    if (materialForDBAdding.Count > 0 || materialForDBUpdating.Count > 0 || materialForDBDeleting.Count > 0)
                    {
                        foreach (var item in materialForDBAdding)
                        {
                            repository.Add_Material(item);
                        }
                        foreach (var item in materialForDBUpdating)
                        {
                            repository.UpdateMaterial(item);
                        }
                        foreach (var item in materialForDBDeleting)
                        {
                            repository.DeleteMaterial(item);
                        }

                        materialForDBAdding.Clear();
                        materialForDBUpdating.Clear();
                        materialForDBDeleting.Clear();

                        PriceInfo_label.Content = "Изменения в прайс внесены успешно.";
                    }
                    else
                    {
                        WarningFlashing("Для начала внесите изменения!", WarningBorder, WarningLabel, Colors.OrangeRed, 2.5);
                        PriceInfo_label.Content = "Изменения не были внесены в прайс, так как изменений нет.";
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void dataBaseGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                if (e.EditAction != DataGridEditAction.Commit)
                    return;

                Material selectedItem = (Material)dataBaseGrid.SelectedItem;
                if (selectedItem == null)
                    return;

                var editedCell = e.EditingElement as TextBox;
                string newText = editedCell.Text;

                if (e.Column.Header.ToString() == "Цена")
                {
                    bool res = true;

                    //Обработка пустого ввода ДО TryParse
                    if (string.IsNullOrEmpty(newText))
                    {
                        selectedItem.IsCellCorrects[6] = false;
                        selectedItem.Cost = 0;
                        editedCell.Text = "0";
                        MessageBox.Show("Недопустимое значение. Установлено стандартное значение, которое следует заменить");
                        return;
                    }

                    if (float.TryParse(newText, out float newCost))
                    {
                        string column = e.Column.Header.ToString();
                        string cleanedText = CleanNumericInput(newText);

                        if (string.IsNullOrEmpty(cleanedText) || cleanedText == "0" || cleanedText == ".")
                        {
                            cleanedText = "0";
                            selectedItem.IsCellCorrects[6] = false;
                            res = false;
                        }
                        else
                        {
                            selectedItem.IsCellCorrects[6] = true;
                        }

                        ((TextBox)e.EditingElement).Text = cleanedText;

                        if (newCost != selectedItem.Cost)
                        {
                            selectedItem.LastCostUpdate = DateTime.Now.ToString("dd.MM.yyyy");
                        }
                    }
                    else
                    {
                        //Некорректный ввод (не число)
                        selectedItem.IsCellCorrects[6] = false;
                        editedCell.Text = "0";
                        res = false;
                    }

                    if (!res)
                    {
                        MessageBox.Show("Недопустимое значение. Установлено стандартное значение, которое следует заменить");
                    }
                }
                else
                {
                    bool cellCorrectValue = !string.IsNullOrWhiteSpace(newText);

                    switch (e.Column.Header.ToString())
                    {
                        case "Производитель": { selectedItem.IsCellCorrects[0] = cellCorrectValue; break; }
                        case "Наименование": { selectedItem.IsCellCorrects[1] = cellCorrectValue; break; }
                        case "Наименование на английском": { selectedItem.IsCellCorrects[2] = cellCorrectValue; break; }
                        case "Артикул": { selectedItem.IsCellCorrects[3] = cellCorrectValue; break; }
                        case "Ед. измерения": { selectedItem.IsCellCorrects[4] = cellCorrectValue; break; }
                        case "Ед. измерения на английском": { selectedItem.IsCellCorrects[5] = cellCorrectValue; break; }
                    }
                }

                materialForDBUpdating.Add(selectedItem);
                actions.Push(new UpdateDB_Action(MaterialBeginEdit, selectedItem, this));

                // Откладываем обновление до завершения редактирования
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    UpdateDataInSearch();

                }), DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (materialForDBAdding.Count != 0 || materialForDBDeleting.Count != 0 || materialForDBUpdating.Count != 0)
                {
                    MessageBoxResult res = MessageBox.Show("В Базу были внесены изменения. Желаете сохранить изменения?", "Изменения в Базе", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);
                    if (res == MessageBoxResult.Yes)
                    {
                        saveDBChanges_button_ClickAsync(e, new RoutedEventArgs());
                    }
                    if (res == MessageBoxResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                }

                if (!isCalcSaved)
                {
                    MessageBoxResult res = MessageBox.Show("В Расчёт были внесены изменения. Желаете сохранить изменения?", "Изменения в Расчёте", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);
                    if (res == MessageBoxResult.Yes)
                    {
                        saveCaalc_menuItem_Click(e, new RoutedEventArgs());
                        e.Cancel = true;
                    }
                    if (res == MessageBoxResult.Cancel)
                    {
                        e.Cancel = true;
                    }
                }

                fileImporter.ExportSettingsOnFile(this);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void Calc_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string diferenceWithDbText = "Соответствие с прайсом нарушено.";
                //CalcController.Refresh(CalcDataGrid, calcItems); //Обновление
                if (CalcController.CheckingDifferencesWithDB(CalcDataGrid, this))
                {
                    diferenceWithDbText = "Соответствие с Прайсом не нарушено.";
                }

                var selectedCountry = (Country)allCountries_comboBox.SelectedItem;

                CalcController.Calculation(this);
                //CalcController.ClearBackgroundsColors(this);

                foreach (var item in calcItems) //Перебор всех элементов
                {
                    if (item.ID > 0) //Если не раздел
                    {
                        item.Cost = item.RealCost * selectedCountry.coefficient; //Коэф страны * цену

                        foreach (var countryManufacturer in selectedCountry.manufacturers)
                        {
                            if (countryManufacturer.name == item.Manufacturer) //Если это местный поставщик выбранной страны
                            {
                                double discount = item.RealCost * selectedCountry.discount / 100; //Скидка
                                item.Cost = item.Cost - discount; //Цена со скидкой
                            }
                        }

                        item.Cost = Math.Round(item.Cost, 2);

                        CalcController.ValidateCalcItem(item);
                    }
                }
                calcItems[calcItems.Count - 1].SelectedCountryName = selectedCountry.name;

                MovingLabel.Visibility = Visibility.Hidden;
                CalcController.Refresh(CalcDataGrid, calcItems);
                isCalculationNeed = false;
                CalcInfo_label.Content = "Расчёт был произведён успешно. " + diferenceWithDbText;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void AddDependency_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem;
                if (selectedItem != null)
                {
                    if (selectedItem.ID > 0)
                    {
                        DependencyImage.Visibility = Visibility.Hidden;
                        DependencyDataGrid.Visibility = Visibility.Visible;
                        DependencyButtons.Visibility = Visibility.Visible;
                        DependencyDataGrid.ItemsSource = selectedItem.dependencies;
                        selectedItem.isDependency = true;
                        CalcController.Refresh(CalcDataGrid, calcItems);
                        CalcController.ValidateCalcItem(selectedItem);
                        actions.Push(new CreateCalcDependency_Action(selectedItem, this));
                        CalcController.ActivateNeedCalculation(this);
                        CalcInfo_label.Content = "Зависимость для выбранной строки добавлена.";
                    }
                    else
                    {
                        CalcInfo_label.Content = "Невозможно добавить зависимость для выбранной строки.";
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void DeleteDependency_button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem;
                CalcProduct oldProduct = null;
                if (selectedItem != null)
                {
                    oldProduct = selectedItem.Clone();

                    if (selectedItem.ID > 0)
                    {
                        DependencyImage.Visibility = Visibility.Visible;
                        DependencyDataGrid.Visibility = Visibility.Hidden;
                        DependencyButtons.Visibility = Visibility.Hidden;
                        selectedItem.isDependency = false;
                        selectedItem.dependencies = new ObservableCollection<Dependency>();
                        selectedItem.Count = "1";
                    }

                    actions.Push(new DeleteCalcDependency_Action(oldProduct, selectedItem, this));
                    CalcInfo_label.Content = "Зависимость для выбранной строки удалена.";
                    CalcController.Refresh(CalcDataGrid, calcItems);
                    CalcController.ValidateCalcItem(selectedItem);
                    CalcController.ActivateNeedCalculation(this);
                }
                else
                {
                    if (selectItemForDependencies != null)
                    {
                        oldProduct = selectItemForDependencies.Clone();
                        if (isAddtoDependency)
                        {
                            startStopAddingDependency_button_Click(sender, e);
                            DependencyImage.Visibility = Visibility.Visible;
                            DependencyDataGrid.Visibility = Visibility.Hidden;
                            DependencyButtons.Visibility = Visibility.Hidden;
                            selectItemForDependencies.Count = "1";
                            selectItemForDependencies.isDependency = false;
                            selectItemForDependencies.dependencies = new ObservableCollection<Dependency>();
                        }

                        actions.Push(new DeleteCalcDependency_Action(oldProduct, selectItemForDependencies, this));
                        CalcInfo_label.Content = "Зависимость для выбранной строки удалена.";
                        CalcController.Refresh(CalcDataGrid, calcItems);
                        CalcController.ValidateCalcItem(selectItemForDependencies);
                        CalcController.ActivateNeedCalculation(this);
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void CopyCalc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var items = CalcDataGrid.SelectedItems;

                var options = new JsonSerializerOptions
                {
                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                };
                // Создаем список для хранения выделенных элементов нужного типа
                List<CalcProduct> selectedItems = new List<CalcProduct>();

                //Перебираем выделенные элементы и добавляем их в список
                foreach (var item in items)
                {
                    CalcProduct product = (CalcProduct)item;
                    selectedItems.Add(product);
                }

                var itemsToCopy = new List<CalcProduct>();
                foreach (var item in selectedItems)
                {
                    if (item is CalcProduct product)
                    {
                        if (product.ID != -50)
                        {
                            itemsToCopy.Add(product.Clone());
                            if (product.ID < 1 && product.hideButtonContext == "+")
                            {
                                int index = calcItems.IndexOf(product);
                                for (int i = index + 1; i < calcItems.Count - 1; i++)
                                {
                                    itemsToCopy.Add(calcItems[i].Clone());
                                    if ((product.ID == 0 && calcItems[i + 1].ID < 1) || (product.ID == -1 && calcItems[i + 1].ID == -1))
                                    {
                                        break;
                                    }
                                }
                            }

                            if (itemsToCopy[itemsToCopy.Count - 1].isDependency)
                            {
                                itemsToCopy[itemsToCopy.Count - 1].dependencies = new ObservableCollection<Dependency>();
                                itemsToCopy[itemsToCopy.Count - 1].isDependency = false;
                                itemsToCopy[itemsToCopy.Count - 1].Count = "1";
                            }
                        }
                    }
                }

                if (itemsToCopy.Count > 0)
                {
                    string json = JsonSerializer.Serialize(itemsToCopy, options);
                    Clipboard.SetText(json); // Сохраняем в буфер обмена
                    CalcInfo_label.Content = "Выбранные элементы скопированы в буфер.";
                    isCalcSaved = false;
                }
                else
                {
                    CalcInfo_label.Content = "Выбранные элементы не скопированы в буфер.";
                }

                e.Handled = true; // Указываем, что событие обработано
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void PasteCalc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem;
                if (selectedItem != null)
                {
                    if (!IsDataGridCellEditing(CalcDataGrid))
                    {
                        var options = new JsonSerializerOptions
                        {
                            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                        };

                        string json = Clipboard.GetText();

                        var itemsToPaste = JsonSerializer.Deserialize<List<CalcProduct>>(json, options);

                        if (itemsToPaste != null && json != "[]" && selectedItem.ID != -50)
                        {
                            int addedIndexForPaste = 1; //Добавочный индекс

                            int index = calcItems.IndexOf(selectedItem);
                            if (selectedItem.ID < 1)
                            {
                                if(selectedItem.hideButtonContext == "+")
                                {
                                    for (int i = index + 1; i < calcItems.Count - 1; i++)
                                    {
                                        index = i;
                                        if ((selectedItem.ID == 0 && calcItems[i + 1].ID < 1) || (selectedItem.ID == -1 && calcItems[i + 1].ID == -1))
                                        {
                                            break;
                                        }
                                    }
                                }

                                //Если добавляется под категорию, то добавляется в раздел внутри этой категории
                                //А если нет раздела в этой категории, то он создаётся а потом туда добавляются данные
                                if (selectedItem.ID == -1 && itemsToPaste[0].ID > 0)
                                {
                                    if (calcItems.IndexOf(selectedItem) == calcItems.Count - 2 || calcItems[calcItems.IndexOf(selectedItem) + 1].ID == -1)
                                    {
                                        calcItems.Insert(calcItems.IndexOf(selectedItem) + 1,

                                        new CalcProduct
                                        {
                                            ID = 0,
                                            Manufacturer = "Раздел",
                                            Cost = double.NaN,
                                            TotalCost = double.NaN,
                                            isHidingButton = true,
                                            RowColor = CalcController.ColorToHex(Color.FromRgb(223, 242, 253)),
                                            RowForegroundColor = CalcController.ColorToHex(Colors.Black)
                                        });
                                    }
                                    addedIndexForPaste = 2;
                                }
                            }


                            bool isItemsVisible = true;
                            if (selectedItem.ID == -1 && selectedItem.hideButtonContext == "+" && itemsToPaste[0].ID == 0)
                            {
                                isItemsVisible = false;
                            }

                            List<CalcProduct> products = new List<CalcProduct>();
                            for (int i = itemsToPaste.Count - 1; i >= 0; i--)
                            {
                                if (itemsToPaste[i].ID > 0)
                                {
                                    int MaxId = calcItems.Max(i => i.ID);
                                    if (MaxId == -1)
                                        MaxId = 0;
                                    itemsToPaste[i].ID = MaxId + 1;
                                    CalcController.ValidateCalcItem(itemsToPaste[i]);
                                }
                                if (!isItemsVisible) itemsToPaste[i].isVisible = false;
                                calcItems.Insert(index + addedIndexForPaste, itemsToPaste[i]);
                                products.Add(itemsToPaste[i]);
                            }
                            actions.Push(new AddCalc_Action(products, this));
                            CalcController.Refresh(CalcDataGrid, calcItems);
                            CalcInfo_label.Content = "Элементы вставлены из буфера.";
                            isCalcSaved = false;
                            CalcController.ActivateNeedCalculation(this);
                        }
                        else
                        {
                            CalcInfo_label.Content = "Не удалось вставить элементы из буфера.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Warning", new StackTrace(), "noneUser", ex);




                CalcInfo_label.Content = "Не удалось вставить элементы из буфера.";
            }
            
        }

        public void startStopAddingDependency_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!isAddtoDependency) //Если добавление начинается сейчас
                {
                    CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem; //Выбранный элемент
                    if (selectedItem != null)
                    {
                        if (selectedItem.ID == 0) //Если раздел
                        {
                            return;
                        }
                        CalcController.UpdateCellStyle(CalcDataGrid, Brushes.MediumSeaGreen, Brushes.White); //Теперь при выборе цвет становится салатовым
                        CalcDataGrid.SelectedItem = null; //Выделенный элемент убирается
                        selectedItem.RowColor = CalcController.ColorToHex(Colors.CornflowerBlue); //Выбранный элемент становится зелёным, чтобы было видно какому элементу добавляются зависимости
                        selectedItem.RowForegroundColor = CalcController.ColorToHex(Colors.White); //Цвет текста у выделенного элемента
                        CalcDataGrid.Items.Refresh();
                        selectItemForDependencies = selectedItem; //Запоминаем выделенный элемент
                        isAddtoDependency = true;
                        isAddingFirstDependencyPosition = false;
                        isAddingSecondDependencyPosition = false;
                        DependencyDataGrid.ItemsSource = selectedItem.dependencies;

                        if (selectItemForDependencies.dependencies.Count > 0)
                        {
                            foreach (var dependency in selectItemForDependencies.dependencies) //Отображение всех зависимостей
                            {
                                CalcProduct foundProduct = calcItems.FirstOrDefault(p => p.ID == dependency.ProductId);
                                if (foundProduct != null)
                                {
                                    foundProduct.RowColor = CalcController.ColorToHex(Colors.MediumSeaGreen);
                                    foundProduct.RowForegroundColor = CalcController.ColorToHex(Colors.White);
                                }
                            }
                        }


                        //Изменение стиля кнопки
                        startStopAddingDependency_button.Background = Brushes.Coral;

                        string stopImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/stop.png");
                        startStopAddingDependency_image.Source = new BitmapImage(new Uri(stopImagePath, UriKind.Absolute));


                        startStopAddingDependency_image.ToolTip = "Прекратить добавление зависимостей";
                        CalcInfo_label.Content = "Добавление зависимостей начато.";
                    }
                }
                else
                {
                    //Возвращение всего на свои места при повторном нажатии кнопки
                    isAddtoDependency = false;
                    CalcController.UpdateCellStyle(CalcDataGrid, Brushes.CornflowerBlue, Brushes.White);
                    selectItemForDependencies.RowColor = CalcController.ColorToHex(Colors.Transparent);
                    selectItemForDependencies.RowForegroundColor = CalcController.ColorToHex(Colors.Gray);
                    CalcDataGrid.SelectedItem = selectItemForDependencies;
                    CalcController.Refresh(CalcDataGrid, calcItems);
                    startStopAddingDependency_button.Background = Brushes.MediumSeaGreen;

                    string playImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/play.png");
                    startStopAddingDependency_image.Source = new BitmapImage(new Uri(playImagePath, UriKind.Absolute));


                    startStopAddingDependency_image.ToolTip = "Начать добавление зависимостей";
                    CalcInfo_label.Content = "Добавление зависимостей окончено.";
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }

        }

        private void DependencyDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                isDependencySelected = true;
                CalcController.Refresh(CalcDataGrid, calcItems);
                CalcProduct selectedCalc = (CalcProduct)CalcDataGrid.SelectedItem;
                if (selectedCalc != null)
                {
                    int count = selectedCalc.dependencies.Select(d => d.ProductId > 0 || d.SecondProductId > 0).ToList().Count;
                    if (count > 1)
                    {
                        Dependency selectedDependency = (Dependency)DependencyDataGrid.SelectedItem;
                        if (selectedDependency != null)
                        {
                            if (selectedDependency.ProductId > 0)
                            {
                                calcItems.FirstOrDefault(i => i.ID == selectedDependency.ProductId).RowColor = CalcController.ColorToHex(Colors.SeaGreen);
                            }

                            if (selectedDependency.SecondProductId > 0)
                            {
                                calcItems.FirstOrDefault(i => i.ID == selectedDependency.SecondProductId).RowColor = CalcController.ColorToHex(Colors.SeaGreen); //LightSeaGreen //DarkSeaGreen //SeaGreen
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        // Вспомогательный метод для поиска родительского элемента
        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            try
            {
                while (child != null && !(child is T))
                {
                    child = VisualTreeHelper.GetParent(child);
                }
                return child as T;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                //if (isCalcOpened)
                //    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                //else
                //    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации."; 
                
                
                return null;
            }
        }

        private void CalcDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                var selectedItem = (CalcProduct)CalcDataGrid.SelectedItem;

                if (selectedItem == null) return;

                if (selectedItem.ID < 1)
                {
                    actions.Push(new UpdateCalc_Action(CalcProductBeginEdit, selectedItem, this));
                    return;
                }

                string column = e.Column.Header.ToString();
                string newText = ((TextBox)e.EditingElement).Text;

                if (column != "Цена" && column != "Количество")
                {
                    if (column == "№" || column == "Изображение") return;

                    bool cellCorrectValue = !string.IsNullOrWhiteSpace(newText);

                    switch (column)
                    {
                        case "Производитель": { selectedItem.IsCellCorrects[0] = cellCorrectValue; break; }
                        case "Наименование": { selectedItem.IsCellCorrects[1] = cellCorrectValue; break; }
                        case "Наименование на английском": { selectedItem.IsCellCorrects[2] = cellCorrectValue; break; }
                        case "Артикул":
                            {
                                selectedItem.IsCellCorrects[3] = cellCorrectValue;
                                CalcController.ActivateNeedCalculation(this);
                                break;
                            }
                        case "Ед. измерения": { selectedItem.IsCellCorrects[4] = cellCorrectValue; break; }
                        case "Ед. измерения на английском": { selectedItem.IsCellCorrects[5] = cellCorrectValue; break; }
                    }
                    actions.Push(new UpdateCalc_Action(CalcProductBeginEdit, selectedItem, this));
                    return;
                }

                if (selectedItem.isDependency && column == "Количество" && !ConfirmDependencyRemoval()) return;

                if (selectedItem.isDependency && column == "Количество")
                {
                    selectedItem.isDependency = false;
                    selectedItem.dependencies.Clear();
                }

                string cleanedText = CleanNumericInput(newText);

                if (string.IsNullOrEmpty(cleanedText) || cleanedText == "0" || cleanedText == ".")
                {
                    cleanedText = "0";
                    if (column == "Цена")
                    {
                        selectedItem.IsCellCorrects[6] = false;
                    }
                    else
                    {
                        selectedItem.IsCellCorrects[7] = false;
                    }
                    MessageBox.Show("Недопустимое значение. Установлено стандартное значение, которое следует заменить");
                }
                else
                {
                    if (column == "Цена")
                    {
                        selectedItem.IsCellCorrects[6] = true;
                    }
                    else
                    {
                        selectedItem.IsCellCorrects[7] = true;
                    }
                }
                CalcController.ActivateNeedCalculation(this);

                ((TextBox)e.EditingElement).Text = cleanedText;

                //if (column == "Цена" && double.TryParse(cleanedText, out double cost))
                //{
                //    isCalculationNeed = true;
                //}

                actions.Push(new UpdateCalc_Action(CalcProductBeginEdit, selectedItem, this));
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        // Получаем значение ячейки (поддерживает TextBox и TextBlock)
        private string GetCellValue(DataGridCell cell)
        {
            try
            {
                if (cell.Content is TextBlock textBlock)
                    return textBlock.Text;
                if (cell.Content is TextBox textBox)
                    return textBox.Text;
                if (cell.Content is string str)
                    return str;

                return cell.Content?.ToString();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";


                return null;
            }
        }

        private bool ConfirmDependencyRemoval()
        {
            try
            {
                var result = MessageBox.Show(
                    "Вы точно хотите установить значение количества для строки с зависимостью? Это действие удалит зависимость.",
                    "", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                return result == MessageBoxResult.Yes;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";



                return false;
            }
        }

        private string CleanNumericInput(string input)
        {
            try
            {
                bool dotSeen = false;
                StringBuilder valid = new StringBuilder();

                foreach (char c in input)
                {
                    if (char.IsDigit(c))
                    {
                        valid.Append(c);
                    }
                    else if (c == '.' && !dotSeen)
                    {
                        valid.Append(c);
                        dotSeen = true;
                    }
                }

                return valid.ToString();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";

                return null;
            }
        }

        private void ExportAllCalcToPrice_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isAdded = false;
                bool isAllItemsAdded = true;

                List<CalcProduct> items = calcItems.ToList();
                List<Material> dbNewItems = new List<Material>();
                foreach (var item in items)
                {
                    if (item.ID > 0)
                        if (item.IsCellCorrects[3])
                        {
                            Material newMaterial = new Material
                            {
                                Manufacturer = item.Manufacturer,
                                Type = item.Type,
                                ProductName = item.ProductName,
                                EnglishProductName = item.EnglishProductName,
                                Unit = item.Unit,
                                EnglishUnit = item.EnglishUnit,
                                Article = item.Article,
                                Photo = item.Photo,
                                Cost = (float)item.Cost,
                                LastCostUpdate = DateTime.Now.ToString("dd.MM.yyyy")
                            };

                            dbNewItems.Add(newMaterial);
                            dbItems.Add(newMaterial);
                            DbController.ValidateDbItem(newMaterial);

                            materialForDBAdding.Add(newMaterial);
                            isAdded = true;
                        }
                        else
                        {
                            isAllItemsAdded = false;
                        }
                }

                if (isAdded)
                {
                    actions.Push(new ExportCalcToPrice_Action(dbNewItems, this));
                    if (!isAllItemsAdded)
                    {
                        CalcInfo_label.Content = "Не все выбранные строки строки перенесены в прайс.";
                        MessageBox.Show("Незаполненные строки не были перенесены. Проверьте корректность ввода Артикула.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        CalcInfo_label.Content = "Выбранные строки успешно перенесены в прайс.";
                    }
                    UpdateDataInSearch();
                    productsCount_label.Content = "из " + dataBaseGrid.Items.Count.ToString();
                    CalcController.ActivateNeedCalculation(this);
                }
                else
                {
                    CalcInfo_label.Content = "Все строки не были перенесены в прайс.";
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void FastSearch_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FastSearch fastSearch = new FastSearch(this);
                fastSearch.Owner = this;
                Keyboard.ClearFocus();
                fastSearch.ShowDialog();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }



        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //Сортировка
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        //Кнопка вызова сортировки
        private async void SortButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Progress<string> progress = new Progress<string>(msg => PriceInfo_label.Content = msg);
                await SortMaterialsAsync(progress);

                isSortBDSave = true;


                //Обновляем панель поиска
                UpdateDataInSearch();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                MessageBox.Show($"Возникла ошибка при сортировке: {ex.Message}");
            }
        }

        //Сама сортировка
        public async Task SortMaterialsAsync(IProgress<string> progress = null)
        {
            try
            {
                // Копируем в список для сортировки
                var tempList = dbItems.ToList();
                int total = tempList.Count;
                int processed = 0;

                progress?.Report($"Сортировка по Manufacturer...");

                var sorted = tempList
                    .OrderBy(m => NormalizeForSorting(m.Manufacturer))
                    .ThenBy(m => NormalizeForSorting(m.Type))
                    .ThenBy(m => NormalizeForSorting(m.ProductName))
                    .ToList();

                // Очищаем и перезаписываем коллекцию
                dbItems.Clear();
                foreach (var item in sorted)
                {
                    dbItems.Add(item);
                    processed++;
                    if (processed % 100 == 0 || processed == total)
                    {
                        progress?.Report($"Добавлено {processed} из {total}...");
                        await Task.Delay(1); // Даёт UI "вдохнуть", чтобы успел обновиться
                    }
                }

                progress?.Report("Сортировка завершена!");
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        //Кастомный компаратор для нужного порядка
        public static string NormalizeForSorting(string input)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(input))
                    return "9"; // Пустые строки идут в конец

                var sb = new StringBuilder();

                foreach (var c in input)
                {
                    if (char.IsDigit(c))
                    {
                        sb.Append("0" + c); // Цифры
                    }
                    else if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                    {
                        sb.Append("1" + char.ToLowerInvariant(c)); // Латиница
                    }
                    else if ((c >= 'А' && c <= 'я') || (c == 'ё') || (c == 'Ё'))
                    {
                        sb.Append("2" + char.ToLower(c)); // Кириллица
                    }
                    else
                    {
                        sb.Append("3" + c); // Прочие символы идут в конец
                    }
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Warning", new StackTrace(), "noneUser", ex);




                //if (isCalcOpened)
                //    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                //else
                //    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";


                return null;
            }

        }



        private void productImage_MouseDown(object sender, MouseButtonEventArgs e)
        { try
            {
                if (e.ClickCount == 2)
                {
                    Material selectedItem = (Material)dataBaseGrid.SelectedItem;

                    var fileImageBytes = converter.ConvertFromFileImageToByteArray(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/without_image_database.png"));
                    if (selectedItem != null)
                    {
                        if (selectedItem.ID != 0)
                        {
                            if (BitConverter.ToString(fileImageBytes) != BitConverter.ToString(selectedItem.Photo))
                            {
                                FullImagePage imagePage = new FullImagePage(selectedItem.Photo);
                                imagePage.Show();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void CalcProductImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ClickCount == 2)
                {
                    CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem;

                    var fileImageBytes = converter.ConvertFromFileImageToByteArray(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/without_image_database.png"));
                    if (selectedItem != null)
                    {
                        if (selectedItem.ID > 0)
                        {
                            if (BitConverter.ToString(fileImageBytes) != BitConverter.ToString(selectedItem.Photo))
                            {
                                FullImagePage imagePage = new FullImagePage(selectedItem.Photo);
                                imagePage.Show();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        // Метод для обмена элементами в списке
        public void Swap(ObservableCollection<CalcProduct> list, int indexA, int indexB)
        {
            try
            {
                var temp = list[indexA];
                list[indexA] = list[indexB];
                list[indexB] = temp;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void MoveUp_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<(CalcProduct, int)> movedItems = new List<(CalcProduct item, int originalIndex)>();

                int selectedIndex = CalcDataGrid.SelectedIndex;
                bool isMoving = false;
                if (selectedIndex != -1 && selectedIndex != calcItems.Count - 1 && selectedIndex != 0)
                {
                    CalcProduct selectedItem = calcItems[selectedIndex];

                    if (calcItems[selectedIndex - 1].ID == -1 && selectedIndex - 1 == 0)
                    {
                        isMoving = false;
                    }
                    else if (selectedItem.ID < 1) //Если раздел или категория
                    {
                        if (selectedItem.hideButtonContext == "+") //Если скрыт
                        {
                            //Создание и заполнение коллекции,в которой находятся элементы для перемещения
                            List<CalcProduct> selectedProducts = new List<CalcProduct> { calcItems[selectedIndex] };
                            movedItems.Add((calcItems[selectedIndex], calcItems.IndexOf(calcItems[selectedIndex])));
                            int findID = selectedItem.ID;
                            for (int i = selectedIndex + 1; i < calcItems.Count - 1; i++)
                            {
                                if ((findID == 0 && calcItems[i].ID < 1) || (findID == -1 && calcItems[i].ID == -1))
                                    break;

                                movedItems.Add((calcItems[i], calcItems.IndexOf(calcItems[i])));

                                selectedProducts.Add(calcItems[i]);
                            }

                            //Перемещение
                            if (selectedProducts[0].ID == 0) //Если перемещается свернутый раздел
                            {
                                foreach (var product in selectedProducts)
                                {
                                    for (int i = calcItems.IndexOf(product); i > 0; i--)
                                    {
                                        Swap(calcItems, i, i - 1);

                                        if (calcItems[i].ID < 1)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                            else //Если перемещается свернутая категория
                            {

                                foreach (var product in selectedProducts)
                                {
                                    for (int i = calcItems.IndexOf(product); i > 0; i--)
                                    {
                                        Swap(calcItems, i, i - 1);

                                        if (calcItems[i].ID == -1)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }

                            CalcDataGrid.SelectedItem = selectedProducts[0];
                            isMoving = true;
                        }
                        else //Если открыт
                        {
                            //Если следующим идёт раздел или категория                           //если следующим идёт именно категория
                            if ((selectedItem.ID == 0 && calcItems[selectedIndex + 1].ID < 1) || (selectedItem.ID == -1 && calcItems[selectedIndex + 1].ID == -1)) //Если пустой (разное условие для раздела и категории)
                            {
                                //Перемещение над следующим разделом
                                movedItems.Add((selectedItem, calcItems.IndexOf(selectedItem)));
                                for (int i = selectedIndex; i > 0; i--)
                                {
                                    if (calcItems[i - 1].ID != 0 && calcItems[i - 1].ID != -1)
                                    {
                                        Swap(calcItems, i, i - 1);
                                        continue;
                                    }

                                    Swap(calcItems, i, i - 1);
                                    CalcDataGrid.SelectedIndex = i - 1;
                                    break;
                                }
                                isMoving = true;
                            }
                        }
                    }
                    else
                    {
                        movedItems.Add((selectedItem, calcItems.IndexOf(selectedItem)));
                        if (calcItems[selectedIndex - 1].ID == 0) //Если раздел находится над выбранным элементов
                        {
                            if (selectedIndex - 2 >= 0) //Если над разделом есть элемент
                            {
                                if (calcItems[selectedIndex - 2].ID == -1 && selectedIndex - 2 != 0) //Если над разделом категория
                                {
                                    Swap(calcItems, selectedIndex, selectedIndex - 1);
                                    Swap(calcItems, selectedIndex - 1, selectedIndex - 2);
                                    CalcDataGrid.SelectedIndex = selectedIndex - 2;
                                    isMoving = true;
                                }
                                else if (calcItems[selectedIndex - 2].ID >= 0 && selectedIndex - 1 != 0)//Если над разделом ничего нет
                                {
                                    Swap(calcItems, selectedIndex, selectedIndex - 1);
                                    CalcDataGrid.SelectedIndex = selectedIndex - 1;
                                    isMoving = true;
                                }
                            }
                        }
                        else //Если над выбранным элементом нет раздела
                        {
                            Swap(calcItems, selectedIndex, selectedIndex - 1);
                            CalcDataGrid.SelectedIndex = selectedIndex - 1;
                            isMoving = true;
                        }

                    }
                }

                if (isMoving)
                {
                    actions.Push(new MoveCalc_Action(this, movedItems, true));
                    CalcInfo_label.Content = "Выбранная позиция успешно перенесена выше.";
                    CalcController.Refresh(CalcDataGrid, calcItems);
                }
                else
                {
                    CalcInfo_label.Content = "Выбранная позиция не может быть перемещена выше.";
                }
                e.Handled = true;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void MoveDown_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<(CalcProduct, int)> movedItems = new List<(CalcProduct item, int originalIndex)>();

                int selectedIndex = CalcDataGrid.SelectedIndex;
                bool isMoving = false;
                if (selectedIndex != -1 && selectedIndex < calcItems.Count - 2)
                {
                    CalcProduct selectedItem = calcItems[selectedIndex];
                    if (selectedItem.ID < 1) //Если раздел или категория
                    {
                        if (selectedItem.hideButtonContext == "+") //если скрыт
                        {
                            //Cоздание и заполнение коллекции,в которой находятся элементы для перемещения
                            List<CalcProduct> selectedProducts = new List<CalcProduct> { selectedItem };
                            movedItems.Add((selectedItem, calcItems.IndexOf(selectedItem)));
                            int findID = selectedItem.ID;
                            bool isBreak = false;
                            for (int i = selectedIndex + 1; i < calcItems.Count - 1; i++)
                            {
                                if ((findID == 0 && calcItems[i].ID < 1) || (findID == -1 && calcItems[i].ID == -1))
                                {
                                    isBreak = true;
                                    break;
                                }
                                movedItems.Add((calcItems[i], calcItems.IndexOf(calcItems[i])));
                                selectedProducts.Add(calcItems[i]);
                            }

                            if (isBreak)
                            {
                                //Перемещение
                                if (selectedProducts[0].ID == 0) //Если перемещается свернутый раздел
                                {
                                    foreach (var product in selectedProducts)
                                    {
                                        int chapterCount = 0;
                                        for (int i = calcItems.IndexOf(product); i < calcItems.Count - 2; i++)
                                        {
                                            if (calcItems[i + 1].ID < 1) //Если следующий элемент это категорий или раздел
                                            {
                                                //Если сейчас перемещается раздел, то количество встречаемых разделов (категорий) = 1, если обычный элемент - 2
                                                if ((product.ID < 1 && chapterCount == 1) || (product.ID >= 1 && chapterCount == 2))
                                                {
                                                    break;
                                                }

                                                chapterCount++;
                                            }

                                            Swap(calcItems, i, i + 1);
                                        }
                                    }

                                    CalcDataGrid.SelectedItem = selectedProducts[0];
                                }
                                else //Если перемещается свернутая категория
                                {
                                    int nextCategoryItemsCount = 1; //Так как начинается отсчёт не с категории а с первого элемента внутри неё
                                    int categoryCounter = 0;
                                    for (int i = calcItems.IndexOf(selectedProducts[selectedProducts.Count - 1]) + 2; i < calcItems.Count - 1; i++)
                                    {
                                        if (calcItems[i].ID != -1)
                                            nextCategoryItemsCount++;
                                        else
                                            break;
                                    }

                                    selectedProducts.Reverse();
                                    foreach (var product in selectedProducts)
                                    {
                                        int index = calcItems.IndexOf(product);
                                        for (int i = 1; i <= nextCategoryItemsCount; i++)
                                        {
                                            Swap(calcItems, index + i - 1, index + i);
                                        }
                                    }

                                    CalcDataGrid.SelectedItem = selectedProducts[selectedProducts.Count - 1];
                                }

                                isMoving = true;
                            }

                        }
                        else //если открыт
                        {
                            movedItems.Add((selectedItem, calcItems.IndexOf(selectedItem)));
                            //если следующим идёт раздел или категория                           //если следующим идёт именно категория
                            if ((selectedItem.ID == 0 && calcItems[selectedIndex + 1].ID < 1) || (selectedItem.ID == -1 && calcItems[selectedIndex + 1].ID == -1)) //если пустой (разное условие для раздела и категории)
                            {
                                //Перемещение над следующим разделом
                                int chapterCount = 0;
                                for (int i = selectedIndex; i < calcItems.Count - 2; i++)
                                {
                                    if (calcItems[i + 1].ID < 1) //Если следующий элемент это категорий или раздел
                                    {
                                        //Если количество встречаемых разделов (категорий) = 1
                                        if (chapterCount == 1)
                                        {
                                            break;
                                        }

                                        chapterCount++;
                                    }

                                    Swap(calcItems, i, i + 1);
                                    CalcDataGrid.SelectedIndex = i + 1;
                                }

                                isMoving = true;
                            }
                        }
                    }
                    else
                    {
                        movedItems.Add((selectedItem, calcItems.IndexOf(selectedItem)));
                        if (calcItems[selectedIndex + 1].ID == -1) //Если следующая категория
                        {
                            Swap(calcItems, selectedIndex, selectedIndex + 1);
                            Swap(calcItems, selectedIndex + 1, selectedIndex + 2);
                            CalcDataGrid.SelectedIndex = selectedIndex + 2;
                        }
                        else
                        {
                            Swap(calcItems, selectedIndex, selectedIndex + 1);
                            CalcDataGrid.SelectedIndex = selectedIndex + 1;
                        }

                        isMoving = true;
                    }
                }


                if (isMoving)
                {
                    actions.Push(new MoveCalc_Action(this, movedItems, false));
                    CalcInfo_label.Content = "Выбранная позиция успешно перенесена ниже.";
                    CalcController.Refresh(CalcDataGrid, calcItems);
                }
                else
                {
                    CalcInfo_label.Content = "Выбранная позиция не может быть перемещена ниже.";
                }
                e.Handled = true;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void addCaalc_menuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CalcDataGrid.SelectedItem = null;
                fileImporter.AddCalcFromFile(this);
                isCalcSaved = false;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);



                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void toggleButton_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                ToggleButton toggleButton = (ToggleButton)sender;
                toggleButton.Background = new SolidColorBrush(Colors.MediumSeaGreen);
                toggleButton.BorderBrush = new SolidColorBrush(Colors.MediumSeaGreen);

                if (!isFullProductNames)
                {
                    foreach (var column in dataBaseGrid.Columns)
                    {
                        if (column.Header.ToString() == "Наименование на английском" || column.Header.ToString() == "Ед. измерения на английском")
                        {
                            column.Visibility = Visibility.Visible;
                        }
                        if (column.Header.ToString() == "Наименование" || column.Header.ToString() == "Ед. измерения")
                        {
                            column.Visibility = Visibility.Collapsed;
                        }
                    }
                    foreach (var column in CalcDataGrid.Columns)
                    {
                        if (column.Header.ToString() == "Наименование на английском" || column.Header.ToString() == "Ед. измерения на английском")
                        {
                            column.Visibility = Visibility.Visible;
                        }
                        if (column.Header.ToString() == "Наименование" || column.Header.ToString() == "Ед. измерения")
                        {
                            column.Visibility = Visibility.Collapsed;
                        }
                    }
                }

                settings.isEnglishNameVisible = true;
                ProductName_comboBox.DisplayMemberPath = "EnglishProductName";

                var selectedItem = (Material)dataBaseGrid.SelectedItem;
                if (selectedItem != null)
                {
                    ProductNameInformation_textBox.Text = selectedItem.EnglishProductName;
                    UnitInformation_textBox.Text = selectedItem.EnglishUnit;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                

                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void toggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                ToggleButton toggleButton = (ToggleButton)sender;
                toggleButton.Background = new SolidColorBrush(Colors.OrangeRed);
                toggleButton.BorderBrush = new SolidColorBrush(Colors.OrangeRed);

                if (!isFullProductNames)
                {
                    foreach (var column in dataBaseGrid.Columns)
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
                    foreach (var column in CalcDataGrid.Columns)
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

                settings.isEnglishNameVisible = false;
                ProductName_comboBox.DisplayMemberPath = "ProductName";
                var selectedItem = (Material)dataBaseGrid.SelectedItem;
                if (selectedItem != null)
                {
                    ProductNameInformation_textBox.Text = selectedItem.ProductName;
                    UnitInformation_textBox.Text = selectedItem.Unit;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void dataBaseGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            try
            {
                MaterialBeginEdit = repository.CloneMaterial(e.Row.Item as Material);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void CalcDataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            try
            {
                var column = e.Column as DataGridColumn;

                //Если пытается редактироваться последняя строка (итого) или любой раздел / категория (кроме имени и примечания), то отменется редактирование
                if (e.Row.GetIndex() == CalcDataGrid.Items.Count - 1
                    || (calcItems[e.Row.GetIndex()].ID < 1 && column?.Header?.ToString() != "Производитель" && column?.Header?.ToString() != "Примечание"))
                {
                    e.Cancel = true; // Отменяем редактирование
                }
                else
                {
                    CalcProductBeginEdit = (e.Row.Item as CalcProduct).Clone();
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void dataGridHideButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!IsDataGridCellEditing(CalcDataGrid))
                {
                    Button currentHideButton = sender as Button;
                    string hideButtonContext = null;
                    CalcProduct currentProduct = CalcDataGrid.SelectedItem as CalcProduct;
                    if (currentProduct != null)
                    {
                        bool isVisible;
                        if (currentProduct.hideButtonContext != "+")
                        {
                            hideButtonContext = "+";
                            isVisible = false;
                        }
                        else
                        {
                            hideButtonContext = "-";
                            isVisible = true;
                        }

                        int searchingId = 0;

                        if (currentProduct.ID == -1)
                        {
                            searchingId = -1;
                        }

                        int index = calcItems.IndexOf(currentProduct);
                        for (int i = index + 1; i < calcItems.Count - 1; i++)
                        {
                            if (searchingId == 0)
                            {
                                if (calcItems[i].ID == searchingId || calcItems[i].ID == -1)
                                {
                                    break;
                                }

                                calcItems[i].isVisible = isVisible;
                                currentProduct.hideButtonContext = hideButtonContext;
                                currentHideButton.Content = hideButtonContext;
                            }
                            else
                            {
                                if (calcItems[i].ID == searchingId)
                                {
                                    break;
                                }

                                calcItems[i].isVisible = isVisible;
                                currentProduct.hideButtonContext = hideButtonContext;
                                currentHideButton.Content = hideButtonContext;

                                if (isVisible && calcItems[i].ID == 0 && calcItems[i].hideButtonContext == "+")
                                {
                                    int j = i + 1;
                                    while (j < calcItems.Count - 1)
                                    {
                                        if (calcItems[j].ID == 0)
                                        {
                                            break;
                                        }
                                        j++;
                                    }
                                    if (calcItems[j - 1].ID == -1)
                                    {
                                        break;
                                    }
                                    i = j - 1;
                                }
                            }
                        }

                        CalcDataGrid.Items.Refresh();
                        CalcDataGrid.ScrollIntoView(currentProduct);
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        public bool IsDataGridCellEditing(DataGrid dataGrid)
        {
            try
            {
                if (dataGrid.CurrentCell == null)
                    return false;

                // Получаем строку и ячейку
                var row = dataGrid.ItemContainerGenerator.ContainerFromItem(dataGrid.CurrentItem) as DataGridRow;
                if (row == null) return false;

                var cell = GetCell(dataGrid, row, dataGrid.CurrentColumn.DisplayIndex);
                return cell?.IsEditing == true;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                return false;
            }
        }

        // Вспомогательный метод для поиска ячейки
        private DataGridCell GetCell(DataGrid grid, DataGridRow row, int columnIndex)
        {
            try
            {
                var presenter = FindVisualChild<DataGridCellsPresenter>(row);
                return presenter?.ItemContainerGenerator.ContainerFromIndex(columnIndex) as DataGridCell;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                return null;

            }
}


        List<List<string>> rowColorsList = new List<List<string>>();
        private void dataGridHideButton_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                Button currentHideButton = sender as Button;
                CalcProduct currentProduct = currentHideButton.DataContext as CalcProduct;

                if (currentProduct != null)
                {
                    int index = calcItems.IndexOf(currentProduct);

                    int searchingId = 0;

                    if (currentProduct.ID == -1)
                    {
                        searchingId = -1;
                    }

                    for (int i = index + 1; i < calcItems.Count - 1; i++)
                    {
                        if (searchingId == 0)
                        {
                            if (calcItems[i].ID == searchingId || calcItems[i].ID == -1)
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (calcItems[i].ID == searchingId)
                            {
                                break;
                            }
                        }
                        rowColorsList.Add(new List<string> { calcItems[i].RowColor, calcItems[i].RowForegroundColor });
                        calcItems[i].RowColor = CalcController.ColorToHex(Colors.LightSkyBlue);
                        calcItems[i].RowForegroundColor = CalcController.ColorToHex(Colors.White);
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void dataGridHideButton_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                Button currentHideButton = sender as Button;
                CalcProduct currentProduct = currentHideButton.DataContext as CalcProduct;

                if (currentProduct != null)
                {
                    int index = calcItems.IndexOf(currentProduct);

                    int searchingId = 0;

                    if (currentProduct.ID == -1)
                    {
                        searchingId = -1;
                    }

                    for (int i = index + 1; i < calcItems.Count - 1; i++)
                    {
                        if (searchingId == 0)
                        {
                            if (calcItems[i].ID == searchingId || calcItems[i].ID == -1)
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (calcItems[i].ID == searchingId)
                            {
                                break;
                            }
                        }
                        calcItems[i].RowColor = rowColorsList[i - 1 - index][0];
                        calcItems[i].RowForegroundColor = rowColorsList[i - 1 - index][1];
                    }
                    rowColorsList.Clear();
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        //Загрузить прайс-лист из Excel
        private void LoadPriceFromExcel_buttunClic(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!isCalcOpened)
                {
                    priceCalcButton_Click(sender, e);
                }

                //Путь к загружаемому файлу
                string filePath = null;
                string directoryPath = null;
                string fileName = string.Empty;

                //Считанные данные с Excel + Картинки
                ObservableCollection<Material> buff_material;

                // Открываем диалоговое окно для выбора файла
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Excel Files|*.xlsx;*.xls",
                    Title = "Выберите Excel файл"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    filePath = openFileDialog.FileName;
                    fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
                    directoryPath = System.IO.Path.GetDirectoryName(filePath); // Путь к папке с файлом
                }

                //Проверяем пустая ли строка 
                if (filePath != null)
                {
                    //Очищаем расчётку
                    calcItems.Clear();
                    calcItems.Add(new CalcProduct
                    {
                        Manufacturer = fileName,
                        Cost = double.NaN,
                        TotalCost = double.NaN,
                        isHidingButton = true,
                        RowColor = CalcController.ColorToHex(Color.FromRgb(223, 242, 253)),
                        RowForegroundColor = CalcController.ColorToHex(Colors.Black)
                    });

                    calcItems.Add(new CalcProduct { Count = settings.FullCostType, TotalCost = 0, ID = -50 });

                    //Подтягиваем данные из Excel
                    buff_material = fileImporter.ImportPrise_ExcelTo_DB(filePath);

                    //Подтягиваем картинку
                    fileImporter.ImportPhotoToDB(buff_material, directoryPath);
                    CalcController.Add_AllMaterial_FromExcel(buff_material, CalcDataGrid, this);
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }

        }

        //Выгрузить прайс-лист в Excel
        private void UnloadPriceListToExcel_buttunClic(object sender, RoutedEventArgs e)
        {
            try
            {
                ExportPriceListToExcel priceToExcel = new ExportPriceListToExcel();
                priceToExcel.ShowDialog();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        //Обновить цену из Excel
        private void UpdatePriceFromExcel_buttunClic(object sender, RoutedEventArgs e)
        {
            try
            {
                string filePath = null;
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Excel Files|*.xlsx;*.xls",
                    Title = "Выберите Excel файл"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    filePath = openFileDialog.FileName;
                }

                if (filePath != null)
                {
                    ObservableCollection<Material> buff_material = fileImporter.ImportPrise_ExcelTo_DB(filePath);
                    foreach (Material material in buff_material)
                    {
                        Material findMaterial = dbItems.FirstOrDefault(m => m.Article == material.Article);
                        if (findMaterial != null)
                        {
                            if (findMaterial.Cost != material.Cost)
                            {
                                findMaterial.Cost = material.Cost;
                                findMaterial.LastCostUpdate = DateTime.Now.ToString("dd.MM.yyyy");
                                materialForDBUpdating.Add(findMaterial);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        //Изменить цену в прайсе
        private void ChangePriceInPrice_buttunClic(object sender, RoutedEventArgs e)
        {
            try
            {
                ChangePriceForm changePriceForm = new ChangePriceForm(this);
                changePriceForm.ShowDialog();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void secondItemDependencyDataGridButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (isAddtoDependency)
                {
                    startStopAddingDependency_button_Click(sender, e);
                }

                DependencyBeginEdit = ((Dependency)DependencyDataGrid.SelectedItem).Clone();

                isAddingSecondDependencyPosition = true;
                isAddtoDependency = true;
                selectItemForDependencies = (CalcProduct)CalcDataGrid.SelectedItem;
                CalcDataGrid.SelectedItem = null;
                selectItemForDependencies.RowColor = CalcController.ColorToHex(Colors.CornflowerBlue);
                selectItemForDependencies.RowForegroundColor = CalcController.ColorToHex(Colors.White);

                var button = (Button)sender;
                button.Visibility = Visibility.Collapsed;

                //if (sender is Button button && button.DataContext is Dependency item)
                //{
                //    if (isAddtoDependency)
                //    {
                //        startStopAddingDependency_button_Click(sender, e);
                //    }

                //    DependencyBeginEdit = ((Dependency)DependencyDataGrid.SelectedItem).Clone();

                //    isAddingSecondDependencyPosition = true;
                //    isAddtoDependency = true;
                //    selectItemForDependencies = (CalcProduct)CalcDataGrid.SelectedItem;
                //    //CalcDataGrid.SelectedItem = null;
                //    selectItemForDependencies.RowColor = CalcController.ColorToHex(Colors.CornflowerBlue);
                //    selectItemForDependencies.RowForegroundColor = CalcController.ColorToHex(Colors.White);

                //    button.Visibility = Visibility.Collapsed;
                //    DependencyDataGrid.SelectedItem = item;
                //}
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void secondItemDependencyDataGridRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Dependency selectedDependency = (Dependency)DependencyDataGrid.SelectedItem;
                DependencyBeginEdit = selectedDependency.Clone();
                if (selectedDependency != null)
                {
                    CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem;
                    selectedDependency.IsSecondButtonVisible = true;
                    selectedDependency.SecondProductId = -2;
                    selectedDependency.SecondProductName = string.Empty;
                    selectedDependency.SecondMultiplier = 1;
                    CalcController.Refresh(CalcDataGrid, calcItems);
                    actions.Push(new UpdateDependency_Action(selectedItem, DependencyBeginEdit, selectedDependency, this));

                    DependencyDataGrid.SelectedItem = null;
                    DependencyDataGrid.SelectedItem = selectedDependency;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void firstItemDependencyDataGridButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (isAddtoDependency)
                {
                    startStopAddingDependency_button_Click(sender, e);
                }

                DependencyBeginEdit = ((Dependency)DependencyDataGrid.SelectedItem).Clone();

                isAddingFirstDependencyPosition = true;
                isAddtoDependency = true;
                selectItemForDependencies = (CalcProduct)CalcDataGrid.SelectedItem;
                CalcDataGrid.SelectedItem = null;
                selectItemForDependencies.RowColor = CalcController.ColorToHex(Colors.CornflowerBlue);
                selectItemForDependencies.RowForegroundColor = CalcController.ColorToHex(Colors.White);

                var button = (Button)sender;
                button.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void firstItemDependencyDataGridRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Dependency selectedDependency = (Dependency)DependencyDataGrid.SelectedItem;
                DependencyBeginEdit = selectedDependency.Clone();
                if (selectedDependency != null)
                {
                    CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem;
                    selectedDependency.IsFirstButtonVisible = true;
                    selectedDependency.ProductId = -2;
                    selectedDependency.ProductName = string.Empty;
                    selectedDependency.Multiplier = 1;
                    CalcController.Refresh(CalcDataGrid, calcItems);
                    actions.Push(new UpdateDependency_Action(selectedItem, DependencyBeginEdit, selectedDependency, this));
                    DependencyDataGrid.SelectedItem = null;
                    DependencyDataGrid.SelectedItem = selectedDependency;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void addNewDependency_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (isAddtoDependency)
                {
                    startStopAddingDependency_button_Click(sender, e);
                }

                CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem;
                if (selectedItem != null)
                {
                    Dependency newDependency = new Dependency { Multiplier = 1, SecondMultiplier = 1 };
                    selectedItem.dependencies.Add(newDependency);
                    CalcController.Refresh(CalcDataGrid, calcItems);
                    CalcController.ValidateCalcItem(selectedItem);
                    CalcController.ActivateNeedCalculation(this);
                    CalcInfo_label.Content = "Пустая зависимость успешно добавлена.";
                    actions.Push(new AddDependency_Action(this, selectedItem, newDependency));
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void duplicateSearch_menuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var duplicates = dbItems.Select((material, index) => new { material.Article, Index = index })
                                      .GroupBy(x => x.Article)
                                      .Where(g => g.Count() > 1)
                                      .Select(g => new
                                      {
                                          Article = g.Key,
                                          Indices = g.Select(x => x.Index).ToList()
                                      });

                List<Dublicate> duplicatesList = new List<Dublicate>();
                int counter = 1;
                foreach (var entry in duplicates)
                {
                    if (entry.Indices.Count % 2 == 0)
                    {
                        for (int i = 0; i < entry.Indices.Count; i += 2)
                        {
                            duplicatesList.Add(new Dublicate { Num = counter, FirstRowIndex = (entry.Indices[i] + 1).ToString(), SecondRowIndex = (entry.Indices[i + 1] + 1).ToString(), Article = entry.Article });
                            counter++;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < entry.Indices.Count - 1; i += 2)
                        {
                            duplicatesList.Add(new Dublicate { Num = counter, FirstRowIndex = (entry.Indices[i] + 1).ToString(), SecondRowIndex = (entry.Indices[i + 1] + 1).ToString(), Article = entry.Article });
                            counter++;
                        }
                        duplicatesList.Add(new Dublicate { Num = counter, FirstRowIndex = (entry.Indices[entry.Indices.Count - 2] + 1).ToString(), SecondRowIndex = (entry.Indices[entry.Indices.Count - 1] + 1).ToString(), Article = entry.Article });
                        counter++;
                    }
                }

                if (duplicatesList.Count > 0)
                {
                    DuplicateSearchPage duplicateSearchPage = new DuplicateSearchPage(this, duplicatesList);
                    duplicateSearchPage.Show();
                }
                else
                {
                    MessageBox.Show("Повторы в прайсе не обнаружены");
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }
        private void ProductName_comboBox_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var comboBox = sender as ComboBox;
                var textBox = comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;

                if (textBox != null)
                {
                    textBox.TextChanged += ProductNameTextBox_TextChanged;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void ProductNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                //if (productNameSuppressEvent)
                //{
                //    return;
                //}

                if (!isManufacturerMainFilter && !isTypeMainFilter)
                {
                    isNoneMainFilter = true;
                }

                if (isNoneMainFilter)
                {
                    var ProductNameTextBox = ProductName_comboBox.Template.FindName("PART_EditableTextBox", ProductName_comboBox) as TextBox;
                    if (productNameSuppressEvent || !ProductNameTextBox.IsFocused)
                    {
                        productNameSuppressEvent = false;
                        return;
                    }

                    var textBox = sender as TextBox;
                    if (textBox == null) return;

                    string searchText = textBox.Text;
                    if (string.IsNullOrWhiteSpace(searchText))
                    {
                        Manufacturer_comboBox.ItemsSource = CountryManager.Instance.allManufacturers;
                        ProductName_comboBox.ItemsSource = dbItems;
                        Type_comboBox.ItemsSource = dataBaseGrid.ItemsSource.Cast<Material>().Select(i => i.Type).Distinct().ToList();
                        Article_comboBox.ItemsSource = dbItems;
                        Cost_comboBox.ItemsSource = dbItems;

                        ProductName_comboBox.Text = "";
                        Manufacturer_comboBox.Text = "";
                        Article_comboBox.Text = "";
                        Cost_comboBox.Text = "";
                        Type_comboBox.Text = "";
                    }
                    else
                    {
                        // Фильтрация элементов по введенному тексту (по началу строки)
                        var filteredItems = dbItems.Where(item =>
                            settings.isEnglishNameVisible
                                ? item.EnglishProductName.StartsWith(searchText, StringComparison.OrdinalIgnoreCase)
                                : item.ProductName.StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
                            .ToList();

                        if (filteredItems.Count == 0)
                        {
                            ProductName_comboBox.SelectedItem = null;
                        }
                        else
                        {
                            var selectManufacturer = CountryManager.Instance.allManufacturers
                                .FirstOrDefault(item => item.name == filteredItems[0].Manufacturer);

                            Manufacturer_comboBox.SelectedItem = selectManufacturer;
                            Type_comboBox.ItemsSource = filteredItems.Select(item => item.Type).Distinct().ToList();
                            Article_comboBox.ItemsSource = filteredItems;
                            Cost_comboBox.ItemsSource = filteredItems;

                            Type_comboBox.SelectedIndex = 0;
                            Article_comboBox.SelectedIndex = 0;
                            Cost_comboBox.SelectedIndex = 0;

                            ProductName_comboBox.ItemsSource = filteredItems;
                            dataBaseGrid.SelectedItem = filteredItems[0];
                            dataBaseGrid.ScrollIntoView(filteredItems[0]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }


        private void Article_comboBox_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var comboBox = sender as ComboBox;
                var textBox = comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;

                if (textBox != null)
                {
                    textBox.TextChanged += ArticleTextBox_TextChanged;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void ArticleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                //if (articleSuppressEvent)
                //{
                //    return;
                //}

                if (!isManufacturerMainFilter && !isTypeMainFilter)
                {
                    isNoneMainFilter = true;
                }

                if (isNoneMainFilter)
                {
                    var ArticleTextBox = Article_comboBox.Template.FindName("PART_EditableTextBox", Article_comboBox) as TextBox;
                    if (articleSuppressEvent || !ArticleTextBox.IsFocused)
                    {
                        articleSuppressEvent = false;
                        return;
                    }

                    var textBox = sender as TextBox;
                    string searchText = textBox.Text;
                    if (string.IsNullOrWhiteSpace(searchText))
                    {
                        Manufacturer_comboBox.ItemsSource = CountryManager.Instance.allManufacturers;
                        ProductName_comboBox.ItemsSource = dbItems;
                        Type_comboBox.ItemsSource = dataBaseGrid.ItemsSource.Cast<Material>().Select(i => i.Type).Distinct().ToList();
                        Article_comboBox.ItemsSource = dbItems;
                        Cost_comboBox.ItemsSource = dbItems;

                        ProductName_comboBox.Text = "";
                        Manufacturer_comboBox.Text = "";
                        Article_comboBox.Text = "";
                        Cost_comboBox.Text = "";
                        Type_comboBox.Text = "";
                    }
                    else
                    {
                        var filteredItems = dbItems.Where(item => item.Article.StartsWith(searchText, StringComparison.OrdinalIgnoreCase)).ToList();

                        if (filteredItems.Count == 0)
                        {
                            Article_comboBox.SelectedItem = null;
                        }
                        else
                        {
                            var selectManufacturer = CountryManager.Instance.allManufacturers.First(item => item.name == filteredItems[0].Manufacturer);
                            Manufacturer_comboBox.SelectedItem = selectManufacturer;

                            Type_comboBox.ItemsSource = filteredItems.Select(item => item.Type).Distinct().ToList();
                            ProductName_comboBox.ItemsSource = filteredItems;
                            Cost_comboBox.ItemsSource = filteredItems;

                            Type_comboBox.SelectedIndex = 0;
                            ProductName_comboBox.SelectedIndex = 0;
                            Cost_comboBox.SelectedIndex = 0;

                            Article_comboBox.ItemsSource = filteredItems;
                            dataBaseGrid.SelectedItem = filteredItems[0];
                            dataBaseGrid.ScrollIntoView(filteredItems[0]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void Type_comboBox_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var comboBox = sender as ComboBox;
                var textBox = comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;

                if (textBox != null)
                {
                    textBox.TextChanged += TypeTextBox_TextChanged;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void TypeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        { try
            {
                if (typeSuppressEvent)
                {
                    if (isNoneMainFilter)
                    {
                        isManufacturerMainFilter = false;
                        isTypeMainFilter = false;
                        typeSuppressEvent = false;
                    }
                }

                if (!isManufacturerMainFilter && !isTypeMainFilter)
                {
                    isNoneMainFilter = true;
                    //MessageBox.Show($"Manufacturer:{isManufacturerMainFilter} Type:{isTypeMainFilter} 1");
                }
                else
                {
                    if (isManufacturerMainFilter || isTypeMainFilter)
                    {
                        isNoneMainFilter = false;
                        //MessageBox.Show($"Manufacturer:{isManufacturerMainFilter} Type:{isTypeMainFilter} 2");
                    }
                    else
                    {
                        isNoneMainFilter = true;
                        //MessageBox.Show($"Manufacturer:{isManufacturerMainFilter} Type:{isTypeMainFilter} 3");
                    }
                }

                if (!isNoneMainFilter)
                {
                    if (isManufacturerMainFilter)
                    {
                        var TypeTextBox = Type_comboBox.Template.FindName("PART_EditableTextBox", Type_comboBox) as TextBox;
                        if (typeSuppressEvent || !TypeTextBox.IsFocused)
                        {
                            typeSuppressEvent = false;
                            return;
                        }

                        var textBox = sender as TextBox;
                        string searchText = textBox.Text;
                        if (string.IsNullOrWhiteSpace(searchText))
                        {
                            return;
                        }

                        var filteredItems = dbItems.Where(item => item.Type.StartsWith(searchText, StringComparison.OrdinalIgnoreCase)).ToList();

                        if (filteredItems.Count == 0)
                        {
                            Type_comboBox.SelectedItem = null;
                        }
                        else
                        {
                            var selectManufacturer = CountryManager.Instance.allManufacturers.First(item => item.name == filteredItems[0].Manufacturer);
                            Manufacturer_comboBox.SelectedItem = selectManufacturer;

                            Article_comboBox.ItemsSource = filteredItems;
                            ProductName_comboBox.ItemsSource = filteredItems;
                            Cost_comboBox.ItemsSource = filteredItems;

                            Article_comboBox.SelectedIndex = 0;
                            ProductName_comboBox.SelectedIndex = 0;
                            Cost_comboBox.SelectedIndex = 0;

                            Article_comboBox.ItemsSource = filteredItems;
                            dataBaseGrid.SelectedItem = filteredItems[0];
                            dataBaseGrid.ScrollIntoView(filteredItems[0]);
                        }
                    }
                    else
                    {
                        var textBox = sender as TextBox;
                        string searchText = textBox.Text;
                        if (true)
                        {
                            if (string.IsNullOrWhiteSpace(searchText))
                            {
                                Manufacturer_comboBox.ItemsSource = CountryManager.Instance.allManufacturers;
                                ProductName_comboBox.ItemsSource = dbItems;
                                Type_comboBox.ItemsSource = dataBaseGrid.ItemsSource.Cast<Material>().Select(i => i.Type).Distinct().ToList();
                                Article_comboBox.ItemsSource = dbItems;
                                Cost_comboBox.ItemsSource = dbItems;

                                ProductName_comboBox.Text = "";
                                Manufacturer_comboBox.Text = "";
                                Article_comboBox.Text = "";
                                Cost_comboBox.Text = "";
                                Type_comboBox.Width = 471;
                                isTypeMainFilter = false;
                            }
                            else
                            {
                                Type_comboBox.Width = 451;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void Type_comboBox_DropDownOpened(object sender, EventArgs e)
        { try
            {
                if (isNoneMainFilter)
                {

                    isNoneMainFilter = false;
                    isManufacturerMainFilter = false;
                    isTypeMainFilter = false;

                    manufacturerSuppressEvent = true;
                    typeSuppressEvent = false;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }

        }

        private void Manufacturer_comboBox_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var comboBox = sender as ComboBox;
                var textBox = comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;

                if (textBox != null)
                {
                    textBox.TextChanged += ManufacturerTextBox_TextChanged;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void ManufacturerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (manufacturerSuppressEvent)
                {
                    if (isNoneMainFilter)
                    {
                        isManufacturerMainFilter = false;
                        isTypeMainFilter = false;
                        manufacturerSuppressEvent = false;
                    }
                }

                if (!isManufacturerMainFilter && !isTypeMainFilter)
                {
                    isNoneMainFilter = true;
                    //MessageBox.Show($"Manufacturer:{isManufacturerMainFilter} Type:{isTypeMainFilter} 1");
                }
                else
                {
                    if (isManufacturerMainFilter || isTypeMainFilter)
                    {
                        isNoneMainFilter = false;
                        //MessageBox.Show($"Manufacturer:{isManufacturerMainFilter} Type:{isTypeMainFilter} 2");
                    }
                    else
                    {
                        isNoneMainFilter = true;
                        //MessageBox.Show($"Manufacturer:{isManufacturerMainFilter} Type:{isTypeMainFilter} 3");
                    }
                }

                if (!isNoneMainFilter)
                {
                    if (isManufacturerMainFilter)
                    {
                        var textBox = sender as TextBox;
                        string searchText = textBox.Text;
                        if (true)
                        {
                            if (string.IsNullOrWhiteSpace(searchText))
                            {
                                Manufacturer_comboBox.ItemsSource = CountryManager.Instance.allManufacturers;
                                ProductName_comboBox.ItemsSource = dbItems;
                                Type_comboBox.ItemsSource = dataBaseGrid.ItemsSource.Cast<Material>().Select(i => i.Type).Distinct().ToList();
                                Article_comboBox.ItemsSource = dbItems;
                                Cost_comboBox.ItemsSource = dbItems;

                                ProductName_comboBox.Text = "";
                                Type_comboBox.Text = "";
                                Article_comboBox.Text = "";
                                Cost_comboBox.Text = "";
                                Manufacturer_comboBox.Width = 311;
                                isManufacturerMainFilter = false;
                            }
                            else
                            {
                                Manufacturer_comboBox.Width = 291;
                            }
                        }
                    }
                    else
                    {
                        var ManufacturerTextBox = Manufacturer_comboBox.Template.FindName("PART_EditableTextBox", Manufacturer_comboBox) as TextBox;
                        if (manufacturerSuppressEvent || ManufacturerTextBox.IsFocused)
                        {
                            manufacturerSuppressEvent = false;
                            return;
                        }
                        var textBox = sender as TextBox;
                        string searchText = textBox.Text;
                        if (string.IsNullOrWhiteSpace(searchText))
                        {
                            return;
                        }

                        var filteredItems = dbItems.Where(item => item.Manufacturer.StartsWith(searchText, StringComparison.OrdinalIgnoreCase) && item.Type == Type_comboBox.Text).ToList();

                        if (filteredItems.Count == 0)
                        {
                            Manufacturer_comboBox.SelectedItem = null;
                        }
                        else
                        {
                            Article_comboBox.ItemsSource = filteredItems;
                            ProductName_comboBox.ItemsSource = filteredItems;
                            Cost_comboBox.ItemsSource = filteredItems;

                            Article_comboBox.SelectedIndex = 0;
                            ProductName_comboBox.SelectedIndex = 0;
                            Cost_comboBox.SelectedIndex = 0;

                            dataBaseGrid.SelectedItem = filteredItems[0];
                            dataBaseGrid.ScrollIntoView(filteredItems[0]);
                        }
                    }
                }
                else
                {
                    //var ManufacturerTextBox = Manufacturer_comboBox.Template.FindName("PART_EditableTextBox", Manufacturer_comboBox) as TextBox;
                    //if (manufacturerSuppressEvent || ManufacturerTextBox.IsFocused)
                    //{
                    //    manufacturerSuppressEvent = false;
                    //    return;
                    //}
                    //var textBox = sender as TextBox;
                    //string searchText = textBox.Text;
                    //if (string.IsNullOrWhiteSpace(searchText))
                    //{
                    //    return;
                    //}

                    //var filteredItems = dbItems.Where(item => item.Manufacturer.StartsWith(searchText, StringComparison.OrdinalIgnoreCase)).ToList();

                    //if (filteredItems.Count == 0)
                    //{
                    //    Manufacturer_comboBox.SelectedItem = null;
                    //}
                    //else
                    //{
                    //    Article_comboBox.ItemsSource = filteredItems;
                    //    ProductName_comboBox.ItemsSource = filteredItems;
                    //    Cost_comboBox.ItemsSource = filteredItems;

                    //    Article_comboBox.SelectedIndex = 0;
                    //    ProductName_comboBox.SelectedIndex = 0;
                    //    Cost_comboBox.SelectedIndex = 0;

                    //    dataBaseGrid.SelectedItem = filteredItems[0];
                    //    dataBaseGrid.ScrollIntoView(filteredItems[0]);
                    //}
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void Manufacturer_comboBox_DropDownOpened(object sender, EventArgs e)
        {
            try
            {
                if (isNoneMainFilter)
                {

                    isNoneMainFilter = false;
                    isManufacturerMainFilter = false;
                    isTypeMainFilter = false;

                    manufacturerSuppressEvent = false;
                    typeSuppressEvent = true;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void Cost_comboBox_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var comboBox = sender as ComboBox;
                var textBox = comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;

                if (textBox != null)
                {
                    textBox.TextChanged += CostTextBox_TextChanged;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }

        }

        private void CostTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                //if (costSuppressEvent)
                //{
                //    return;
                //}

                if (!isManufacturerMainFilter && !isTypeMainFilter)
                {
                    isNoneMainFilter = true;
                }

                if (isNoneMainFilter)
                {
                    var CostTextBox = Cost_comboBox.Template.FindName("PART_EditableTextBox", Cost_comboBox) as TextBox;
                    if (costSuppressEvent || !CostTextBox.IsFocused)
                    {
                        costSuppressEvent = false;
                        return;
                    }

                    var textBox = sender as TextBox;
                    string searchText = textBox.Text;
                    if (string.IsNullOrWhiteSpace(searchText))
                    {
                        Manufacturer_comboBox.ItemsSource = CountryManager.Instance.allManufacturers;
                        ProductName_comboBox.ItemsSource = dbItems;
                        Type_comboBox.ItemsSource = dataBaseGrid.ItemsSource.Cast<Material>().Select(i => i.Type).Distinct().ToList();
                        Article_comboBox.ItemsSource = dbItems;
                        Cost_comboBox.ItemsSource = dbItems;

                        ProductName_comboBox.Text = "";
                        Manufacturer_comboBox.Text = "";
                        Article_comboBox.Text = "";
                        Cost_comboBox.Text = "";
                        Type_comboBox.Text = "";
                    }
                    else
                    {
                        var filteredItems = dbItems.Where(item => item.Cost.ToString("F2").StartsWith(searchText, StringComparison.OrdinalIgnoreCase)).ToList();

                        if (filteredItems.Count == 0)
                        {
                            Cost_comboBox.SelectedItem = null;
                        }
                        else
                        {
                            var selectManufacturer = CountryManager.Instance.allManufacturers.First(item => item.name == filteredItems[0].Manufacturer);
                            Manufacturer_comboBox.SelectedItem = selectManufacturer;

                            Type_comboBox.ItemsSource = filteredItems.Select(item => item.Type).Distinct().ToList();
                            ProductName_comboBox.ItemsSource = filteredItems;
                            Article_comboBox.ItemsSource = filteredItems;

                            Type_comboBox.SelectedIndex = 0;
                            ProductName_comboBox.SelectedIndex = 0;
                            Article_comboBox.SelectedIndex = 0;

                            dataBaseGrid.SelectedItem = filteredItems[0];
                            dataBaseGrid.ScrollIntoView(filteredItems[0]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void DependencyDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                Dependency selectedDependency = (Dependency)DependencyDataGrid.SelectedItem;

                if (e.Column.DisplayIndex == 0)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (selectedDependency.Multiplier > 0)
                        {
                            CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem;
                            selectedDependency.IsFirstButtonVisible = true;
                            selectedDependency.ProductId = -2;
                            selectedDependency.ProductName = string.Empty;
                            CalcController.Refresh(CalcDataGrid, calcItems);
                            actions.Push(new UpdateDependency_Action(selectedItem, DependencyBeginEdit, selectedDependency, this));
                        }
                    }), DispatcherPriority.Background);
                }

                if (e.Column.DisplayIndex == 4)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (selectedDependency.SecondMultiplier > 0)
                        {
                            CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem;
                            selectedDependency.IsSecondButtonVisible = true;
                            selectedDependency.SecondProductId = -2;
                            selectedDependency.SecondProductName = string.Empty;
                            CalcController.Refresh(CalcDataGrid, calcItems);
                            actions.Push(new UpdateDependency_Action(selectedItem, DependencyBeginEdit, selectedDependency, this));
                        }
                    }), DispatcherPriority.Background);
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void DependencyType_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Dependency selectedDependency = (Dependency)DependencyDataGrid.SelectedItem;

                ComboBox comboBox = sender as ComboBox;
                if (comboBox != null)
                {
                    string selectedItem = comboBox.SelectedItem.ToString();
                    if (selectedItem != DependencyBeginEdit.SelectedType)
                    {
                        CalcProduct selectedCalcProduct = (CalcProduct)CalcDataGrid.SelectedItem;
                        selectedDependency.SelectedType = selectedItem;
                        actions.Push(new UpdateDependency_Action(selectedCalcProduct, DependencyBeginEdit, selectedDependency, this));
                        CalcController.Refresh(CalcDataGrid, calcItems);
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void openDepartmentRequesPage_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DepartmentRequestPage departmentRequestPage = new DepartmentRequestPage(lastDepartmentRequest);
                departmentRequestPage.Owner = this;
                shaderEffectsService.ApplyBlurEffect(this, 20);
                departmentRequestPage.ShowDialog();
                lastDepartmentRequest = departmentRequestPage.departmentRequest;
                isDepartmentRequesComplete = departmentRequestPage.isDepartmentRequestComplete;
                if (settings.isDepartmentRequestExportWithCalc == true)
                {
                    if (isDepartmentRequesComplete)
                    {
                        openDepartmentRequesPage_button.Background = new SolidColorBrush(Colors.MediumSeaGreen);
                    }
                    else
                    {
                        openDepartmentRequesPage_button.Background = new SolidColorBrush(Colors.Coral);
                    }
                }
                shaderEffectsService.ClearEffect(this);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }


        public void UpdateDataInSearch()
        {
            try
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        // Создаем временный список для обновления
                        var tempManufacturers = dataBaseGrid.ItemsSource
                            .Cast<Material>()
                            .Select(item => item.Manufacturer)
                            .Distinct()
                            .Select(name => new Manufacturer { name = name })
                            .ToList();

                        // Полностью заменяем коллекцию вместо очистки и добавления
                        CountryManager.Instance.allManufacturers = new ObservableCollection<Manufacturer>(tempManufacturers);

                        //// Обновляем ItemsSource вместо вызова Refresh()
                        //Manufacturer_comboBox.ItemsSource = CountryManager.Instance.allManufacturers;
                        //ProductName_comboBox.ItemsSource = dataBaseGrid.ItemsSource.Cast<Material>().Select(i => new { ProductName = i.ProductName }).Distinct().ToList();
                        //Type_comboBox.ItemsSource = dataBaseGrid.ItemsSource.Cast<Material>().Select(i =>  new { Type = i.Type }).Distinct().ToList();
                        //Article_comboBox.ItemsSource = dataBaseGrid.ItemsSource.Cast<Material>().Select(i => new { Article = i.Article }).Distinct().ToList();
                        //Cost_comboBox.ItemsSource = dataBaseGrid.ItemsSource.Cast<Material>().Select(i => new { Cost = i.Cost }).Distinct().ToList();


                        //Добавление ItemSource компонентам
                        Manufacturer_comboBox.ItemsSource = CountryManager.Instance.allManufacturers;
                        ProductName_comboBox.ItemsSource = dbItems;
                        List<string> types = dataBaseGrid.ItemsSource.Cast<Material>().Select(i => i.Type).Distinct().ToList();
                        Type_comboBox.ItemsSource = types;
                        Article_comboBox.ItemsSource = dbItems;
                        Cost_comboBox.ItemsSource = dbItems;



                        //Type_comboBox.Text = "";
                        //Type_comboBox.Width = 471;
                        //Manufacturer_comboBox.Text = "";
                        //Manufacturer_comboBox.Width = 311;

                        //isTypeMainFilter = false;
                        //isManufacturerMainFilter = false;
                        //isNoneMainFilter = false;

                        //manufacturerSuppressEvent = false;
                        //typeSuppressEvent = false;
                        //productNameSuppressEvent = false;
                        //articleSuppressEvent = false;
                        //costSuppressEvent = false;

                    }
                    catch (Exception ex)
                    {
                        var log = new Log_Repository();
                        log.Add("Error", new StackTrace(), "noneUser", ex);

                        // Логирование ошибки
                        Console.WriteLine($"Ошибка при обновлении данных: {ex.Message}");
                    }
                }), DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void clearManufacturer_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Manufacturer_comboBox.Text = "";
                Manufacturer_comboBox.Width = 311;

                isTypeMainFilter = false;
                isManufacturerMainFilter = false;
                isNoneMainFilter = false;

                manufacturerSuppressEvent = false;
                typeSuppressEvent = false;
                productNameSuppressEvent = false;
                articleSuppressEvent = false;
                costSuppressEvent = false;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации."; ;
            }
        }

        private void clearType_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Type_comboBox.Text = "";
                Type_comboBox.Width = 471;

                isTypeMainFilter = false;
                isManufacturerMainFilter = false;
                isNoneMainFilter = false;

                manufacturerSuppressEvent = false;
                typeSuppressEvent = false;
                productNameSuppressEvent = false;
                articleSuppressEvent = false;
                costSuppressEvent = false;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }


        bool isDbFocused;
        private void DataGridCell_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is DataGridCell cell)
                {
                    isDbFocused = true;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void DataGridCell_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is DataGridCell cell)
                {
                    isDbFocused = false;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void DependencyDataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            try
            {
                DependencyBeginEdit = (e.Row.Item as Dependency).Clone();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void log_menuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LogPage logPage = new LogPage();
                logPage.Show();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);


                if (isCalcOpened)
                    CalcInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
                else
                    PriceInfo_label.Content = "Возникла ошибка, зайдите в журнал логов для получения большей информации.";
            }
        }

        private void whatsNew_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            shaderEffectsService.ApplyBlurEffect(this, 20);

            WhatsNewPage whatsNewPage = new WhatsNewPage(fileImporter.LoadUpdates());
            whatsNewPage.Owner = this;
            whatsNewPage.ShowDialog();

            shaderEffectsService.ClearEffect(this);
        }
    }
}
