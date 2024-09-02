
using BackendChat.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendChat.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<AppUser> Users { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatType> ChatTypes { get; set; }
        public DbSet<ChatParticipant> ChatParticipants { get; set; }

        public DbSet<UserConnection> UserConnections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChatMessage>()
                .HasKey(cm => cm.MessageId);

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
                tb.Property(col => col.Nickname).HasMaxLength(20).IsRequired();
                tb.Property(col => col.Password).HasMaxLength(100);
                tb.Property(col => col.Email).HasMaxLength(50);
                tb.Property(col => col.ProfilePictureUrl).IsRequired(true);
                tb.Property(col => col.EmailConfirmationToken);
                tb.Property(col => col.EmailConfirmed).IsRequired(true).HasDefaultValue(false);
            });

            modelBuilder.Entity<AppUser>().ToTable("User");

            //Configure composite primary keys in ChatParticipant
            modelBuilder.Entity<ChatParticipant>()
                .HasKey(cp => new {cp.ChatId, cp.UserId});

            //One-Many relationships
            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Chat)
                .WithMany(c => c.ChatMessages)
                .HasForeignKey(m => m.ChatId);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.User)
                .WithMany(u => u.ChatMessages)
                .HasForeignKey(m => m.UserId);

            modelBuilder.Entity<ChatParticipant>()
                .HasOne(cp => cp.Chat)
                .WithMany(c => c.ChatParticipants)
                .HasForeignKey(cp => cp.ChatId);

            modelBuilder.Entity<ChatParticipant>()
                .HasOne(cp => cp.User)
                .WithMany(u => u.ChatParticipants)
                .HasForeignKey(cp => cp.UserId);

            //One-Many relationship between ChatType and Chat
            modelBuilder.Entity<Chat>()
                .HasOne(c => c.ChatType)
                .WithMany(ct => ct.Chats)
                .HasForeignKey(c => c.ChatTypeId);

            //One-Many relationship between Chat and User (Chat creator)
            modelBuilder.Entity<Chat>()
                .HasOne(c => c.CreatedBy)
                .WithMany()
                .HasForeignKey(c => c.CreatedByUserId);

            //Relationship between UserConnection and User
            modelBuilder.Entity<UserConnection>()
                .HasOne(uc => uc.User)
                .WithMany()
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
