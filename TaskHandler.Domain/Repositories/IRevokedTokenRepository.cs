namespace TaskHandler.Domain.Repositories;

public interface IRevokedTokenRepository
{
    Task<string> GetRevokedToken(string token);
    Task RevokeToken(string token);
    Task<bool> IsRevoked(string token);
}