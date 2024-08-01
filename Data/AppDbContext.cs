
using BackendChat.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendChat.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<AppUser> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppUser>(tb =>
            {
                tb.HasKey(col => col.Id);
                tb.Property(col => col.Id)
                .UseIdentityColumn()
                .ValueGeneratedOnAdd();

                tb.HasIndex(col => col.Nickname)
                  .IsUnique();

                tb.Property(col => col.FirstName).HasMaxLength(50);
                tb.Property(col => col.LastName).HasMaxLength(50);
                tb.Property(col => col.Nickname).HasMaxLength(20);
                tb.Property(col => col.Password).HasMaxLength(100);
                tb.Property(col => col.Email).HasMaxLength(50);
            });

            modelBuilder.Entity<AppUser>().ToTable("User");
        }
    }
}
