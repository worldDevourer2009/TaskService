using TaskHandler.Domain.Entities;
using TaskHandler.Domain.Repositories;
using TaskHandler.Domain.Services;

namespace TaskHandler.Infrastructure.Services;

public class UserPasswordService : IUserPasswordService
{
    private readonly IUserRepository _context;
    
    public UserPasswordService(IUserRepository context)
    {
        _context = context;
    }

    public Task<bool> VerifyPasswordAsync(User user, string password)
    {
        if (user is null)
        {
            throw new Exception("User not found"); 
        }

        if (user.Password is null)
        {
            throw new Exception("User password not found");
        }

        return Task.FromResult(BCrypt.Net.BCrypt.Verify(password, user.Password.Hash));
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        if (currentPassword == newPassword)
        {
            throw new Exception("New password cannot be the same as the current password");
        }
        
        var user = await _context.GetById(userId);

        if (user is null)
        {
            throw new Exception("User not found");
        }
        
        if (!await VerifyPasswordAsync(user, currentPassword))
        {
            return false;
        }
        
        user.UpdatePassword(newPassword);

        await _context.UpdateUser(user);

        return true;
    }

    public async Task<bool> ResetPasswordAsync(Guid userId, string resetToken, string newPassword)
    {
        throw new NotImplementedException();
    }

    public async Task<string> GeneratePasswordResetTokenAsync(Guid userId)
    {
        throw new NotImplementedException();
    }
}