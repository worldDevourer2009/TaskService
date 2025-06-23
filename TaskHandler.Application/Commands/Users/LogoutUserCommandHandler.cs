using TaskHandler.Domain.Services;

namespace TaskHandler.Application.Commands.Users;

public record UserLogoutCommand(Guid UserId) : ICommand<UserLogoutCommandResponse>;

public record UserLogoutCommandResponse(bool Success, string? Message = null);

public class LogoutUserCommandHandler : ICommandHandler<UserLogoutCommand, UserLogoutCommandResponse>
{
    private readonly IUserLogoutService _userLogoutService;
    
    public LogoutUserCommandHandler(IUserLogoutService userLogoutService)
    {
        _userLogoutService = userLogoutService;
    }
    
    public async Task<UserLogoutCommandResponse> Handle(UserLogoutCommand request, CancellationToken cancellationToken)
    {
        var result = await _userLogoutService.LogoutUserById(request.UserId, cancellationToken);
        
        if (!result)
        {
            return new UserLogoutCommandResponse(result, "Logout failed");
        }
        
        return new UserLogoutCommandResponse(result, "Logout success");
    }
}