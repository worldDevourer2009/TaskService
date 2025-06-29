using TaskHandler.Domain.ValueObjects;

namespace TaskHandler.Application.Interfaces;

public interface ITokenService
{
    Task<(AccessToken accessToken, RefreshToken refreshToken, string rawRefreshToken, string rawAccessToken)> GenerateTokenPair(Guid userId,
        CancellationToken cancellationToken = default);
    Task<bool> ValidateToken(string token, CancellationToken cancellationToken = default);
    Task<(AccessToken newAccessToken, RefreshToken newRefreshToken, string rawRefreshToken, string rawAccessToken)> RefreshTokens(
        string rawRefreshToken, CancellationToken cancellationToken = default
    );
    Task<bool> IsRefreshTokenValid(string rawRefreshToken);
    Task<bool> IsAccessTokenRevoked(string token);
    Task<bool> RevokeAccessToken(string token);
    Task<bool> RevokeRefreshToken(string rawRefreshToken);
    Task<bool> RevokeAllTokensForUser(Guid userId, CancellationToken cancellationToken = default);
}