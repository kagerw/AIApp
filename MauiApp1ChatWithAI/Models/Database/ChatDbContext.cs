using Microsoft.EntityFrameworkCore;

namespace MauiApp1ChatWithAI.Models.Database
{
    public class ChatDbContext : DbContext
    {
        public DbSet<ChatThread> Threads { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageElement> MessageElements { get; set; }
        public DbSet<MigrationHistory> MigrationHistory { get; set; }

        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
        {
            Database.EnsureCreated();  // 開発中は自動マイグレーション
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ChatThread>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.Provider).IsRequired()
                      .HasDefaultValue("Claude");
                entity.Property(e => e.SystemPrompt).IsRequired(false);
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne<ChatThread>()
                      .WithMany()
                      .HasForeignKey(e => e.ThreadId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MessageElement>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Language).IsRequired(false);
                entity.HasOne(e => e.Message)
                      .WithMany(m => m.MessageElements)
                      .HasForeignKey(e => e.MessageId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MigrationHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Version).IsRequired();
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.AppliedAt).IsRequired();
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.IsSuccess).IsRequired();
                entity.Property(e => e.ErrorMessage).IsRequired(false);
            });
        }
    }
}