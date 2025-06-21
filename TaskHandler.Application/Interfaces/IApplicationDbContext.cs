using Microsoft.EntityFrameworkCore;
using TaskHandler.Domain;
using TaskHandler.Domain.Entities;
using TaskHandler.Domain.ValueObjects;

namespace TaskHandler.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<TaskItem> TaskItems { get; }
    DbSet<RefreshToken> RevokedTokens { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}