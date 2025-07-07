using TaskHandler.Domain.DomainsEvents.TaskGroups;
using TaskHandler.Domain.Exceptions;

namespace TaskHandler.Domain.Entities;

public class TaskGroup : Entity
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public List<string> TaskIds { get; private set; }
    public HashSet<string> UserIds { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    
    private TaskGroup()
    {
        AddDomainEvent(new GroupCreatedDomainEvent(Id, UserIds!));
    }

    public static TaskGroup Create(string title, string description, HashSet<string>? userIds)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new TaskGroupException("Title cannot be null or whitespace");
        }

        if (userIds == null)
        {
            throw new TaskGroupException("UserIds cannot be null");
        }

        return new TaskGroup()
        {
            Title = title,
            Description = description,
            UserIds = userIds,
            CreatedAt = DateTime.UtcNow,
            TaskIds = new List<string>()
        };
    }

    public void AddUsers(List<string> ids)
    {
        foreach (var id in ids)
        {
            if (UserIds.Contains(id))
            {
                continue;
            }

            UserIds.Add(id);
            AddDomainEvent(new UserAddedToGroupDomainEvent(Id, id));
        }
    }

    public void AddUser(string id)
    {
        if (UserIds.Contains(id))
        {
            throw new TaskGroupException("User was already added");
        }

        UserIds.Add(id);
        AddDomainEvent(new UserAddedToGroupDomainEvent(Id, id));
    }

    public void RemoveUser(string id)
    {
        if (!UserIds.Contains(id))
        {
            throw new TaskGroupException("User was already removed");
        }
        
        UserIds.Remove(id);
        AddDomainEvent(new UserRemovedFromGroupDomainEvent(Id, id));
    }

    public void AddTask(string id)
    {
        if (!Guid.TryParse(id, out var guid))
        {
            throw new TaskGroupException("Invalid guid of task");
        }

        if (TaskIds.Contains(id))
        {
            throw new TaskGroupException("Task was already added");
        }
        
        TaskIds.Add(id);
        AddDomainEvent(new TaskAddedToGroupDomainEvent(id, Id, UserIds));
    }

    public void AddTasks(List<string> ids)
    {
        foreach (var id in ids)
        {
            if (!Guid.TryParse(id, out var guid))
            {
                continue;
            }

            if (TaskIds.Contains(id))
            {
                continue;
            }
            
            TaskIds.Add(id);
            AddDomainEvent(new TaskAddedToGroupDomainEvent(id, Id, UserIds));
        }
    }

    public void RemoveTask(string id)
    {
        if (!TaskIds.Contains(id))
        {
            throw new TaskGroupException("Task was already removed");
        }

        TaskIds.Remove(id);
    }

    public void RemoveAllTasks()
    {
        TaskIds.Clear();
    }
}