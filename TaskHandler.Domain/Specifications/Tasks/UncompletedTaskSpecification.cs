using TaskHandler.Domain.Entities;

namespace TaskHandler.Domain.Specifications.Tasks;

public class UncompletedTaskSpecification : ISpecification<TaskItem>
{
    public bool IsSatisfiedBy(TaskItem entity)
    {
        return !entity.IsCompleted;
    }
    
    public Func<TaskItem, bool> ToExpression()
    {
        return task => !task.IsCompleted;
    }
}