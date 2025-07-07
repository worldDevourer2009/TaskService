using Microsoft.EntityFrameworkCore;
using TaskHandler.Domain.Entities;

namespace TaskHandler.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TaskItem> TaskItems { get; set; }
    DbSet<TaskGroup> TaskGroups { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}