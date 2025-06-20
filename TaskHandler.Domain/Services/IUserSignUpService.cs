namespace TaskHandler.Domain.Services;

public interface IUserSignUpService
{
    Task<bool> SignUpAsync(string name, string email, string password, CancellationToken cancellationToken = default);
}