using TaskHandler.Domain.DomainsEvents;

namespace TaskHandler.Application.Interfaces;

public interface IDomainEventHandler<in TEvent> 
    where TEvent : IDomainEvent 
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}