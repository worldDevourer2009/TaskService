using TaskHandler.Domain.Entities;

namespace TaskHandler.Domain.Services;

public interface IUserPasswordService
{
    Task<bool> VerifyPasswordAsync(User user, string password, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
    Task<bool> ResetPasswordAsync(Guid userId, string resetToken, string newPassword, CancellationToken cancellationToken = default);
    Task<string> GeneratePasswordResetTokenAsync(Guid userId, CancellationToken cancellationToken = default);
}