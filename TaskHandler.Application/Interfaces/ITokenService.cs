using TaskHandler.Domain.ValueObjects;

namespace TaskHandler.Application.Interfaces;

public interface ITokenService
{
    Task<AccessToken> GenerateAccessToken(Guid userId, CancellationToken cancellationToken = default);
    Task<Guid> GetUserIdFromToken(string token, CancellationToken cancellationToken = default);

    Task<(AccessToken accessToken, RefreshToken refreshToken, string rawRefreshToken)> GenerateTokenPair(Guid userId,
        CancellationToken cancellationToken = default);
    Task<(RefreshToken hashedToken, string rawToken)> GenerateRefreshToken(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ValidateToken(string token, CancellationToken cancellationToken = default);
    Task<(AccessToken newAccessToken, RefreshToken newRefreshToken, string rawRefreshToken)> RefreshTokens(
        string rawRefreshToken, CancellationToken cancellationToken = default
    );
    Task<bool> IsRefreshTokenValid(string rawRefreshToken);
    Task<bool> IsAccessTokenRevoked(string token);
    Task<bool> RevokeAccessToken(string token);
    Task<bool> RevokeRefreshToken(string rawRefreshToken);
}