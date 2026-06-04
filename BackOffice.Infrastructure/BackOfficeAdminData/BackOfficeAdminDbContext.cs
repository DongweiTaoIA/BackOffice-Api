using BackOffice.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackOffice.Infrastructure.BackOfficeAdminData;

public class BackOfficeAdminDbContext : DbContext
{
    public BackOfficeAdminDbContext(DbContextOptions<BackOfficeAdminDbContext> options)
        : base(options)
    {
    }

    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<ChatHistory> ChatHistories => Set<ChatHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
            entity.HasMany(e => e.Messages)
                .WithOne(e => e.Chat)
                .HasForeignKey(e => e.ChatId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChatHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.HasIndex(e => e.ChatId);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
        });
    }
}
