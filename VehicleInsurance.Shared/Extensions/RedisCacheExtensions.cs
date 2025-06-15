using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VehicleInsurance.Shared.Services;

namespace VehicleInsurance.Shared.Extensions;

public static class RedisCacheExtensions
{
    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = configuration.GetValue<string>("RedisInstanceName");
        });
        services.AddScoped<ICacheService, RedisCacheService>();
        return services;
    }
}
