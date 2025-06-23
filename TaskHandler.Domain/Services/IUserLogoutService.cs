namespace TaskHandler.Domain.Services;

public interface IUserLogoutService
{
    Task<bool> LogoutUserById(Guid userId, CancellationToken cancellationToken = default);
}