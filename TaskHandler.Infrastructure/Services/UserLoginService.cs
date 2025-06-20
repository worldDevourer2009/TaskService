using TaskHandler.Domain.Repositories;
using TaskHandler.Domain.Services;

namespace TaskHandler.Infrastructure.Services;

public class UserLoginService : IUserLoginService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserPasswordService _userPasswordService;

    public UserLoginService(IUserRepository userRepository, IUserPasswordService userPasswordService)
    {
        _userRepository = userRepository;
        _userPasswordService = userPasswordService;
    }
    
    public async Task<bool> LoginAsync(string email, string passwordHash, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetUserByEmail(email, cancellationToken);

        if (user == null)
        {
            throw new Exception("User not found");
        }

        if (!await _userPasswordService.VerifyPasswordAsync(user, passwordHash, cancellationToken))
        {
            return false;
        }

        user.UpdateLastLogin();
        return true;
    }

    public async Task<bool> LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetById(userId, cancellationToken);

        if (user == null)
        {
            throw new Exception("User not found, can't logout");
        }

        if (user.LastLogin == null)
        {
            return false;
        }
        
        user.Inactivate();
        return true;
    }
}