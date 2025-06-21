namespace TaskHandler.Application.Interfaces;

public interface ITokenService
{
    Task<string> GenerateToken(Guid userId, CancellationToken cancellationToken = default);
    Task<Guid> GetUserIdFromToken(string token, CancellationToken cancellationToken = default);   
    Task<bool> ValidateToken(string token, CancellationToken cancellationToken = default); 
    Task<bool> RevokeToken(string token, CancellationToken cancellationToken = default);
    Task<bool> RevokeTokenForUser(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> RevokeAllTokensForUser(Guid userId, CancellationToken cancellationToken = default);   
    Task<bool> RevokeAllTokens(CancellationToken cancellationToken = default); 
    Task<bool> IsTokenRevoked(string token, CancellationToken cancellationToken = default);
    Task<bool> IsTokenRevokedForUser(Guid userId, CancellationToken cancellationToken = default);
}