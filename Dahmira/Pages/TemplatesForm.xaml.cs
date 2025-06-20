using Dahmira_DB.DAL.Repository;
using Dahmira.Interfaces;
using Dahmira.Models;
using Dahmira.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Dahmira_Log.DAL.Repository;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


//using System.Windows.Shapes;

namespace Dahmira.Pages
{
    /// <summary>
    /// Логика взаимодействия для TemplatesForm.xaml
    /// </summary>
    public partial class TemplatesForm : Window
    {
        public ObservableCollection<TemplatesTable_Model> Table_AllFile_DAH { get; set; } = new ObservableCollection<TemplatesTable_Model>();
        public ObservableCollection<string> Chapter { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<TemplatesTable_Model> Templates { get; set; } = new ObservableCollection<TemplatesTable_Model>();

        public MainWindow mainWindow = Application.Current.MainWindow as MainWindow;




        // Списки для хранения совпадений
        private List<TemplatesTable_Model> _foundTemplates = new List<TemplatesTable_Model>();
        private List<string> _foundChapter = new List<string>();

        // Индексы для перебора совпадений
        private int _templateIndex = 0;
        private int _ChapterIndex = 0;
        private bool _template_End = false;




        //Для работы с файлами
        private IFileImporter fileImporter = new FileImporter_Services();
        private ICalcController CalcController = new CalcController_Services();
        // Флаг, указывающий на процесс скачивания
        private bool isDownloading = false;

        public TemplatesForm()
        {
            try
            {
                InitializeComponent();

                OpenCalcPage();

                Get_AllFile_DAH();

                this.DataContext = this;

                // Выделение первой ячейки
                Loaded += Templates_Loaded;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }


        // Обработчик события закрытия окна
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (isDownloading) // Если идет процесс скачивания
                {
                    MessageBox.Show("Нельзя закрыть окно, пока идет скачивание файлов.");
                    e.Cancel = true; // Отменить закрытие
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }




        public void OpenCalcPage()
        {
            try
            {
                //Открываем сразу расчетку
                if (!mainWindow.isCalcOpened)
                {
                    mainWindow.priceCalcButton_Click(mainWindow, new RoutedEventArgs());
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }


        // Обработчик события закрытия окна
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (isDownloading) // Если идет процесс скачивания
                {
                    MessageBox.Show("Нельзя закрыть окно, пока идет скачивание файлов.");
                }
                else
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



        //Первая загрузка
        public void Get_AllFile_DAH()
        {
            try
            {
                DownloadProgressBar.Visibility = Visibility.Hidden;

                Table_AllFile_DAH = new ObservableCollection<TemplatesTable_Model>();

                // Относительный путь
                string relativePath = @"Шаблоны";

                //Если нет папки создаем
                if (!Directory.Exists(relativePath))
                {
                    Directory.CreateDirectory(relativePath);
                }

                // Получение абсолютного пути
                string absolutePath = System.IO.Path.GetFullPath(relativePath);

                // Получение всех файлов с расширением .txt
                string[] files = Directory.GetFiles(absolutePath, "*.dah", SearchOption.AllDirectories);

                //Если файлов шаблонав нет
                if (files.Length == 0)
                {
                    MessageBoxResult res = MessageBox.Show("Скачать с шаблоны?", "Шаблоны отсутсвуют", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (res == MessageBoxResult.Yes)
                    {
                        DownloadTemplates();
                    }
                }

                // Добавляем, полный путь к файлу, имя аздела и имя шаблона
                for (int i = 0; i < files.Length; i++)
                {
                    var buff_data = System.IO.Path.GetFileName(files[i]).Split('_', 2);

                    if (buff_data.Length > 1)
                    {
                        Table_AllFile_DAH.Add(new TemplatesTable_Model { FullPuth = files[i], NameChapter = buff_data[0], NameTemplates_DAH = buff_data[1] });
                    }
                    else
                    {
                        Table_AllFile_DAH.Add(new TemplatesTable_Model { FullPuth = files[i], NameChapter = buff_data[0], NameTemplates_DAH = buff_data[0] });
                    }
                }

                Chapter.Clear();
                var buff = Table_AllFile_DAH.GroupBy(g => g.NameChapter).Select(s => s.First()).ToList().Select(S => S.NameChapter).ToList();
                foreach (var item in buff)
                {
                    Chapter.Add(item);
                }


                if (DG_Chapter.SelectedItem is string selectedItem)
                {
                    TB_Chapter.Text = selectedItem;
                    Templates.Clear();

                    for (int i = 0; i < Table_AllFile_DAH.Count; i++)
                    {
                        if (Table_AllFile_DAH[i].NameChapter == selectedItem)

                            Templates.Add(Table_AllFile_DAH[i]);
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




        private void Get_OpenChapter_File_DAH(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (DG_Chapter.SelectedItem is string selectedItem)
                {
                    TB_Chapter.Text = selectedItem;
                    Templates.Clear();

                    for (int i = 0; i < Table_AllFile_DAH.Count; i++)
                    {
                        if (Table_AllFile_DAH[i].NameChapter == selectedItem)
                        {
                            Templates.Add(Table_AllFile_DAH[i]);
                        }
                    }

                    //// Установка фокуса на DataGrid
                    //DG_Templates.Focus();

                    if (DG_Templates.SelectedIndex == -1)
                    {
                        // Выбор первой строки и первого столбца
                        if (DG_Templates.Items.Count > 0)
                        {
                            DG_Templates.SelectedIndex = 0; // Выбираем первую строку
                            DG_Templates.CurrentCell = new DataGridCellInfo(DG_Templates.Items[0], DG_Templates.Columns[0]); // Устанавливаем текущую ячейку
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




        //Первое выделение при полной загрузке формы
        private void Templates_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Set_FocusDataGrids();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        // Выделяем  ячейки
        public void Set_FocusDataGrids(int count = 0)
        {
            try
            {
                // Установка фокуса на DataGrid
                DG_Chapter.Focus();

                // Выбор первой строки и первого столбца
                if (DG_Chapter.Items.Count > 0)
                {
                    DG_Chapter.SelectedIndex = count; // Выбираем первую строку
                    DG_Chapter.CurrentCell = new DataGridCellInfo(DG_Chapter.Items[0], DG_Chapter.Columns[0]); // Устанавливаем текущую ячейку
                }

                // Установка фокуса на DataGrid
                DG_Templates.Focus();

                // Выбор первой строки и первого столбца
                if (DG_Templates.Items.Count > 0)
                {
                    DG_Templates.SelectedIndex = 0; // Выбираем первую строку
                    DG_Templates.CurrentCell = new DataGridCellInfo(DG_Templates.Items[0], DG_Templates.Columns[0]); // Устанавливаем текущую ячейку
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }




        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //           Изменить шаблон        !!!Сделать без основной формы!!!
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        #region
        private void ChangeCalc_Click(object sender, RoutedEventArgs e) //Открытие расчётки из шаблона
        {
            try
            {
                // Проверяем, есть ли выбранный элемент
                if (DG_Templates.SelectedItem is TemplatesTable_Model SelectedItem)
                {
                    // Получаем выбранный элемент (строку)
                    mainWindow.openCalc_Templates_Click(SelectedItem.FullPuth);
                }
                else
                {
                    MessageBox.Show("Шаблон не выбран! Выберите шаблон!");
                }

                this.Close();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }
        #endregion



        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //           Удалить шаблон
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        #region
        private void DeleteCalc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем, есть ли выбранный элемент
                if (DG_Templates.SelectedItem is TemplatesTable_Model SelectedItem)
                {
                    MessageBoxResult res = MessageBox.Show("Точно стираем?", "Уточнение", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (res == MessageBoxResult.Yes)
                    {
                        var buff_Focus = DG_Chapter.SelectedIndex;

                        FileInfo fileInf = new FileInfo(SelectedItem.FullPuth);
                        if (fileInf.Exists)
                        {
                            fileInf.Delete();
                        }

                        Get_AllFile_DAH();

                        if (DG_Chapter.Items.Count == buff_Focus)
                            buff_Focus--;

                        Set_FocusDataGrids(buff_Focus);
                    }
                }
                else
                {
                    MessageBox.Show("Шаблон не выбран! Выберите шаблон!");
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }
        #endregion



        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //           Создать шаблон
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        #region
        private void CreateTemplates_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TB_Chapter.Text == null || TB_Chapter.Text == "")
                {
                    MessageBox.Show("Сначала придумайте название раздела!", "Название раздела");
                    return;
                }

                if (TB_Templates.Text == null || TB_Templates.Text == "")
                {
                    MessageBox.Show("Сначала придумайте название шаблона!", "Название шаблона");
                    return;
                }


                var buff_Focus = DG_Chapter.SelectedIndex;

                if (mainWindow.saveTemplatesCalc("Шаблоны/" + TB_Chapter.Text + "_" + TB_Templates.Text + ".DAH"))
                {
                    Get_AllFile_DAH();

                    // Ищем элемент в коллекции по имени
                    var itemToFind = Chapter.FirstOrDefault(а => а == TB_Chapter.Text);

                    if (itemToFind != null)
                    {
                        // Выделяем найденный элемент в DataGrid
                        DG_Chapter.SelectedItem = itemToFind;

                        // Прокручиваем DataGrid к найденному элементу
                        DG_Chapter.ScrollIntoView(itemToFind);
                    }
                    else
                    {
                        Set_FocusDataGrids(buff_Focus);
                    }


                    var itemToFind1 = Templates.FirstOrDefault(а => а.NameTemplates_DAH == TB_Templates.Text + ".DAH");


                    if (itemToFind1 != null)
                    {
                        // Выделяем найденный элемент в DataGrid
                        DG_Templates.SelectedItem = itemToFind1;

                        // Прокручиваем DataGrid к найденному элементу
                        DG_Templates.ScrollIntoView(itemToFind1);
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
        #endregion



        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //           Поиск
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        #region
        // Обработчик для первого поиска
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                SearchMatches(TB_Search.Text);


                // Получаем текст для поиска
                var searchText = TB_Search.Text.ToLower();

                // Ищем по шаблонам
                var foundTemplate = Table_AllFile_DAH.FirstOrDefault(template => template.NameTemplates_DAH.ToLower().Contains(searchText));
                if (foundTemplate != null)
                {
                    DG_Chapter.SelectedItem = foundTemplate.NameChapter;      // Выбираем элемент
                    DG_Chapter.ScrollIntoView(foundTemplate.NameChapter);     // Прокручиваем к нему

                    // Если нашли в шаблонах
                    DG_Templates.SelectedItem = foundTemplate;    // Выбираем элемент
                    DG_Templates.ScrollIntoView(foundTemplate);   // Прокручиваем к нему

                    LB_info.Content = "Найдено в шаблоне";
                    LB_info.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5AAF49"));

                    return;  // Завершаем поиск
                }

                // Если не нашли в шаблонах, ищем по разделам
                var foundSection = Chapter.FirstOrDefault(section => section.ToLower().Contains(searchText));
                if (foundSection != null)
                {
                    DG_Chapter.SelectedItem = foundSection;      // Выбираем элемент
                    DG_Chapter.ScrollIntoView(foundSection);     // Прокручиваем к нему

                    LB_info.Content = "Найдено в разделе";
                    LB_info.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5AAF49"));

                    return;
                }


                LB_info.Content = "Ничего не найдено!";
                LB_info.Foreground = new SolidColorBrush(Colors.Red);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }



        // Обработчик события нажатия кнопки "Next"
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_foundTemplates.Count == 0 && _foundChapter.Count == 0)
                {

                    _ChapterIndex = 0;
                    _template_End = false;
                    _templateIndex = 0;

                    LB_info.Content = "Поиск окончен!";
                    LB_info.Foreground = new SolidColorBrush(Colors.Red);
                    return;
                }

                if (!_template_End)
                {
                    // Если есть найденные шаблоны
                    if (_foundTemplates.Count > 0)
                    {
                        _templateIndex++;
                        if (_templateIndex < _foundTemplates.Count)
                        {
                            DG_Chapter.SelectedItem = _foundTemplates[_templateIndex].NameChapter;      // Выбираем элемент
                            DG_Chapter.ScrollIntoView(_foundTemplates[_templateIndex].NameChapter);     // Прокручиваем к нему


                            DG_Templates.SelectedItem = _foundTemplates[_templateIndex];
                            DG_Templates.ScrollIntoView(_foundTemplates[_templateIndex]);

                            LB_info.Content = "Найдено в шаблоне";
                            LB_info.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5AAF49"));

                            if (_templateIndex == (_foundTemplates.Count - 1))
                            {
                                //MessageBox.Show("Больше шаблонов не найдено!");
                                _templateIndex = 0; // Сброс индекса для нового поиска
                                _template_End = true;
                            }
                        }
                    }
                }
                // Если шаблонов не найдено, ищем в разделах
                else if (_foundChapter.Count > 0)
                {
                    _ChapterIndex++;
                    if (_ChapterIndex < _foundChapter.Count)
                    {
                        DG_Chapter.SelectedItem = _foundChapter[_ChapterIndex];
                        DG_Chapter.ScrollIntoView(_foundChapter[_ChapterIndex]);

                        LB_info.Content = "Найдено в разделе";
                        LB_info.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5AAF49"));
                    }
                    else
                    {
                        //MessageBox.Show("Больше разделов не найдено!");
                        _ChapterIndex = 0; // Сброс индекса для нового поиска
                        _template_End = false;

                        LB_info.Content = "Поиск окончен!";
                        LB_info.Foreground = new SolidColorBrush(Colors.Red);
                    }
                }
                // Если ничего не найдено
                else
                {
                    //MessageBox.Show("Больше разделов не найдено!");
                    _ChapterIndex = 0; // Сброс индекса для нового поиска
                    _template_End = false;

                    LB_info.Content = "Поиск окончен!";
                    LB_info.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }


        // Метод поиска совпадений в шаблонах и разделах
        private void SearchMatches(string searchText)
        {
            try
            {
                // Очистка найденных совпадений
                _foundTemplates.Clear();
                _foundChapter.Clear();
                _templateIndex = 0;
                _ChapterIndex = 0;
                _template_End = false;

                // Поиск совпадений в шаблонах
                foreach (var section in Table_AllFile_DAH)
                {

                    if (section.NameTemplates_DAH.ToLower().Contains(searchText.ToLower()))
                    {
                        _foundTemplates.Add(section);
                    }

                }

                // Поиск совпадений в разделах
                foreach (var section in Chapter)
                {
                    if (section.ToLower().Contains(searchText.ToLower()))
                    {
                        _foundChapter.Add(section);
                    }
                }


                if (_foundTemplates.Count == 0)
                {
                    _template_End = true;
                }



                // Ищем по шаблонам
                var foundTemplate = Table_AllFile_DAH.FirstOrDefault(template => template.NameTemplates_DAH.ToLower().Contains(searchText));
                if (foundTemplate != null)
                {
                    DG_Chapter.SelectedItem = foundTemplate.NameChapter;      // Выбираем элемент
                    DG_Chapter.ScrollIntoView(foundTemplate.NameChapter);     // Прокручиваем к нему

                    // Если нашли в шаблонах
                    DG_Templates.SelectedItem = foundTemplate;    // Выбираем элемент
                    DG_Templates.ScrollIntoView(foundTemplate);   // Прокручиваем к нему
                    return;  // Завершаем поиск
                }

                // Если не нашли в шаблонах, ищем по разделам
                var foundSection = Chapter.FirstOrDefault(section => section.ToLower().Contains(searchText));
                if (foundSection != null)
                {
                    DG_Chapter.SelectedItem = foundSection;      // Выбираем элемент
                    DG_Chapter.ScrollIntoView(foundSection);     // Прокручиваем к нему
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }
        #endregion



        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //           Загрузка Шаблонов
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        #region
        private async void DownloadTemplates()
        {
            try
            {
                isDownloading = true;

                DownloadProgressBar.Visibility = Visibility.Visible;

                // Путь назначения для сохранения файлов
                string localPath = @"Шаблоны";

                // Создание папки, если её нет
                if (!Directory.Exists(localPath))
                {
                    Directory.CreateDirectory(localPath);
                }

                // Получение списка файлов с FTP-сервера
                List<string> files = fileImporter.GetFileListFromFtp(mainWindow.settings);

                try
                {
                    if (files.Count > 0)
                    {
                        DownloadProgressBar.Maximum = files.Count;

                        for (int i = 0; i < files.Count; i++)
                        {
                            string fileName = files[i];
                            await fileImporter.DownloadFileAsync(mainWindow.settings, fileName, localPath + "\\" + fileName);

                            // Обновляем прогресс после скачивания каждого файла
                            DownloadProgressBar.Value = i + 1;

                            LB_info.Content = $"Скачан файл {i + 1} из {files.Count}";
                            LB_info.Foreground = new SolidColorBrush(Colors.MediumSeaGreen);
                        }
                        LB_info.Content = "Скачивание завершено!";
                        LB_info.Foreground = new SolidColorBrush(Colors.MediumSeaGreen);
                    }
                    else
                    {
                        LB_info.Content = "Файлы не найдены!";
                        LB_info.Foreground = new SolidColorBrush(Colors.OrangeRed);
                    }

                    DownloadProgressBar.Visibility = Visibility.Hidden;

                    isDownloading = false;

                    Get_AllFile_DAH();
                }
                catch (Exception ex)
                {
                    var log = new Log_Repository();
                    log.Add("Error", new StackTrace(), "noneUser", ex);

                    LB_info.Content = "FTP не доступен!";
                    LB_info.Foreground = new SolidColorBrush(Colors.OrangeRed);
                    MessageBox.Show("FTP сервер временно не доступен! Попробуйте позже!");

                    DownloadProgressBar.Visibility = Visibility.Hidden;

                    isDownloading = false;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }
        #endregion



        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //           Добавление в расчетку
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        #region
        private void AddCalcTempletes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем, есть ли выбранный элемент
                if (DG_Templates.SelectedItem is TemplatesTable_Model SelectedItem)
                {
                    //mainWindow.CalcDataGrid.SelectedItem = null;
                    int index = mainWindow.calcItems.Count - 1;
                    //Проверяем на невреный файл или поврежденный
                    try
                    {

                        //Альтернативная десериализацтя JSON
                        var buff_maxId = mainWindow.calcItems.Max(p => p.ID);
                        if (buff_maxId < 1) buff_maxId = 0;

                        var buff_calcList = fileImporter.Get_JsonList(SelectedItem.FullPuth, mainWindow);



                        if (!mainWindow.calcItems.Any(i => i.ID == -1) && mainWindow.calcItems.Count > 1) //если в расчёте нет категории
                        {
                            if (buff_calcList.Any(item => item.ID == -1)) //если в шаблоне есть категория
                            {
                                MessageBoxResult res = MessageBox.Show("В шаблоне присутствуют категории. Добавить шаблон с категориями?" +
                                "\nДа - шаблон добавится с категорией в конец, а в начало расчёта будет добавлена категория." +
                                "\nНет — шаблон будет добавлен без категорий под выделенную строку.",
                                "Добавление шаблона", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);

                                if (res == MessageBoxResult.No)
                                {
                                    for (int i = buff_calcList.Count - 1; i >= 0; i--)
                                    {
                                        if (buff_calcList[i].ID == -1)
                                        {
                                            buff_calcList.RemoveAt(i);
                                        }
                                    }

                                    index = mainWindow.CalcDataGrid.SelectedIndex + 1;
                                    if (index == -1) index = mainWindow.calcItems.Count - 1;
                                }
                                else if (res == MessageBoxResult.Yes)
                                {
                                    CalcProduct category = new CalcProduct
                                    {
                                        ID = -1,
                                        Manufacturer = "Категория",
                                        Cost = double.NaN,
                                        TotalCost = double.NaN,
                                        isHidingButton = true,
                                        RowForegroundColor = CalcController.ColorToHex(Colors.Black),
                                        RowColor = CalcController.ColorToHex(Color.FromRgb(254, 241, 230)),
                                    };

                                    mainWindow.calcItems.Insert(0, category);
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }
                        else //если в расчёте есть категория
                        {
                            if (!buff_calcList.Any(item => item.ID == -1) && mainWindow.calcItems.Count > 1) // если в шаблоне нет категории
                            {
                                MessageBoxResult res = MessageBox.Show("В шаблоне отсутствуют категории. Добавить шаблон с категориями?" +
                                "\nДа - шаблон добавится в конец с категорией." +
                                "\nНет — шаблон добавится без категории под выделенным элементом.",
                                "Добавление шаблона", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);

                                if (res == MessageBoxResult.Yes)
                                {
                                    mainWindow.calcItems.Insert(mainWindow.calcItems.Count - 1, new CalcProduct { Manufacturer = $"{SelectedItem.NameTemplates_DAH}", ID = -1, isHidingButton = true, RowColor = CalcController.ColorToHex(Color.FromRgb(254, 241, 230)) });
                                    index = mainWindow.calcItems.Count - 1;
                                }
                                else if (res == MessageBoxResult.No)
                                {
                                    index = mainWindow.CalcDataGrid.SelectedIndex + 1;
                                    if (index == -1) index = mainWindow.calcItems.Count - 1;
                                }
                                else if (res == MessageBoxResult.Cancel)
                                {
                                    return;
                                }
                            }
                        }


                        for (int i = 0; i < (buff_calcList.Count - 1); i++)
                        {
                            if (buff_calcList[i].ID > 0)
                            {
                                buff_calcList[i].ID += buff_maxId;
                                CalcController.ValidateCalcItem(buff_calcList[i]);
                                foreach (var item in buff_calcList[i].dependencies)
                                {
                                    item.ProductId += buff_maxId;
                                }
                            }

                            mainWindow.calcItems.Insert(index + i, buff_calcList[i]);
                        }

                        CalcController.Refresh(mainWindow.CalcDataGrid, mainWindow.calcItems); //Обновление
                        CalcController.ActivateNeedCalculation(mainWindow);
                    }
                    catch (System.Text.Json.JsonException)
                    {
                        string fullPath = SelectedItem.FullPuth; //Полный путь к файлу
                        List<CalcProduct> calcProducts = fileImporter.ImportCalcFromOldFileDAH(mainWindow, fullPath);

                        if (mainWindow.calcItems.Any(i => i.ID == -1)) //если в расчёте есть категория
                        {
                            if (!calcProducts.Any(item => item.ID == -1)) // если в шаблоне нет категории
                            {
                                MessageBoxResult res = MessageBox.Show("В шаблоне отсутствуют категории. Добавить шаблон с категориями?" +
                                "\nДа - шаблон добавится в конец с категорией." +
                                "\nНет — шаблон добавится без категории под выделенным элементом.",
                                "Добавление шаблона", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);

                                if (res == MessageBoxResult.Yes)
                                {
                                    mainWindow.calcItems.Insert(mainWindow.calcItems.Count - 1, new CalcProduct { Manufacturer = $"{SelectedItem.NameTemplates_DAH}", ID = -1, isHidingButton = true, RowColor = CalcController.ColorToHex(Color.FromRgb(254, 241, 230)) });
                                    index = mainWindow.calcItems.Count - 1;
                                }
                                else if (res == MessageBoxResult.No)
                                {
                                    index = mainWindow.CalcDataGrid.SelectedIndex + 1;
                                    if (index == -1) index = mainWindow.calcItems.Count - 1;
                                }
                                else if (res == MessageBoxResult.Cancel)
                                {
                                    return;
                                }
                            }
                        }

                        for (int i = 0; i < calcProducts.Count - 1; i++)
                        {
                            if (calcProducts[i].ID > 0)
                                CalcController.ValidateCalcItem(calcProducts[i]);
                            mainWindow.calcItems.Insert(index + i, calcProducts[i]);
                        }
                        CalcController.Refresh(mainWindow.CalcDataGrid, mainWindow.calcItems);
                        CalcController.ActivateNeedCalculation(mainWindow);
                    }
                    catch (Exception ex)
                    {
                        var log = new Log_Repository();
                        log.Add("Error", new StackTrace(), "noneUser", ex);

                        MessageBox.Show($"Неизвестная ошибка: {ex.Message}");
                    }
                }
                else
                {
                    MessageBox.Show("Шаблон не выбран! Выберите шаблон!");
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }
        #endregion

        private void DownloadTebplate_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DownloadTemplates();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void DG_Templates_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                AddCalcTempletes_Click(sender, e);
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

