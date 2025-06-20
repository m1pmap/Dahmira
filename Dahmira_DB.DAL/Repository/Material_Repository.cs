using Dahmira_DB.DAL.Interfaces;
using Dahmira_DB.DAL.Model;
using Dahmira_Log.DAL.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dahmira_DB.DAL.Repository
{
    public class Material_Repository : IMaterial
    {
        public bool Add_Material(Material material)
        {
            try
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    db.Materials.Add(material);
                    db.SaveChanges();
                }

                return true;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return false;
            }
        }

        public ObservableCollection<Material> Get_AllMaterials()
        {
            try
            {
                using (ApplicationContext db = new ApplicationContext())
                {

                    var dbMaterials = db.Materials.ToList();
                    return new ObservableCollection<Material>(dbMaterials.Select(s => s).ToList());


                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return null;
            }
        }

        //Аналогично считываем все данные, но добавляем проресс 
        public List<Material> Get_AllMaterialsWithProgress(Action<int, int> reportProgress)
        {
            try
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    var materialsList = db.Materials.ToList();
                    int total = materialsList.Count;
                    int current = 0;

                    foreach (var material in materialsList)
                    {
                        current++;
                        reportProgress?.Invoke(current, total);
                    }

                    return materialsList;
                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                 return null;
            }
        }


        public bool DeleteMaterial(Material material)
        {   
            try
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    db.Materials.Remove(material);
                    db.SaveChanges();

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

        public bool UpdateMaterial(Material newMaterial)
        {
            try
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    var updateMaterial = db.Materials.FirstOrDefault(f => f.ID == newMaterial.ID);

                    if (updateMaterial != null)
                    {
                        updateMaterial.Article = newMaterial.Article;
                        updateMaterial.Unit = newMaterial.Unit;
                        updateMaterial.EnglishUnit = newMaterial.EnglishUnit;
                        updateMaterial.Manufacturer = newMaterial.Manufacturer;
                        updateMaterial.Type = newMaterial.Type;
                        updateMaterial.ProductName = newMaterial.ProductName;
                        updateMaterial.EnglishProductName = newMaterial.EnglishProductName;
                        updateMaterial.Cost = newMaterial.Cost;
                        updateMaterial.Photo = newMaterial.Photo;
                        updateMaterial.LastCostUpdate = newMaterial.LastCostUpdate;
                        db.SaveChanges();
                    }
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

        public Material CloneMaterial(Material material)
        {
            try
            {
                return new Material
                {
                    ID = material.ID,
                    Article = material.Article,
                    Unit = material.Unit,
                    Type = material.Type,
                    EnglishUnit = material.EnglishUnit,
                    Manufacturer = material.Manufacturer,
                    ProductName = material.ProductName,
                    EnglishProductName = material.EnglishProductName,
                    Cost = material.Cost,
                    Photo = material.Photo,
                    LastCostUpdate = material.LastCostUpdate,
                };
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return null;
            }
        }


        //Удаляем и пересохраняем бд
        public bool ReplaceAllMaterials(ObservableCollection<Material> newMaterials, Action<int, int>? reportProgress = null)
        {
            try
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    // Удалить все старые записи
                    db.Materials.ExecuteDelete(); // Требует EF Core 7+
                                                  // Или если EF Core < 7.0:
                                                  // db.Materials.RemoveRange(db.Materials);
                                                  // db.SaveChanges();

                    // Добавить новые с прогрессом
                    int total = newMaterials.Count;
                    for (int i = 0; i < total; i++)
                    {
                        newMaterials[i].ID = 0;
                        db.Materials.Add(newMaterials[i]);

                        // Отправка прогресса каждый N шаг
                        if (i % 100 == 0 || i == total - 1)
                        {
                            reportProgress?.Invoke(i + 1, total);
                        }
                    }

                    db.SaveChanges();
                }

                return true;
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                // Логировать ex при необходимости
                return false;
            }
        }


    }
}
