using TaskHandler.Domain.DomainsEvents.Tasks;
using TaskHandler.Domain.Enums;
using TaskStatus = TaskHandler.Domain.Enums.TaskStatus;

namespace TaskHandler.Domain.Entities;

public class TaskItem : Entity
{
    public Guid? UserId { get; private set; }
    public string Title { get; private set; } = "New Task";
    public string Description { get; private set; } = string.Empty;
    public TaskStatus Status { get; private set; } = TaskStatus.Pending;
    public TaskType TaskType { get; set; } = TaskType.None;
    public DateTime? CreatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public DateTime? CompletionDate { get; private set; }
    public TaskPriority Priority { get; private set; } = 0;
    public bool IsCompleted { get; private set; }

    public TaskItem()
    {
        AddDomainEvent(new TaskCreatedDomainEvent());
    }
    
    public static TaskItem Create(Guid userId)
    {
        return new TaskItem()
        {
            UserId = userId,
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void Update(TaskItem taskItem)
    {
        Title = taskItem.Title;
        Description = taskItem.Description;
        Status = taskItem.Status;
        TaskType = taskItem.TaskType;
        Priority = taskItem.Priority;
        CompletionDate = taskItem.CompletionDate;
        
        AddDomainEvent(new TaskUpdatedDomainEvent());
    }
    
    public void SetTitle(string title)
    {
        Title = title;
    }
    
    public void SetTaskType(TaskType taskType)
    {
        TaskType = taskType;
    }

    public void SetDescription(string description)
    {
        Description = description;
    }

    public void SetStatus(TaskStatus status)
    {
        Status = status;
    }

    public void SetPriority(TaskPriority priority)
    {
        Priority = priority;
    }

    public void SetCompletionDate(DateTime? completionDate)
    {
        CompletionDate = completionDate;
    }

    public void Delete()
    {
        DeletedAt = DateTime.UtcNow;
    }
    
    public void Complete()
    {
        IsCompleted = true;
    }
}