using TaskHandler.Application.Interfaces;
using TaskHandler.Domain.DomainsEvents.Users;
using TaskHandler.Domain.Entities;
using TaskHandler.Domain.Repositories;
using TaskHandler.Domain.Services;
using TaskHandler.Domain.ValueObjects;

namespace TaskHandler.Infrastructure.Services;

public class UserSignUpService : IUserSignUpService
{
    private readonly IUserRepository _userRepository;
    private readonly IDomainDispatcher _dispatcher;
    
    public UserSignUpService(IUserRepository userRepository, IDomainDispatcher dispatcher)
    {
        _userRepository = userRepository;
        _dispatcher = dispatcher;
    }
    
    public async Task<bool> SignUpAsync(string name, string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetUserByEmail(email, cancellationToken);

        if (user != null)
        {
            throw new Exception($"User with email : {email}, already exists");
        }
        
        var emailObj = Email.Create(email);
        var passwordObj = Password.Create(password);
        var newUser = User.Create(emailObj, passwordObj, name);
        
        if (!await _userRepository.TryAddUser(newUser, cancellationToken))
        {
            throw new Exception("Can't add user, try again later");
        }
        newUser.AddDomainEvent(new UserCreatedDomainEvent());
        await _dispatcher.DispatchAsync(newUser.DomainEvents, cancellationToken);
        newUser.ClearDomainEvents();
        return true;
    }
}