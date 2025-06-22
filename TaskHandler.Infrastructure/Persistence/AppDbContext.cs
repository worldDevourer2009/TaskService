using Microsoft.EntityFrameworkCore;
using TaskHandler.Application.Interfaces;
using TaskHandler.Domain.Entities;
using TaskHandler.Domain.ValueObjects;

namespace TaskHandler.Infrastructure.Persistence;

public class AppDbContext : DbContext, IApplicationDbContext
{
    public DbSet<TaskItem> TaskItems { get; private set; } = null!;
    public DbSet<User> Users { get; private set; } = null!;
    public DbSet<RefreshToken> RevokedTokens { get; private set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("task_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(250).IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(1500);
            entity.Property(e => e.Priority).HasColumnName("priority").HasConversion<string>();
            entity.Property(e => e.TaskType).HasColumnName("task_type").HasConversion<string>();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.CompletionDate).HasColumnName("completion_date");
            entity.Property(e => e.IsCompleted).HasColumnName("is_completed");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(250).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.LastLogin).HasColumnName("last_login");

            entity.Property(e => e.Email)
                .HasColumnName("email")
                .IsRequired()
                .HasConversion(
                    email => email!.Value,
                    value => Email.Create(value));
            
            entity.HasIndex(e => e.Email).IsUnique();

            entity.Property(e => e.Password)
                .HasColumnName("password")
                .IsRequired()
                .HasMaxLength(250)
                .HasConversion(
                    password => password!.Hash,
                    hash => Password.FromHash(hash));

            entity.OwnsMany(u => u.PasswordResetTokens, rt =>
            {
                rt.ToTable("password_reset_tokens");
                rt.WithOwner().HasForeignKey("UserId");
                rt.HasKey("UserId", "TokenHash");
    
                rt.Property(t => t.TokenHash).HasColumnName("token_hash").IsRequired();
                rt.Property(t => t.ExpirationDate).HasColumnName("expiration_date").IsRequired();
            });
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("revoked_refresh_tokens");
            entity.HasKey(e => e.TokenHash);
            entity.Property(e => e.TokenHash).HasColumnName("token_hash").IsRequired();
            entity.Property(e => e.ExpirationDate).HasColumnName("expiration_date").IsRequired();
        });
    }
}