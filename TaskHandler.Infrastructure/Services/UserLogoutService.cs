using Microsoft.Extensions.Logging;
using TaskHandler.Application.Interfaces;
using TaskHandler.Domain.Repositories;
using TaskHandler.Domain.Services;

namespace TaskHandler.Infrastructure.Services;

public class UserLogoutService : IUserLogoutService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly ILogger<UserLogoutService> _logger;
    private readonly IDomainDispatcher _dispatcher;

    public UserLogoutService(IUserRepository userRepository, ITokenService tokenService, IDomainDispatcher dispatcher, ILogger<UserLogoutService> logger)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task<bool> LogoutUserById(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetById(userId, cancellationToken);

        if (user == null)
        {
            return false;
        }
        
        try
        {
            await _tokenService.RevokeAllTokensForUser(userId, cancellationToken);
            user.Logout();
            await _userRepository.UpdateUser(user, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return false;
        }
    }
}