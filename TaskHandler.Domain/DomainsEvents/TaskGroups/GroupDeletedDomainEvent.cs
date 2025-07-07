namespace TaskHandler.Domain.DomainsEvents.TaskGroups;

public class GroupDeletedDomainEvent : DomainEvent
{
    public Guid Id { get; }
    public HashSet<string> Users { get; }

    public GroupDeletedDomainEvent(Guid id, HashSet<string> users)
    {
        Id = id;
        Users = users;
    }
}