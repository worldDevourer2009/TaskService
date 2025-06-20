using Microsoft.Extensions.Logging;
using TaskHandler.Application.DTOs.User;
using TaskHandler.Domain.Services;

namespace TaskHandler.Application.Commands.Users;

public record LoginUserCommand(string email, string password) : ICommand<LoginUserResponse>;
public record LoginUserResponse(bool success, string? message);

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
        var dto = new UserLoginDTO();
        dto.Email = command.email;
        dto.Password = command.password;

        try
        {
            var result = await _userLoginService.LoginAsync(dto.Email, dto.Password, cancellationToken);
            
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