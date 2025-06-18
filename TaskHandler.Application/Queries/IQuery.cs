namespace TaskHandler.Application.Queries;

public interface IQuery<out TResponse> : IRequest<TResponse> 
    where TResponse : notnull
{
}