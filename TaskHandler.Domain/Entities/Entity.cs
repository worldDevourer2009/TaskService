using TaskHandler.Domain.DomainsEvents;

namespace TaskHandler.Domain.Entities;

public abstract class Entity
{
    public Guid Id { get; protected set; }
    
    private readonly List<IDomainEvent> _domainEvents;
    
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected Entity()
    {
        _domainEvents = new List<IDomainEvent>();
    }

    protected Entity(Guid id)
    {
        Id = id;
        _domainEvents = new List<IDomainEvent>();
    }

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }
    
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }
        
        if (obj is not Entity other)
        {
            return false;
        }
        
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        if (Id == Guid.Empty || other.Id == Guid.Empty)
        {
            return false;
        }
        
        return Id == other.Id;
    }

    public static bool operator ==(Entity? left, Entity? right)
    {
        if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
        {
            return true;
        }
        
        if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
        {
            return false;
        }
        
        return left.Equals(right);
    }

    public static bool operator !=(Entity? left, Entity? right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return (GetType().ToString() + Id).GetHashCode();
    }
}