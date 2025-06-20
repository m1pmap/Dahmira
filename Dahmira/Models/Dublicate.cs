using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dahmira.Models
{
    public class Dublicate : INotifyPropertyChanged
    {
        private int _Num = -1;
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

        private string _FirstRowIndex = string.Empty;
        public string FirstRowIndex
        {
            get => _FirstRowIndex;
            set
            {
                if (_FirstRowIndex != value)
                {
                    _FirstRowIndex = value;
                    OnPropertyChanged(nameof(FirstRowIndex));
                }
            }
        }

        private string _SecondRowIndex = string.Empty;
        public string SecondRowIndex
        {
            get => _SecondRowIndex;
            set
            {
                if (_SecondRowIndex != value)
                {
                    _SecondRowIndex = value;
                    OnPropertyChanged(nameof(SecondRowIndex));
                }
            }
        }

        private string _Article = string.Empty;
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

        private bool _IsFirstRowDelete = false;
        public bool IsFirstRowDelete
        {
            get => _IsFirstRowDelete;
            set
            {
                if (_IsFirstRowDelete != value)
                {
                    _IsFirstRowDelete = value;
                    OnPropertyChanged(nameof(IsFirstRowDelete));
                }
            }
        }

        private bool _IsSecondRowDelete = false;
        public bool IsSecondRowDelete
        {
            get => _IsSecondRowDelete;
            set
            {
                if (_IsSecondRowDelete != value)
                {
                    _IsSecondRowDelete = value;
                    OnPropertyChanged(nameof(IsSecondRowDelete));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
