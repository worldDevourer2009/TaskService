using TaskHandler.Domain.ValueObjects;

namespace TaskHandler.Domain.Repositories;

public interface IRevokedRefreshTokenRepository
{
    Task<RefreshToken> GetRevokedToken(string token);
    Task RevokeToken(string token);
    Task<bool> IsRevoked(string token);
}