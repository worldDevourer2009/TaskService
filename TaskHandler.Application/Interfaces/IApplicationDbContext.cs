using Microsoft.EntityFrameworkCore;
using TaskHandler.Domain.Entities;

namespace TaskHandler.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TaskItem> TaskItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}