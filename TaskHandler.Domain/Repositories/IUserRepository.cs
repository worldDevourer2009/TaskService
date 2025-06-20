using TaskHandler.Domain.Entities;

namespace TaskHandler.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserById(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetAllUsers();
    Task UpdateUser(User user, CancellationToken cancellationToken = default);
    Task<bool> TryAddUser(User user, CancellationToken cancellationToken = default);
    Task<User?> GetUserByEmail(string email, CancellationToken cancellationToken = default);
    Task Save(CancellationToken cancellationToken = default);
}