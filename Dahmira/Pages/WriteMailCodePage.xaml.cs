using Dahmira.Interfaces;
using Dahmira.Services;
using Dahmira_Log.DAL.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Dahmira.Pages
{
    /// <summary>
    /// Логика взаимодействия для WriteMailCodePage.xaml
    /// </summary>
    public partial class WriteMailCodePage : Window
    {
        private List<TextBox> allTextBoxes;
        private int textBoxIndex = 0;
        string code;
        string mail;
        public WriteMailCodePage(string code, string mail)
        {
            try
            {
                InitializeComponent();

                this.code = code;
                this.mail = mail;
                allTextBoxes = new List<TextBox> { textBox1, textBox2, textBox3, textBox4, textBox5, textBox6 };
                allTextBoxes[0].Focus();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                e.Handled = !Regex.IsMatch(e.Text, @"^[A-Z0-9]$");
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TextBox currentTextBox = sender as TextBox;
                if (currentTextBox != null && currentTextBox.Text.Length == 1)
                {
                    if (textBoxIndex != 5)
                    {
                        allTextBoxes[textBoxIndex + 1].Focus();
                        textBoxIndex++;
                    }
                    else
                    {
                        bool res = checkCorrectCode(code);
                        if (res)
                        {
                            this.Close();
                            MessageBox.Show("Почта успешно подтверждена!");
                        }
                        else
                        {
                            foreach (TextBox textBox in allTextBoxes)
                            {
                                textBox.BorderBrush = new SolidColorBrush(Colors.OrangeRed);
                                textBox.Text = string.Empty;
                            }
                            takeCodeAgain.Focus();
                        }
                        textBoxIndex = 0;
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

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBox currentTextBox = sender as TextBox;
                currentTextBox.BorderBrush = new SolidColorBrush(Colors.Gray);
                currentTextBox.Text = string.Empty;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void TextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TextBox currentTextBox = sender as TextBox;
                textBoxIndex = allTextBoxes.IndexOf(currentTextBox);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private bool checkCorrectCode(string code)
        {
            try
            {
                string textBoxesCode = string.Join("", allTextBoxes.Select(tb => tb.Text.Trim()));

                return code.Equals(textBoxesCode, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return false;
            }
        }

        private void takeCodeAgain_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string mailFrom = "dok_koks@mail.ru";
                string pass = "TKZB28r34gSTNVmY7DeW";
                string subject = "Подтверждение почты";
                code = GenerateAlphaNumericCode(6);
                string text = $"Ваш код подтверждения: {code}";


                IFileImporter importer = new FileImporter_Services();

                if (importer.SendEmail(mailFrom, pass, mail, subject, text))
                {
                    MessageBox.Show("Код успешно отправлен на почту");
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        static string GenerateAlphaNumericCode(int length)
        {
            try
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                Random random = new Random();
                return new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return null;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if (e.Key == Key.V)
                    {
                        //MessageBox.Show("1");
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

        private void cancel_button_Click(object sender, RoutedEventArgs e)
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
