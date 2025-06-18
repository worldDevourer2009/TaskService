namespace TaskHandler.Domain.Specifications;

public interface ISpecification<in T>
{
    bool IsSatisfiedBy(T entity);
    Func<T, bool> ToExpression();
}