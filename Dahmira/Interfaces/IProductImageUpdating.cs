using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Dahmira.Interfaces
{
    internal interface IProductImageUpdating
    {
        bool UploadImageFromFile(Image image, MainWindow window); //Загрузка картинки из файла
        bool DownloadImageToFile(Image image); //Сохранение картинки в файл
        void DeleteImage(Image image); //Удаление картинки
        int UploadImageFromClipboard(Image image, MainWindow window); //Загрузка картинки из буфера обмена
        void DownloadImageToClipboard(Image image); //Сохранение картинки в буфер обмена
    }
}
