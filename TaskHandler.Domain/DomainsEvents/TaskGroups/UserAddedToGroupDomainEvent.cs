namespace TaskHandler.Domain.DomainsEvents.TaskGroups;

public class UserAddedToGroupDomainEvent : DomainEvent
{
    public Guid GroupId { get; }
    public string UserId { get; }

    public UserAddedToGroupDomainEvent(Guid groupId, string userId)
    {
        GroupId = groupId;
        UserId = userId;
    }
}