using User.DB.Entities;
using Microsoft.EntityFrameworkCore;

namespace User.DB
{
    public class MainContext : DbContext
    {
        public MainContext()
        {

        }
        public MainContext(DbContextOptions<MainContext> options)
            : base(options)
        {

        }

        public DbSet<DbUser> Users { get; set; }
/*        public DbSet<DbGameStatus> GameStatus { get; set; }*/

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DbUser>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
            });
/*            modelBuilder.Entity<DbUserGame>()
                .HasOne(ug => ug.Status)
                .WithMany(u => u.UserGames)
                .HasForeignKey(ug => ug.StatusId);*/

            /*modelBuilder.Entity<DbGameStatus>();*/
        }
    }
}
