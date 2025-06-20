using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dahmira_Log.DAL.Model
{
    public class Log
    {
        public uint  ID { get; set; }
        public DateTime? DateTime { get; set; }
        
        public string? Lvl { get; set; }
        public string? EntryPoint { get; set; }
        public string? CurrentUser { get; set; }



        public string? Ex_HResult { get; set; }
        public string? Ex_GetType { get; set; }
        public string? Ex_Mesage { get; set; }
        public string? Ex_InnerExceptionMesage { get; set; }
        public string? Ex_StackTrace { get; set; }

    }
}
