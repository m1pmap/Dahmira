using Dahmira_DB.DAL.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dahmira_DB.DAL.Interfaces
{
    public interface IMaterial
    {
        ObservableCollection<Material> Get_AllMaterials();              //Получить всё из БД
        bool Add_Material(Material material);           //Добавление
        bool DeleteMaterial(Material material);
        bool UpdateMaterial(Material newMaterial);
        Material CloneMaterial(Material material);
    }
}
