
using hng13Stage2.Entities;
using Microsoft.EntityFrameworkCore;

namespace hng13Stage2.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {

        public DbSet<Country> Countries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Country>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<Country>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Country>()
                .Property(c => c.EstimatedGdp)
                .HasColumnType("decimal(22,2)");

            modelBuilder.Entity<Country>()
                .Property(c => c.ExchangeRate)
                .HasColumnType("decimal(18,6)");  
        }
    }
}
