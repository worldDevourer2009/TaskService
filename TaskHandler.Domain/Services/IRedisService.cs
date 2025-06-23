namespace TaskHandler.Domain.Services;

public interface IRedisService
{
    Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null);
    Task<string?> GetAsync(string key);
    Task<bool> RemoveAsync(string key);
    Task<bool> KeyExistsAsync(string key);
    Task<IEnumerable<string>> GetKeysByPatternAsync(string pattern);
}