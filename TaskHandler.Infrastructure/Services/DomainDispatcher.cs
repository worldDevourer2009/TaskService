using TaskHandler.Application.Interfaces;
using TaskHandler.Domain.DomainsEvents;

namespace TaskHandler.Infrastructure.Services;

public class DomainDispatcher : IDomainDispatcher
{
    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
    }
}