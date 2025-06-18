using TaskHandler.Domain.Entities;

namespace TaskHandler.Domain.Specifications.Users;

public class ActiveUsersSpecification : ISpecification<User>
{
    public bool IsSatisfiedBy(User entity)
    {
        return entity.IsActive;
    }

    public Func<User, bool> ToExpression()
    {
        return user => user.IsActive;
    }
}