namespace TaskHandler.Domain.DomainsEvents;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccuredOn { get; }
}

public abstract class DomainEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccuredOn { get; }

    protected DomainEvent()
    {
        Id = Guid.NewGuid();
        OccuredOn = DateTime.UtcNow;
    }
}