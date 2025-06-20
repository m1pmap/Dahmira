using Dahmira.Interfaces;
using Dahmira.Models;
using Dahmira.Pages;
using Dahmira.Services;
using Dahmira_Log.DAL.Repository;
using HealthPassport.Interfaces;
using HealthPassport.Services;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Dahmira
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //Обработка необработанных исключений
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception ex = args.ExceptionObject as Exception;
                File.WriteAllText("error_log.txt", ex?.ToString() ?? "Неизвестная ошибка (AppDomain)");
                MessageBox.Show("Произошла критическая ошибка. Приложение будет закрыто.",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            };

            //this.DispatcherUnhandledException += (sender, args) =>
            //{
            //    File.WriteAllText("error_log.txt", args.Exception?.ToString() ?? "Неизвестная ошибка (Dispatcher)");
            //    MessageBox.Show("Произошла ошибка в UI. Приложение будет закрыто.",
            //                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            //    args.Handled = true;
            //    Current.Shutdown();
            //};

            // Устанавливаем рабочую директорию
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            // Показываем окно загрузки
            var loadingWindow = new LoadingWindow();
            loadingWindow.Show();

            // Создаём главное окно
            var mainWindow = new MainWindow();

            // Устанавливаем главное окно
            Application.Current.MainWindow = mainWindow;

            // Привязываем прогресс
            mainWindow.OnProgressUpdate = (msg, percent) =>
            {
                loadingWindow.UpdateProgress(msg, percent);
            };

            // Выполняем инициализацию с прогрессом
            await mainWindow.InitializeWithProgress();

            // Закрываем окно загрузки
            loadingWindow.Close();

            // Обработка запуска с файлом .DAH
            if (e.Args.Length > 0)
            {
                string filePath = e.Args[0];
                mainWindow.Loaded += (s, ev) =>
                {
                    mainWindow.openCalcTest(filePath);
                };
            }

            // Показываем главное окно
            mainWindow.Show();

            IFileImporter fileImporter = new FileImporter_Services();
            var updates = fileImporter.LoadUpdates();
            if(updates != null && updates.Count > 0)
                mainWindow.softwareVersion_label.Content = updates[0].Version;

            if (mainWindow.settings.isShowUpdates)
            {
                Thread.Sleep(30);

                IShaderEffects shader = new ShaderEffects_Service();
                shader.ApplyBlurEffect(mainWindow, 20);

                WhatsNewPage whatsNewPage = new WhatsNewPage(updates);
                whatsNewPage.Owner = mainWindow;
                whatsNewPage.ShowDialog();

                shader.ClearEffect(mainWindow);
            }
        }
    }
}