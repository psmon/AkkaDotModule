using AkkaDotBootApi.Config;
using AkkaDotBootApi.Entity.User;
using Microsoft.EntityFrameworkCore;

namespace AkkaDotBootApi.Repositories
{
    public class UserRepository : DbContext
    {
        private string database = "webnori";

        protected DbSet<UserEntity> userEntities { get; set; }

        protected AppSettings appSettings { get; set; }

        public UserRepository(AppSettings _appSettings)
        {
            appSettings = _appSettings;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<UserEntity>(eb =>
                {
                    eb.HasKey(b => b.UserId)
                        .HasName("PrimaryKey_UserId");

                    eb.ToTable("tb_user");
                });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbOption = "Convert Zero Datetime=true;";
            string dbConnectionString = appSettings.DBConnectionMysql + $"database={database};" + dbOption;            
            optionsBuilder.UseMySql(dbConnectionString);
            base.OnConfiguring(optionsBuilder);
        }
    }
}
