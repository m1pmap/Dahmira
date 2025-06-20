using Dahmira_DB.DAL.Repository;
using Dahmira.Interfaces;
using Dahmira.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dahmira_Log.DAL.Repository;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Dahmira.Pages
{
    /// <summary>
    /// Логика взаимодействия для ProgressBarPage.xaml
    /// </summary>
    public partial class ProgressBarPage : Window
    {
        private CancellationTokenSource cancellationTokenSource;
        public ProgressBarPage(MainWindow window, string type)
        {
            try
            {
                InitializeComponent();

                IFileImporter importer = new FileImporter_Services();

                cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;

                switch (type)
                {
                    case "downloadDataBase":
                        {
                            try
                            {
                                // Запускаем загрузку базы данных
                                importer.ImportDBFromFTP(window, progressBar, progressBarLabel, this, cancellationToken);
                            }
                            catch (OperationCanceledException)
                            {
                                MessageBox.Show("Загрузка была прервана.");
                            }
                            catch (Exception ex)
                            {
                                var log = new Log_Repository();
                                log.Add("Error", new StackTrace(), "noneUser", ex);
                            }

                            break;
                        }
                    case "exportTemplatesOnFTP":
                        {
                            try
                            {
                                // Запускаем загрузку базы данных
                                importer.ExportTemplatesOnFTP(window, progressBar, progressBarLabel, this, cancellationToken);
                            }
                            catch (OperationCanceledException)
                            {
                                MessageBox.Show("Выгрузка была прервана.");
                            }
                            catch (Exception ex)
                            {
                                var log = new Log_Repository();
                                log.Add("Error", new StackTrace(), "noneUser", ex);

                            }
                            break;
                        }
                    case "importTemplatesFromFTP":
                        {
                            try
                            {
                                // Запускаем загрузку базы данных
                                importer.ImportTemplatesFromFTP(window, progressBar, progressBarLabel, this, cancellationToken);
                            }
                            catch (OperationCanceledException)
                            {
                                MessageBox.Show("Загрузка была прервана.");
                            }
                            catch (Exception ex)
                            {
                                var log = new Log_Repository();
                                log.Add("Error", new StackTrace(), "noneUser", ex);
                            }
                            break;
                        }
                    case "exportDataBaseOnFTP":
                        {
                            try
                            {
                                // Запускаем загрузку базы данных
                                importer.ExportDBOnFTP(window, progressBar, progressBarLabel, this, cancellationToken);
                            }
                            catch (OperationCanceledException)
                            {
                                MessageBox.Show("Загрузка была прервана.");
                            }
                            catch (Exception ex)
                            {
                                var log = new Log_Repository();
                                log.Add("Error", new StackTrace(), "noneUser", ex);
                            }
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (cancellationTokenSource != null)
                {
                    cancellationTokenSource.Cancel();  // Отменяем все асинхронные операции
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void Cancel_button_Click(object sender, RoutedEventArgs e)
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
    }
}
