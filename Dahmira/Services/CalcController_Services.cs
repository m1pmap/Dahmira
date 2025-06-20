using Dahmira_DB.DAL.Model;
using Dahmira.Interfaces;
using Dahmira.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Dahmira_Log.DAL.Repository;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using static OfficeOpenXml.ExcelErrorValue;
using Dahmira.Services.Actions;
using Dahmira_DB.DAL.Repository;
using System.Diagnostics;

namespace Dahmira.Services
{
    internal class CalcController_Services : ICalcController
    {

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

        public void Refresh(DataGrid CalcGrid, ObservableCollection<CalcProduct> calcItems) //Обновление расчётки
        {
            try
            {
                ICalcController CalcController = new CalcController_Services();
                double fullCost = 0;
                int chapterCount = 0;
                bool isNowAddToDependencies = false;
                CalcProduct selectFordependency = null;

                for (int i = 0; i < calcItems.Count - 1; i++) //Перебор всех элементов
                {
                    CalcProduct item = calcItems[i];
                    double.TryParse(NormalizeDecimalInput(item.Count), NumberStyles.Any, new CultureInfo("en-US"), out double count);
                    //double count = Convert.ToDouble(item.Count);
                    if (item.isDependency == true)
                    {
                        count = 0;

                        //Перечисление всех зависимостей и получение результата
                        foreach (var dep in item.dependencies)
                        {
                            double firstMultiplier = 1;
                            double firstProductCount = 1;
                            double secondProductCount = 1;
                            double secondMultiplier = 1;

                            if (dep.Multiplier > 0)
                            {
                                firstMultiplier = dep.Multiplier;
                            }
                            else
                            {
                                CalcProduct firstFoundProduct = calcItems.FirstOrDefault(i => i.ID == dep.ProductId);
                                if (firstFoundProduct != null)
                                {
                                    firstProductCount = Convert.ToDouble(firstFoundProduct.Count);
                                }
                            }

                            if (dep.SecondMultiplier > 0)
                            {
                                secondMultiplier = dep.SecondMultiplier;
                            }
                            else
                            {
                                CalcProduct secondFoundProduct = calcItems.FirstOrDefault(i => i.ID == dep.SecondProductId);
                                if (secondFoundProduct != null)
                                {
                                    secondProductCount = Convert.ToDouble(secondFoundProduct.Count);
                                }
                            }

                            switch (dep.SelectedType)
                            {
                                case "*":
                                    {
                                        count += (firstMultiplier * firstProductCount) * (secondProductCount * secondMultiplier);
                                        break;
                                    }
                                case "+":
                                    {
                                        count += (firstMultiplier * firstProductCount) + (secondProductCount * secondMultiplier);
                                        break;
                                    }
                                case "-":
                                    {
                                        count += (firstMultiplier * firstProductCount) - (secondProductCount * secondMultiplier);
                                        break;
                                    }
                                case "/":
                                    {
                                        count += (firstMultiplier * firstProductCount) / (secondProductCount * secondMultiplier);
                                        break;
                                    }
                            }
                        }
                    }

                    if (item.ID > 0)
                    {
                        item.Count = count.ToString();
                        item.TotalCost = item.Cost * Convert.ToDouble(item.Count); //Обновление итоговой цены
                    }

                    if (item.Num != i + 1) //Если номер идёт не по порядку
                    {
                        if (item.Num == 0) //Если это раздел
                        {
                            chapterCount++;
                        }
                        else
                        {
                            item.Num = i + 1 - chapterCount;
                        }
                    }
                    else
                    {
                        item.Num = i + 1 - chapterCount;
                    }

                    if (item.TotalCost > 0) //Если цена положительная
                    {
                        fullCost += item.TotalCost;
                    }

                    if (item.ID == 0) //Если это раздел, то изменяем ему фон на светло-синий
                    {
                        item.RowColor = ColorToHex(Color.FromRgb(223, 242, 253));
                        item.RowForegroundColor = ColorToHex(Colors.Black);
                    }
                    else
                    {
                        if (item.ID == -1)
                        {
                            item.RowColor = ColorToHex(Color.FromRgb(254, 241, 230));
                            item.RowForegroundColor = ColorToHex(Colors.Black);
                        }
                        else
                        {
                            if (item.RowColor == ColorToHex(Colors.Coral))
                            {
                                item.RowColor = ColorToHex(Colors.Coral);
                                item.RowForegroundColor = ColorToHex(Colors.White);
                            }
                            else
                            {
                                if (item.HasErrorInRow) //Если элемента нет в бд, то оставляем цвет красным
                                {
                                    item.RowColor = ColorToHex(Colors.OrangeRed);
                                    item.RowForegroundColor = ColorToHex(Colors.White);
                                }
                                else
                                {
                                    if (item.RowColor == ColorToHex(Colors.CornflowerBlue)) //Если цвет Синий, то оставляем цвет таким же
                                    {
                                        item.RowColor = ColorToHex(Colors.CornflowerBlue);
                                        item.RowForegroundColor = ColorToHex(Colors.White);
                                        selectFordependency = item;
                                        isNowAddToDependencies = true;
                                    }
                                    else //Иначе делаем все поля прозрачными с серым цветом
                                    {
                                        item.RowColor = ColorToHex(Colors.Transparent);
                                        item.RowForegroundColor = ColorToHex(Colors.Black);
                                    }
                                }
                            }
                        }
                    }

                    if (item.dependencies.Count > 0)
                    {
                        item.HasErrorInDependency = false;
                    }
                    else
                    {
                        item.HasErrorInDependency = true;
                    }
                }

                var selectedItem = (CalcProduct)CalcGrid.SelectedItem; //Получаем выбранный элемент
                if (selectedItem != null)
                {
                    if (isNowAddToDependencies) //Если сейчас идёт добавление
                    {
                        selectedItem = selectFordependency; //Меняем выбранный элемент на то, в который идёт добавление
                    }
                    foreach (var dependency in selectedItem.dependencies) //Отображение всех зависимостей
                    {
                        CalcProduct foundProduct = calcItems.FirstOrDefault(p => p.ID == dependency.ProductId);
                        if (foundProduct != null)
                        {
                            foundProduct.RowColor = ColorToHex(Colors.MediumSeaGreen);
                            foundProduct.RowForegroundColor = ColorToHex(Colors.White);
                        }
                        foundProduct = calcItems.FirstOrDefault(p => p.ID == dependency.SecondProductId);
                        if (foundProduct != null)
                        {
                            foundProduct.RowColor = ColorToHex(Colors.MediumSeaGreen);
                            foundProduct.RowForegroundColor = ColorToHex(Colors.White);
                        }
                    }

                    //ValidateCalcItem(selectedItem);
                }



                calcItems[calcItems.Count - 1].TotalCost = Math.Round(fullCost, 2);
                CalcGrid.CommitEdit();
                //CalcGrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }
        public bool AddToCalc(DataGrid DBGrid, DataGrid CalcGrid, MainWindow window, string count = "1", string position = "Last") //Добавление в расчётку товара
        {
            try
            {
                if(window.calcItems.Count > 1)
                {
                    Material selectedDBItem = (Material)DBGrid.SelectedItem; //Текущий выбранный элемент в БД
                    int selectedCalcItemIndex = CalcGrid.SelectedIndex; //Индекс текущего выбранного элемента в расчётке

                    int maxId = window.calcItems.Max(item => item.ID);

                    //Создание нового элемента расчётки
                    CalcProduct newCalcProductItem = new()
                    {
                        ID = maxId + 1,
                        Num = window.calcItems.Count + 1,
                        Manufacturer = selectedDBItem.Manufacturer,
                        ProductName = selectedDBItem.ProductName,
                        EnglishProductName = selectedDBItem.EnglishProductName,
                        Article = selectedDBItem.Article,
                        Unit = selectedDBItem.Unit,
                        EnglishUnit = selectedDBItem.EnglishUnit,
                        Photo = selectedDBItem.Photo,
                        RealCost = Math.Round(selectedDBItem.Cost, 2),
                        Cost = Math.Round(selectedDBItem.Cost, 2),
                        Count = count,
                        TotalCost = Math.Round(selectedDBItem.Cost, 2),
                        Note = ""
                    };

                    switch (position) //В зависимости от выбранной позиции для добавления
                    {
                        case "Last": //В конец
                            {
                                if (window.calcItems[window.calcItems.Count - 2].isVisible == false)
                                {
                                    newCalcProductItem.isVisible = false;
                                }

                                if (window.calcItems[window.calcItems.Count - 2].ID == -1)
                                {
                                    return false;
                                }

                                window.PriceInfo_label.Content = $"Строка {DBGrid.SelectedIndex + 1 } прайса добавлена под {window.calcItems.Count - 1} в расчёте.";
                                window.calcItems.Insert(window.calcItems.Count - 1, newCalcProductItem);
                                window.CalcDataGrid.SelectedItem = newCalcProductItem;
                                window.WarningFlashing("Добавлено!", window.WarningBorder, window.WarningLabel, Colors.MediumSeaGreen, 1);
                                window.actions.Push(new AddCalc_Action(new List<CalcProduct> { newCalcProductItem }, window));
                                break;
                            }
                        case "UnderSelect": //Под выбранным
                            {

                                if (CalcGrid.SelectedItem == null)
                                {
                                    window.PriceInfo_label.Content = $"Строка прайса не добавлена в расчёт. Для начала выберите строку в расчёте.";
                                    break;
                                }

                                if (window.calcItems[selectedCalcItemIndex].ID == -1)
                                {
                                    if (window.calcItems[selectedCalcItemIndex + 1].ID == 0)
                                    {
                                        selectedCalcItemIndex++;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }

                                if (window.calcItems[selectedCalcItemIndex].isVisible == false)
                                {
                                    newCalcProductItem.isVisible = false;
                                }

                                window.PriceInfo_label.Content = $"Строка {DBGrid.SelectedIndex + 1} прайса добавлена под {CalcGrid.SelectedIndex + 1} в расчёте.";
                                if (CalcGrid.SelectedIndex != window.calcItems.Count - 1)
                                {
                                    window.calcItems.Insert(selectedCalcItemIndex + 1, newCalcProductItem);
                                }
                                else
                                {
                                    for (int i = selectedCalcItemIndex - 1; i >= 0; i--)
                                    {
                                        if (window.calcItems[i].hideButtonContext == "+")
                                        {
                                            newCalcProductItem.isVisible = false;
                                            break;
                                        }
                                    }
                                    window.calcItems.Insert(selectedCalcItemIndex, newCalcProductItem);
                                }
                                window.CalcDataGrid.SelectedItem = newCalcProductItem;
                                window.WarningFlashing("Добавлено!", window.WarningBorder, window.WarningLabel, Colors.MediumSeaGreen, 1);
                                window.actions.Push(new AddCalc_Action(new List<CalcProduct> { newCalcProductItem }, window));
                                break;
                            }
                        case "Replace": //Заменить
                            {
                                if (CalcGrid.SelectedItem == null)
                                {
                                    window.PriceInfo_label.Content = $"Строка не заменена. Для начала выберите строку в расчёте.";
                                    window.WarningFlashing("Выберите строку!", window.WarningBorder, window.WarningLabel, Colors.OrangeRed, 1);
                                    break;
                                }
                                if (window.calcItems[selectedCalcItemIndex].ID == -1 || window.calcItems[selectedCalcItemIndex].ID == 0)
                                {
                                    window.PriceInfo_label.Content = $"Выбранную строку в расчёте нельзя заменить.";
                                    window.WarningFlashing("Нельзя заменить!", window.WarningBorder, window.WarningLabel, Colors.OrangeRed, 1);
                                    break;
                                }

                                if (selectedCalcItemIndex != window.calcItems.Count - 1)
                                {
                                    CalcProduct oldProduct = window.calcItems[selectedCalcItemIndex].Clone();
                                    if(oldProduct.isDependency == true)
                                    {
                                        window.DependencyImage.Visibility = Visibility.Visible;
                                        window.DependencyDataGrid.Visibility = Visibility.Hidden;
                                        window.DependencyButtons.Visibility = Visibility.Hidden;
                                    }
                                    window.PriceInfo_label.Content = $"Строка {DBGrid.SelectedIndex + 1} прайса заменила {CalcGrid.SelectedIndex + 1} в расчёте.";

                                    window.calcItems[selectedCalcItemIndex].ID = newCalcProductItem.ID;
                                    window.calcItems[selectedCalcItemIndex].Num = newCalcProductItem.Num;
                                    window.calcItems[selectedCalcItemIndex].Manufacturer = newCalcProductItem.Manufacturer;
                                    window.calcItems[selectedCalcItemIndex].ProductName = newCalcProductItem.ProductName;
                                    window.calcItems[selectedCalcItemIndex].EnglishProductName = newCalcProductItem.EnglishProductName;
                                    window.calcItems[selectedCalcItemIndex].Article = newCalcProductItem.Article;
                                    window.calcItems[selectedCalcItemIndex].Unit = newCalcProductItem.Unit;
                                    window.calcItems[selectedCalcItemIndex].EnglishUnit = newCalcProductItem.EnglishUnit;
                                    window.calcItems[selectedCalcItemIndex].Photo = newCalcProductItem.Photo;
                                    window.calcItems[selectedCalcItemIndex].RealCost = newCalcProductItem.RealCost;
                                    window.calcItems[selectedCalcItemIndex].Cost = newCalcProductItem.Cost;
                                    window.calcItems[selectedCalcItemIndex].Count = newCalcProductItem.Count;
                                    window.calcItems[selectedCalcItemIndex].TotalCost = newCalcProductItem.TotalCost;
                                    window.calcItems[selectedCalcItemIndex].Note = newCalcProductItem.Note;
                                    window.calcItems[selectedCalcItemIndex].RowColor = "#FFFFFF";
                                    window.calcItems[selectedCalcItemIndex].RowForegroundColor = "#000000";
                                    window.calcItems[selectedCalcItemIndex].isDependency = false;
                                    window.calcItems[selectedCalcItemIndex].dependencies = new ObservableCollection<Dependency>();

                                    window.WarningFlashing("Заменено!", window.WarningBorder, window.WarningLabel, Colors.MediumSeaGreen, 1);

                                    CalcProduct newProduct = window.calcItems[selectedCalcItemIndex];
                                    window.actions.Push(new ReplaceCalc_Action(oldProduct, newProduct, window));
                                }
                                break;
                            }
                    }

                    ValidateCalcItem(newCalcProductItem);

                    if (!window.isCalculationNeed)
                    {
                        window.isCalculationNeed = true;
                        window.MovingLabel.Visibility = Visibility.Visible;
                    }
                    Refresh(CalcGrid, window.calcItems); //Обновление
                    window.isCalcSaved = false;
                    return true;
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

        public void ObjectFlashing(Border target, Color initialColor, Color flashingColor, double interval) //Анимация мигания выбранной кнопки и выбранными цветами
        {
            try
            {
                // Создаем анимацию
                var storyboard = new Storyboard();

                // Создаем анимацию цвета фона
                ColorAnimation colorAnimation = new ColorAnimation
                {
                    From = initialColor,
                    To = flashingColor,
                    Duration = new Duration(TimeSpan.FromMilliseconds(750)),
                    AutoReverse = true,
                    RepeatBehavior = new RepeatBehavior(interval) // Количество миганий
                };

                // Применяем анимацию к фону кнопки
                Storyboard.SetTarget(colorAnimation, target);
                Storyboard.SetTargetProperty(colorAnimation, new PropertyPath("Background.Color"));

                // Добавляем анимацию в storyboard
                storyboard.Children.Add(colorAnimation);

                // Запускаем анимацию
                storyboard.Begin();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }

        public string ColorToHex(Color color)
        {
            try
            {
                return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return null;
            }
        }

        public Color HexToColor(string hex)
        {
            try
            {
                // Удаляем символ '#' если он есть
                hex = hex.Replace("#", "");

                // Если длина строки 6, добавляем 2 символа для альфа-канала (полностью непрозрачный)
                if (hex.Length == 6)
                {
                    hex = "FF" + hex;
                }

                // Преобразуем HEX в ARGB
                byte a = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                byte r = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                byte g = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
                byte b = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);

                return Color.FromArgb(a, r, g, b);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return new Color();
            }
        }

        void ICalcController.UpdateCellStyle(DataGrid dataGrid, Brush backgroundColor, Brush foregroundColor)
        {
            try
            {
                Style oldCellStyle = dataGrid.CellStyle;

                // Создаем новый стиль, наследуя старый
                Style newCellStyle = new Style(typeof(DataGridCell), oldCellStyle);

                // Добавляем триггер для выделенных ячеек
                Trigger selectedTrigger = new Trigger
                {
                    Property = DataGridCell.IsSelectedProperty,
                    Value = true
                };

                // Устанавливаем фон и цвет текста для выделенной ячейки
                selectedTrigger.Setters.Add(new Setter(DataGridCell.BackgroundProperty, backgroundColor));
                selectedTrigger.Setters.Add(new Setter(DataGridCell.ForegroundProperty, foregroundColor));

                // Добавляем триггер в новый стиль
                newCellStyle.Triggers.Add(selectedTrigger);

                // Применяем новый стиль к DataGrid
                dataGrid.CellStyle = newCellStyle;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }

        public bool ArePhotosEqual(byte[] photo1, byte[] photo2) //Равны ли 2 фото, представленные в массиве байтов
        {
            try
            {
                if (photo1 == null && photo2 == null) //Если оба фото не указаны 
                {
                    return true;
                }

                if (photo1 == null || photo2 == null) //Одно из фото не указано
                {
                    return false;
                }

                if (photo1.Length != photo2.Length) //Если у фото разная длина
                {
                    return false;
                }

                for (int i = 0; i < photo1.Length; i++)
                {
                    if (photo1[i] != photo2[i]) //Если у фото разное содержимое
                    {
                        return false;
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

        

        bool ICalcController.CheckingDifferencesWithDB(DataGrid CalcDataGrid, MainWindow window) //Проверка идентичности данных в БД и расчетке
        {
            try
            {
                bool isDifferent = false; //Указвает на присутствие отличий в фото
                                          //bool isRemovedOnDB = false; //Указвает на то, есть ли элементы, которых нет в бд

                ClearBackgroundsColors(window);
                int sameAnswerCount = 0;
                MessageBoxResult lastResult = new MessageBoxResult();
                int answersCountNecessity = 3;
                bool isMessagesSkip = false;

                foreach (var item in window.calcItems)
                {
                    if (item.ID > 0) //Если не раздел
                    {
                        Material dbMaterial = window.dbItems.FirstOrDefault(i => i.Article == item.Article); //Проверяем наличие этого элемента в БД
                        if (dbMaterial != null) //Если элемент есть
                        {
                            if (!ArePhotosEqual(item.Photo, dbMaterial.Photo)) //Если фото не равны
                            {
                                item.Photo = dbMaterial.Photo;
                                isDifferent = true;
                            }
                            if (!window.settings.isEnglishNameVisible && (item.ProductName != dbMaterial.ProductName || item.Unit != dbMaterial.Unit) ||
                                (window.settings.isEnglishNameVisible && (item.EnglishProductName != dbMaterial.EnglishProductName || item.EnglishUnit != dbMaterial.EnglishUnit))
                                || item.Manufacturer != dbMaterial.Manufacturer)
                            {
                                item.RowColor = ColorToHex(Colors.LightGray);
                                item.RowForegroundColor = ColorToHex(Colors.White);

                                CalcDataGrid.Items.Refresh();

                                MessageBoxResult result;
                                if (isMessagesSkip)
                                {
                                    result = lastResult;
                                }
                                else
                                {
                                    result = MessageBox.Show("В расчётке:" +
                                                                              "\nПроизводитель: " + item.Manufacturer +
                                                                              "\nНаименование: " + item.ProductName +
                                                                              "\n\nВ Базе Данных:\nПроизводитель: " + dbMaterial.Manufacturer +
                                                                              "\nНаименование: " + dbMaterial.ProductName +
                                                                              "\n\nЗаменить в расчётке эти данные или оставить как есть?\nВ любом случае он будет подсвечен серым",
                                                                              "Несоответствие с Прайсом", MessageBoxButton.YesNo, MessageBoxImage.Information);
                                }

                                if (result == MessageBoxResult.Yes)
                                {
                                    if (window.settings.isEnglishNameVisible)
                                    {
                                        item.EnglishUnit = dbMaterial.EnglishUnit;
                                        item.EnglishProductName = dbMaterial.EnglishProductName;
                                    }
                                    else
                                    {
                                        item.Unit = dbMaterial.Unit;
                                        item.ProductName = dbMaterial.ProductName;
                                    }

                                    item.Manufacturer = dbMaterial.Manufacturer;
                                }

                                if (sameAnswerCount > 0)
                                {
                                    if (lastResult == result)
                                    {
                                        sameAnswerCount++;
                                    }
                                    else
                                    {
                                        sameAnswerCount = 0;
                                    }
                                }
                                else
                                {
                                    sameAnswerCount++;
                                }

                                lastResult = result;

                                if (sameAnswerCount == answersCountNecessity)
                                {
                                    MessageBoxResult answerResult = MessageBox.Show(
                                                    $"Вы {answersCountNecessity} раз{(answersCountNecessity == 3 ? "а" : "")} выбрали одно и то же действие. " +
                                                    "Не хотите выполнить его для всех оставшихся элементов?",
                                                    "Подтверждение",
                                                    MessageBoxButton.YesNo,
                                                    MessageBoxImage.Warning);

                                    if (answerResult == MessageBoxResult.Yes)
                                    {
                                        isMessagesSkip = true;
                                    }
                                    else
                                    {
                                        sameAnswerCount = 0;
                                        answersCountNecessity = 10;
                                    }
                                }


                                isDifferent = true;
                            }
                            else //Если производитель и наименование равно
                            {
                                item.RowColor = ColorToHex(Colors.Transparent);
                                item.RowForegroundColor = ColorToHex(Colors.Black);
                            }
                        }
                        else //Если элемента нет
                        {
                            item.HasErrorInRow = true;
                            item.RowColor = ColorToHex(Colors.OrangeRed);
                            item.RowForegroundColor = ColorToHex(Colors.White);
                            CalcDataGrid.Items.Refresh();
                            MessageBox.Show("Артикула нет в прайсе!" +
                                            "\nОтсутствующий артикул подсвечен красным." +
                                            "\nНомер элемента: " + item.Num.ToString() +
                                            "\nПроизводитель: " + item.Manufacturer +
                                            "\nНаименование: " + item.ProductName +
                                            "\nАртикул " + item.Article, "Товар отсутствует в Прайсе", MessageBoxButton.OK, MessageBoxImage.Error); ;
                            isDifferent = true;
                        }
                    }
                    CalcDataGrid.Items.Refresh();

                }

                if (!isDifferent)
                {
                    window.CalcInfo_label.Content = "Соответствие с Прайсом не нарушена.";
                    return true;
                }

                window.CalcInfo_label.Content = "Нарушено соответствие с Прайсом.";
                return false;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return false;
            }
        }

        public void Calculation(MainWindow window)
        {
            try
            {
                foreach (var item in window.calcItems)
                {
                    if (item.ID > 0) //Если не раздел
                    {
                        if (window.dbItems.Any(i => i.Article == item.Article))
                        {
                            Material material = window.dbItems.First(i => i.Article == item.Article);
                            if (material.Cost != item.RealCost)
                            {
                                item.RealCost = material.Cost;
                                //item.RowColor = ColorToHex(Colors.Transparent);
                            }
                        }
                        else
                        {
                            item.RealCost = item.Cost;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }

        public void ClearBackgroundsColors(MainWindow window)
        {
            try
            {
                foreach (var item in window.calcItems)
                {
                    if (item.RowColor != ColorToHex(Colors.Transparent) && item.RowColor != ColorToHex(Color.FromRgb(223, 242, 253)) && item.RowColor != ColorToHex(Color.FromRgb(254, 241, 230)))
                    {
                        item.RowColor = ColorToHex(Colors.Transparent);
                        item.RowForegroundColor = ColorToHex(Colors.Black);
                        item.HasErrorInRow = false;
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


        public bool Add_AllMaterial_FromExcel(ObservableCollection<Material> DBItems, DataGrid CalcGrid, MainWindow window) //Добавление в расчётку товара
        {
            try
            {
                int maxId = window.calcItems.Max(item => item.ID);
                for (int i = 0; i < DBItems.Count; i++)
                {
                    maxId++;
                    //Создание нового элемента расчётки
                    CalcProduct newCalcProductItem = new()
                    {
                        ID = maxId,
                        Num = window.calcItems.Count + 1,
                        Type = DBItems[i].Type,
                        Manufacturer = DBItems[i].Manufacturer,
                        ProductName = DBItems[i].ProductName,
                        EnglishProductName = DBItems[i].EnglishProductName,
                        Article = DBItems[i].Article,
                        Unit = DBItems[i].Unit,
                        EnglishUnit = DBItems[i].EnglishUnit,
                        Photo = DBItems[i].Photo,
                        RealCost = Math.Round(DBItems[i].Cost, 2),
                        Cost = Math.Round(DBItems[i].Cost, 2),
                        Count = "1",
                        TotalCost = Math.Round(DBItems[i].Cost, 2),
                        Note = ""
                    };

                    window.calcItems.Insert(window.calcItems.Count - 1, newCalcProductItem);
                    ValidateCalcItem(newCalcProductItem);
                }
                Refresh(CalcGrid, window.calcItems); //Обновление
                window.isCalcSaved = false;
                return true;

            }

            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return false;
            }
        }

        public void ValidateCalcItems(ObservableCollection<CalcProduct> calcItems)
        {
            try
            {
                foreach (CalcProduct calcItem in calcItems)
                {
                    if (calcItem.ID > 0)
                    {
                        ValidateCalcItem(calcItem);
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }

        public void ValidateCalcItem(CalcProduct calcItem)
        {
            try
            {
                calcItem.IsCellCorrects[0] = !string.IsNullOrWhiteSpace(calcItem.Manufacturer);
                calcItem.IsCellCorrects[1] = !string.IsNullOrWhiteSpace(calcItem.ProductName);
                calcItem.IsCellCorrects[2] = !string.IsNullOrWhiteSpace(calcItem.EnglishProductName);
                calcItem.IsCellCorrects[3] = !string.IsNullOrWhiteSpace(calcItem.Article);
                calcItem.IsCellCorrects[4] = !string.IsNullOrWhiteSpace(calcItem.Unit);
                calcItem.IsCellCorrects[5] = !string.IsNullOrWhiteSpace(calcItem.EnglishUnit);
                calcItem.IsCellCorrects[6] = calcItem.Cost != 0;
                calcItem.IsCellCorrects[7] = calcItem.Count != "0";
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }

        public bool IsCalcValid(ObservableCollection<CalcProduct> calcItems, SettingsParameters settings)
        {
            try
            {
                //Индексы проверки в зависимости от языка
                int[] languageIndexes = settings.isEnglishNameVisible
                    ? [2, 5]
                    : [1, 4];

                //Обязательные индексы для всех элементов
                var mandatoryIndexes = new[] { 0, 3, 6, 7 };

                //Проверка условия
                bool languageValid = !calcItems.Any(item => languageIndexes.Any(idx => !item.IsCellCorrects[idx]));
                bool mandatoryValid = !calcItems.Any(item => mandatoryIndexes.Any(idx => !item.IsCellCorrects[idx]));

                return languageValid && mandatoryValid;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return false;
            }
        }

        public void ActivateNeedCalculation(MainWindow window)
        {
            try
            {
                if (!window.isCalculationNeed)
                {
                    window.MovingLabel.Visibility = Visibility.Visible;
                    window.isCalculationNeed = true;
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
