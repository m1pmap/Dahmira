using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Dahmira.Models
{
    public class SettingsParameters
    {
        //Общие настройки
        public int Theme { get; set; } = 0;//0 - светлая, 1 - темная
        public bool? IsNotificationsWithSound {get; set;} = false; //Уведомления со звуком
        public double CheckingIntervalFromMail { get; set; } = 1.1; //Интервал проверки сообщений с mail
        public string? PriceFolderPath { get; set; } = AppDomain.CurrentDomain.BaseDirectory; //Папка для сохранения прайса

        public bool IsAdministrator { get; set; } = false;

        //Менеджер цен
        //...

        //Вывод данных
        //Excel
        public ColorItem ExcelTitleColor { get; set; } = new ColorItem ("Зелёный", Color.LightGreen); //Цвет заголовка
        public ColorItem ExcelCategoryColor { get; set; } = new ColorItem ("Светло-оранжевый", Color.NavajoWhite); //Цвет категории
        public ColorItem ExcelChapterColor { get; set; } = new ColorItem ("Жёлтый", Color.LightYellow); //Цвет раздела
        public ColorItem ExcelDataColor { get; set; } = new ColorItem ("Прозрачный", Color.Transparent); // Цвет данных
        public ColorItem ExcelPhotoBackgroundColor { get; set; } = new ColorItem ("Прозрачный", Color.Transparent); //Цвет за фото
        public ColorItem ExcelNotesColor { get; set; } = new ColorItem ("Прозрачный", Color.Transparent); //Цвет примечаний
        public ColorItem ExcelNumberColor { get; set; } = new ColorItem ("Прозрачный", Color.Transparent); //Цвет номеров
        public bool? IsInsertExcelPicture { get; set; } = true; //Добавляется ли картинка в Excel
        public bool IsInsertExcelCategory { get; set; } = true; //Добавляется ли категория в Excel
        public bool? isDepartmentRequestExportWithCalc { get; set; } = true; //Экспортируется ли заявка техотдела с расчётом
        public int ExcelPhotoWidth { get; set; } = 100; //Ширина картинки в Excel
        public int ExcelPhotoHeight { get; set; } = 100; //Высота картинки в Excel
        public string FullCostType { get; set; } = "ИТОГО:"; //Тип полной стоимости

        //Пути сохранения
        public string ExcelFolderPath { get; set; } = "C:\\"; //Папка для сохранения Excel
        public string CalcFolderPath { get; set; } = "C:\\"; //Папка для сохранения расчётки
        public string PathExport_Price { get; set; } = "С:\\"; //Папка для сохранения выгруженных данных из прайса 

        public string Price { get; set; } = "|DataDirectory|\\db\\Dahmira_DB_beta.mdf"; //Путь к прайсу

        public List<ColumnInfo> CalcColumnInfos { get; set; } = new List<ColumnInfo>(); //Информация о колонках расчёта
        public List<ColumnInfo> DBColumnInfos { get; set; } = new List<ColumnInfo>(); //Информация о колонках прайса

        public bool isEnglishNameVisible { get; set; } = false; //Видны ли английские наименования
        public bool isIdOnCalcVisible { get; set; } = false;
        public bool isShowUpdates { get; set; } = true; //показывать ли окно с обновлениями при входе

        public string url_praise { get; set; } = "ftp://ftp.dahmira.by";
        public string ftpUsername { get; set; } = "9Af6yMZyo-Qy1eeiHkgk";
        public string ftpPassword { get; set; } = "W3)Zh4~d_x";

    }
}
