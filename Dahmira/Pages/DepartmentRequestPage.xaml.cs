using Dahmira.Models;
using Dahmira_Log.DAL.Repository;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Dahmira.Pages
{
    /// <summary>
    /// Логика взаимодействия для DepartmentRequestPage.xaml
    /// </summary>
    public partial class DepartmentRequestPage : Window
    {

        public DepartmentRequest departmentRequest = null;
        public bool isDepartmentRequestComplete = false;
        public DepartmentRequestPage(DepartmentRequest lastDepartmentRequest)
        {
            try
            {
                InitializeComponent();

                double screenHeight = SystemParameters.PrimaryScreenHeight;

                if (screenHeight < 1080)
                {
                    Height = screenHeight * 0.94;
                }

                CultureInfo culture = new CultureInfo("ru-RU"); // Устанавливаем российскую локаль (dd.MM.yyyy)
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;

                requestNum_textBox.Text = lastDepartmentRequest.RequestNum;
                date_datePicker.DisplayDate = lastDepartmentRequest.Date;
                manager_textBox.Text = lastDepartmentRequest.Manager;
                client_textBox.Text = lastDepartmentRequest.Client;
                country_comboBox.Text = lastDepartmentRequest.Country;
                location_textBox.Text = lastDepartmentRequest.Location;

                AnimalType_comboBox.Text = lastDepartmentRequest.AnimalType;
                headCount_textBox.Text = lastDepartmentRequest.HeadCount;
                isBuildingHas_comboBox.Text = lastDepartmentRequest.IsBuildingHas ? "Да" : "Нет";
                group_textBox.Text = lastDepartmentRequest.Group;
                maxWeight_textBox.Text = lastDepartmentRequest.MaxWeight;
                sectionCount_textBox.Text = lastDepartmentRequest.SectionCount;

                LSize_textBox.Text = lastDepartmentRequest.LSize;
                H1Size_textBox.Text = lastDepartmentRequest.H1Size;
                H2Size_textBox.Text = lastDepartmentRequest.H2Size;
                WSize_textBox.Text = lastDepartmentRequest.WSize;
                L1Size_textBox.Text = lastDepartmentRequest.L1Size;
                L2Size_textBox.Text = lastDepartmentRequest.L2Size;

                maintanance_textBox.Text = lastDepartmentRequest.Maintanance;
                feeding_textBox.Text = lastDepartmentRequest.Feeding;
                watering_textBox.Text = lastDepartmentRequest.Watering;
                microclimate_textBox.Text = lastDepartmentRequest.Microclimate;
                manureRemoval_textBox.Text = lastDepartmentRequest.ManureRemoval;
                electricity_textBox.Text = lastDepartmentRequest.Electricity;


                CheckRequestCompleteness();
                note_textBox.Text = lastDepartmentRequest.Note;
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

        private bool CheckTextBoxWhiteSpace(TextBox textBox)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.BorderBrush = Brushes.OrangeRed;
                    return false;
                }
                else
                {
                    textBox.BorderBrush = Brushes.Gray;
                    return true;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return false;
            }
        }

        private bool CheckComboBoxWhiteSpace(ComboBox comboBox)
        {
            try
            {
                var border = GetParentBorder(comboBox);
                if (string.IsNullOrWhiteSpace(comboBox.Text))
                {
                    border.BorderBrush = Brushes.OrangeRed;
                    return false;
                }
                else
                {
                    border.BorderBrush = Brushes.Gray;
                    return true;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return false;
            }
        }

        bool CheckRequestCompleteness()
        {
            try
            {
                bool res = true;
                if (requestNum_textBox.Text == "0" || requestNum_textBox.Text == "")
                {
                    requestNum_textBox.BorderBrush = Brushes.OrangeRed;
                    res = false;
                }
                else
                {
                    requestNum_textBox.BorderBrush = Brushes.Gray;
                }

                res = CheckTextBoxWhiteSpace(manager_textBox) && res;

                res = CheckTextBoxWhiteSpace(client_textBox) && res;
                res = CheckComboBoxWhiteSpace(country_comboBox) && res;

                res = CheckComboBoxWhiteSpace(AnimalType_comboBox) && res;
                res = CheckTextBoxWhiteSpace(headCount_textBox) && res;
                res = CheckComboBoxWhiteSpace(isBuildingHas_comboBox) && res;

                if (isBuildingHas_comboBox.SelectedIndex == 0)
                {
                    res = CheckTextBoxWhiteSpace(LSize_textBox) && res;
                    res = CheckTextBoxWhiteSpace(WSize_textBox) && res;
                }
                else
                {
                    LSize_textBox.BorderBrush = Brushes.Gray;
                    WSize_textBox.BorderBrush = Brushes.Gray;
                }

                if (string.IsNullOrWhiteSpace(maintanance_textBox.Text) &&
                   string.IsNullOrWhiteSpace(maintanance_textBox.Text) &&
                   string.IsNullOrWhiteSpace(maintanance_textBox.Text) &&
                   string.IsNullOrWhiteSpace(maintanance_textBox.Text) &&
                   string.IsNullOrWhiteSpace(maintanance_textBox.Text) &&

                   maintanance_checkBox.IsChecked == false &&
                   feeding_checkBox.IsChecked == false &&
                   watering_checkBox.IsChecked == false &&
                   microclimate_checkBox.IsChecked == false &&
                   manureRemoval_checkBox.IsChecked == false &&
                   electricity_checkBox.IsChecked == false)
                {
                    maintanance_textBox.BorderBrush = Brushes.OrangeRed;
                    feeding_textBox.BorderBrush = Brushes.OrangeRed;
                    watering_textBox.BorderBrush = Brushes.OrangeRed;
                    microclimate_textBox.BorderBrush = Brushes.OrangeRed;
                    manureRemoval_textBox.BorderBrush = Brushes.OrangeRed;
                    electricity_textBox.BorderBrush = Brushes.OrangeRed;
                    res = false;
                }
                else
                {
                    res = CheckAdditionallyCheckBox(maintanance_checkBox, maintanance_textBox) && res;
                    res = CheckAdditionallyCheckBox(feeding_checkBox, feeding_textBox) && res;
                    res = CheckAdditionallyCheckBox(watering_checkBox, watering_textBox) && res;
                    res = CheckAdditionallyCheckBox(microclimate_checkBox, microclimate_textBox) && res;
                    res = CheckAdditionallyCheckBox(manureRemoval_checkBox, manureRemoval_textBox) && res;
                    res = CheckAdditionallyCheckBox(electricity_checkBox, electricity_textBox) && res;
                }

                return res;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return false;
            }
        }

        bool CheckAdditionallyCheckBox(CheckBox checkBox, TextBox textBox)
        {
            try
            {
                if (checkBox.IsChecked == true)
                {
                    if (string.IsNullOrWhiteSpace(textBox.Text))
                    {
                        textBox.BorderBrush = Brushes.OrangeRed;
                        return false;
                    }
                }

                textBox.BorderBrush = Brushes.Gray;
                return true;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return false;
            }
        }

        public Border? GetParentBorder(DependencyObject child)
        {
            try
            {
                DependencyObject parent = child;

                while (parent != null && parent is not Border)
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }

                return parent as Border;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return null;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                date_datePicker.SelectedDate = DateTime.Now;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void maintanance_textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(maintanance_textBox.Text))
                    maintanance_checkBox.IsChecked = false;
                else
                    maintanance_checkBox.IsChecked = true;

                CheckRequestCompleteness();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void feeding_textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(feeding_textBox.Text))
                    feeding_checkBox.IsChecked = false;
                else
                    feeding_checkBox.IsChecked = true;

                CheckRequestCompleteness();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void watering_textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(watering_textBox.Text))
                    watering_checkBox.IsChecked = false;
                else
                    watering_checkBox.IsChecked = true;

                CheckRequestCompleteness();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void microclimate_textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(microclimate_textBox.Text))
                    microclimate_checkBox.IsChecked = false;
                else
                    microclimate_checkBox.IsChecked = true;

                CheckRequestCompleteness();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void manureRemoval_textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(manureRemoval_textBox.Text))
                    manureRemoval_checkBox.IsChecked = false;
                else
                    manureRemoval_checkBox.IsChecked = true;

                CheckRequestCompleteness();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void electricity_textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(electricity_textBox.Text))
                    electricity_checkBox.IsChecked = false;
                else
                    electricity_checkBox.IsChecked = true;

                CheckRequestCompleteness();
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
                if (CheckRequestCompleteness())
                {
                    isDepartmentRequestComplete = true;
                }
                else
                {
                    isDepartmentRequestComplete = false;
                }

                departmentRequest = new DepartmentRequest
                {
                    RequestNum = requestNum_textBox.Text,
                    Date = date_datePicker.DisplayDate,
                    Manager = manager_textBox.Text,
                    Client = client_textBox.Text,
                    Country = country_comboBox.Text,
                    Location = location_textBox.Text,

                    AnimalType = AnimalType_comboBox.Text,
                    HeadCount = headCount_textBox.Text,
                    IsBuildingHas = (isBuildingHas_comboBox.SelectedIndex == 0),
                    Group = group_textBox.Text,
                    MaxWeight = maxWeight_textBox.Text,
                    SectionCount = sectionCount_textBox.Text,

                    LSize = LSize_textBox.Text,
                    H1Size = H1Size_textBox.Text,
                    H2Size = H2Size_textBox.Text,
                    WSize = WSize_textBox.Text,
                    L1Size = L1Size_textBox.Text,
                    L2Size = L2Size_textBox.Text,

                    Maintanance = maintanance_textBox.Text,
                    Feeding = feeding_textBox.Text,
                    Watering = watering_textBox.Text,
                    Microclimate = microclimate_textBox.Text,
                    ManureRemoval = manureRemoval_textBox.Text,
                    Electricity = electricity_textBox.Text,

                    Note = note_textBox.Text
                };
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
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

        private void date_datePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(date_datePicker.Text))
                {
                    date_datePicker.Text = DateTime.Now.ToString("dd.MM.yyyy");
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void maintanance_checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                maintanance_textBox.Text = string.Empty;
                CheckRequestCompleteness();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }

        }

        private void feeding_checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                feeding_textBox.Text = string.Empty;
                CheckRequestCompleteness();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }

        }

        private void watering_checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                watering_textBox.Text = string.Empty;
                CheckRequestCompleteness();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void microclimate_checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                microclimate_textBox.Text = string.Empty;
                CheckRequestCompleteness();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void manureRemoval_checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                manureRemoval_textBox.Text = string.Empty;
                CheckRequestCompleteness();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void electricity_checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                electricity_textBox.Text = string.Empty;
                CheckRequestCompleteness();
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
                CheckRequestCompleteness();
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // return null;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckRequestCompleteness();
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
