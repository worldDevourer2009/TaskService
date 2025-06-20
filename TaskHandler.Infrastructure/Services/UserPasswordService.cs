using TaskHandler.Domain.Entities;
using TaskHandler.Domain.Repositories;
using TaskHandler.Domain.Services;
using TaskHandler.Domain.ValueObjects;

namespace TaskHandler.Infrastructure.Services;

public class UserPasswordService : IUserPasswordService
{
    private readonly IUserRepository _context;
    private readonly TimeSpan _tokenExpiration = TimeSpan.FromHours(2);
    
    public UserPasswordService(IUserRepository context)
    {
        _context = context;
    }

    public async Task<string> GeneratePasswordResetTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.GetById(userId, cancellationToken);

        if (user is null)
        {
            throw new Exception("User not found");
        }

        var (token, plain) = PasswordResetToken.Create(_tokenExpiration);
        user.AddResetToken(token);
        await _context.UpdateUser(user, cancellationToken);
        return plain;
    }

    public Task<bool> VerifyPasswordAsync(User user, string password, CancellationToken cancellationToken = default)
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

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        if (currentPassword == newPassword)
        {
            throw new Exception("New password cannot be the same as the current password");
        }
        
        var user = await _context.GetById(userId, cancellationToken);

        if (user is null)
        {
            throw new Exception("User not found");
        }
        
        if (!await VerifyPasswordAsync(user, currentPassword, cancellationToken))
        {
            return false;
        }
        
        user.UpdatePassword(newPassword);

        await _context.UpdateUser(user, cancellationToken);

        return true;
    }

    public async Task<bool> ResetPasswordAsync(Guid userId, string resetToken, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _context.GetById(userId, cancellationToken);

        if (user is null)
        {
            throw new Exception("User not found");
        }

        var token = user.GetValidResetToken();

        if (token is null)
        {
            return false;
        }
        
        user.UpdatePassword(newPassword);
        await _context.UpdateUser(user, cancellationToken);
        return true;
    }
}