namespace TaskHandler.Domain.Services;

public interface IUserLoginService
{
    Task<bool> LoginAsync(string email, string passwordHash, CancellationToken cancellationToken = default);
    Task<bool> LogoutAsync(Guid userId, CancellationToken cancellationToken = default);
}