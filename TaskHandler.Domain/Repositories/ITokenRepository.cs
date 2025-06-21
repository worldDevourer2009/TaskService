namespace TaskHandler.Domain.Repositories;

public interface ITokenRepository
{
    Task AddToken(string token);
    Task<string?> GetToken(string token);
    Task RemoveToken(string token);
}