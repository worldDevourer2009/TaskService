namespace TaskHandler.Domain.Exceptions;

public class TaskGroupException : DomainException
{
    public TaskGroupException(string reason) : base(reason)
    {
    }
}