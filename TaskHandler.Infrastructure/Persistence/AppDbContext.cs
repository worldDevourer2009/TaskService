using Microsoft.EntityFrameworkCore;
using TaskHandler.Application.Interfaces;
using TaskHandler.Domain.Entities;

namespace TaskHandler.Infrastructure.Persistence;

public class AppDbContext : DbContext, IApplicationDbContext
{
    public DbSet<TaskItem> TaskItems { get; private set; } = null!;

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
    }
}