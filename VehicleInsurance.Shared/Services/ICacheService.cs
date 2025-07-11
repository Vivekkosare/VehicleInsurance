using VehicleInsurance.Shared.DTOs;

namespace VehicleInsurance.Shared.Services;

public interface ICacheService
{
    Task<Result<T?>> GetAsync<T>(string key);
    Task<Result<bool>> SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task<Result<bool>> RemoveAsync(string key);
    Task ClearAsync(List<string> keys);
    Task<bool> ExistsAsync(string key);
    Task ClearByPatternAsync(string pattern);
}
