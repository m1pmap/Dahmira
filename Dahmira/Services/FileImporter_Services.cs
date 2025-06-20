using Dahmira.Interfaces;
using Dahmira.Models;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using Dahmira_Log.DAL.Repository;
using Microsoft.Win32;
using Dahmira.Pages;
using System.Text.Json;
using System.Windows;
using System.Net;
using System.Text.Json.Serialization;
using Dahmira_DB.DAL.Model;
using System.Diagnostics;
using Dahmira_DB.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Net.Mail;
using HealthPassport.Interfaces;
using HealthPassport.Services;
using System.Drawing;
using OfficeOpenXml.Drawing.Controls;
using System;
using Dahmira_DB.DAL.Repository;

namespace Dahmira.Services
{
    public class FileImporter_Services : IFileImporter
    {
        void IFileImporter.ExportToExcel(MainWindow window) //Экспорт расчётки в Excel
        {
            try
            {
                MessageBoxResult res = MessageBox.Show($"Excel будет создан с вставкой:\n\nЦена для: {window.calcItems[window.calcItems.Count - 1].SelectedCountryName}", "Важное замечание", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                if(res == MessageBoxResult.Cancel)
                {
                    return;
                }
                else
                {
                    bool isSaved = false;

                    ExcelPackage.LicenseContext = LicenseContext.Commercial; //Вид лицензии

                    var package = new ExcelPackage(); //Создание нового документа
                    var worksheet = package.Workbook.Worksheets.Add("Лист1");
                    int lastColumnIndex = 10;

                    if (window.settings.IsInsertExcelPicture == false) //Если фото не добавляется, то количество столбцов меньше
                    {
                        lastColumnIndex = 9;
                    }

                    //Изменено
                    List<string> englishColumns = ["№", "Manufacturer", "Product name", "Article", "Image", "Count", "Unit", "Cost", "Full Cost", "Note"];
                    List<string> russianColumns = ["№", "Производитель", "Наименование", "Артикул", "Изображение", "Количество", "Ед. изм.", "Цена без НДС", "Сумма без НДС", "Примечание"];

                    int j = 0;
                    // Записываем заголовки столбцов
                    for (int i = 0; i < lastColumnIndex; i++)
                    {
                        if (i >= 4 && window.settings.IsInsertExcelPicture == false) //Если картинка не добавляется
                        {
                            if (window.settings.isEnglishNameVisible)
                            {
                                worksheet.Cells[1, i + 1].Value = englishColumns[j + 1];
                            }
                            else
                            {
                                worksheet.Cells[1, i + 1].Value = russianColumns[j + 1];
                            }
                        }
                        else
                        {
                            if (window.settings.isEnglishNameVisible)
                            {
                                worksheet.Cells[1, i + 1].Value = englishColumns[j];
                            }
                            else
                            {
                                worksheet.Cells[1, i + 1].Value = russianColumns[j];
                            }
                        }
                        j++;
                    }

                    //Установка стилей для Header 
                    ExcelRange titleRange = worksheet.Cells[1, 1, 1, lastColumnIndex];
                    SetRangeStyles(titleRange, window.settings.ExcelTitleColor.GetColor(), Color.Black, Color.Gray);

                    worksheet.Row(1).Height = 25; //Ширина первого ряда
                    //Установка ширины столбцов 
                    worksheet.Column(1).Width = 4.29; //Номер
                    worksheet.Column(2).Width = 21.14; //Производитель
                    worksheet.Column(3).Width = 50.86; //Наименование
                    worksheet.Column(4).Width = 22.14; //Артикул
                    worksheet.Column(lastColumnIndex - 4).Width = 12.14; //Количество
                    worksheet.Column(lastColumnIndex - 3).Width = 10.14; //Ед. измерения
                    worksheet.Column(lastColumnIndex - 2).Width = 15.43; //Цена
                    worksheet.Column(lastColumnIndex - 1).Width = 24.14; //Сумма
                    worksheet.Column(lastColumnIndex).Width = 18.43; //Примечание

                    int categoryCount = 0;

                    // Записываем данные из DataGrid
                    for (int i = 0; i < window.calcItems.Count - 1; i++)
                    {
                        CalcProduct item = window.calcItems[i];

                        if (item.ID == -1 && window.settings.IsInsertExcelCategory == false)
                        {
                            categoryCount++;
                            continue;
                        }

                        int rowIndex = i - categoryCount;

                        if (item.Photo == null) //Если фото равно нулю (Раздел)
                        {
                            //Стили для раздела
                            ExcelRange chapterRange = worksheet.Cells[rowIndex + 2, 1, rowIndex + 2, lastColumnIndex];
                            if(item.ID == -1)
                            {
                                SetRangeStyles(chapterRange, window.settings.ExcelCategoryColor.GetColor(), Color.Black, window.settings.ExcelCategoryColor.GetColor(), isMerge: false);
                                SetRangeStyles(chapterRange, window.settings.ExcelCategoryColor.GetColor(), Color.Black, Color.Gray, isMerge: false, isBorderOnlyBottom: true);
                            }
                            else
                            {
                                SetRangeStyles(chapterRange, window.settings.ExcelChapterColor.GetColor(), Color.Black, window.settings.ExcelChapterColor.GetColor(), isMerge: false);
                                SetRangeStyles(chapterRange, window.settings.ExcelChapterColor.GetColor(), Color.Black, Color.Gray, isMerge: false, isBorderOnlyBottom: true);
                            }

                            chapterRange.Style.WrapText = false;
                            worksheet.Cells[rowIndex + 2, lastColumnIndex].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            worksheet.Cells[rowIndex + 2, lastColumnIndex].Style.Border.Right.Color.SetColor(Color.Gray);


                            if (double.TryParse(item.Manufacturer, out double parseChapter))
                                worksheet.Cells[rowIndex + 2, 5].Value = parseChapter;
                            else
                                worksheet.Cells[rowIndex + 2, 5].Value = item.Manufacturer;
                            continue;
                        }

                        //Установка стилей всех данных
                        ExcelRange dataRange = worksheet.Cells[rowIndex + 2, 1, rowIndex + 2, lastColumnIndex];
                        SetRangeStyles(dataRange, window.settings.ExcelDataColor.GetColor(), Color.Black, Color.Gray);

                        //Установка стилей для примечаний
                        ExcelRange notesRange = worksheet.Cells[rowIndex + 2, lastColumnIndex];
                        SetRangeStyles(notesRange, window.settings.ExcelNotesColor.GetColor(), Color.Black, Color.Gray);

                        //Установка стилей для номера
                        ExcelRange numberRange = worksheet.Cells[rowIndex + 2, 1];
                        SetRangeStyles(numberRange, window.settings.ExcelNumberColor.GetColor(), Color.Black, Color.Gray);

                        //Добавление данных в ячейки
                        worksheet.Cells[rowIndex + 2, 1].Value = item.Num;
                        //Если производитель состоит только из цифр то и парсим в double. Убираем ошибку форматов в excel
                        if (double.TryParse(item.Manufacturer, out double parseManufacturer))
                            worksheet.Cells[rowIndex + 2, 2].Value = parseManufacturer;
                        else
                            worksheet.Cells[rowIndex + 2, 2].Value = item.Manufacturer;

                        if (window.settings.isEnglishNameVisible)
                        {
                            worksheet.Cells[rowIndex + 2, 3].Value = item.EnglishProductName;
                            worksheet.Cells[rowIndex + 2, lastColumnIndex - 3].Value = item.EnglishUnit;
                        }
                        else
                        {
                            worksheet.Cells[rowIndex + 2, 3].Value = item.ProductName;
                            worksheet.Cells[rowIndex + 2, lastColumnIndex - 3].Value = item.Unit;
                        }

                        //Если артикул состоит только из цифр то и парсим в double. Убираем ошибку форматов в excel
                        if (double.TryParse(item.Article, out double parseArticle))
                            worksheet.Cells[rowIndex + 2, 4].Value = parseArticle;
                        else
                            worksheet.Cells[rowIndex + 2, 4].Value = item.Article;


                        if (window.settings.IsInsertExcelPicture == true) //Если картинку надо добавить
                        {
                            //Установка стилей для фона фото 
                            ExcelRange photoRange = worksheet.Cells[rowIndex + 2, 5];
                            SetRangeStyles(photoRange, window.settings.ExcelPhotoBackgroundColor.GetColor(), Color.Black, Color.Gray);

                            ByteArrayToImageSourceConverter_Services converter = new ByteArrayToImageSourceConverter_Services();

                            //Конвертация картинки в Excel с заданной шириной и высотой
                            int width = 100;
                            int height = width / 2;

                            BitmapImage bitmapImage = (BitmapImage)converter.Convert(item.Photo, typeof(BitmapImage), null, CultureInfo.CurrentCulture);
                            int originalWidth = bitmapImage.PixelWidth; //Оригинальная ширина картинки
                            int originalHeight = bitmapImage.PixelHeight; //Оригинальная высота картинки
                            CalcController_Services calcController = new CalcController_Services();

                            if(calcController.ArePhotosEqual(item.Photo, converter.ConvertFromFileImageToByteArray(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/without_image_database.png"))))
                            {
                                worksheet.Cells[rowIndex + 2, 5].Value = "—";
                                worksheet.Column(5).Width = width / 6.5;
                            }
                            else
                            {
                                using (var memoryStream = new MemoryStream())
                                {
                                    int excelImageWidth = originalWidth;
                                    int excelImageHeight = originalHeight;

                                    var encoder = new PngBitmapEncoder();
                                    encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                                    encoder.Save(memoryStream);

                                    memoryStream.Position = 0;
                                    var excelImage = worksheet.Drawings.AddPicture(i.ToString(), memoryStream);

                                    if (excelImageWidth > width) //Если оригинальный размер больше необходимого
                                    {
                                        excelImageWidth = width;
                                        excelImageHeight = (int)((double)width / originalWidth * originalHeight);
                                    }

                                    if (excelImageHeight > height)
                                    {
                                        excelImageHeight = height;
                                        excelImageWidth = (int)((double)excelImageHeight / originalHeight * originalWidth);
                                    }

                                    excelImage.SetPosition(rowIndex + 1, 5, 4, (width - excelImageWidth) / 2 + 5);

                                    worksheet.Column(5).Width = width / 6.5;
                                    worksheet.Rows[rowIndex + 2].Height = (height + 10) / 1.33;

                                    excelImage.SetSize(excelImageWidth, excelImageHeight);
                                }
                            }
                        }

                        worksheet.Cells[rowIndex + 2, lastColumnIndex - 2].Style.Numberformat.Format = @" €\ * #,##0.00 ";
                        worksheet.Cells[rowIndex + 2, lastColumnIndex - 1].Style.Numberformat.Format = @" €\ * #,##0.00 ";

                        //Добавление остальных данных в ячейки
                        worksheet.Cells[rowIndex + 2, lastColumnIndex - 4].Value = Convert.ToDouble(item.Count);
                        worksheet.Cells[rowIndex + 2, lastColumnIndex - 2].Value = item.Cost;
                        worksheet.Cells[rowIndex + 2, lastColumnIndex - 1].Value = item.TotalCost;
                        worksheet.Cells[rowIndex + 2, lastColumnIndex].Value = item.Note;
                    }
                    string priceEngRus = "Цена для: ";
                    string priceType = window.settings.FullCostType;
                    if (window.settings.isEnglishNameVisible)
                    {
                        priceEngRus = "The price for: ";

                        switch (priceType)
                        {
                            case "ИТОГО:":
                                {
                                    priceType = "TOTAL:";
                                    break;
                                }
                            case "ИТОГО БЕЗ НДС:":
                                {
                                    priceType = "TOTAL WITHOUT VAT:";
                                    break;
                                }
                            case "ИТОГО С НДС:":
                                {
                                    priceType = "Total WITH VAT:";
                                    break;
                                }
                            case "ЦЕНА:":
                                {
                                    priceType = "PRICE:";
                                    break;
                                }
                        }
                    }

                    ExcelRange totalCostRng = worksheet.Cells[window.calcItems.Count - categoryCount + 2 - 1, 1, window.calcItems.Count - categoryCount + 2 - 1, lastColumnIndex];
                    SetRangeStyles(totalCostRng, Color.Transparent, Color.Black, Color.Gray);
                    worksheet.Cells[window.calcItems.Count - categoryCount + 2 - 1, lastColumnIndex - 1].Style.Numberformat.Format = @" €\ * #,##0.00 ";
                    worksheet.Cells[window.calcItems.Count - categoryCount + 2 - 1, lastColumnIndex - 1].Value = window.calcItems[window.calcItems.Count - 1].TotalCost;
                    worksheet.Cells[window.calcItems.Count - categoryCount + 2 - 1, lastColumnIndex - 1].Style.Fill.BackgroundColor.SetColor(window.settings.ExcelTitleColor.GetColor());

                    worksheet.Cells[window.calcItems.Count - categoryCount + 2 - 1, 3].Value = priceType;
                    worksheet.Cells[window.calcItems.Count - categoryCount + 2 - 1, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ExcelRange countryRng = worksheet.Cells[window.calcItems.Count - categoryCount + 4 - 1, lastColumnIndex - 2, window.calcItems.Count - categoryCount + 4 - 1, lastColumnIndex - 1];
                    SetRangeStyles(countryRng, Color.Transparent, Color.Black, Color.Gray, isMerge:false);

                    Country selectedCountry = (Country)window.allCountries_comboBox.SelectedItem;
                    worksheet.Cells[window.calcItems.Count - categoryCount + 4 - 1, lastColumnIndex - 2].Value = priceEngRus;
                    worksheet.Cells[window.calcItems.Count - categoryCount + 4 - 1, lastColumnIndex - 1].Value = selectedCountry.name;


                    //Диалоговое окно для сохранения
                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = "Excel Files (*.xlsx)|*.xlsx",
                        Title = "Сохранить Excel документ",
                        InitialDirectory = window.settings.ExcelFolderPath
                    };
                    //Сохранение по выбранному пути
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        string filePath = saveFileDialog.FileName;
                        worksheet.Protection.IsProtected = false;
                        worksheet.Protection.AllowSelectLockedCells = false;
                        package.SaveAs(new FileInfo(filePath));
                        isSaved = true;

                        if (window.settings.isDepartmentRequestExportWithCalc == true)
                        {
                            ExportToExcelDepartmentRequest(window.lastDepartmentRequest, filePath);
                        }
                    }

                    if (isSaved)
                    {
                        window.CalcInfo_label.Content = $"Расчёт успешно сохранён в Excel по пути: {saveFileDialog.FileName}.";
                    }
                    else
                    {
                        window.CalcInfo_label.Content = "Расчёт не сохранён в Excel.";
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                MessageBox.Show(ex.Message);
                window.CalcInfo_label.Content = "Расчёт не сохранён в Excel.";
            }
        }

        private void SetRangeStyles(ExcelRange range,
            Color backgroundColor,
            Color foreground,
            Color borderColor,
            string fontFamily = null,
            bool isMerge = false,
            bool isBorderOnlyBottom = false,
            ExcelHorizontalAlignment hAligment = ExcelHorizontalAlignment.Center)
        {
            try
            {
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(backgroundColor);

                range.Style.Font.Color.SetColor(foreground);

                if (borderColor != Color.Transparent)
                {
                    if (isBorderOnlyBottom)
                    {
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Color.SetColor(borderColor);
                    }
                    else
                    {
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                        range.Style.Border.Top.Color.SetColor(borderColor);
                        range.Style.Border.Bottom.Color.SetColor(borderColor);
                        range.Style.Border.Left.Color.SetColor(borderColor);
                        range.Style.Border.Right.Color.SetColor(borderColor);
                    }
                }

                if (fontFamily != null)
                {
                    range.Style.Font.Name = fontFamily;
                }

                if (isMerge) { range.Merge = true; }

                if (hAligment != null)
                {
                    range.Style.HorizontalAlignment = hAligment;
                }
                else { range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; }

                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                range.Style.WrapText = true;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }

        void IFileImporter.ExportToExcelAsNewSheet(MainWindow window) //Экспорт расчётки в Excel в качестве нового листа
        {
            try
            {
                MessageBoxResult res = MessageBox.Show($"Excel будет создан с вставкой:\n\nЦена для: {window.calcItems[window.calcItems.Count - 1].SelectedCountryName}", "Важное замечание", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                if (res == MessageBoxResult.Cancel)
                {
                    return;
                }
                else
                {
                    bool isSaved = false;

                    ExcelPackage.LicenseContext = LicenseContext.Commercial; //Лицензия
                                                                             //Диалоговое окно открытия файла
                    OpenFileDialog openFileDialog = new OpenFileDialog
                    {
                        Filter = "Excel Files (*.xlsx)|*.xlsx",
                        Title = "Добавить в Excel документ",
                        InitialDirectory = window.settings.ExcelFolderPath
                    };

                    if (openFileDialog.ShowDialog() == true)
                    {
                        //Диалоговое окно для того чтобы узнать имя нового листа Excel
                        DialogPage dialogPage = new DialogPage();
                        IShaderEffects shaderEffects = new ShaderEffects_Service();
                        shaderEffects.ApplyBlurEffect(window, 10);
                        dialogPage.ShowDialog();
                        shaderEffects.ClearEffect(window);

                        if (dialogPage.Result != string.Empty)
                        {
                            string filePath = openFileDialog.FileName;
                            var package = new ExcelPackage(new FileInfo(filePath));
                            var worksheet = package.Workbook.Worksheets.Add(dialogPage.Result);

                            int lastColumnIndex = 10;

                            if (window.settings.IsInsertExcelPicture == false) //Если фото не добавляется, то количество столбцов меньше
                            {
                                lastColumnIndex = 9;
                            }

                            List<string> englishColumns = ["№", "Manufacturer", "Product name", "Article", "Image", "Count", "Unit", "Cost", "Full Cost", "Note"];
                            List<string> russianColumns = ["№", "Производитель", "Наименование", "Артикул", "Изображение", "Количество", "Ед. изм.", "Цена  без НДС", "Сумма без НДС", "Примечание"];

                            int j = 0;
                            // Записываем заголовки столбцов
                            for (int i = 0; i < lastColumnIndex; i++)
                            {
                                if (i >= 4 && window.settings.IsInsertExcelPicture == false) //Если картинка не добавляется
                                {
                                    if (window.settings.isEnglishNameVisible)
                                    {
                                        worksheet.Cells[1, i + 1].Value = englishColumns[j + 1];
                                    }
                                    else
                                    {
                                        worksheet.Cells[1, i + 1].Value = russianColumns[j + 1];
                                    }
                                }
                                else
                                {
                                    if (window.settings.isEnglishNameVisible)
                                    {
                                        worksheet.Cells[1, i + 1].Value = englishColumns[j];
                                    }
                                    else
                                    {
                                        worksheet.Cells[1, i + 1].Value = russianColumns[j];
                                    }
                                }
                                j++;
                            }

                            //Установка стилей для Header 
                            ExcelRange titleRange = worksheet.Cells[1, 1, 1, lastColumnIndex];
                            SetRangeStyles(titleRange, window.settings.ExcelTitleColor.GetColor(), Color.Black, Color.Gray);

                            worksheet.Row(1).Height = 25; //Ширина первого ряда
                            //Установка ширины столбцов 
                            worksheet.Column(1).Width = 4.29; //Номер
                            worksheet.Column(2).Width = 21.14; //Производитель
                            worksheet.Column(3).Width = 50.86; //Наименование
                            worksheet.Column(4).Width = 22.14; //Артикул
                            worksheet.Column(lastColumnIndex - 4).Width = 12.14; //Количество
                            worksheet.Column(lastColumnIndex - 3).Width = 10.14; //Ед. измерения
                            worksheet.Column(lastColumnIndex - 2).Width = 15.43; //Цена
                            worksheet.Column(lastColumnIndex - 1).Width = 24.14; //Сумма
                            worksheet.Column(lastColumnIndex).Width = 18.43; //Примечание

                            int categoryCount = 0;
                            // Записываем данные из DataGrid
                            for (int i = 0; i < window.calcItems.Count - 1; i++)
                            {
                                CalcProduct item = window.calcItems[i];

                                if (item.ID == -1 && window.settings.IsInsertExcelCategory == false)
                                {
                                    categoryCount++;
                                    continue;
                                }

                                int rowIndex = i - categoryCount;

                                if (item.Photo == null) //Если фото равно нулю (Раздел)
                                {
                                    //Стили для раздела
                                    ExcelRange chapterRange = worksheet.Cells[rowIndex + 2, 1, rowIndex + 2, lastColumnIndex];
                                    if (item.ID == -1)
                                    {
                                        SetRangeStyles(chapterRange, window.settings.ExcelCategoryColor.GetColor(), Color.Black, window.settings.ExcelCategoryColor.GetColor(), isMerge: false);
                                        SetRangeStyles(chapterRange, window.settings.ExcelCategoryColor.GetColor(), Color.Black, Color.Gray, isMerge: false, isBorderOnlyBottom: true);
                                    }
                                    else
                                    {
                                        SetRangeStyles(chapterRange, window.settings.ExcelChapterColor.GetColor(), Color.Black, window.settings.ExcelChapterColor.GetColor(), isMerge: false);
                                        SetRangeStyles(chapterRange, window.settings.ExcelChapterColor.GetColor(), Color.Black, Color.Gray, isMerge: false, isBorderOnlyBottom: true);
                                    }

                                    chapterRange.Style.WrapText = false;
                                    worksheet.Cells[rowIndex + 2, lastColumnIndex].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                    worksheet.Cells[rowIndex + 2, lastColumnIndex].Style.Border.Right.Color.SetColor(Color.Gray);


                                    if (double.TryParse(item.Manufacturer, out double parseChapter))
                                        worksheet.Cells[rowIndex + 2, 5].Value = parseChapter;
                                    else
                                        worksheet.Cells[rowIndex + 2, 5].Value = item.Manufacturer;
                                    continue;
                                }

                                //Установка стилей всех данных
                                ExcelRange dataRange = worksheet.Cells[rowIndex + 2, 1, rowIndex + 2, lastColumnIndex];
                                SetRangeStyles(dataRange, window.settings.ExcelDataColor.GetColor(), Color.Black, Color.Gray);

                                //Установка стилей для примечаний
                                ExcelRange notesRange = worksheet.Cells[rowIndex + 2, lastColumnIndex];
                                SetRangeStyles(notesRange, window.settings.ExcelNotesColor.GetColor(), Color.Black, Color.Gray);

                                //Установка стилей для номера
                                ExcelRange numberRange = worksheet.Cells[rowIndex + 2, 1];
                                SetRangeStyles(numberRange, window.settings.ExcelNumberColor.GetColor(), Color.Black, Color.Gray);

                                //Добавление данных в ячейки
                                worksheet.Cells[rowIndex + 2, 1].Value = item.Num;
                                //Если производитель состоит только из цифр то и парсим в double. Убираем ошибку форматов в excel
                                if (double.TryParse(item.Manufacturer, out double parseManufacturer))
                                    worksheet.Cells[rowIndex + 2, 2].Value = parseManufacturer;
                                else
                                    worksheet.Cells[rowIndex + 2, 2].Value = item.Manufacturer;

                                if (window.settings.isEnglishNameVisible)
                                {
                                    worksheet.Cells[rowIndex + 2, 3].Value = item.EnglishProductName;
                                    worksheet.Cells[rowIndex + 2, lastColumnIndex - 3].Value = item.EnglishUnit;
                                }
                                else
                                {
                                    worksheet.Cells[rowIndex + 2, 3].Value = item.ProductName;
                                    worksheet.Cells[rowIndex + 2, lastColumnIndex - 3].Value = item.Unit;
                                }
                                //Если артикл состоит только из цифр то и парсим в double. Убираем ошибку форматов в excel
                                if (double.TryParse(item.Article, out double parseArticle))
                                    worksheet.Cells[rowIndex + 2, 4].Value = parseArticle;
                                else
                                    worksheet.Cells[rowIndex + 2, 4].Value = item.Article;

                                if (window.settings.IsInsertExcelPicture == true) //Если картинку надо добавить
                                {
                                    //Установка стилей для фона фото 
                                    ExcelRange photoRange = worksheet.Cells[rowIndex + 2, 5];
                                    SetRangeStyles(photoRange, window.settings.ExcelPhotoBackgroundColor.GetColor(), Color.Black, Color.Gray);

                                    ByteArrayToImageSourceConverter_Services converter = new ByteArrayToImageSourceConverter_Services();

                                    //Конвертация картинки в Excel с заданной шириной и высотой
                                    int width = 100;
                                    int height = width / 2;

                                    BitmapImage bitmapImage = (BitmapImage)converter.Convert(item.Photo, typeof(BitmapImage), null, CultureInfo.CurrentCulture);
                                    int originalWidth = bitmapImage.PixelWidth; //Оригинальная ширина картинки
                                    int originalHeight = bitmapImage.PixelHeight; //Оригинальная высота картинки
                                    CalcController_Services calcController = new CalcController_Services();

                                    if (calcController.ArePhotosEqual(item.Photo, converter.ConvertFromFileImageToByteArray(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/without_image_database.png"))))
                                    {
                                        worksheet.Cells[rowIndex + 2, 5].Value = "—";
                                        worksheet.Column(5).Width = width / 6.5;
                                    }
                                    else
                                    {
                                        using (var memoryStream = new MemoryStream())
                                        {
                                            int excelImageWidth = originalWidth;
                                            int excelImageHeight = originalHeight;

                                            var encoder = new PngBitmapEncoder();
                                            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                                            encoder.Save(memoryStream);

                                            memoryStream.Position = 0;
                                            var excelImage = worksheet.Drawings.AddPicture(i.ToString(), memoryStream);

                                            if (excelImageWidth > width) //Если оригинальный размер больше необходимого
                                            {
                                                excelImageWidth = width;
                                                excelImageHeight = (int)((double)width / originalWidth * originalHeight);
                                            }

                                            if (excelImageHeight > height)
                                            {
                                                excelImageHeight = height;
                                                excelImageWidth = (int)((double)excelImageHeight / originalHeight * originalWidth);
                                            }

                                            excelImage.SetPosition(rowIndex + 1, 5, 4, (width - excelImageWidth) / 2 + 5);

                                            worksheet.Column(5).Width = width / 6.5;
                                            worksheet.Rows[rowIndex + 2].Height = (height + 10) / 1.33;

                                            excelImage.SetSize(excelImageWidth, excelImageHeight);
                                        }
                                    }
                                }

                                worksheet.Cells[rowIndex + 2, lastColumnIndex - 2].Style.Numberformat.Format = @" €\ * #,##0.00 ";
                                worksheet.Cells[rowIndex + 2, lastColumnIndex - 1].Style.Numberformat.Format = @" €\ * #,##0.00 ";

                                //Добавление остальных данных в ячейки
                                worksheet.Cells[rowIndex + 2, lastColumnIndex - 4].Value = Convert.ToDouble(item.Count);
                                worksheet.Cells[rowIndex + 2, lastColumnIndex - 2].Value = item.Cost;
                                worksheet.Cells[rowIndex + 2, lastColumnIndex - 1].Value = item.TotalCost;
                                worksheet.Cells[rowIndex + 2, lastColumnIndex].Value = item.Note;
                            }
                            string priceEngRus = "Цена для: ";
                            string priceType = window.settings.FullCostType;
                            if (window.settings.isEnglishNameVisible)
                            {
                                priceEngRus = "The price for: ";

                                switch (priceType)
                                {
                                    case "ИТОГО:":
                                        {
                                            priceType = "TOTAL:";
                                            break;
                                        }
                                    case "ИТОГО БЕЗ НДС:":
                                        {
                                            priceType = "TOTAL WITHOUT VAT:";
                                            break;
                                        }
                                    case "ИТОГО С НДС:":
                                        {
                                            priceType = "Total WITH VAT:";
                                            break;
                                        }
                                    case "ЦЕНА:":
                                        {
                                            priceType = "PRICE:";
                                            break;
                                        }
                                }
                            }

                            ExcelRange totalCostRng = worksheet.Cells[window.calcItems.Count - categoryCount + 2 - 1, 1, window.calcItems.Count - categoryCount + 2 - 1, lastColumnIndex];
                            SetRangeStyles(totalCostRng, Color.Transparent, Color.Black, Color.Gray);
                            worksheet.Cells[window.calcItems.Count - categoryCount + 2 - 1, lastColumnIndex - 1].Style.Numberformat.Format = @" €\ * #,##0.00 ";
                            worksheet.Cells[window.calcItems.Count - categoryCount + 2 - 1, lastColumnIndex - 1].Value = window.calcItems[window.calcItems.Count - 1].TotalCost;
                            worksheet.Cells[window.calcItems.Count - categoryCount + 2 - 1, lastColumnIndex - 1].Style.Fill.BackgroundColor.SetColor(window.settings.ExcelTitleColor.GetColor());

                            worksheet.Cells[window.calcItems.Count - categoryCount + 2 - 1, 3].Value = priceType;
                            worksheet.Cells[window.calcItems.Count - categoryCount + 2 - 1, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                            ExcelRange countryRng = worksheet.Cells[window.calcItems.Count - categoryCount + 4 - 1, lastColumnIndex - 2, window.calcItems.Count - categoryCount + 4 - 1, lastColumnIndex - 1];
                            SetRangeStyles(countryRng, Color.Transparent, Color.Black, Color.Gray, isMerge: false);

                            Country selectedCountry = (Country)window.allCountries_comboBox.SelectedItem;
                            worksheet.Cells[window.calcItems.Count - categoryCount + 4 - 1, lastColumnIndex - 2].Value = priceEngRus;
                            worksheet.Cells[window.calcItems.Count - categoryCount + 4 - 1, lastColumnIndex - 1].Value = selectedCountry.name;

                            package.Save();

                            isSaved = true;
                        }
                    }

                    if (isSaved)
                    {
                        window.CalcInfo_label.Content = $"Расчётка успешно добавлена новым листом по пути: {openFileDialog.FileName}.";
                    }
                    else
                    {
                        window.CalcInfo_label.Content = "Расчёт не сохранён в существующий Excel.";
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                MessageBox.Show(ex.Message);
                window.CalcInfo_label.Content = "Расчёт не сохранён в существующий Excel.";
            }
        }

        public void ExportToExcelDepartmentRequest(DepartmentRequest departmentRequest, string filePath)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                var package = new ExcelPackage(new FileInfo(filePath));
                var worksheet = package.Workbook.Worksheets.Add("Заявка техотдела");

                worksheet.View.ShowGridLines = false;

                worksheet.PrinterSettings.PrintArea = worksheet.Cells["B2:L51"];
                worksheet.PrinterSettings.Scale = 78;
                worksheet.PrinterSettings.HorizontalCentered = true;
                worksheet.View.PageBreakView = true;

                //Установка картинки логотипа
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/fullLogo.png");
                BitmapImage bitmapImage = new BitmapImage(new Uri(path, UriKind.Absolute));
                using (var memoryStream = new MemoryStream())
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                    encoder.Save(memoryStream);
                    memoryStream.Position = 0;
                    var excelImage = worksheet.Drawings.AddPicture("logo", memoryStream);
                    excelImage.SetPosition(1, 10, 6, 0);
                    excelImage.SetSize(28);
                }

                //Номер заявки
                worksheet.Cells["C7"].Value = "№ Заявки:";
                ExcelRange requestNumRange = worksheet.Cells["D7:G7"];
                SetRangeStyles(requestNumRange, Color.Transparent, Color.Black, Color.Gray, isMerge: true, isBorderOnlyBottom: true, hAligment: ExcelHorizontalAlignment.Left);
                requestNumRange.Value = departmentRequest.RequestNum;

                //Дата
                worksheet.Cells["C8"].Value = "Дата:";
                ExcelRange dateRange = worksheet.Cells["D8:G8"];
                SetRangeStyles(dateRange, Color.Transparent, Color.Black, Color.Gray, isMerge: true, isBorderOnlyBottom: true, hAligment: ExcelHorizontalAlignment.Left);
                dateRange.Value = departmentRequest.Date.ToString("dd.MM.yyyy");

                //Менеджер
                worksheet.Cells["C9"].Value = "Менеджер:";
                ExcelRange managerRange = worksheet.Cells["D9:G9"];
                SetRangeStyles(managerRange, Color.Transparent, Color.Black, Color.Gray, isMerge: true, isBorderOnlyBottom: true, hAligment: ExcelHorizontalAlignment.Left);
                managerRange.Value = departmentRequest.Manager;



                //Клиент
                worksheet.Cells["I7"].Value = "Клиент:";
                ExcelRange clientRange = worksheet.Cells["J7:K7"];
                SetRangeStyles(clientRange, Color.Transparent, Color.Black, Color.Gray, isMerge: true, isBorderOnlyBottom: true, hAligment: ExcelHorizontalAlignment.Left);
                clientRange.Value = departmentRequest.Client;

                //Страна
                worksheet.Cells["I8"].Value = "Страна:";
                ExcelRange countryRange = worksheet.Cells["J8:K8"];
                SetRangeStyles(countryRange, Color.Transparent, Color.Black, Color.Gray, isMerge: true, isBorderOnlyBottom: true, hAligment: ExcelHorizontalAlignment.Left);
                countryRange.Value = departmentRequest.Country;

                //Локация
                worksheet.Cells["I9"].Value = "Локация:";
                ExcelRange locationRange = worksheet.Cells["J9:K9"];
                SetRangeStyles(locationRange, Color.Transparent, Color.Black, Color.Gray, isMerge: true, isBorderOnlyBottom: true, hAligment: ExcelHorizontalAlignment.Left);
                locationRange.Value = departmentRequest.Location;

                //Надпись технических данных
                ExcelRange technicalDataLabelRange = worksheet.Cells["B12:L12"];
                SetRangeStyles(technicalDataLabelRange, Color.Transparent, Color.Gray, Color.Transparent, fontFamily: "Arial Black", isMerge: true);
                technicalDataLabelRange.Value = "Технические данные";



                //Вид животного
                ExcelRange animalTypeLabelRange = worksheet.Cells["C14:E14"];
                SetRangeStyles(animalTypeLabelRange, Color.Transparent, Color.Black, Color.Transparent, isMerge: true, hAligment: ExcelHorizontalAlignment.Left);
                animalTypeLabelRange.Value = "Вид животного:";
                ExcelRange animalTypeRange = worksheet.Cells["F14:G14"];
                SetRangeStyles(animalTypeRange, Color.Transparent, Color.Black, Color.Gray, isBorderOnlyBottom: true, isMerge: true, hAligment: ExcelHorizontalAlignment.Left);
                animalTypeRange.Value = departmentRequest.AnimalType;

                //Количество голов
                ExcelRange headCountLabelRange = worksheet.Cells["C15:E15"];
                SetRangeStyles(headCountLabelRange, Color.Transparent, Color.Black, Color.Transparent, isMerge: true, hAligment: ExcelHorizontalAlignment.Left);
                headCountLabelRange.Value = "Количество голов:";
                ExcelRange headCountRange = worksheet.Cells["F15:G15"];
                SetRangeStyles(headCountRange, Color.Transparent, Color.Black, Color.Gray, isBorderOnlyBottom: true, isMerge: true, hAligment: ExcelHorizontalAlignment.Left);
                headCountRange.Value = departmentRequest.HeadCount;

                //Существующее здание
                ExcelRange isBuildingHasLabelRange = worksheet.Cells["C16:E16"];
                SetRangeStyles(isBuildingHasLabelRange, Color.Transparent, Color.Black, Color.Transparent, isMerge: true, hAligment: ExcelHorizontalAlignment.Left);
                isBuildingHasLabelRange.Value = "Существующее здание:";
                ExcelRange isBuildingHasRange = worksheet.Cells["F16:G16"];
                SetRangeStyles(isBuildingHasRange, Color.Transparent, Color.Black, Color.Gray, isBorderOnlyBottom: true, isMerge: true, hAligment: ExcelHorizontalAlignment.Left);
                isBuildingHasRange.Value = departmentRequest.IsBuildingHas ? "Да" : "Нет";



                //Группа
                ExcelRange groupLabelRange = worksheet.Cells["I14:J14"];
                SetRangeStyles(groupLabelRange, Color.Transparent, Color.Black, Color.Transparent, isMerge: true, hAligment: ExcelHorizontalAlignment.Left);
                groupLabelRange.Value = "Группа:";
                ExcelRange groupRange = worksheet.Cells["K14"];
                SetRangeStyles(groupRange, Color.Transparent, Color.Black, Color.Gray, isBorderOnlyBottom: true, isMerge: true, hAligment: ExcelHorizontalAlignment.Left);
                groupRange.Value = departmentRequest.Group;

                //Максимальный вес
                ExcelRange maxWeightLabelRange = worksheet.Cells["I15:J15"];
                SetRangeStyles(maxWeightLabelRange, Color.Transparent, Color.Black, Color.Transparent, isMerge: true, hAligment: ExcelHorizontalAlignment.Left);
                maxWeightLabelRange.Value = "Максимальный вес:";
                ExcelRange maxWeightRange = worksheet.Cells["K15"];
                SetRangeStyles(maxWeightRange, Color.Transparent, Color.Black, Color.Gray, isBorderOnlyBottom: true, isMerge: true, hAligment: ExcelHorizontalAlignment.Left);
                maxWeightRange.Value = departmentRequest.MaxWeight;

                //Количество секций
                ExcelRange sectionCountLabelRange = worksheet.Cells["I16:J16"];
                SetRangeStyles(sectionCountLabelRange, Color.Transparent, Color.Black, Color.Transparent, isMerge: true, hAligment: ExcelHorizontalAlignment.Left);
                sectionCountLabelRange.Value = "Количество секций:";
                ExcelRange sectionCountRange = worksheet.Cells["K16"];
                SetRangeStyles(sectionCountRange, Color.Transparent, Color.Black, Color.Gray, isBorderOnlyBottom: true, isMerge: true, hAligment: ExcelHorizontalAlignment.Left);
                sectionCountRange.Value = departmentRequest.SectionCount;

                worksheet.Cells["C18"].Value = "Размеры в метрах:";

                //Надписи L и W
                ExcelRange LWLabelsRange = worksheet.Cells["E19:E20"];
                SetRangeStyles(LWLabelsRange, Color.Transparent, Color.Black, Color.Transparent, hAligment: ExcelHorizontalAlignment.Right);
                worksheet.Cells["E19"].Value = "L=";
                worksheet.Cells["E20"].Value = "W=";

                ExcelRange LWRange = worksheet.Cells["F19:F20"];
                SetRangeStyles(LWRange, Color.Transparent, Color.Black, Color.Gray, isBorderOnlyBottom: true, hAligment: ExcelHorizontalAlignment.Left);
                worksheet.Cells["F19"].Value = departmentRequest.LSize;
                worksheet.Cells["F20"].Value = departmentRequest.WSize;

                //Надписи H1 и L1
                ExcelRange H1L1LabelsRange = worksheet.Cells["G19:G20"];
                SetRangeStyles(H1L1LabelsRange, Color.Transparent, Color.Black, Color.Transparent, hAligment: ExcelHorizontalAlignment.Right);
                worksheet.Cells["G19"].Value = "H1=";
                worksheet.Cells["G20"].Value = "L1=";

                ExcelRange H1L1Range = worksheet.Cells["H19:H20"];
                SetRangeStyles(H1L1Range, Color.Transparent, Color.Black, Color.Gray, isBorderOnlyBottom: true, hAligment: ExcelHorizontalAlignment.Left);
                worksheet.Cells["H19"].Value = departmentRequest.H1Size;
                worksheet.Cells["H20"].Value = departmentRequest.L1Size;

                //Надписи H2 и L2
                ExcelRange H2L2LabelsRange = worksheet.Cells["I19:I20"];
                SetRangeStyles(H2L2LabelsRange, Color.Transparent, Color.Black, Color.Transparent, hAligment: ExcelHorizontalAlignment.Right);
                worksheet.Cells["I19"].Value = "H2=";
                worksheet.Cells["I20"].Value = "L2=";

                ExcelRange H2L2Range = worksheet.Cells["J19:J20"];
                SetRangeStyles(H2L2Range, Color.Transparent, Color.Black, Color.Gray, isBorderOnlyBottom: true, hAligment: ExcelHorizontalAlignment.Left);
                worksheet.Cells["J19"].Value = departmentRequest.H2Size;
                worksheet.Cells["J20"].Value = departmentRequest.L2Size;


                //Установка картинки чертежа
                string planPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/requestPlan.png");
                BitmapImage planBitmapImage = new BitmapImage(new Uri(planPath, UriKind.Absolute));
                using (var memoryStream = new MemoryStream())
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(planBitmapImage));
                    encoder.Save(memoryStream);
                    memoryStream.Position = 0;
                    var excelImage = worksheet.Drawings.AddPicture("plan", memoryStream);
                    excelImage.SetPosition(21, 0, 2, -10);
                    excelImage.SetSize(32);
                }

                //Дополнитеьные данные системы
                ExcelRange additionalDataLabelRange = worksheet.Cells["B35:L35"];
                SetRangeStyles(additionalDataLabelRange, Color.Transparent, Color.Gray, Color.Transparent, fontFamily: "Arial Black", isMerge: true);
                additionalDataLabelRange.Value = "Дополнительные данные системы";

                //Содержание
                ExcelRange maintananceLabelRange = worksheet.Cells["C37:D37"];
                SetRangeStyles(maintananceLabelRange, Color.Transparent, Color.Black, Color.Transparent, isMerge: true, hAligment: ExcelHorizontalAlignment.Left);
                maintananceLabelRange.Value = "Содержание:";
                ExcelRange maintananceRange = worksheet.Cells["E37:K37"];
                SetRangeStyles(maintananceRange, Color.Transparent, Color.Black, Color.Gray, isBorderOnlyBottom: true, isMerge: true, hAligment: ExcelHorizontalAlignment.Left);
                var maintanancecheckbox = worksheet.Drawings.AddControl("CheckBox1", eControlType.CheckBox);
                maintanancecheckbox.SetPosition(36, 0, 1, 31);
                maintanancecheckbox.SetSize(140, 22);
                var excelmaintananceCheckBox = (ExcelControlCheckBox)maintanancecheckbox;
                excelmaintananceCheckBox.Text = "";
                if (departmentRequest.Maintanance != string.Empty)
                {
                    excelmaintananceCheckBox.Checked = eCheckState.Checked;
                    maintananceRange.Value = departmentRequest.Maintanance;
                }
                else
                    excelmaintananceCheckBox.Checked = eCheckState.Unchecked;

                //Кормление
                ExcelRange feedingLabelRange = worksheet.Cells["C39:D39"];
                SetRangeStyles(feedingLabelRange, Color.Transparent, Color.Black, Color.Transparent, isMerge: true, hAligment: ExcelHorizontalAlignment.Left);
                feedingLabelRange.Value = "Кормление:";
                ExcelRange feedingRange = worksheet.Cells["E39:K39"];
                SetRangeStyles(feedingRange, Color.Transparent, Color.Black, Color.Gray, isBorderOnlyBottom: true, isMerge: true, hAligment: ExcelHorizontalAlignment.Left);
                var feedingCheckbox = worksheet.Drawings.AddControl("CheckBox2", eControlType.CheckBox);
                feedingCheckbox.SetPosition(38, 0, 1, 31);
                feedingCheckbox.SetSize(140, 22);
                var excelFeedingCheckBox = (ExcelControlCheckBox)feedingCheckbox;
                excelFeedingCheckBox.Text = "";
                if (departmentRequest.Feeding != string.Empty)
                {
                    excelFeedingCheckBox.Checked = eCheckState.Checked;
                    feedingRange.Value = departmentRequest.Feeding;
                }
                else
                    excelFeedingCheckBox.Checked = eCheckState.Unchecked;

                //Поение
                ExcelRange wateringLabelRange = worksheet.Cells["C41:D41"];
                SetRangeStyles(wateringLabelRange, Color.Transparent, Color.Black, Color.Transparent, isMerge: true, hAligment: ExcelHorizontalAlignment.Left);
                wateringLabelRange.Value = "Поение:";
                ExcelRange wateringRange = worksheet.Cells["E41:K41"];
                SetRangeStyles(wateringRange, Color.Transparent, Color.Black, Color.Gray, isBorderOnlyBottom: true, isMerge: true, hAligment: ExcelHorizontalAlignment.Left);
                var wateringCheckbox = worksheet.Drawings.AddControl("CheckBox3", eControlType.CheckBox);
                wateringCheckbox.SetPosition(40, 0, 1, 31);
                wateringCheckbox.SetSize(140, 22);
                var excelWateringCheckBox = (ExcelControlCheckBox)wateringCheckbox;
                excelWateringCheckBox.Text = "";
                if (departmentRequest.Watering != string.Empty)
                {
                    excelWateringCheckBox.Checked = eCheckState.Checked;
                    wateringRange.Value = departmentRequest.Watering;
                }
                else
                    excelWateringCheckBox.Checked = eCheckState.Unchecked;

                //Микроклимат
                ExcelRange microclimateLabelRange = worksheet.Cells["C43:D43"];
                SetRangeStyles(microclimateLabelRange, Color.Transparent, Color.Black, Color.Transparent, isMerge: true, hAligment: ExcelHorizontalAlignment.Left);
                microclimateLabelRange.Value = "Микроклимат:";
                ExcelRange microclimateRange = worksheet.Cells["E43:K43"];
                SetRangeStyles(microclimateRange, Color.Transparent, Color.Black, Color.Gray, isBorderOnlyBottom: true, isMerge: true, hAligment: ExcelHorizontalAlignment.Left);
                var microclimateCheckbox = worksheet.Drawings.AddControl("CheckBox4", eControlType.CheckBox);
                microclimateCheckbox.SetPosition(42, 0, 1, 31);
                microclimateCheckbox.SetSize(140, 22);
                var excelMicroclimateCheckBox = (ExcelControlCheckBox)microclimateCheckbox;
                excelMicroclimateCheckBox.Text = "";
                if (departmentRequest.Microclimate != string.Empty)
                {
                    excelMicroclimateCheckBox.Checked = eCheckState.Checked;
                    microclimateRange.Value = departmentRequest.Microclimate;
                }
                else
                    excelMicroclimateCheckBox.Checked = eCheckState.Unchecked;

                //Новозоудаление
                ExcelRange manureRemovalLabelRange = worksheet.Cells["C45:D45"];
                SetRangeStyles(manureRemovalLabelRange, Color.Transparent, Color.Black, Color.Transparent, isMerge: true, hAligment: ExcelHorizontalAlignment.Left);
                manureRemovalLabelRange.Value = "Новозоудаление:";
                ExcelRange manureRemovalRange = worksheet.Cells["E45:K45"];
                SetRangeStyles(manureRemovalRange, Color.Transparent, Color.Black, Color.Gray, isBorderOnlyBottom: true, isMerge: true, hAligment: ExcelHorizontalAlignment.Left);
                var manureRemovalCheckbox = worksheet.Drawings.AddControl("CheckBox5", eControlType.CheckBox);
                manureRemovalCheckbox.SetPosition(44, 0, 1, 31);
                manureRemovalCheckbox.SetSize(140, 22);
                var excelManureRemovalCheckBox = (ExcelControlCheckBox)manureRemovalCheckbox;
                excelManureRemovalCheckBox.Text = "";
                if (departmentRequest.ManureRemoval != string.Empty)
                {
                    excelManureRemovalCheckBox.Checked = eCheckState.Checked;
                    manureRemovalRange.Value = departmentRequest.ManureRemoval;
                }
                else
                    excelManureRemovalCheckBox.Checked = eCheckState.Unchecked;

                //Электричество
                ExcelRange electricityLabelRange = worksheet.Cells["C47:D47"];
                SetRangeStyles(electricityLabelRange, Color.Transparent, Color.Black, Color.Transparent, isMerge: true, hAligment: ExcelHorizontalAlignment.Left);
                electricityLabelRange.Value = "Электричество:";
                ExcelRange electricityRange = worksheet.Cells["E47:K47"];
                SetRangeStyles(electricityRange, Color.Transparent, Color.Black, Color.Gray, isBorderOnlyBottom: true, isMerge: true, hAligment: ExcelHorizontalAlignment.Left);
                var electricityCheckbox = worksheet.Drawings.AddControl("CheckBox6", eControlType.CheckBox);
                electricityCheckbox.SetPosition(46, 0, 1, 31);
                electricityCheckbox.SetSize(140, 22);
                var excelElectricityCheckBox = (ExcelControlCheckBox)electricityCheckbox;
                excelElectricityCheckBox.Text = "";
                if (departmentRequest.Electricity != string.Empty)
                {
                    excelElectricityCheckBox.Checked = eCheckState.Checked;
                    electricityRange.Value = departmentRequest.Electricity;
                }
                else
                    excelElectricityCheckBox.Checked = eCheckState.Unchecked;

                //Примечение
                worksheet.Cells["C49"].Value = "Примечание:";
                ExcelRange noteRange = worksheet.Cells["D49:K49"];
                SetRangeStyles(noteRange, Color.Transparent, Color.Black, Color.Gray, isBorderOnlyBottom: true, isMerge: true, hAligment: ExcelHorizontalAlignment.Left);
                noteRange.Value = departmentRequest.Note;



                worksheet.Column(2).Width = 7.29;
                worksheet.Column(3).Width = 12.14;
                worksheet.Column(4).Width = 5;
                worksheet.Column(5).Width = 9;
                worksheet.Column(6).Width = 7.57;
                worksheet.Column(7).Width = 10.86;
                worksheet.Column(8).Width = 8.3;
                worksheet.Column(9).Width = 10.14;
                worksheet.Column(10).Width = 8.6;
                worksheet.Column(11).Width = 18.57;
                worksheet.Column(12).Width = 8.43;

                package.Save();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }

        void IFileImporter.ExportToPDF(bool isImporting) //Экспорт в PDF
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }

        void IFileImporter.ImportSettingsFromFile(MainWindow window) //Импорт настроек из файл
        {
            try
            {
                string filePath = "settings.json";
                string jsonString = File.ReadAllText(filePath);
                if (jsonString != string.Empty)
                {
                    window.settings = JsonSerializer.Deserialize<SettingsParameters>(jsonString);
                }
                window.settings.IsAdministrator = false;

                foreach (var columnInfo in window.settings.CalcColumnInfos)
                {
                    var column = window.CalcDataGrid.Columns.First(c => c.Header.ToString() == columnInfo.Header);
                    if (column != null)
                    {
                        column.DisplayIndex = columnInfo.DisplayIndex;
                        if (double.TryParse(columnInfo.Width.TrimEnd('*'), out double width))
                        {
                            column.Width = new DataGridLength(width, DataGridLengthUnitType.Star);
                        }
                        else
                        {
                            switch (columnInfo.DisplayIndex)
                            {
                                case 0:
                                    {
                                        column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                                        break;
                                    }
                                case 1:
                                case 4:
                                    {
                                        column.Width = new DataGridLength(6, DataGridLengthUnitType.Star);
                                        break;
                                    }
                                case 2:
                                case 3:
                                    {
                                        column.Width = new DataGridLength(30, DataGridLengthUnitType.Star);
                                        break;
                                    }
                                case 5:
                                case 7:
                                case 9:
                                case 10:
                                    {
                                        column.Width = new DataGridLength(5, DataGridLengthUnitType.Star);
                                        break;
                                    }
                                case 8:
                                case 11:
                                    {
                                        column.Width = new DataGridLength(2, DataGridLengthUnitType.Star);
                                        break;
                                    }
                                case 6:
                                case 12:
                                    {
                                        column.Width = new DataGridLength(9, DataGridLengthUnitType.Star);
                                        break;
                                    }
                            }
                        }
                    }
                }

                foreach (var columnInfo in window.settings.DBColumnInfos)
                {
                    var column = window.dataBaseGrid.Columns.First(c => c.Header.ToString() == columnInfo.Header);
                    if (column != null)
                    {
                        column.DisplayIndex = columnInfo.DisplayIndex;
                        if (double.TryParse(columnInfo.Width.TrimEnd('*'), out double width))
                        {
                            column.Width = new DataGridLength(width, DataGridLengthUnitType.Star);
                        }
                        else
                        {
                            switch (columnInfo.DisplayIndex) 
                            {
                                case 0:
                                case 4:
                                    {
                                        column.Width = new DataGridLength(20, DataGridLengthUnitType.Star);
                                        break;
                                    }
                                case 2:
                                case 3:
                                    {
                                        column.Width = new DataGridLength(50, DataGridLengthUnitType.Star);
                                        break;
                                    }
                                case 5:
                                    {
                                        column.Width = new DataGridLength(7, DataGridLengthUnitType.Star);
                                        break;
                                    }
                                case 6:
                                    {
                                        column.Width = new DataGridLength(15, DataGridLengthUnitType.Star);
                                        break;
                                    }
                                case 1:
                                case 7:
                                    {
                                        column.Width = new DataGridLength(10, DataGridLengthUnitType.Star);
                                        break;
                                    }
                                case 8:
                                    {
                                        column.Width = new DataGridLength(5, DataGridLengthUnitType.Star);
                                        break;
                                    }
                            }
                        }
                    }
                }
                if(window.settings.isIdOnCalcVisible)
                {
                    foreach (var column in window.CalcDataGrid.Columns)
                    {
                        if (column.Header.ToString() == "ID")
                        {
                            column.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Warning", new StackTrace(), "noneUser", ex);

                MessageBox.Show(ex.Message);
            }
        }

        void IFileImporter.ExportSettingsOnFile(MainWindow window) //Экспорт настроек в файла
        {
            try
            {
                window.settings.CalcColumnInfos.Clear();
                window.settings.DBColumnInfos.Clear();

                foreach (var column in window.CalcDataGrid.Columns)
                {
                    window.settings.CalcColumnInfos.Add(new ColumnInfo
                    {
                        Header = column.Header.ToString(),
                        DisplayIndex = column.DisplayIndex,
                        Width = column.Width.ToString()
                    });
                }

                foreach (var column in window.dataBaseGrid.Columns)
                {
                    window.settings.DBColumnInfos.Add(new ColumnInfo
                    {
                        Header = column.Header.ToString(),
                        DisplayIndex = column.DisplayIndex,
                        Width = column.Width.ToString()
                    });
                }

                string jsonString = JsonSerializer.Serialize(window.settings);
                string filePath = "settings.json";
                File.WriteAllText(filePath, jsonString);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                MessageBox.Show(ex.Message);
            }
        }

        void IFileImporter.ImportCountriesFromFTP(SettingsParameters settings)
        {
            try
            {
                string ftpFilePath = "/Dahmira/countries/countriesTest.json";
                string localJsonString = string.Empty;

                if (File.Exists("countries.json"))
                {
                    localJsonString = File.ReadAllText("countries.json");
                }

                try
                {
                    string ftpJsonString = null;

                    var task = Task.Run(() =>
                    {
                        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(settings.url_praise + ftpFilePath);
                        request.Method = WebRequestMethods.Ftp.DownloadFile;
                        request.Credentials = new NetworkCredential(settings.ftpUsername, settings.ftpPassword);
                        request.UseBinary = true;
                        request.Timeout = 3000; // 5 секунд

                        using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                        using (Stream responseStream = response.GetResponseStream())
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            return reader.ReadToEnd();
                        }
                    });

                    bool completedInTime = task.Wait(TimeSpan.FromSeconds(3));
                    ftpJsonString = completedInTime ? task.Result : null;

                    if (string.IsNullOrEmpty(ftpJsonString))
                    {
                        throw new TimeoutException("Сервер не ответил за 5 секунд.");
                    }

                    if (localJsonString != ftpJsonString)
                    {
                        MessageBox.Show("Менеджер цен не совпадал, но был обновлён");
                        CountryManager.Instance.priceManager = JsonSerializer.Deserialize<PriceManager>(ftpJsonString);
                        File.WriteAllText("countries.json", ftpJsonString);
                    }
                    else
                    {
                        CountryManager.Instance.priceManager = JsonSerializer.Deserialize<PriceManager>(localJsonString);
                    }
                }
                catch (Exception ex)
                {
                    var log = new Log_Repository();
                    log.Add("Error", new StackTrace(), "noneUser", ex);

                    MessageBox.Show($"Ошибка чтения файла по адресу {settings.url_praise + ftpFilePath}");

                    if (string.IsNullOrEmpty(localJsonString))
                    {
                        CountryManager.Instance.priceManager = new PriceManager();
                    }
                    else
                    {
                        CountryManager.Instance.priceManager = JsonSerializer.Deserialize<PriceManager>(localJsonString);
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }


        bool CreateFtpDirectory(string folderUri, string ftpUsername, string ftpPassword)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(folderUri);
                request.Method = WebRequestMethods.Ftp.MakeDirectory; // Команда MKD
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
                request.UsePassive = true;
                request.UseBinary = true;
                request.KeepAlive = false;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    return true; // Папка создана успешно
                }
            }
            catch (WebException ex)
            {
                FtpWebResponse response = ex.Response as FtpWebResponse;
                if (response != null && response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    return false;
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

        void IFileImporter.ExportCountriesToFTP(SettingsParameters settings) //Экспорт стран на фтп сервера
        {
            try
            {
                string ftpFilePath = "/Dahmira/countries/countriesTest.json";
                string jsonString = JsonSerializer.Serialize(CountryManager.Instance.priceManager);
                File.WriteAllText("countries.json", jsonString);

                createDirectoriesOnFTP(settings, ftpFilePath, 1);

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(settings.url_praise + ftpFilePath);
                request.Credentials = new NetworkCredential(settings.ftpUsername, settings.ftpPassword);
                request.Method = WebRequestMethods.Ftp.UploadFile;

                byte[] fileBytes = Encoding.UTF8.GetBytes(jsonString);

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(fileBytes, 0, fileBytes.Length);
                }

                MessageBox.Show("Менеджер цен обновлён");
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                MessageBox.Show(ex.Message);
            }
        }

        void IFileImporter.ExportCalcToFile(MainWindow window) //Экспорт расчётки в файл
        {
            try
            {
                if (window.calcItems.Count > 0)
                {
                    var options = new JsonSerializerOptions
                    {
                        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                    };

                    string jsonString = JsonSerializer.Serialize(window.calcItems, options);

                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = "Json Files (*.DAH)|*.DAH",
                        Title = "Сохранить json файл",
                        InitialDirectory = window.settings.CalcFolderPath
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        string filePath = saveFileDialog.FileName;
                        window.CalcPath_label.Content = $"Имя файла расчёта: {filePath}";
                        File.WriteAllText(filePath, jsonString);
                        window.calcFilePath = filePath;
                        window.CalcInfo_label.Content = $"Расчёт успешно сохранён по пути: {filePath}.";
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }

        void IFileImporter.ImportCalcFromFile(MainWindow window) //Испорт расчётки из файла
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Json Files (*.DAH)|*.DAH",
                    Title = "Открыть json файл",
                    InitialDirectory = window.settings.CalcFolderPath
                };

                ICalcController calcController = new CalcController_Services();

                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;
                    window.CalcPath_label.Content = $"Имя файла расчёта: {filePath}";
                    window.calcItems.Clear();
                    try
                    {
                        string jsonString = File.ReadAllText(filePath);
                        var options = new JsonSerializerOptions
                        {
                            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
                        };
                        var newItems = JsonSerializer.Deserialize<ObservableCollection<CalcProduct>>(jsonString, options);
                        if (newItems[0].ID != -1)
                        {
                            window.calcItems.Add(new CalcProduct { Manufacturer = "Основной расчёт", ID = -1, isHidingButton = true, RowColor = calcController.ColorToHex(System.Windows.Media.Color.FromRgb(254, 241, 230)) });
                        }
                        foreach (var item in newItems)
                        {
                            if (item.ID > 0)
                                calcController.ValidateCalcItem(item);
                            window.calcItems.Add(item);
                        }
                        Country selectedCountry = CountryManager.Instance.priceManager.countries.FirstOrDefault(m => m.name == window.calcItems[window.calcItems.Count - 1].SelectedCountryName);
                        window.allCountries_comboBox.SelectedItem = selectedCountry;
                        window.isCalculationNeed = true;
                        window.MovingLabel.Visibility = Visibility.Visible;
                        window.calcFilePath = filePath;
                    }
                    catch (JsonException)
                    {
                        //Добавление файла .DAH из старой программы
                        window.calcItems.Add(new CalcProduct { Manufacturer = "Основной расчёт", ID = -1, isHidingButton = true, RowColor = calcController.ColorToHex(System.Windows.Media.Color.FromRgb(254, 241, 230)) });
                        window.calcItems.Add(new CalcProduct { Count = window.settings.FullCostType, TotalCost = 0, ID = -50 });

                        List<CalcProduct> calcProducts = ImportCalcFromOldFileDAH(window, filePath);
                        foreach (var item in calcProducts)
                        {
                            if (item.ID > 0)
                                calcController.ValidateCalcItem(item);
                            window.calcItems.Insert(window.calcItems.Count - 1, item);
                        }

                        window.allCountries_comboBox.SelectedIndex = 0;
                        window.isCalculationNeed = true;
                        window.MovingLabel.Visibility = Visibility.Visible;
                        window.calcFilePath = filePath;

                        calcController.Refresh(window.CalcDataGrid, window.calcItems);
                    }
                    catch (Exception ex)
                    {
                        var log = new Log_Repository();
                        log.Add("Error", new StackTrace(), "noneUser", ex);

                        MessageBox.Show($"Непредвиденная ошибка при открытии файла: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }

        void IFileImporter.AddCalcFromFile(MainWindow window) //Испорт расчётки из файла
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Json Files (*.DAH)|*.DAH",
                    Title = "Открыть json файл",
                    InitialDirectory = window.settings.CalcFolderPath
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    ICalcController CalcController = new CalcController_Services();//Для работы с обновлением/добавлением в расчётку
                    string filePath = openFileDialog.FileName;

                    int maxId = 0;
                    if (window.calcItems.Count > 2)
                    {
                        maxId = window.calcItems.Max(item => item.ID);
                    }

                    try
                    {

                        string jsonString = File.ReadAllText(filePath);
                        var options = new JsonSerializerOptions
                        {
                            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
                        };
                        var newItems = JsonSerializer.Deserialize<ObservableCollection<CalcProduct>>(jsonString, options);
                        //window.calcItems.Remove(window.calcItems[window.calcItems.Count - 1]);

                        foreach (var item in newItems)
                        {
                            if (item.ID >= -1)
                            {
                                if (item.ID > 0)
                                {
                                    item.ID += maxId;
                                    if (item.isDependency)
                                    {
                                        foreach (var dep in item.dependencies)
                                        {
                                            dep.ProductId += maxId;
                                        }
                                    }
                                    CalcController.ValidateCalcItem(item);
                                }

                                window.calcItems.Insert(window.calcItems.Count - 1, item);
                            }
                        }
                    }
                    catch (JsonException)
                    {
                        List<CalcProduct> calcProducts = ImportCalcFromOldFileDAH(window, filePath);

                        foreach (var item in calcProducts)
                        {
                            if (item.ID >= 0)
                            {
                                if (item.ID != 0)
                                {
                                    item.ID += maxId;
                                    if (item.isDependency)
                                    {
                                        foreach (var dep in item.dependencies)
                                        {
                                            dep.ProductId += maxId;
                                        }
                                    }
                                    CalcController.ValidateCalcItem(item);
                                }
                                window.calcItems.Insert(window.calcItems.Count - 1, item);
                            }
                        }


                    }
                    catch (Exception ex)
                    {
                        var log = new Log_Repository();
                        log.Add("Error", new StackTrace(), "noneUser", ex);

                        MessageBox.Show($"Непредвиденная ошибка при добавления расчёта: {ex.Message}");
                    }
                    finally
                    {
                        CalcController.Refresh(window.CalcDataGrid, window.calcItems);
                        CalcController.ActivateNeedCalculation(window);
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }

        void IFileImporter.ImportCalcFromFile_StartDUH(string path, MainWindow window) //Испорт расчётки при запске dah файла
        {
            try
            {
                string filePath = path;
                window.CalcPath_label.Content = $"Имя файла расчёта: {filePath}";
                window.calcItems.Clear();
                ICalcController calcController = new CalcController_Services();

                try
                {
                    string jsonString = File.ReadAllText(filePath);
                    var options = new JsonSerializerOptions
                    {
                        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
                    };
                    var newItems = JsonSerializer.Deserialize<ObservableCollection<CalcProduct>>(jsonString, options);
                    foreach (var item in newItems)
                    {
                        window.calcItems.Add(item);
                        if (item.ID > 0)
                            calcController.ValidateCalcItem(item);
                    }
                    Country selectedCountry = CountryManager.Instance.priceManager.countries.FirstOrDefault(m => m.name == window.calcItems[window.calcItems.Count - 1].SelectedCountryName);
                    window.allCountries_comboBox.SelectedItem = selectedCountry;
                    window.calcFilePath = filePath;

                    calcController.Refresh(window.CalcDataGrid, window.calcItems);
                    calcController.ActivateNeedCalculation(window);
                }
                catch (JsonException)
                {
                    //Добавление файла .DAH из старой программы
                    window.calcItems.Add(new CalcProduct { Count = window.settings.FullCostType, TotalCost = 0, ID = -50 });

                    List<CalcProduct> calcProducts = ImportCalcFromOldFileDAH(window, filePath);
                    foreach (var item in calcProducts)
                    {
                        window.calcItems.Insert(window.calcItems.Count - 1, item);
                        if (item.ID > 0)
                            calcController.ValidateCalcItem(item);
                    }

                    window.allCountries_comboBox.SelectedIndex = 0;
                    calcController.ActivateNeedCalculation(window);
                    window.calcFilePath = filePath;

                    calcController.Refresh(window.CalcDataGrid, window.calcItems);
                }
                catch (Exception ex)
                {
                    var log = new Log_Repository();
                    log.Add("Error", new StackTrace(), "noneUser", ex);

                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }



            async Task IFileImporter.ImportDBFromFTP(MainWindow window, ProgressBar progressBar, Label progressBarLabel, ProgressBarPage progressBarPage, CancellationToken cancellationToken)
        {
            try
            {
                progressBar.Maximum = 100;
                string ftpFilePath = "/Dahmira/data_price_test/db/Dahmira_DB_beta.bak";
                //string localFilePath = AppDomain.CurrentDomain.BaseDirectory + "/db/Dahmira_DB_beta.bak";
                string localFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db", "Dahmira_DB_beta.bak");

                try
                {
                    // Скачиваем основной файл
                    progressBarLabel.Content = "Загрузка резервной копии файла прайса...";
                    await DownloadFileAsync(window.settings.url_praise + ftpFilePath, window.settings.ftpUsername, window.settings.ftpPassword, localFilePath, new Progress<int>(percent => progressBar.Value = percent), cancellationToken);
                    RestoreDatabase(localFilePath, window);

                    //Закрываем текущее приложение
                    string exePath = AppDomain.CurrentDomain.BaseDirectory + "Dahmira.exe";
                    System.Windows.Application.Current.Shutdown();
                    Process.Start(exePath);

                }
                catch (OperationCanceledException)
                {
                    MessageBox.Show("Загрузка была прервана.");
                    progressBarPage.Close();
                }
                catch (Exception ex)
                {
                    var log = new Log_Repository();
                    log.Add("Error", new StackTrace(), "noneUser", ex);

                    MessageBox.Show(ex.Message);
                    progressBarPage.Close();
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }

        private async Task DownloadFileAsync(string ftpUrl, string ftpUsername, string ftpPassword, string localPath, IProgress<int> progress, CancellationToken cancellationToken)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
                request.UseBinary = true;

                // Создаем директорию, если она не существует
                string directoryPath = Path.GetDirectoryName(localPath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                using (Stream responseStream = response.GetResponseStream())
                using (FileStream fileStream = new FileStream(localPath, FileMode.Create))
                {
                    long totalBytes = GetFileSize(ftpUrl, ftpUsername, ftpPassword);
                    byte[] buffer = new byte[262144]; // Буфер для чтения данных
                    long bytesReadTotal = 0;
                    int bytesRead;

                    while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                    {
                        // Если токен отменен, выбрасываем исключение
                        cancellationToken.ThrowIfCancellationRequested();

                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        bytesReadTotal += bytesRead;

                        // Обновляем прогресс
                        int percentComplete = (int)((bytesReadTotal * 100) / totalBytes);
                        progress.Report(percentComplete);
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }

        private long GetFileSize(string ftpUrl, string ftpUsername, string ftpPassword)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                request.Method = WebRequestMethods.Ftp.GetFileSize;
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    return response.ContentLength; // Теперь ContentLength вернёт размер файла
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return 1000000000;
            }
        }

        public async Task ExportDBOnFTP(MainWindow window, ProgressBar progressBar, Label progressBarLabel, ProgressBarPage progressBarPage, CancellationToken cancellationToken)
        {
            try
            {
                progressBar.Maximum = 100;
                string backupFilePath = AppDomain.CurrentDomain.BaseDirectory + "/db/Dahmira_DB_beta.bak";
                BackupDatabase(window, backupFilePath);  // Создаём резервную копию
                progressBarLabel.Content = $"Выгрузка резервной копии файла прайса...";

                try
                {
                    //Создаём полный путь на сервере FTP
                    string ftpPath = "/Dahmira/data_price_test/db/Dahmira_DB_beta.bak";
                    createDirectoriesOnFTP(window.settings, ftpPath, 1);

                    // Создаём FTP-запрос
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(window.settings.url_praise + ftpPath);
                    request.Method = WebRequestMethods.Ftp.UploadFile;

                    // Указываем имя пользователя и пароль для доступа к FTP
                    request.Credentials = new NetworkCredential(window.settings.ftpUsername, window.settings.ftpPassword);
                    request.UseBinary = true;  // Для двоичных данных
                    request.UsePassive = true; // Включаем пассивный режим, если нужно

                    // Читаем файл частями и загружаем
                    byte[] buffer = new byte[8192];  // Буфер для чтения данных (8 KB)
                    long totalBytes = new FileInfo(backupFilePath).Length;  // Общий размер файла
                    long bytesSent = 0;  // Количество переданных байт

                    // Открываем поток для чтения файла
                    using (FileStream fs = new FileStream(backupFilePath, FileMode.Open, FileAccess.Read))
                    using (Stream requestStream = await request.GetRequestStreamAsync())
                    {
                        int bytesRead;
                        while ((bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            cancellationToken.ThrowIfCancellationRequested();  // Проверка отмены

                            // Отправляем данные на сервер
                            await requestStream.WriteAsync(buffer, 0, bytesRead);

                            // Обновляем количество переданных байт
                            bytesSent += bytesRead;

                            // Обновляем прогресс
                            int progress = (int)((bytesSent * 100) / totalBytes);
                            progressBar.Value = progress;

                            // Обновляем метку
                        }
                    }

                    MessageBox.Show("Выгрузка резервной копии файла прайса завершена успешно.");
                }
                catch (OperationCanceledException)
                {
                    MessageBox.Show("Выгрузка была прервана.");
                }
                catch (Exception ex)
                {
                    var log = new Log_Repository();
                    log.Add("Error", new StackTrace(), "noneUser", ex);

                    MessageBox.Show($"Ошибка при выгрузке: {ex.Message}");
                }
                finally
                {
                    progressBarPage.Close();  // Закрываем окно прогресса в любом случае
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }

        public void BackupDatabase(MainWindow window, string backupFilePath)
        {
            try
            {
                using (var context = new ApplicationContext())
                {
                    // SQL-запрос для создания резервной копии
                    var backupCommand = $@"
                    BACKUP DATABASE [{window.settings.Price}] 
                    TO DISK = '{backupFilePath}' 
                    WITH FORMAT, INIT, 
                    NAME = 'Full Backup of Dahmira_DB_beta';";

                    // Выполняем SQL-запрос через ExecuteSqlRaw
                    context.Database.ExecuteSqlRaw(backupCommand);
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                MessageBox.Show($"Ошибка при создании резервной копии: {ex.Message}");
            }
        }

        public void RestoreDatabase(string backupFilePath, MainWindow window)
        {
            try
            {
                string databaseName = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db", "Dahmira_DB_beta.mdf");
                string mdfPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db", "Dahmira_DB_beta.mdf");
                string ldfPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db", "Dahmira_DB_beta_log.ldf");
                
                
                //string databaseName = AppDomain.CurrentDomain.BaseDirectory + "Dahmira_DB_beta.mdf";
                //string mdfPath = AppDomain.CurrentDomain.BaseDirectory + "Dahmira_DB_beta.mdf";
                //string ldfPath = AppDomain.CurrentDomain.BaseDirectory + "Dahmira_DB_beta_log.ldf";

                DeleteDatabase(databaseName);


                using (SqlConnection connection = new SqlConnection("Data Source = (LocalDB)\\MSSQLLocalDB; Trusted_Connection=True;MultipleActiveResultSets=true; Integrated Security=True;Connect Timeout=30; TrustServerCertificate=True"))
                {

                    // Команда восстановления
                    var restoreCommand = $@"
                        USE master;
                        RESTORE DATABASE [{databaseName}] 
                        FROM DISK = '{backupFilePath}' 
                        WITH 
                            MOVE 'Dahmira_DB_beta' TO '{mdfPath}', 
                            MOVE 'Dahmira_DB_beta_log' TO '{ldfPath}', 
                            REPLACE,
                            RECOVERY;";

                    connection.Open();
                    using (SqlCommand command = new SqlCommand(restoreCommand, connection))
                    {
                        command.ExecuteNonQuery();
                    }


                    Console.WriteLine("База данных успешно восстановлена.");
                }

            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                Console.WriteLine($"Ошибка при восстановлении базы данных: {ex.Message}");
            }
        }

        public void DeleteDatabase(string databaseName)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection("Data Source = (LocalDB)\\MSSQLLocalDB; Trusted_Connection=True;MultipleActiveResultSets=true; Integrated Security=True;Connect Timeout=30; TrustServerCertificate=True"))
                {
                    connection.Open();

                    // Завершаем все соединения с базой и переводим в режим SINGLE_USER
                    string closeConnectionsQuery = $@"
                            ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                        ";

                    using (SqlCommand command = new SqlCommand(closeConnectionsQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    // Удаляем базу данных
                    string dropDatabaseQuery = $@"
                            DROP DATABASE [{databaseName}];
                        ";

                    using (SqlCommand command = new SqlCommand(dropDatabaseQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    Console.WriteLine($"Database {databaseName} deleted successfully.");
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Warning", new StackTrace(), "noneUser", ex);

                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        async Task IFileImporter.ExportTemplatesOnFTP(MainWindow window, ProgressBar progressBar, Label progressBarLabel, ProgressBarPage progressBarPage, CancellationToken cancellationToken)
        {
            try
            {
                // Получаем все файлы в папке с шаблонами
                string[] files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\Шаблоны");
                if (files.Length == 0)
                {
                    MessageBox.Show("Нет шаблонов прайса, подходящих для выгрузки на сервер.");
                    progressBarPage.Close();
                    return;
                }
                string ftpFilePath = "/Dahmira/data_price_test/templates";
                createDirectoriesOnFTP(window.settings, ftpFilePath);

                progressBar.Maximum = files.Length;

                try
                {
                    // Перебираем каждый файл и загружаем его
                    foreach (var file in files)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        string fileName = Path.GetFileName(file); // Имя файла
                        string remoteFilePath = ftpFilePath + "/" + fileName; // Путь на сервере для этого файла

                        // Обновляем метку
                        progressBarLabel.Content = $"Выгрузка файла: {fileName}";

                        // Создаем FTP-запрос для загрузки файла
                        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(window.settings.url_praise + remoteFilePath);
                        request.Method = WebRequestMethods.Ftp.UploadFile;
                        request.Credentials = new NetworkCredential(window.settings.ftpUsername, window.settings.ftpPassword);
                        request.UseBinary = true; // Для двоичных данных
                        request.UsePassive = true; // Включаем пассивный режим, если нужно

                        // Чтение содержимого файла в байты
                        byte[] fileBytes = File.ReadAllBytes(file);


                        using (Stream requestStream = await request.GetRequestStreamAsync())
                        {
                            // Записываем файл на сервер
                            await requestStream.WriteAsync(fileBytes, 0, fileBytes.Length);
                        }

                        // Обновляем прогресс
                        progressBar.Value++;
                    }

                    MessageBox.Show("Все файлы выгружены!");
                    progressBarPage.Close();
                }
                catch (OperationCanceledException)
                {
                    MessageBox.Show("Выгрузка была прервана.");
                    progressBarPage.Close();
                }
                catch (Exception ex)
                {
                    var log = new Log_Repository();
                    log.Add("Error", new StackTrace(), "noneUser", ex);

                    MessageBox.Show(ex.Message);
                    progressBarPage.Close();
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }

        async Task IFileImporter.ImportTemplatesFromFTP(MainWindow window, ProgressBar progressBar, Label progressBarLabel, ProgressBarPage progressBarPage, CancellationToken cancellationToken)
        {
            try
            {
                string ftpDirectory = "/Dahmira/data_price_test/templates"; // Директория на FTP, откуда нужно скачать файлы
                string localDirectory = AppDomain.CurrentDomain.BaseDirectory + "\\Шаблоны"; // Локальная папка, куда сохранять файлы

                // Создаем папку, если она не существует
                Directory.CreateDirectory(localDirectory);

                // Получаем список файлов в директории на FTP
                FtpWebRequest listRequest = (FtpWebRequest)WebRequest.Create(window.settings.url_praise + ftpDirectory);
                listRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                listRequest.Credentials = new NetworkCredential(window.settings.ftpUsername, window.settings.ftpPassword);

                List<string> fileNames = new List<string>();

                try
                {
                    using (FtpWebResponse listResponse = (FtpWebResponse)await listRequest.GetResponseAsync())
                    using (StreamReader reader = new StreamReader(listResponse.GetResponseStream()))
                    {
                        string line;
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            fileNames.Add(line); // Добавляем имя каждого файла в список
                        }
                    }

                    // Устанавливаем максимальное количество для прогресс-бара
                    progressBar.Maximum = fileNames.Count;

                    // Перебираем каждый файл и загружаем его
                    foreach (var fileName in fileNames)
                    {
                        cancellationToken.ThrowIfCancellationRequested(); // Проверка на отмену

                        string remoteFilePath = ftpDirectory + "/" + fileName; // Путь к файлу на сервере
                        string localFilePath = Path.Combine(localDirectory, fileName); // Путь для сохранения файла локально

                        // Обновляем метку
                        progressBarLabel.Content = $"Загрузка файла: {fileName}";

                        // Создаем FTP-запрос для скачивания файла
                        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(window.settings.url_praise + remoteFilePath);
                        request.Method = WebRequestMethods.Ftp.DownloadFile;
                        request.Credentials = new NetworkCredential(window.settings.ftpUsername, window.settings.ftpPassword);
                        request.UseBinary = true; // Для двоичных данных
                        request.UsePassive = true; // Включаем пассивный режим

                        using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                        using (Stream responseStream = response.GetResponseStream())
                        using (FileStream fileStream = new FileStream(localFilePath, FileMode.Create))
                        {
                            // Скачиваем файл
                            await responseStream.CopyToAsync(fileStream);
                        }

                        // Обновляем прогресс
                        progressBar.Value++;
                    }

                    MessageBox.Show("Все файлы загружены!");
                }
                catch (OperationCanceledException)
                {
                    MessageBox.Show("Загрузка была прервана.");
                }
                catch (Exception ex)
                {
                    var log = new Log_Repository();
                    log.Add("Error", new StackTrace(), "noneUser", ex);

                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                }
                finally
                {
                    progressBarPage.Close(); // Закрываем окно прогресса
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }

            //Экспорт расчётки в шаблон
        void IFileImporter.ExportCalcToTemlates(MainWindow window, string patch)
        {
            try
            {
                if (window.calcItems.Count > 0)
                {
                    var options = new JsonSerializerOptions
                    {
                        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                    };
                    List<CalcProduct> items = window.calcItems.Select(i => i.Clone()).ToList();
                    string jsonString = JsonSerializer.Serialize(items, options);

                    window.CalcPath_label.Content = $"Имя файла расчёта: {Path.GetFileName(patch)}";
                    File.WriteAllText(patch, jsonString);
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }


        //Возвращаем массив данных после десериализации из json
        ObservableCollection<CalcProduct> IFileImporter.Get_JsonList(string path, MainWindow window)
        {
            try
            {
                string filePath = path;
                window.CalcPath_label.Content = $"Имя файла расчёта: {Path.GetFileName(filePath)}";
                string jsonString = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions
                {
                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
                };

                var newItems = JsonSerializer.Deserialize<ObservableCollection<CalcProduct>>(jsonString, options);
                return newItems;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Info", new StackTrace(), "noneUser", ex);

                throw new JsonException();
            }
        }


        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //           Загрузка Шаблонов
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        // Получение списка файлов на FTP-сервере
        List<string> IFileImporter.GetFileListFromFtp(SettingsParameters settings)
        {
            try
            {


                List<string> files = new List<string>();

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(settings.url_praise + "/Dahmira/data_price_test/templates/");
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(settings.ftpUsername, settings.ftpPassword);


                try
                {
                    using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.EndsWith("DAH", StringComparison.OrdinalIgnoreCase))
                            {
                                files.Add(line);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var log = new Log_Repository();
                    log.Add("Error", new StackTrace(), "noneUser", ex);

                    //LB_info.Content = "FTP не доступен!";
                    //LB_info.Foreground = new SolidColorBrush(Colors.DarkGoldenrod);
                    MessageBox.Show("FTP сервер временно не доступен! Попробуйте позже!");

                    //DownloadProgressBar.Visibility = Visibility.Hidden;
                }
                return files;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return null;
            }
        }

        // Асинхронное скачивание файла с FTP-сервера
        async Task IFileImporter.DownloadFileAsync(SettingsParameters settings, string ftpServerUrl, string localFilePath)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(settings.url_praise + "/Dahmira/data_price_test/templates/" + ftpServerUrl);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(settings.ftpUsername, settings.ftpPassword);

                using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                using (Stream responseStream = response.GetResponseStream())
                using (FileStream localFileStream = new FileStream(localFilePath, FileMode.Create))
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead;
                    while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await localFileStream.WriteAsync(buffer, 0, bytesRead);
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //           Выгрузка и загрузка данных в excel
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        bool IFileImporter.ExportPriseToExcel_DB(string nameListFile, SettingsParameters settings, ObservableCollection<Material> dbItems, string patch) //Экспорт расчётки в Excel
        {
            try
            {
                bool isSaved = false;

                ExcelPackage.LicenseContext = LicenseContext.Commercial; //Вид лицензии

                var package = new ExcelPackage(); //Создание нового документа
                var worksheet = package.Workbook.Worksheets.Add("Лист1");
                int lastColumnIndex = 9;

                // Записываем заголовки столбцов
                worksheet.Cells[1, 1].Value = "Производитель";
                worksheet.Cells[1, 2].Value = "Тип";
                worksheet.Cells[1, 3].Value = "Наименование RUS";
                worksheet.Cells[1, 4].Value = "Наименование ENG";
                worksheet.Cells[1, 5].Value = "Артикул";
                worksheet.Cells[1, 6].Value = "Цена";
                worksheet.Cells[1, 7].Value = "Ед. изм.";
                worksheet.Cells[1, 8].Value = "Ед. изм. ENG";
                worksheet.Cells[1, 9].Value = "Дата обновления цены";

                //Установка стилей для Header 
                ExcelRange titleRange = worksheet.Cells[1, 1, 1, lastColumnIndex];
                titleRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                titleRange.Style.Fill.BackgroundColor.SetColor(settings.ExcelTitleColor.GetColor());

                //Установка стилей для всего рабочего пространства
                ExcelRange Rng = worksheet.Cells[1, 1, dbItems.Count + 2 - 1, lastColumnIndex];
                Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                Rng.Style.WrapText = true;

                Rng.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                Rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                Rng.Style.Border.Top.Color.SetColor(System.Drawing.Color.Gray);
                Rng.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Gray);
                Rng.Style.Border.Left.Color.SetColor(System.Drawing.Color.Gray);
                Rng.Style.Border.Right.Color.SetColor(System.Drawing.Color.Gray);

                ExcelRange Rng2 = worksheet.Cells[1, 5, dbItems.Count + 2 - 1, 5];
                Rng2.Style.WrapText = false;


                //Установка ширины столбцов 
                worksheet.Column(1).Width = 15;
                worksheet.Column(2).Width = 15;
                worksheet.Column(3).Width = 25;
                worksheet.Column(4).Width = 25;
                worksheet.Column(5).Width = 20;
                worksheet.Column(6).Width = 10;
                worksheet.Column(7).Width = 10;
                worksheet.Column(8).Width = 10;
                worksheet.Column(9).Width = 12.5;

                // Записываем данные из DataGrid
                for (int i = 0; i < dbItems.Count; i++)
                {
                    var item = dbItems[i];

                    //Установка стилей всех данных
                    ExcelRange dataRange = worksheet.Cells[i + 2, 1, i + 2, lastColumnIndex];
                    dataRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    dataRange.Style.Fill.BackgroundColor.SetColor(settings.ExcelDataColor.GetColor());

                    //Установка стилей для примечаний
                    ExcelRange notesRange = worksheet.Cells[i + 2, lastColumnIndex];
                    notesRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    notesRange.Style.Fill.BackgroundColor.SetColor(settings.ExcelNotesColor.GetColor());

                    //Установка стилей для номера
                    ExcelRange numberRange = worksheet.Cells[i + 2, 1];
                    numberRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    numberRange.Style.Fill.BackgroundColor.SetColor(settings.ExcelNumberColor.GetColor());

                    //Добавление данных в ячейки
                    worksheet.Cells[i + 2, 1].Value = item.Manufacturer;
                    worksheet.Cells[i + 2, 2].Value = item.Type;
                    worksheet.Cells[i + 2, 3].Value = item.ProductName;
                    worksheet.Cells[i + 2, 4].Value = item.EnglishProductName;
                    worksheet.Cells[i + 2, 5].Value = item.Article;
                    worksheet.Cells[i + 2, 6].Value = item.Cost;
                    worksheet.Cells[i + 2, 7].Value = item.Unit;
                    worksheet.Cells[i + 2, 8].Value = item.EnglishUnit;
                    worksheet.Cells[i + 2, 9].Value = item.LastCostUpdate;
                }

                worksheet.Protection.IsProtected = false;
                worksheet.Protection.AllowSelectLockedCells = false;
                package.SaveAs(new FileInfo(patch));
                isSaved = true;

                return isSaved;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                MessageBox.Show(ex.Message);

                return false;
            }
        }
        bool IFileImporter.ExportPhotoToJPG_DB(ObservableCollection<Material> dbItems, string patch) //Экспорт расчётки в Excel
        {
            try
            {
                ByteArrayToImageSourceConverter_Services converter = new ByteArrayToImageSourceConverter_Services();
                CalcController_Services calcController = new CalcController_Services();

                for (int i = 0; i < dbItems.Count; i++)
                {
                    if (dbItems[i].Photo != null && dbItems[i].Photo.Length > 0 && !calcController.ArePhotosEqual(dbItems[i].Photo, converter.ConvertFromFileImageToByteArray(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/without_image_database.png"))))
                    {
                        //Заменяем символы с которыми нельзя сохранять
                        var buff = dbItems[i].Article.Replace("<", "lt")
                                                     .Replace(">", "gt")
                                                     .Replace(":", "colon")
                                                     .Replace("\"", "dblQuote")
                                                     .Replace("/", "slash")
                                                     .Replace("\\", "backslash")
                                                     .Replace("|", "pipe")
                                                     .Replace("?", "question")
                                                     .Replace("\n", "newline")
                                                     .Replace("*", "asterisk");

                        // Определяем путь для сохранения
                        string filePath = Path.Combine(patch + "\\", $"{buff}.png");
                        // Сохраняем изображение на диск
                        System.IO.File.WriteAllBytes(filePath, dbItems[i].Photo);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                MessageBox.Show(ex.Message);

                return false;
            }

        }

        public ObservableCollection<Material> ImportPrise_ExcelTo_DB(string patch)
        {
            try
            {
                // Устанавливаем контекст лицензии
                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                //Создаем временную перменую для хранения материалов
                var buff_materials = new ObservableCollection<Material>();

                //Читаем Excel-файл
                using (var package = new ExcelPackage(new FileInfo(patch)))
                {
                    var worksheet = package.Workbook.Worksheets[0]; // Первый лист
                    int row = 2; // Начинаем со второй строки, так как первая строка — это заголовки

                    while (worksheet.Cells[row, 1].Value != null)
                    {
                        var material = new Material
                        {
                            Manufacturer = worksheet.Cells[row, 1].Text,
                            Type = worksheet.Cells[row, 2].Text,
                            ProductName = worksheet.Cells[row, 3].Text,
                            EnglishProductName = worksheet.Cells[row, 4].Text,
                            Article = worksheet.Cells[row, 5].Text,
                            Cost = float.Parse(worksheet.Cells[row, 6].Text),
                            Unit = worksheet.Cells[row, 7].Text,
                            EnglishUnit = worksheet.Cells[row, 8].Text,
                            LastCostUpdate = worksheet.Cells[row, 9].Text
                        };

                        buff_materials.Add(material);
                        row++;
                    }
                }

                return buff_materials;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return null;
            }
        }

        public bool ImportPhotoToDB(ObservableCollection<Material> dbItems, string patch) //Импорт картинок в БД
        {
            try
            {
                for (int i = 0; i < dbItems.Count; i++)
                {
                    // Замена символа / на %
                    string formattedArticle = dbItems[i].Article.Replace("<", "lt")
                                                                .Replace(">", "gt")
                                                                .Replace(":", "colon")
                                                                .Replace("\"", "dblQuote")
                                                                .Replace("/", "slash")
                                                                .Replace("\\", "backslash")
                                                                .Replace("|", "pipe")
                                                                .Replace("?", "question")
                                                                .Replace("\n", "newline")
                                                                .Replace("*", "asterisk");

                    dbItems[i].Article = dbItems[i].Article.Replace("\n", "");
                    // Поиск изображения по артикулу
                    string imagePath = Path.Combine(patch, $"{formattedArticle}.png"); // Можно изменить расширение на нужное

                    if (File.Exists(imagePath))
                    {
                        // Читаем файл изображения в массив 
                        dbItems[i].Photo = File.ReadAllBytes(imagePath);
                    }
                    else
                    {
                        ByteArrayToImageSourceConverter_Services converter = new ByteArrayToImageSourceConverter_Services();
                        dbItems[i].Photo = converter.ConvertFromFileImageToByteArray(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/without_image_database.png"));
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return false;
            }
        }

        //Проверка всех каталогов на существание и создание, если необходимо на FTP
        private void createDirectoriesOnFTP(SettingsParameters settings, string ftpFoldersPath, int removedNumber = 0)
        {
            try
            {
                string[] folders = ftpFoldersPath.TrimStart('/').Split('/');
                string currentFolderPath = string.Empty;
                for (int i = 0; i < folders.Length - removedNumber; i++)
                {
                    currentFolderPath += $"/{folders[i]}";
                    CreateFtpDirectory(settings.url_praise + currentFolderPath, settings.ftpUsername, settings.ftpPassword);
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }

        public DateTime GetFileLastModified(SettingsParameters settings, string ftpUrl)
        {
            try
            {
                var task = Task.Run(() =>
                {
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(settings.url_praise + ftpUrl);
                    request.Method = WebRequestMethods.Ftp.GetDateTimestamp;
                    request.Credentials = new NetworkCredential(settings.ftpUsername, settings.ftpPassword);
                    request.Timeout = 3000; // 5 секунд

                    using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                    {
                        return response.LastModified;
                    }
                });

                bool completedInTime = task.Wait(TimeSpan.FromSeconds(3));
                return completedInTime ? task.Result : DateTime.MinValue;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return DateTime.MinValue;
            }
        }

        public List<CalcProduct> ImportCalcFromOldFileDAH(MainWindow mainWindow, string fullPath)
        {
            try
            {
                List<CalcProduct> calcProducts = new List<CalcProduct>();
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                string directoryPath = Path.GetDirectoryName(fullPath); //Путь к каталогу

                try
                {
                    var buff_count = mainWindow.calcItems.Max(p => p.ID);
                    if (buff_count <= 0)
                    {
                        buff_count = 1;
                    }

                    string fileName = Path.GetFileName(fullPath); //Имя файла с расширением

                    string tempFolderPath = Path.Combine(directoryPath, "tempDH");
                    //Разархивировка данных
                    Directory.CreateDirectory(tempFolderPath);
                    using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile(fullPath, Encoding.GetEncoding(866)))
                    {
                        zip.ExtractAll(tempFolderPath);
                    }

                    //Остальное
                    string[] subDirectories = Directory.GetDirectories(tempFolderPath);

                    string firstSubDirectory = subDirectories[0];

                    // Формируем пути к файлам
                    string folderName = new DirectoryInfo(firstSubDirectory).Name;
                    string path_to_arhiv = Path.Combine(firstSubDirectory, folderName + ".DAH");
                    string path_to_arhiv_zav = Path.Combine(firstSubDirectory, "_" + folderName + ".DAH");
                    //string path_to_arhiv_PS = System.IO.Path.Combine(firstSubDirectory, "_PS_" + folderName + ".DAH");

                    Dictionary<string, int> productsId = new Dictionary<string, int>();
                    Dictionary<string, int> productsId_Art = new Dictionary<string, int>();

                    using (BinaryReader reader = new BinaryReader(File.Open(path_to_arhiv, FileMode.Open)))
                    {
                        // Читаем размеры таблицы
                        int columnCount = reader.ReadInt32();
                        int rowCount = reader.ReadInt32();

                        // Заполняем строки DataGridView
                        for (int row = 0; row < rowCount - 1; ++row)
                        {
                            CalcProduct newProduct = new CalcProduct();

                            int notEmptyColumnsCount = 0;
                            for (int col = 0; col < columnCount; ++col)
                            {
                                // Проверяем, есть ли данные в ячейке
                                if (reader.ReadBoolean())
                                {
                                    string value = reader.ReadString();
                                    switch (col)
                                    {
                                        case 0:
                                            {
                                                if (!string.IsNullOrEmpty(value))
                                                {
                                                    newProduct.Num = Convert.ToInt32(value);
                                                    notEmptyColumnsCount++;
                                                }
                                                else
                                                {
                                                    //newProduct.Num = 1;//Convert.ToInt32(value);
                                                }
                                                break;
                                            }
                                        case 1:
                                            {
                                                newProduct.Manufacturer = value;
                                                if (!string.IsNullOrEmpty(value))
                                                {
                                                    notEmptyColumnsCount++;
                                                }
                                                break;
                                            }
                                        case 2:
                                            {
                                                newProduct.ProductName = value;
                                                if (!string.IsNullOrEmpty(value))
                                                {
                                                    notEmptyColumnsCount++;
                                                }
                                                break;
                                            }
                                        case 3:
                                            {
                                                newProduct.Article = value;
                                                newProduct.ID = ++buff_count;
                                                newProduct.Num = newProduct.ID;
                                                if (!string.IsNullOrEmpty(value))
                                                {
                                                    notEmptyColumnsCount++;
                                                }
                                                break;
                                            }
                                        case 4:
                                            {
                                                string imagePath = Path.Combine(firstSubDirectory, $"{row}.jpg");

                                                if (File.Exists(imagePath))
                                                {
                                                    byte[] imageBytes = File.ReadAllBytes(imagePath);
                                                    newProduct.Photo = imageBytes;
                                                    notEmptyColumnsCount++;
                                                }
                                                else
                                                {
                                                    ByteArrayToImageSourceConverter_Services converter = new ByteArrayToImageSourceConverter_Services();
                                                    newProduct.Photo = converter.ConvertFromFileImageToByteArray(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/without_image_database.png"));
                                                }
                                                break;
                                            }
                                        case 5:
                                            {
                                                if (!string.IsNullOrEmpty(value))
                                                {
                                                    double.TryParse(NormalizeDecimalInput(value), NumberStyles.Any, new CultureInfo("en-US"), out double cost);
                                                    newProduct.Cost = cost;
                                                    newProduct.RealCost = cost;
                                                    notEmptyColumnsCount++;
                                                }
                                                break;
                                            }
                                        case 6:
                                            {
                                                newProduct.Count = NormalizeDecimalInput(value);
                                                if (!string.IsNullOrEmpty(value))
                                                {
                                                    notEmptyColumnsCount++;
                                                }
                                                break;
                                            }
                                        case 8:
                                            {
                                                productsId[value] = newProduct.ID;
                                                if (!string.IsNullOrEmpty(value))
                                                {
                                                    notEmptyColumnsCount++;
                                                }
                                                break;
                                            }
                                        case 9:
                                            {
                                                productsId_Art[value] = newProduct.ID;
                                                if (!string.IsNullOrEmpty(value))
                                                {
                                                    notEmptyColumnsCount++;
                                                }
                                                break;
                                            }
                                    }

                                }
                                else
                                {
                                    //Если данных нет, просто пропускаем
                                    reader.ReadBoolean();
                                    if (col == 4 && newProduct.ID != 0)
                                    {
                                        ByteArrayToImageSourceConverter_Services converter = new ByteArrayToImageSourceConverter_Services();
                                        newProduct.Photo = converter.ConvertFromFileImageToByteArray(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/images/without_image_database.png"));
                                    }
                                }
                            }

                            if (notEmptyColumnsCount <= 1)
                            {
                                newProduct.isHidingButton = true;
                                if (newProduct.Manufacturer == string.Empty)
                                {
                                    if (newProduct.ProductName != string.Empty)
                                    {
                                        newProduct.Manufacturer = newProduct.ProductName;
                                        newProduct.ProductName = string.Empty;
                                    }
                                    else if (newProduct.Article != string.Empty)
                                    {
                                        newProduct.Manufacturer = newProduct.Article;
                                        newProduct.Article = string.Empty;
                                    }
                                }
                            }
                            calcProducts.Add(newProduct);
                        }
                    }

                    using (BinaryReader reader = new BinaryReader(File.Open(path_to_arhiv_zav, FileMode.Open)))
                    {
                        // Читаем размеры таблицы
                        int columnCount = reader.ReadInt32();
                        int rowCount = reader.ReadInt32();
                        for (int row = 0; row < rowCount; ++row)
                        {
                            Dependency newDependency = new Dependency();
                            CalcProduct selectedCalc = new CalcProduct();
                            for (int col = 0; col < columnCount; ++col)
                            {
                                if (reader.ReadBoolean())
                                {
                                    string value = reader.ReadString();

                                    switch (col)
                                    {
                                        case 2:
                                            {
                                                if (value == "x")
                                                {
                                                    value = "*";
                                                }

                                                newDependency.SelectedType = value;
                                                break;
                                            }
                                        case 3:
                                            {
                                                double.TryParse(NormalizeDecimalInput(value), NumberStyles.Any, new CultureInfo("en-US"), out double multiplier);
                                                newDependency.SecondMultiplier = Convert.ToDouble(multiplier);
                                                break;
                                            }
                                        case 4:
                                            {
                                                if (productsId.ContainsKey(value))
                                                {
                                                    selectedCalc = calcProducts.FirstOrDefault(i => i.ID == productsId[value]);
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                                break;
                                            }
                                        case 5:
                                            {
                                                if (productsId_Art.ContainsKey(value))
                                                {
                                                    CalcProduct findProduct = calcProducts.FirstOrDefault(i => i.ID == productsId_Art[value]);
                                                    newDependency.ProductName = findProduct.ProductName;
                                                    newDependency.IsFirstButtonVisible = false;
                                                    newDependency.ProductId = findProduct.ID;
                                                }
                                                else
                                                {
                                                    continue;
                                                }

                                                break;
                                            }
                                    }
                                }
                                else
                                {
                                    reader.ReadBoolean();
                                }
                            }
                            selectedCalc.dependencies.Add(newDependency);
                            selectedCalc.isDependency = true;
                        }
                    }

                    Directory.Delete(directoryPath + "\\tempDH", true);
                    return calcProducts;
                }
                catch (FileNotFoundException ex)
                {
                    MessageBox.Show($"Ошибка: файл не найден. {ex.Message}");
                    Directory.Delete(directoryPath + "\\tempDH", true);
                    return calcProducts;
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show($"Ошибка доступа: {ex.Message}");
                    Directory.Delete(directoryPath + "\\tempDH", true);
                    return calcProducts;
                }
                catch (Ionic.Zip.ZipException ex)
                {
                    MessageBox.Show($"Ошибка в архиве: {ex.Message}");
                    Directory.Delete(directoryPath + "\\tempDH", true);
                    return calcProducts;
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Ошибка ввода-вывода: {ex.Message}");
                    Directory.Delete(directoryPath + "\\tempDH", true);
                    return calcProducts;
                }
                catch (Exception ex)
                {
                    var log = new Log_Repository();
                    log.Add("Error", new StackTrace(), "noneUser", ex);

                    MessageBox.Show($"Неизвестная ошибка: {ex.Message}");
                    Directory.Delete(directoryPath + "\\tempDH", true);
                    return calcProducts;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return null;
            }
        }

        string NormalizeDecimalInput(string input)
        {
            try
            {
                input = input.Trim();

                if (input.StartsWith(",") || input.StartsWith("."))
                    input = "0" + input;

                input = input.Replace(',', '.');

                return input;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return null;
            }
        }

        public bool SendEmail(string mailFrom, string pass, string mailTo, string subject, string text)
        {
            try
            {
                int currentYear = DateTime.Now.Year;

                MailMessage message = new MailMessage();
                message.From = new MailAddress(mailFrom);
                message.To.Add(mailTo);
                message.Subject = subject;
                message.IsBodyHtml = true; // Включаем HTML-форматирование

                // HTML-стили для красивого оформления письма
                string htmlBody = $@"
                <html>
                <head>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            background-color: #f4f4f4;
                            text-align: center;
                            padding: 20px;
                        }}
                        .container {{
                            background: white;
                            padding: 20px;
                            border-radius: 10px;
                            box-shadow: 0px 0px 10px rgba(0, 0, 0, 0.1);
                            display: inline-block;
                            text-align: center;
                        }}
                        .title {{
                            font-size: 24px;
                            font-weight: bold;
                            color: #2d89ef;
                        }}
                        .code {{
                            font-size: 28px;
                            font-weight: bold;
                            color: #ffffff;
                            background: #2d89ef;
                            padding: 10px 20px;
                            display: inline-block;
                            border-radius: 5px;
                            margin-top: 15px;
                        }}
                        .footer {{
                            font-size: 12px;
                            color: gray;
                            margin-top: 20px;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='title'>🔐 Код подтверждения</div>
                        <div class='code'>{text}</div>
                        <div class='footer'>Если вы не запрашивали этот код, просто проигнорируйте письмо.<br>© {currentYear} Dahmira. Все права защищены.</div>
                    </div>
                </body>
                </html>";

                message.Body = htmlBody;

                SmtpClient smtp = new SmtpClient("smtp.mail.ru", 587);
                smtp.Credentials = new NetworkCredential(mailFrom, pass);
                smtp.EnableSsl = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Timeout = 120000;

                smtp.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                MessageBox.Show($"Ошибка отправки сообщения: {ex.Message}");
                return false;
            }
        }

        public List<UpdateInfo> LoadUpdates()
        {
            try
            {
                var files = Directory.GetFiles("resources/updates", "*.json");
                var updates = new List<UpdateInfo>();

                foreach (var file in files)
                {
                    var json = File.ReadAllText(file);
                    var update = JsonSerializer.Deserialize<UpdateInfo>(json);
                    updates.Add(update);
                }

                return updates
                        .OrderByDescending(u => u.Date) // от новых к старым
                        .ToList();

            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Warning", new StackTrace(), "noneUser", ex);

                return null;
            }
        }
    }
}
