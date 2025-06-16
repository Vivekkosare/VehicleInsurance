using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using VehicleInsurance.Shared.DTOs;

namespace VehicleInsurance.Shared.Services;

public class RedisCacheService(IDistributedCache _cache,
ILogger<RedisCacheService> _logger,
IConnectionMultiplexer _redis) : ICacheService
{
    public async Task ClearAsync(List<string> keys)
    {
        if (keys == null || !keys.Any())
        {
            _logger.LogWarning("Attempted to clear cache with a null or empty key list.");
            return;
        }

        foreach (var key in keys)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.LogWarning("Attempted to clear cache with a null or whitespace key.");
                continue;
            }

            try
            {
                await _cache.RemoveAsync(key);
                _logger.LogInformation("Successfully cleared cache for key: {Key}", key);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache for key: {Key}", key);
            }
        }
    }


    public async Task<bool> ExistsAsync(string key)
    {
        var data = await _cache.GetAsync(key);
        if (data == null)
        {
            _logger.LogInformation("No data found in cache for key: {Key}", key);
            return false;
        }
        _logger.LogInformation("Data found in cache for key: {Key}", key);
        return true;
    }

    public async Task<Result<T?>> GetAsync<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("Attempted to retrieve data from cache with a null or whitespace key.");
            return Result<T?>.Failure("Key cannot be null or whitespace.");
        }
        try
        {
            var data = await _cache.GetAsync(key);
            if (data == null)
            {
                _logger.LogInformation("No data found in cache for key: {Key}", key);
                return Result<T?>.Failure($"No data found for key: {key}");
            }
            // var value = JsonSerializer.Deserialize<T>(data);
            var value = JsonConvert.DeserializeObject<T>(System.Text.Encoding.UTF8.GetString(data), new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            if (value == null)
            {
                _logger.LogWarning("Failed to deserialize data for key: {Key}", key);
                return Result<T?>.Failure($"Failed to deserialize data for key: {key}");
            }
            _logger.LogInformation("Successfully retrieved data from cache for key: {Key}", key);
            return Result<T?>.Success(value);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error retrieving data from cache for key: {Key}", key);
            return Result<T?>.Failure($"An error occurred while retrieving data for key: {key}. Error: {ex.Message}");
        }

    }

    public async Task<Result<bool>> RemoveAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("Attempted to remove data from cache with a null or whitespace key.");
            return Result<bool>.Failure("Key cannot be null or whitespace.");
        }
        try
        {
            await _cache.RemoveAsync(key);
            _logger.LogInformation("Successfully removed data from cache for key: {Key}", key);
            return Result<bool>.Success(true);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error removing data from cache for key: {Key}", key);
            return Result<bool>.Failure($"An error occurred while removing data for key: {key}. Error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("Attempted to set data in cache with a null or whitespace key.");
            return Result<bool>.Failure("Key cannot be null or whitespace.");
        }
        if (value == null)
        {
            _logger.LogWarning("Attempted to set null value in cache for key: {Key}", key);
            return Result<bool>.Failure("Value cannot be null.");
        }
        try
        {
            var options = new DistributedCacheEntryOptions();
            if (expiry.HasValue)
            {
                options.SetAbsoluteExpiration(expiry.Value);
            }
            else
            {
                options.SetSlidingExpiration(TimeSpan.FromMinutes(1));
            }
            // var data = JsonSerializer.Serialize(value);
            var data = JsonConvert.SerializeObject(value, new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            if (string.IsNullOrEmpty(data))
            {
                _logger.LogWarning("Serialized data is null or empty for key: {Key}", key);
                return Result<bool>.Failure("Serialized data cannot be null or empty.");
            }
            _logger.LogInformation("Setting data in cache for key: {Key}", key);
            await _cache.SetAsync(key, System.Text.Encoding.UTF8.GetBytes(data), options);
            _logger.LogInformation("Successfully set data in cache for key: {Key}", key);

            return Result<bool>.Success(true);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error setting data in cache for key: {Key}", key);
            return Result<bool>.Failure($"An error occurred while setting data for key: {key}. Error: {ex.Message}");
        }
    }

    public async Task ClearByPatternAsync(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            _logger.LogWarning("Attempted to clear cache with a null or whitespace pattern.");
            return;
        }
        try
        {
            var endpoints = _redis.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = _redis.GetServer(endpoint);
                var db = _redis.GetDatabase();
                var keys = server.Keys(pattern: pattern).ToArray();
                foreach (var key in keys)
                {
                    await db.KeyDeleteAsync(key);
                    _logger.LogInformation("Deleted cache key by pattern: {Key}", key);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache by pattern: {Pattern}", pattern);
        }
    }
}
