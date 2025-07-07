using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TaskHandler.Application.Interfaces;
using TaskHandler.Domain.Entities;

namespace TaskHandler.Infrastructure.Persistence;

public class AppDbContext : DbContext, IApplicationDbContext
{
    public DbSet<TaskItem> TaskItems { get; set; } = null!;
    public DbSet<TaskGroup> TaskGroups { get; set; }

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
            
            entity.Property(e => e.Id)
                .HasColumnName("id");
            
            entity.Property(e => e.Title)
                .HasColumnName("title")
                .HasMaxLength(250).IsRequired();
            
            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion<string>();
            
            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(1500);
            
            entity.Property(e => e.Priority)
                .HasColumnName("priority")
                .HasConversion<string>();
            
            entity.Property(e => e.TaskType)
                .HasColumnName("task_type")
                .HasConversion<string>();
            
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");
            
            entity.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");
            
            entity.Property(e => e.CompletionDate)
                .HasColumnName("completion_date");
            
            entity.Property(e => e.IsCompleted)
                .HasColumnName("is_completed");
        });

        modelBuilder.Entity<TaskGroup>(entity =>
        {
            entity.ToTable("task_groups");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id")
                .IsRequired();
            
            entity.Property(e => e.Title).HasColumnName("title")
                .IsRequired();
            
            entity.Property(e => e.Description)
                .HasColumnName("description");
            
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");
            
            entity.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");
            
            entity.Property(e => e.TaskIds)
                .HasColumnName("task_ids")
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)!)
                .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));
            
            entity.Property(e => e.UserIds)
                .HasColumnName("user_ids")
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<HashSet<string>>(v, (JsonSerializerOptions?)null)!)
                .Metadata.SetValueComparer(new ValueComparer<HashSet<string>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToHashSet()));
        });
    }
}