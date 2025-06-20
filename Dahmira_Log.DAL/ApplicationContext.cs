using Dahmira_Log.DAL.Model;
using Microsoft.EntityFrameworkCore;

namespace Dahmira_Log.DAL
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Log> Log { get; set; }

        private const string connectionString =
            @"Server=(LocalDB)\MSSQLLocalDB;
              Database=Dahmira_LogDb;
              Trusted_Connection=True;
              TrustServerCertificate=True;
              MultipleActiveResultSets=true";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseSqlServer(connectionString);
        }

        public static void EnsureLogDbCreated()
        {
            using var context = new ApplicationContext();

            // Применить миграции (если они есть), либо создать пустую базу
            context.Database.Migrate(); // или
            //context.Database.EnsureCreated();
        }
    }
}
