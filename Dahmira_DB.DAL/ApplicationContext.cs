using Dahmira_DB.DAL.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dahmira_DB.DAL
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Material> Materials { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlServer(@"Server=MNS1-212N\SQLEXPRESS;Database=Test_DB;Trusted_Connection=True;MultipleActiveResultSets=true; TrustServerCertificate=True");
            //optionsBuilder.UseSqlServer(@"Server=MNS1-212N\SQLEXPRESS; Database=Test_DB; AttachDbFilename=" + GlobalStatic_Class.connectionString + ";Trusted_Connection=True;MultipleActiveResultSets=true; Integrated Security=True;Connect Timeout=30; TrustServerCertificate=True");

            //optionsBuilder.UseSqlServer("Data Source = (LocalDB)\\MSSQLLocalDB;  AttachDbFilename=C:\\Dahmira\\Dahmira new project\\Dahmira new project\\Dahmira\\bin\\Debug\\net8.0-windows\\db\\Dahmira_Db_beta.mdf; Trusted_Connection=True; Trusted_Connection=True;");
            //optionsBuilder.UseSqlServer("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Dahmira\\Dahmira_Db_beta2.mdf;Integrated Security=True;");
            //optionsBuilder.UseSqlServer(@"Server=(LocalDB)\MSSQLLocalDB;  Database=Dahmira_Db_beta; Trusted_Connection=True;MultipleActiveResultSets=true; Integrated Security=True;Connect Timeout=30; TrustServerCertificate=True; Trusted_Connection=True;");

            //optionsBuilder.UseSqlServer("Data Source = (LocalDB)\\MSSQLLocalDB; Database=DahmiraTest_DB;  AttachDbFilename=|DataDirectory|Dahmira_TestDb.mdf; Trusted_Connection=True;MultipleActiveResultSets=true; Integrated Security=True;Connect Timeout=30; TrustServerCertificate=True");

            //Основная строка
            optionsBuilder.UseSqlServer("Data Source = (LocalDB)\\MSSQLLocalDB;  AttachDbFilename=" + ConnectionString_Global.Value + ";Trusted_Connection=True;MultipleActiveResultSets=true; Integrated Security=True;Connect Timeout=30; TrustServerCertificate=True");

            //для миграции
            //optionsBuilder.UseSqlServer("Data Source = (LocalDB)\\MSSQLLocalDB; Database=Dahmira_Db_beta;  AttachDbFilename=D:\\Dahmira\\Dahmira new project\\Dahmira\\bin\\Debug\\net8.0-windows\\db\\Dahmira_Db_beta.mdf; Trusted_Connection=True;MultipleActiveResultSets=true; Integrated Security=True;Connect Timeout=30; TrustServerCertificate=True");
        }
    }
}
