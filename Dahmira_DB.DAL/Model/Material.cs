using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dahmira_DB.DAL.Model
{
    public class Material : INotifyPropertyChanged
    {
        //Коллекция с флагами валидных значений полей
        [NotMapped]
        private ObservableCollection<bool> _isCellCorrects = new ObservableCollection<bool>
        {
            true, //0.Производитель
            true, //1.Наименование
            true, //2.Английское наименование
            true, //3.Артикул
            true, //4.Ед. измерения
            true, //5.Английская ед. измерения
            true  //6.Цена
        };
        [NotMapped]
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int ID { get; set; }

        [NotMapped]
        private string _Manufacturer;
        [MaxLength(50)]
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

        [NotMapped]
        private string _Type = "";
        [MaxLength(100)]
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

        [NotMapped]
        private string _ProductName;
        [MaxLength(1000)]
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

        [NotMapped]
        private string _EnglishProductName;
        [MaxLength(300)]
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

        [NotMapped]
        private string _Article;
        [MaxLength(300)]
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

        [NotMapped]
        private string _Unit;
        [MaxLength(10)]
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

        [NotMapped]
        private string _EnglishUnit;
        [MaxLength(10)]
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

        [NotMapped]
        private byte[] _Photo;
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

        [NotMapped]
        private float _Cost;
        public float Cost
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

        [NotMapped]
        private string _LastCostUpdate;
        [MaxLength(12)]
        public string LastCostUpdate
        {
            get => _LastCostUpdate;
            set
            {
                if (_LastCostUpdate != value)
                {
                    _LastCostUpdate = value;
                    OnPropertyChanged(nameof(LastCostUpdate));
                }
            }
        }


        public Material Clone()
        {
            return new Material
            {
                ID = this.ID,
                Manufacturer = this.Manufacturer,
                Type = this.Type,
                ProductName = this.ProductName,
                EnglishProductName = this.EnglishProductName,
                Article = this.Article,
                Unit = this.Unit,
                EnglishUnit = this.EnglishUnit,
                Cost = this.Cost,
                LastCostUpdate = this.LastCostUpdate
            };
        }
    }
}