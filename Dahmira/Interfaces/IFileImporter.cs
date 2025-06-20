using Dahmira_DB.DAL.Model; 
using Dahmira.Models;
using Dahmira.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Dahmira.Interfaces
{
    public interface IFileImporter
    {
        public void ExportToExcel(MainWindow window); //Экспорт расчётки в Excel
        public void ExportToExcelAsNewSheet(MainWindow window); //Экспорт расчётки в Excel в качестве нового листа
        public void ExportToExcelDepartmentRequest(DepartmentRequest departmentRequest, string filePath); //Экспорт заявки
        public void ExportToPDF(bool isImporting);  //Экспорт в PDF
        public void ExportSettingsOnFile(MainWindow window); //Экспорт настроек в файл
        public void ImportSettingsFromFile(MainWindow window); //Импорт настроек из файла
        public void ExportCountriesToFTP(SettingsParameters settings); //Экспорт стран на фтп сервера
        public void ImportCountriesFromFTP(SettingsParameters settings); //Импорт стран с фтп сервера 
        public void ExportCalcToFile(MainWindow window); //Экспорт расчётки в файл
        public void ImportCalcFromFile(MainWindow window); //Испорт расчётки из файла
        public void AddCalcFromFile(MainWindow window); //Добавление файла расчётки к существующей
        public Task ImportDBFromFTP(MainWindow window, ProgressBar progressBar, Label progressBarLabel, ProgressBarPage progressBarPage, CancellationToken cancellationToken); //Получение БД с сервера
        public Task ExportTemplatesOnFTP(MainWindow window, ProgressBar progressBar, Label progressBarLabel, ProgressBarPage progressBarPage, CancellationToken cancellationToken);
        public Task ImportTemplatesFromFTP(MainWindow window, ProgressBar progressBar, Label progressBarLabel, ProgressBarPage progressBarPage, CancellationToken cancellationToken);
        public Task ExportDBOnFTP(MainWindow window, ProgressBar progressBar, Label progressBarLabel, ProgressBarPage progressBarPage, CancellationToken cancellationToken); //Получение БД с сервера


        public void ImportCalcFromFile_StartDUH(string path, MainWindow window); //Испорт расчётки при запске dah файла
        public void ExportCalcToTemlates(MainWindow window, string patch); //Экспорт расчётки в шаблон
        public ObservableCollection<CalcProduct> Get_JsonList(string path, MainWindow window);//Возвращаем массив данных после десериализации из json
        public void RestoreDatabase(string backupFilePath, MainWindow window);

        //Такое себе решеиме, нужно подумать будет
        public List<string> GetFileListFromFtp(SettingsParameters settings);
        public async Task DownloadFileAsync(SettingsParameters settings, string ftpServerUrl, string localFilePath) { }

        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //           Выгрузка и загрузка данных в excel
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public bool ExportPriseToExcel_DB(string nameListFile, SettingsParameters settings, ObservableCollection<Material> dbItems, string patch); //Экспорт прайса в Excel из БД
        public bool ExportPhotoToJPG_DB(ObservableCollection<Material> dbItems, string patch); //Экспорт картинок в jpg из БД
        public ObservableCollection<Material> ImportPrise_ExcelTo_DB(string patch); //Импорт прайса из excel в расчет- потом в бд
        public bool ImportPhotoToDB(ObservableCollection<Material> dbItems, string patch); //Импорт картинок в БД



        public DateTime GetFileLastModified(SettingsParameters settings, string ftpUrl); //Получение даты последнего изменения файла на ссервере

        public List<CalcProduct> ImportCalcFromOldFileDAH(MainWindow mainWindow, string fullPath); //Открытие старых файлов .DAH

        public bool SendEmail(string mailFrom, string pass, string mailTo, string subject, string text); //Отправка сообщений на mail

        public List<UpdateInfo> LoadUpdates();

    }
}
