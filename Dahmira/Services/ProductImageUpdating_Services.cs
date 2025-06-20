using Dahmira.Interfaces;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Dahmira_Log.DAL.Repository;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Drawing;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Drawing.Imaging;
using System.Windows.Interop;
using Dahmira_DB.DAL.Repository;
using System.Diagnostics;

namespace Dahmira.Services
{
    internal class ProductImageUpdating_Services : IProductImageUpdating
    {
        public bool UploadImageFromFile(System.Windows.Controls.Image image, MainWindow window) //Загрузка картинки из файла
        {
            try
            {
                // Открываем диалоговое окно для выбора файла
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Title = "Выберите изображение",
                    Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*"
                };

                if (openFileDialog.ShowDialog() == true) // Если файл был выбран
                {
                    string filePath = openFileDialog.FileName;

                    // Загружаем файл в поток памяти
                    byte[] imageBytes = File.ReadAllBytes(filePath);
                    using (MemoryStream memoryStream = new MemoryStream(imageBytes))
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = memoryStream;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();

                        int maxWidth = window.settings.ExcelPhotoWidth; // Максимальная ширина
                        int maxHeight = window.settings.ExcelPhotoHeight; ; // Максимальная высота

                        // Проверяем размеры изображения
                        BitmapSource resizedBitmapSource = bitmapImage;
                        if (bitmapImage.PixelWidth > maxWidth || bitmapImage.PixelHeight > maxHeight)
                        {
                            Bitmap bitmap = new Bitmap(memoryStream);
                            // Изменяем размер изображения
                            MessageBoxResult result = MessageBox.Show($"Размер картинки превышает установленный лимит в {maxHeight}x{maxWidth}  пикселей." +
                                    $"\nКартинка имеет следующие размеры: {bitmapImage.PixelWidth}x{bitmapImage.PixelHeight}. " +
                                    $"\n\nЕсли нажмёте Да - перенесётся оригинальный размер." +
                                    $"\nЕсли нажмёте Нет - размер уменьшится до необходимого." +
                                    $"\nЕсли нажмёте Отмена - ничего не вставится ячейку.", "Проблема", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);
                            if (result == MessageBoxResult.Yes)
                            {
                                resizedBitmapSource = BitmapToBitmapSource(bitmap);

                            }
                            else if (result == MessageBoxResult.No)
                            {
                                Bitmap resizedBitmap = ResizeImage(bitmap, maxWidth, maxHeight);
                                resizedBitmapSource = BitmapToBitmapSource(resizedBitmap);
                            }
                            else
                            {
                                return false;
                            }

                        }

                        image.Source = resizedBitmapSource;
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return false;
            }
        }

        public bool DownloadImageToFile(System.Windows.Controls.Image image) //Сохранение картинки в файл
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg;*.jpeg|All Files|*.*";
                saveFileDialog.Title = "Сохранить изображение товара";

                if (saveFileDialog.ShowDialog() == true)
                {
                    BitmapSource imageSource = (BitmapSource)image.Source;

                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(imageSource));

                    // Сохранение изображения в файл
                    using (FileStream fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                    {
                        encoder.Save(fileStream);
                    }

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return false;
            }
        }

        public void DeleteImage(System.Windows.Controls.Image image)  //Удаление картинки
        {
            try
            {
                image.Source = new BitmapImage(new Uri("pack://application:,,,/resources/images/without_picture.png"));

            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                MessageBox.Show("Хмм, А вот и ошибочка - 7. Найди меня) " + ex.Message);
            }
        }

        public int UploadImageFromClipboard(System.Windows.Controls.Image image, MainWindow window) // Загрузка картинки из буфера обмена
        {
            try
            {
                if (Clipboard.ContainsImage()) // Если в буфере есть изображение
                {
                    BitmapSource bitmapSource = Clipboard.GetImage();
                    Bitmap bitmapImage = BitmapSourceToBitmap(bitmapSource);

                    int maxWidth = window.settings.ExcelPhotoWidth; // Максимальная ширина
                    int maxHeight = window.settings.ExcelPhotoHeight; ; // Максимальная высота

                    // Проверяем размеры изображения
                    if (bitmapSource.PixelWidth > maxWidth || bitmapSource.PixelHeight > maxHeight)
                    {
                        // Изменяем размер изображения
                        MessageBoxResult result = MessageBox.Show($"Размер картинки в буфере превышает установленный лимит в {maxHeight}x{maxWidth}  пикселей." +
                            $"\nКартинка имеет следующие размеры: {bitmapSource.PixelWidth}x{bitmapSource.PixelHeight}. " +
                            $"\n\nЕсли нажмёте Да - перенесётся оригинальный размер." +
                            $"\nЕсли нажмёте Нет - размер уменьшится до необходимого." +
                            $"\nЕсли нажмёте Отмена - ничего не вставится ячейку.", "Проблема", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);
                        if (result == MessageBoxResult.Yes)
                        {
                            image.Source = bitmapSource;
                        }
                        else if (result == MessageBoxResult.No)
                        {
                            bitmapImage = ResizeImage(bitmapImage, maxWidth, maxHeight);
                            BitmapSource resizedBitmapSource = BitmapToBitmapSource(bitmapImage);
                            image.Source = resizedBitmapSource;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return 0;
            }
        }


        public void DownloadImageToClipboard(System.Windows.Controls.Image image) //Сохранение картинки в буфер обмена
        {
            try
            {
                BitmapSource imageSource = (BitmapSource)image.Source;
                Clipboard.SetImage(imageSource);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }

        public Bitmap ResizeImage(Bitmap image, int maxWidth, int maxHeight)
        {
            try
            {
                int newWidth = image.Width;
                int newHeight = image.Height;

                // Вычисляем новые размеры, сохраняя пропорции
                if (image.Width > maxWidth)
                {
                    newWidth = maxWidth;
                    newHeight = (image.Height * maxWidth) / image.Width;
                }

                if (newHeight > maxHeight)
                {
                    newHeight = maxHeight;
                    newWidth = (image.Width * maxHeight) / image.Height;
                }

                // Создаем новое изображение с измененными размерами
                Bitmap resizedImage = new Bitmap(newWidth, newHeight);
                using (Graphics g = Graphics.FromImage(resizedImage))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(image, 0, 0, newWidth, newHeight);
                }

                return resizedImage;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return null;
            }
        }

        public BitmapSource BitmapToBitmapSource(Bitmap bitmap)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    bitmap.Save(memoryStream, ImageFormat.Png);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memoryStream;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();

                    return bitmapImage;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return null;
            }
        }

        public Bitmap BitmapSourceToBitmap(BitmapSource bitmapSource)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                    encoder.Save(memoryStream);
                    using (Bitmap bitmap = new Bitmap(memoryStream))
                    {
                        return new Bitmap(bitmap);
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return null;
            }
        }

    }
}
