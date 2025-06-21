using TaskHandler.Application.Interfaces;
using TaskHandler.Domain.DomainsEvents.Users;
using TaskHandler.Domain.Services;

namespace TaskHandler.Application.Events.Users;

public class NewUserCreatedEventHandler : IDomainEventHandler<UserCreatedDomainEvent>
{
    private const string CompanyEmail = "hellWorld@taskahandler.com";
    private readonly IEmailSender _emailSender;

    public NewUserCreatedEventHandler(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task HandleAsync(UserCreatedDomainEvent @event, CancellationToken cancellationToken = default)
    {
        await _emailSender.SendEmailAsync(CompanyEmail,
            "New user", $"New user created with id : {@event.Id} at time {DateTime.UtcNow}", null, null);
    }
}