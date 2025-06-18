namespace TaskHandler.Application.Exceptions;

public class TaskItemNotFoundException : Exception
{
    public TaskItemNotFoundException(Guid id) 
        : base($"TaskItem with id {id} not found")
    {
    }
}