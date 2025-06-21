using TaskHandler.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TaskHandler.Domain.DomainsEvents;

namespace TaskHandler.Application.Services;

public class DomainDispatcher : IDomainDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var @event in domainEvents)
        {
            var handlerType = typeof(IDomainEventHandler<>)
                .MakeGenericType(@event.GetType());
            
            var handlers = (IEnumerable<object>)_serviceProvider.GetServices(handlerType);

            foreach (dynamic handler in handlers)
            {
                await handler.HandleAsync((dynamic)@event, cancellationToken);
            }
        }
    }
}
