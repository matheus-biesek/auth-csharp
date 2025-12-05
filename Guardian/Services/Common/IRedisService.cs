namespace Guardian.Services.Common;

public interface IRedisService
{
    Task<bool> SetAsync(string key, string value, TimeSpan? expiration = null);
    Task<string?> GetAsync(string key);
    Task<bool> DeleteAsync(string key);
    Task<bool> ExistsAsync(string key);
}
