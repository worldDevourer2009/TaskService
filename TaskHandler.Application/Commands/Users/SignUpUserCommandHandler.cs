using Microsoft.Extensions.Logging;
using TaskHandler.Application.DTOs.User;
using TaskHandler.Domain.Services;

namespace TaskHandler.Application.Commands.Users;

public record SignUpUserCommand(string name, string email, string password) : ICommand<SignUpUserCommandResponse>;

public record SignUpUserCommandResponse(string message, bool success);

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
        var dto = new UserSingUpDTO();
        dto.Email = command.email;
        dto.Name = command.name;
        dto.Password = command.password;

        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return new SignUpUserCommandResponse("Invalid data", false);
            }
            
            var result = new SignUpUserCommandResponse("User has been created", await
                _userSignUpService.SignUpAsync(dto.Name, dto.Email, dto.Password, cancellationToken));
            
            return result;
        }
        catch (Exception e)
        {
            _loggerFactory.LogError(e, "Error creating user");
            return new SignUpUserCommandResponse(e.Message, false);
        }
    }
}