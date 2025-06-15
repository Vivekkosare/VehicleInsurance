
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using VehicleInsurance.Shared.DTOs;

namespace VehicleInsurance.Shared.Services;

public class RedisCacheService(IDistributedCache _cache,
ILogger<RedisCacheService> _logger) : ICacheService
{
    public Task ClearAsync()
    {
        // Redis does not support clearing all keys directly, so this method is not implemented.
        // You would typically manage cache keys with a prefix or pattern to simulate clearing.
        throw new NotImplementedException("Clearing all keys is not supported in Redis. Consider using key prefixes.");
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
            var value = JsonSerializer.Deserialize<T>(data);
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
            var data = JsonSerializer.Serialize(value);
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
}
