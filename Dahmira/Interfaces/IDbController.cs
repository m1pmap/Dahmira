using Dahmira_DB.DAL.Model;
using Dahmira.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dahmira.Interfaces
{
    public interface IDbController
    {
        public void ValidateDb(ObservableCollection<Material> dbItems);
        public void ValidateDbItem(Material dbItem);
    }
}
