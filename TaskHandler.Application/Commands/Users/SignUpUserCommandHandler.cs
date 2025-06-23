using Microsoft.Extensions.Logging;
using TaskHandler.Domain.Services;

namespace TaskHandler.Application.Commands.Users;

public record SignUpUserCommand(string Name, string Email, string Password) : ICommand<SignUpUserCommandResponse>;

public record SignUpUserCommandResponse(string Message, bool Success);

public class SignUpUserCommandHandler : ICommandHandler<SignUpUserCommand, SignUpUserCommandResponse>
{
    private readonly IUserSignUpService _userSignUpService;
    private readonly ILogger<SignUpUserCommandHandler> _loggerFactory;
    
    public SignUpUserCommandHandler(IUserSignUpService userSignUpService, ILogger<SignUpUserCommandHandler> loggerFactory)
    {
        _userSignUpService = userSignUpService;
        _loggerFactory = loggerFactory;
    }
    
    public async Task<SignUpUserCommandResponse> Handle(SignUpUserCommand command, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(command.Name) || string.IsNullOrWhiteSpace(command.Email) ||
                string.IsNullOrWhiteSpace(command.Password))
            {
                return new SignUpUserCommandResponse("Invalid data", false);
            }
            
            var result = new SignUpUserCommandResponse("User has been created", await
                _userSignUpService.SignUpAsync(command.Name, command.Email, command.Password, cancellationToken));
            
            return result;
        }
        catch (Exception e)
        {
            _loggerFactory.LogError(e, "Error creating user");
            return new SignUpUserCommandResponse(e.Message, false);
        }
    }
}