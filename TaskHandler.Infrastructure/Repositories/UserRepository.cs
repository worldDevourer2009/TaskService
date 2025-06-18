using Microsoft.EntityFrameworkCore;
using TaskHandler.Application.Interfaces;
using TaskHandler.Domain.Entities;
using TaskHandler.Domain.Repositories;

namespace TaskHandler.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IApplicationDbContext _context;
    
    public UserRepository(IApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<User?> GetById(Guid id)
    {
        var user = await _context.Users.FindAsync(id).AsTask();
        return user;
    }

    public async Task<bool> DeleteUserById(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(id, cancellationToken).AsTask();

        if (user == null)
        {
            throw new Exception("User not found");
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IEnumerable<User>> GetAllUsers()
    {
        var users = await _context.Users.ToListAsync();
        return users;
    }

    public async Task UpdateUser(User user, CancellationToken cancellationToken = default)
    {
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        var id = user.Id;
        var userDb = await _context.Users.FindAsync(id, cancellationToken).AsTask();
        
        if (userDb == null)
        {
            throw new Exception("User not found");
        }

        if (userDb.Name != user.Name)
        {
            userDb.UpdateName(user.Name);
        }
        
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> TryAddUser(User user, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}