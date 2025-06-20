using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dahmira.Models
{
    public class Dependency : INotifyPropertyChanged
    {
        private int _ProductId = -2;
        public int ProductId
        {
            get => _ProductId;
            set
            {
                if (_ProductId != value)
                {
                    _ProductId = value;
                    OnPropertyChanged(nameof(ProductId));
                }
            }
        }

        private int _SecondProductId = -2;
        public int SecondProductId
        {
            get => _SecondProductId;
            set
            {
                if (_SecondProductId != value)
                {
                    _SecondProductId = value;
                    OnPropertyChanged(nameof(SecondProductId));
                }
            }
        }


        private string _ProductName = string.Empty; //Название товара
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


        private string _SecondProductName = string.Empty; //Название второго товара
        public string SecondProductName
        {
            get => _SecondProductName;
            set
            {
                if (_SecondProductName != value)
                {
                    _SecondProductName = value;
                    OnPropertyChanged(nameof(SecondProductName));
                }
            }
        }


        private double _Multiplier = -1;
        public double Multiplier
        {
            get => _Multiplier;
            set
            {
                if (_Multiplier != value)
                {
                    _Multiplier = value;
                    OnPropertyChanged(nameof(Multiplier));
                }
            }
        }


        private double _SecondMultiplier = -1;
        public double SecondMultiplier
        {
            get => _SecondMultiplier;
            set
            {
                if (_SecondMultiplier != value)
                {
                    _SecondMultiplier = value;
                    OnPropertyChanged(nameof(SecondMultiplier));
                }
            }
        }


        private string _SelectedType = "*"; //Значение, выбранное в ComboBox
        public string SelectedType
        {
            get => _SelectedType;
            set
            {
                if (_SelectedType != value)
                {
                    _SelectedType = value;
                    OnPropertyChanged(nameof(SelectedType));
                }
            }
        }


        private bool _IsSecondButtonVisible = true;
        public bool IsSecondButtonVisible
        {
            get => _IsSecondButtonVisible;
            set
            {
                if (_IsSecondButtonVisible != value)
                {
                    _IsSecondButtonVisible = value;
                    OnPropertyChanged(nameof(IsSecondButtonVisible));
                }
            }
        }

        
        private bool _IsFirstButtonVisible = true;
        public bool IsFirstButtonVisible
        {
            get => _IsFirstButtonVisible;
            set
            {
                if (_IsFirstButtonVisible != value)
                {
                    _IsFirstButtonVisible = value;
                    OnPropertyChanged(nameof(IsFirstButtonVisible));
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Dependency Clone()
        {
            return new Dependency
            {
                IsFirstButtonVisible = this.IsFirstButtonVisible,
                IsSecondButtonVisible = this.IsSecondButtonVisible,
                ProductId = this.ProductId,
                SecondProductId = this.SecondProductId,
                Multiplier = this.Multiplier,
                SecondMultiplier = this.SecondMultiplier,
                ProductName = this.ProductName,
                SecondProductName = this.SecondProductName,
                SelectedType = this.SelectedType
            };
        }
    }
}
