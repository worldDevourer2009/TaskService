namespace TaskHandler.Domain.DomainsEvents.TaskGroups;

public class GroupCreatedDomainEvent : DomainEvent
{
    public Guid GroupId { get; }
    public HashSet<string> Users { get; }

    public GroupCreatedDomainEvent(Guid groupId, HashSet<string> users)
    {
        GroupId = groupId;
        Users = users;
    }
}