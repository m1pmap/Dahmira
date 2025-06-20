using Dahmira_DB.DAL.Model;
using Dahmira.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dahmira_Log.DAL.Repository;
using System.Diagnostics;

namespace Dahmira.Services
{
    class DbController_Service : IDbController
    {
        public void ValidateDb(ObservableCollection<Material> dbItems)
        {
            try
            {
                foreach (Material dbItem in dbItems)
                    ValidateDbItem(dbItem);
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }

        public void ValidateDbItem(Material dbItem)
        {
            try 
            {
                dbItem.IsCellCorrects[0] = !string.IsNullOrWhiteSpace(dbItem.Manufacturer);
                dbItem.IsCellCorrects[1] = !string.IsNullOrWhiteSpace(dbItem.ProductName);
                dbItem.IsCellCorrects[2] = !string.IsNullOrWhiteSpace(dbItem.EnglishProductName);
                dbItem.IsCellCorrects[3] = !string.IsNullOrWhiteSpace(dbItem.Article);
                dbItem.IsCellCorrects[4] = !string.IsNullOrWhiteSpace(dbItem.Unit);
                dbItem.IsCellCorrects[5] = !string.IsNullOrWhiteSpace(dbItem.EnglishUnit);
                dbItem.IsCellCorrects[6] = dbItem.Cost != 0;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);
            }
        }
    }
}
