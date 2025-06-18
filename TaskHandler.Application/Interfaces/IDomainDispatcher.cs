using TaskHandler.Domain.DomainsEvents;

namespace TaskHandler.Application.Interfaces;

public interface IDomainDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}