using Dahmira_Log.DAL.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dahmira_Log.DAL.Interfaces
{
    public interface ILog
    {
        public bool Add(string lvl, StackTrace stackTrace, string currentUser, Exception except);           //Получить всё из БД
        public ObservableCollection<Log> GetAlL();                              //Добавление
        public bool DeleteOld();
        public bool DeleteAll();
    }
}
