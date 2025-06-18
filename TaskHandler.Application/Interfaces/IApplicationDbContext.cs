using Microsoft.EntityFrameworkCore;
using TaskHandler.Domain;
using TaskHandler.Domain.Entities;

namespace TaskHandler.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<TaskItem> TaskItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}