using StackExchange.Redis;
using TaskHandler.Domain.Services;

namespace TaskHandler.Infrastructure.Services;

public class RedisService : IRedisService
{
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    
    public RedisService(ConnectionMultiplexer redis)
    {
        _redis = redis;
        _database = _redis.GetDatabase();
    }
    
    public async Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
        {
            throw new Exception("Key or value can't be null or empty");
        }
        return await _database.StringSetAsync(key, value, expiry);
    }

    public async Task<string?> GetAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new Exception("Key can't be null or empty");
        }
        
        return await _database.StringGetAsync(key);
    }

    public async Task<bool> RemoveAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new Exception("Key can't be null or empty");
        }

        return await _database.KeyDeleteAsync(key);
    }

    public async Task<bool> KeyExistsAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new Exception("Key can't be null or empty");
        }

        return await _database.KeyExistsAsync(key);
    }

    public async Task<IEnumerable<string>> GetKeysByPatternAsync(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            throw new ArgumentException("Pattern can't be null or empty", nameof(pattern));
        }

        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = await Task.Run(() => server.Keys(pattern: pattern));
            return keys.Select(key => key.ToString()).ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting keys by pattern '{pattern}'", ex);
        }
    }
}