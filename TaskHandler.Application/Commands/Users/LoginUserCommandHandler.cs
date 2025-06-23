using Microsoft.Extensions.Logging;
using TaskHandler.Domain.Services;

namespace TaskHandler.Application.Commands.Users;

public record LoginUserCommand(string Email, string Password) : ICommand<LoginUserResponse>;
public record LoginUserResponse(bool Success, string? Message);

public class LoginUserCommandHandler : ICommandHandler<LoginUserCommand, LoginUserResponse>
{
    private readonly IUserLoginService _userLoginService;
    private readonly ILogger<LoginUserCommandHandler> _logger;

    public LoginUserCommandHandler(IUserLoginService userLoginService, ILogger<LoginUserCommandHandler> logger)
    {
        _userLoginService = userLoginService;
        _logger = logger;
    }
    
    public async Task<LoginUserResponse> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _userLoginService.LoginAsync(command.Email, command.Password, cancellationToken);
            
            return !result ? new LoginUserResponse(result, "Login failed") 
                : new LoginUserResponse(result, "Login success");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error logging user");
            return new LoginUserResponse(false, e.Message);
        }
    }
}