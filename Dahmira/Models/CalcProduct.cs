using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Dahmira.Models
{
    public class CalcProduct : INotifyPropertyChanged
    {
        //Коллекция с флагами валидных значений полей
        private ObservableCollection<bool> _isCellCorrects = new ObservableCollection<bool> 
        {
            true, //0.Производитель
            true, //1.Наименование
            true, //2.Английское наименование
            true, //3.Артикул
            true, //4.Ед. измерения
            true, //5.Английская ед. измерения
            true, //6.Цена
            true  //7.Количество
        };
        public ObservableCollection<bool> IsCellCorrects
        {
            get => _isCellCorrects;
            set
            {
                if (_isCellCorrects != value)
                {
                    _isCellCorrects = value;
                    OnPropertyChanged(nameof(IsCellCorrects));
                }
            }
        }


        private int _ID = 0;
        public int ID
        {
            get => _ID;
            set
            {
                if (_ID != value)
                {
                    _ID = value;
                    OnPropertyChanged(nameof(ID));
                }
            }
        }


        private int _Num = 0; //Номер
        public int Num
        {
            get => _Num;
            set
            {
                if (_Num != value)
                {
                    _Num = value;
                    OnPropertyChanged(nameof(Num));
                }
            }
        }


        private string _Manufacturer = string.Empty; //Производитель
        public string Manufacturer
        {
            get => _Manufacturer;
            set
            {
                if (_Manufacturer != value)
                {
                    _Manufacturer = value;
                    OnPropertyChanged(nameof(Manufacturer));
                }
            }
        }

        private string _Type = string.Empty; //Тип
        public string Type
        {
            get => _Type;
            set
            {
                if (_Type != value)
                {
                    _Type = value;
                    OnPropertyChanged(nameof(Type));
                }
            }
        }

        private string _ProductName = string.Empty; //Наименование товара
        public string ProductName
        {
            get => _ProductName;
            set
            {
                if (_ProductName != value)
                {
                    _ProductName = value;
                    OnPropertyChanged(nameof(ProductName));
                }
            }
        }


        private string _EnglishProductName = string.Empty; //Наименование товара на английском
        public string EnglishProductName
        {
            get => _EnglishProductName;
            set
            {
                if (_EnglishProductName != value)
                {
                    _EnglishProductName = value;
                    OnPropertyChanged(nameof(EnglishProductName));
                }
            }
        }


        private string _Article = string.Empty; //Артикул
        public string Article
        {
            get => _Article;
            set
            {
                if (_Article != value)
                {
                    _Article = value;
                    OnPropertyChanged(nameof(Article));
                }
            }
        }


        private string _Unit = string.Empty; //Единица измерения
        public string Unit
        {
            get => _Unit;
            set
            {
                if (_Unit != value)
                {
                    _Unit = value;
                    OnPropertyChanged(nameof(Unit));
                }
            }
        }

        private string _EnglishUnit = string.Empty; //Единица измерения на английском
        public string EnglishUnit
        {
            get => _EnglishUnit;
            set
            {
                if (_EnglishUnit != value)
                {
                    _EnglishUnit = value;
                    OnPropertyChanged(nameof(EnglishUnit));
                }
            }
        }


        private byte[] _Photo = null; //Фото
        public byte[] Photo
        {
            get => _Photo;
            set
            {
                if (_Photo != value)
                {
                    _Photo = value;
                    OnPropertyChanged(nameof(Photo));
                }
            }
        }


        private double _RealCost = double.NaN; //Цена товара (реальная)
        public double RealCost
        {
            get => _RealCost;
            set
            {
                if (_RealCost != value)
                {
                    _RealCost = value;
                    OnPropertyChanged(nameof(RealCost));
                }
            }
        }


        private double _Cost = double.NaN; //Цена товара (может изменяться в зависимости от страны)
        public double Cost
        {
            get => _Cost;
            set
            {
                if (_Cost != value)
                {
                    _Cost = value;
                    OnPropertyChanged(nameof(Cost));
                }
            }
        }

        private string _Count = ""; //Корректна ли запись количества
        public string Count
        {
            get => _Count;
            set
            {
                if (_Count != value)
                {
                    _Count = value;
                    OnPropertyChanged(nameof(Count));
                }
            }
        }

        private double _TotalCost = double.NaN; //финальная цена
        public double TotalCost
        {
            get => _TotalCost;
            set
            {
                if (_TotalCost != value)
                {
                    _TotalCost = value;
                    OnPropertyChanged(nameof(TotalCost));
                }
            }
        }


        private int _ID_Art = 0;
        public int ID_Art
        {
            get => _ID_Art;
            set
            {
                if (_ID_Art != value)
                {
                    _ID_Art = value;
                    OnPropertyChanged(nameof(ID_Art));
                }
            }
        }


        private string _Note = string.Empty; //Примечания
        public string Note
        {
            get => _Note;
            set
            {
                if (_Note != value)
                {
                    _Note = value;
                    OnPropertyChanged(nameof(Note));
                }
            }
        }


        private string _rowColor = "#FFFFFF";
        public string RowColor
        {
            get => _rowColor;
            set
            {
                if (_rowColor != value)
                {
                    _rowColor = value;
                    OnPropertyChanged(nameof(RowColor));
                }
            }
        }

        private string _rowForegroundColor = "#000000";
        public string RowForegroundColor
        {
            get => _rowForegroundColor;
            set
            {
                if (_rowForegroundColor != value)
                {
                    _rowForegroundColor = value;
                    OnPropertyChanged(nameof(RowForegroundColor));
                }
            }
        }

        private bool _isDependency = false;
        public bool isDependency
        {
            get => _isDependency;
            set
            {
                if (_isDependency != value)
                {
                    _isDependency = value;
                    OnPropertyChanged(nameof(isDependency));
                }
            }
        }

        private bool _hasErrorInRow = false;
        public bool HasErrorInRow
        {
            get => _hasErrorInRow;
            set
            {
                if (_hasErrorInRow != value)
                {
                    _hasErrorInRow = value;
                    OnPropertyChanged(nameof(HasErrorInRow));
                }
            }
        }

        private bool _hasErrorInDependency = true; 
        public bool HasErrorInDependency
        {
            get => _hasErrorInDependency;
            set
            {
                if (_hasErrorInDependency != value)
                {
                    _hasErrorInDependency = value;
                    OnPropertyChanged(nameof(HasErrorInDependency));
                }
            }
        }


        private ObservableCollection<Dependency> _dependencies = new ObservableCollection<Dependency>(); //Зависимости

        public ObservableCollection<Dependency> dependencies
        {
            get => _dependencies;
            set
            {
                if (_dependencies != value)
                {
                    _dependencies = value;
                    OnPropertyChanged(nameof(dependencies));
                }
            }
        }

        //private bool _isVisible = true;
        //public bool isVisible
        //{
        //    get => _isVisible;
        //    set
        //    {
        //        if (_isVisible != value)
        //        {
        //            _isVisible = value;
        //            OnPropertyChanged(nameof(isVisible));
        //        }
        //    }
        //}

        public bool isVisible { get; set; } = true;

        private bool _isHidingButton = false;
        public bool isHidingButton
        {
            get => _isHidingButton;
            set
            {
                if (_isHidingButton != value)
                {
                    _isHidingButton = value;
                    OnPropertyChanged(nameof(isHidingButton));
                }
            }
        }
        private string _hideButtonContext = "-";
      
        public string hideButtonContext
        {
            get => _hideButtonContext;
            
            set
            {
                if (_hideButtonContext != value)
                {
                    _hideButtonContext = value;
                    OnPropertyChanged(nameof(hideButtonContext));
                }
            }
        }

        private string _SelectedCountryName = string.Empty; //Страна, выьранная в расчёте
        public string SelectedCountryName
        {
            get => _SelectedCountryName;
            set
            {
                if (_SelectedCountryName != value)
                {
                    _SelectedCountryName = value;
                    OnPropertyChanged(nameof(SelectedCountryName));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public CalcProduct Clone()
        {

            return new CalcProduct
            {
                ID = this.ID,
                Num = this.Num,
                Manufacturer = this.Manufacturer,
                ProductName = this.ProductName,
                EnglishProductName = this.EnglishProductName,
                Article = this.Article,
                Unit = this.Unit,
                Photo = this.Photo,
                RealCost = this.RealCost,
                Cost = this.Cost,
                Count = this.Count,
                TotalCost = this.TotalCost,
                ID_Art = this.ID_Art,
                Note = this.Note,
                RowColor = "#FFFFFF",
                RowForegroundColor = "#000000",
                isDependency = this.isDependency,
                dependencies = this.dependencies,
                isVisible = this.isVisible,
                isHidingButton = this.isHidingButton,
                hideButtonContext = this.hideButtonContext,
                IsCellCorrects = this.IsCellCorrects,
                SelectedCountryName = this.SelectedCountryName
            };
        }
    }
}
