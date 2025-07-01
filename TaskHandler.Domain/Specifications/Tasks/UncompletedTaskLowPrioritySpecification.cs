using TaskHandler.Domain.Entities;
using TaskHandler.Shared.Tasks.Enums;

namespace TaskHandler.Domain.Specifications.Tasks;

public class UncompletedTaskLowPrioritySpecification : ISpecification<TaskItem>
{
    public bool IsSatisfiedBy(TaskItem entity)
    {
        return !entity.IsCompleted && entity.Priority == TaskPriority.Low;
    }
    
    public Func<TaskItem, bool> ToExpression()
    {
        return task => !task.IsCompleted && task.Priority == TaskPriority.Low;
    }
}