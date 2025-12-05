using StackExchange.Redis;

namespace Guardian.Services.Common;

public class RedisService : IRedisService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisService> _logger;

    public RedisService(IConnectionMultiplexer redis, ILogger<RedisService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<bool> SetAsync(string key, string value, TimeSpan? expiration = null)
    {
        try
        {
            var db = _redis.GetDatabase();
            return await db.StringSetAsync(key, value, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting Redis key {Key}", key);
            return false;
        }
    }

    public async Task<string?> GetAsync(string key)
    {
        try
        {
            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync(key);
            return value.IsNullOrEmpty ? null : value.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Redis key {Key}", key);
            return null;
        }
    }

    public async Task<bool> DeleteAsync(string key)
    {
        try
        {
            var db = _redis.GetDatabase();
            return await db.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Redis key {Key}", key);
            return false;
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            var db = _redis.GetDatabase();
            return await db.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking Redis key existence {Key}", key);
            return false;
        }
    }

    public async Task<long> IncrementAsync(string key, long value = 1, TimeSpan? expiration = null)
    {
        try
        {
            var db = _redis.GetDatabase();
            var result = await db.StringIncrementAsync(key, value);
            if (expiration.HasValue)
            {
                await db.KeyExpireAsync(key, expiration.Value);
            }
            return (long)result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing Redis key {Key}", key);
            return -1;
        }
    }
}

