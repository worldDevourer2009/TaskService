namespace TaskHandler.Domain.Exceptions;

public class DomainException : Exception
{
    public string Reason { get; }

    public DomainException(string reason) 
        : base()
    {
        Reason = reason;
    }
    
    public DomainException(string reason, Exception innerException) 
        : base(reason, innerException)
    {
        Reason = reason;
    }
}