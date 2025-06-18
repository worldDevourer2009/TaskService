using TaskHandler.Domain.Entities;

namespace TaskHandler.Domain.Specifications.Users;

public class NewUsersSpecification : ISpecification<User>
{
    public bool IsSatisfiedBy(User entity)
    {
        return entity.CreatedAt > DateTime.Now.AddDays(-1);
    }

    public Func<User, bool> ToExpression()
    {
        return user => user.CreatedAt > DateTime.Now.AddDays(-1);
    }
}