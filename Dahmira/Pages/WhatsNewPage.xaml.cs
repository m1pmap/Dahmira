using Dahmira.Models;
using Dahmira_Log.DAL.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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
    /// Логика взаимодействия для WhatsNewPage.xaml
    /// </summary>
    public partial class WhatsNewPage : Window
    {
        List<UpdateInfo> updates = [];

        public WhatsNewPage(List<UpdateInfo> _updates)
        {
            try
            {
                InitializeComponent();

                updates_comboBox.ItemsSource = _updates;
                updates_comboBox.SelectedIndex = 0;


                double screenHeight = SystemParameters.PrimaryScreenHeight;

                Height = screenHeight * 0.90;
                updatesConteiner_scrollViewer.Height = Height - 105;
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

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                MainWindow window = (MainWindow)Owner;
                window.settings.isShowUpdates = !(bool)dontShowAnymore_checkBox.IsChecked;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void updates_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (updates_comboBox.SelectedItem is UpdateInfo selectedUpdate)
                {
                    updates_container.Children.Clear(); // StackPanel внутри ScrollViewer
                    versionDate_label.Content = selectedUpdate.Date.ToString("dd.MM.yyyy");

                    foreach (var category in selectedUpdate.Categories)
                    {
                        // Главная категория (заголовок)
                        var categoryBorder = new Border
                        {
                            CornerRadius = new CornerRadius(7),
                            Background = new LinearGradientBrush(
                                Colors.Red,
                                Colors.Transparent,
                                new Point(0, 0),
                                new Point(1, 0)), // горизонтальный градиент
                            Margin = new Thickness(0, selectedUpdate.Categories.IndexOf(category) == 0 ? 10 : 30, 0, 5),
                            Padding = new Thickness(3),
                            HorizontalAlignment = HorizontalAlignment.Stretch
                        };

                        // Внутренний контейнер с двумя колонками: название и дата
                        var headerGrid = new Grid();
                        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // для названия
                        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });                      // для даты

                        // Название категории
                        var categoryLabel = new Label
                        {
                            Content = category.Title.ToUpper(),
                            FontSize = 20,
                            Foreground = Brushes.White,
                            Background = Brushes.Transparent,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Center,
                            FontFamily = new FontFamily("Arial"),
                            Padding = new Thickness(0),
                            Margin = new Thickness(10, 0, 0, 0)
                        };
                        Grid.SetColumn(categoryLabel, 0);
                        headerGrid.Children.Add(categoryLabel);

                        // Дата (только для первой категории)
                        //if (selectedUpdate.Categories.IndexOf(category) == 0)
                        //{
                        //    var firstdateLabel = new Label
                        //    {
                        //        Content = selectedUpdate.Date.ToString("dd.MM.yyyy"), // или нужный формат
                        //        FontSize = 14,
                        //        Foreground = Brushes.Red,
                        //        Background = Brushes.Transparent,
                        //        HorizontalAlignment = HorizontalAlignment.Right,
                        //        VerticalAlignment = VerticalAlignment.Center,
                        //        FontFamily = new FontFamily("Consolas"),
                        //        Padding = new Thickness(0, 0, 10, 0)
                        //    };

                        //    Grid.SetColumn(firstdateLabel, 1);
                        //    headerGrid.Children.Add(firstdateLabel);
                        //}

                        categoryBorder.Child = headerGrid;
                        updates_container.Children.Add(categoryBorder);

                        foreach (var subcategory in category.Subcategories)
                        {
                            var subcategoryBorder = new Border
                            {
                                CornerRadius = new CornerRadius(7),
                                Background = new LinearGradientBrush(
                                    Color.FromArgb(155, 114, 129, 86),
                                    Colors.Transparent,
                                    new Point(0.2, 0),
                                    new Point(1.3, 0)), // горизонтальный градиент
                                Margin = new Thickness(10, 25, 0, 5),
                                HorizontalAlignment = HorizontalAlignment.Stretch
                            };

                            // Подкатегория
                            var subcategoryLabel = new Label
                            {
                                Content = subcategory.Title.ToUpper(),
                                FontSize = 15,
                                Margin = new Thickness(5, 0, 0, 0),
                                Foreground = Brushes.White,
                                Background = Brushes.Transparent,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Center,
                                FontFamily = new FontFamily("Arial"),
                            };

                            if (category.Subcategories.IndexOf(subcategory) == 0)
                            {
                                subcategoryBorder.Margin = new Thickness(10, 5, 0, 5);
                            }

                            subcategoryBorder.Child = subcategoryLabel;
                            updates_container.Children.Add(subcategoryBorder);

                            // Пункты изменений
                            foreach (var entry in subcategory.Entries)
                            {
                                var rowGrid = new Grid
                                {
                                    Margin = new Thickness(25, 0, 0, 0)
                                };

                                // Две колонки: одна узкая под маркер, вторая — под текст
                                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                                // Маркер (точка)
                                var bullet = new TextBlock
                                {
                                    Text = "•",
                                    FontSize = 17,
                                    Foreground = Brushes.Gray,
                                    Margin = new Thickness(5, 0, 0, 0),
                                    VerticalAlignment = VerticalAlignment.Top
                                };

                                // Текст с переносом
                                var entryText = new TextBlock
                                {
                                    Text = entry,
                                    FontSize = 15,
                                    TextAlignment = TextAlignment.Justify,
                                    FontFamily = new FontFamily("Arial"),
                                    Margin = new Thickness(5, 4, 0, 0),
                                    Foreground = Brushes.Gray,
                                    TextWrapping = TextWrapping.Wrap,
                                };

                                Grid.SetColumn(bullet, 0);
                                Grid.SetColumn(entryText, 1);

                                rowGrid.Children.Add(bullet);
                                rowGrid.Children.Add(entryText);

                                updates_container.Children.Add(rowGrid);
                            }
                        }
                    }

                    //var dateLabel = new Label
                    //{
                    //    Content = selectedUpdate.Date.ToString("dd.MM.yyyy"), // или нужный формат
                    //    FontSize = 14,
                    //    Foreground = Brushes.Red,
                    //    Background = Brushes.Transparent,
                    //    HorizontalAlignment = HorizontalAlignment.Right,
                    //    VerticalAlignment = VerticalAlignment.Center,
                    //    FontFamily = new FontFamily("Consolas"),
                    //    Padding = new Thickness(0, 0, 10, 0)
                    //};

                    //updates_container.Children.Add(dateLabel);
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                dontShowAnymore_checkBox.IsChecked = !((MainWindow)Owner).settings.isShowUpdates;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
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
    }
}
