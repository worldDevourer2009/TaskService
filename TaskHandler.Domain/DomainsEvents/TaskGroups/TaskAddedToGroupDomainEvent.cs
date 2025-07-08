namespace TaskHandler.Domain.DomainsEvents.TaskGroups;

public class TaskAddedToGroupDomainEvent : DomainEvent
{
    public string TaskId { get; }
    public Guid GroupId { get; }
    public HashSet<string> Users { get; }

    public TaskAddedToGroupDomainEvent(string taskId, Guid groupId, HashSet<string> users)
    {
        TaskId = taskId;
        GroupId = groupId;
        Users = users;
    }
}