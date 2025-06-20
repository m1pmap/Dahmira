using Dahmira_Log.DAL.Model;
using Dahmira_Log.DAL.Repository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using System.Windows.Threading;

namespace Dahmira.Pages
{
    /// <summary>
    /// Логика взаимодействия для LogPage.xaml
    /// </summary>
    public partial class LogPage : Window
    {
        private ObservableCollection<Log> _logs;

        Log_Repository log_Repository = new Log_Repository();

        private readonly DispatcherTimer _timer;

        public LogPage()
        {
            try
            {
                InitializeComponent();

                _logs = new ObservableCollection<Log>(log_Repository.GetAlL().Reverse());

                // Устанавливаем привязку
                log_dataGrid.ItemsSource = _logs;

                _timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(2)
                };
                _timer.Tick += Timer_Tick;
                _timer.Start();
                Closed += (s, e) => _timer.Stop();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                var currentIds = new HashSet<uint>(_logs.Select(x => x.ID));
                var newLogs = log_Repository.GetAlL().Reverse().ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Добавляем только новые записи
                    foreach (var log in newLogs.Where(x => !currentIds.Contains(x.ID)))
                    {
                        _logs.Insert(0, log); // Добавляем в начало
                    }
                });
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

        private void clearLogs_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _logs.Clear();
                log_Repository.DeleteAll();
                log_dataGrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        string lastSelectedColum = "Ex_StackTrace";

        private void log_dataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                Log selectedLog = (Log)log_dataGrid.SelectedItem;
                if (selectedLog != null)
                {
                    entryPoint_textBox.Text = selectedLog.EntryPoint;
                    exMessage_textBox.Text = selectedLog.Ex_Mesage;

                    switch (lastSelectedColum)
                    {
                        case "Ex_GetType":
                            {
                                clickedColumn_textBox.Text = selectedLog.Ex_GetType;
                                clickedColumn_label.Content = lastSelectedColum;
                                break;
                            }
                        case "Ex_InnerExceptionMesage":
                            {
                                clickedColumn_textBox.Text = selectedLog.Ex_InnerExceptionMesage;
                                clickedColumn_label.Content = lastSelectedColum;
                                break;
                            }
                        case "Ex_StackTrace":
                            {
                                clickedColumn_textBox.Text = selectedLog.Ex_StackTrace;
                                clickedColumn_label.Content = lastSelectedColum;
                                break;
                            }
                    }

                    entryPoint_textBox.Width = 1608;

                    switch (selectedLog.Lvl.ToLower())
                    {
                        case "error":
                            {
                                logImage_image.Source = new BitmapImage(new Uri(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/errorLog.png"), UriKind.Absolute));
                                break;
                            }
                        case "information":
                            {
                                logImage_image.Source = new BitmapImage(new Uri(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/informationLog.png"), UriKind.Absolute));
                                break;
                            }
                        case "warning":
                            {
                                logImage_image.Source = new BitmapImage(new Uri(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/warningLog.png"), UriKind.Absolute));
                                break;
                            }
                        case "debug":
                            {
                                logImage_image.Source = new BitmapImage(new Uri(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/debugLog.png"), UriKind.Absolute));
                                break;
                            }
                        default:
                            {
                                entryPoint_textBox.Width = 1636;
                                break;
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

        private void DataGridCell_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (sender is DataGridCell cell)
                    {
                        Log selectedLog = (Log)log_dataGrid.SelectedItem;

                        if (log_dataGrid.CurrentColumn != null)
                        {
                            string columnName = log_dataGrid.CurrentColumn.Header?.ToString();

                            switch (columnName)
                            {
                                case "Ex_GetType":
                                    {
                                        clickedColumn_textBox.Text = selectedLog.Ex_GetType;
                                        clickedColumn_label.Content = columnName;
                                        lastSelectedColum = columnName;
                                        break;
                                    }
                                case "Ex_InnerExceptionMesage":
                                    {
                                        clickedColumn_textBox.Text = selectedLog.Ex_InnerExceptionMesage;
                                        clickedColumn_label.Content = columnName;
                                        lastSelectedColum = columnName;
                                        break;
                                    }
                                case "Ex_StackTrace":
                                    {
                                        clickedColumn_textBox.Text = selectedLog.Ex_StackTrace;
                                        clickedColumn_label.Content = columnName;
                                        lastSelectedColum = columnName;
                                        break;
                                    }
                            }
                        }
                    }
                }), DispatcherPriority.Background);
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
