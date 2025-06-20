using Dahmira_Log.DAL.Interfaces;
using Dahmira_Log.DAL.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Windows;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dahmira_Log.DAL.Repository
{
    public class Log_Repository : ILog
    {

        


        public bool Add(string lvl, StackTrace stackTrace, string currentUser, Exception except)
        {
            Log log = new Log();

            log.DateTime = DateTime.Now;
            log.Lvl = lvl;
            log.CurrentUser = currentUser;

            log.Ex_HResult = except.HResult.ToString();
            log.Ex_GetType = except.GetType().FullName;
            log.Ex_Mesage = except.Message;
            log.Ex_InnerExceptionMesage = except.InnerException?.Message;
            log.Ex_StackTrace = except.StackTrace;


       
            var frame = stackTrace.GetFrame(0);
            var method = frame.GetMethod();

            var className = method.DeclaringType?.FullName ?? "UnknownClass";
            var methodName = method.Name;
            var fileName = frame.GetFileName();
            var lineNumber = frame.GetFileLineNumber();
            log.EntryPoint = $"{className}.{methodName} (at {fileName}:{lineNumber})";


            try
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    db.Log.Add(log);
                    db.SaveChanges(); 
                }

                return true;
            }
            catch (Exception ex)
            {
                
                //Add("Error", new StackTrace(), "noneUser", ex);
                

                return false;
            }
        }

        public ObservableCollection<Log> GetAlL()
        {
            try
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    var dbLogss = db.Log.ToList();
                    return new ObservableCollection<Log>(dbLogss.Select(s => s).ToList());

                }
            }
            catch (Exception ex)
            {
                var log = new Log_Repository();
                log.Add("Error", new StackTrace(), "noneUser", ex);

                return null;
            }
        }

     
        
        public bool DeleteOld()
        {   
            try
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    var thresholdDate = DateTime.Now.AddDays(-30);

                    List<Log> oldLogs = db.Log.Where(m => m.DateTime <  thresholdDate).ToList();


                    if (oldLogs.Any())
                    {
                        db.Log.RemoveRange(oldLogs);
                        db.SaveChanges();
                    }


                    db.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                Add("Error", new StackTrace(), "noneUser", ex);

                return false;
            }
        }



        public bool DeleteAll()
        {
            try
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    db.Log.ExecuteDelete();
                    db.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                Add("Error", new StackTrace(), "noneUser", ex);

                return false;
            }
        }





    }
}
