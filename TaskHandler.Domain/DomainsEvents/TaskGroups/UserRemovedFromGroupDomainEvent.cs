namespace TaskHandler.Domain.DomainsEvents.TaskGroups;

public class UserRemovedFromGroupDomainEvent : DomainEvent
{
    public Guid GroupId { get; }
    public string UserId { get; }

    public UserRemovedFromGroupDomainEvent(Guid groupId, string userId)
    {
        GroupId = groupId;
        UserId = userId;
    }
}