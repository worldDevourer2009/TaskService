using TaskHandler.Domain.Entities;
using TaskHandler.Domain.Enums;

namespace TaskHandler.Domain.Specifications.Tasks;

public class UncompletedTaskHighPrioritySpecification : ISpecification<TaskItem>
{
    public bool IsSatisfiedBy(TaskItem entity)
    {
        return !entity.IsCompleted && entity.Priority == TaskPriority.High && entity.CompletionDate > DateTime.UtcNow;
    }

    public Func<TaskItem, bool> ToExpression()
    {
        return task => !task.IsCompleted && task.Priority == TaskPriority.High && task.CompletionDate > DateTime.UtcNow;
    }
}