using TaskHandler.Domain.Entities;
using TaskHandler.Domain.Repositories;
using TaskHandler.Domain.ValueObjects;

namespace TaskHandler.Application.Commands.AddUser;

public record AddUserCommand(string Name, string Email, string Password) : ICommand<AddUserCommandResponse>;

public record AddUserCommandResponse(Guid Id, string Name, string Email);

public class AddUserCommandHandler : ICommandHandler<AddUserCommand, AddUserCommandResponse>
{
    private readonly IUserRepository _userRepository;

    public AddUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<AddUserCommandResponse> Handle(AddUserCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);
        var password = Password.Create(request.Password);
        var newUser = User.Create(email, password, request.Name);
        
        //_userRepository

        return default;
    }
}