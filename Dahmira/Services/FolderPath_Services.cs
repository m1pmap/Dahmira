using Dahmira.Interfaces;
using Microsoft.Win32;
using System.Windows.Controls;
using System.IO;
using Dahmira_Log.DAL.Repository;
using System.Diagnostics;

namespace Dahmira.Services
{
    public class FolderPath_Services : IFolderPath
    {
        void IFolderPath.SelectedFolderPathToTextBox(TextBox textBox) //Отображение выбранного пути в выбраном textBox
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Title = "Выберите папку",
                    Filter = "Папки|*.*", // Устанавливаем фильтр для папок
                    CheckFileExists = false,
                    CheckPathExists = true,
                    FileName = "Папка" // Устанавливаем имя файла по умолчанию
                };

                // Открываем диалог и проверяем результат
                if (openFileDialog.ShowDialog() == true)
                {
                    // Получаем выбранный путь
                    string selectedPath = Path.GetDirectoryName(openFileDialog.FileName);

                    // Записываем путь в TextBox
                    textBox.Text = selectedPath;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }

        void IFolderPath.SelectedFolder(TextBox textBox)
        {
            try
            {
                // Создаем экземпляр FolderBrowserDialog
                var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description = "Выберите папку", // Описание диалога
                    ShowNewFolderButton = true // Разрешаем создание новой папки
                };

                // Открываем диалог и проверяем результат
                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // Получаем выбранный путь
                    string selectedPath = folderBrowserDialog.SelectedPath;

                    // Записываем путь в TextBox
                    textBox.Text = selectedPath;
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
