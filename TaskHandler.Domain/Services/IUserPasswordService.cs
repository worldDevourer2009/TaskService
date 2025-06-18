using TaskHandler.Domain.Entities;

namespace TaskHandler.Domain.Services;

public interface IUserPasswordService
{
    Task<bool> VerifyPasswordAsync(User user, string password);
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    Task<bool> ResetPasswordAsync(Guid userId, string resetToken, string newPassword);
    Task<string> GeneratePasswordResetTokenAsync(Guid userId);
}