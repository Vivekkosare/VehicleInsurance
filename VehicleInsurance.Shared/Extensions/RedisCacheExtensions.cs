using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using VehicleInsurance.Shared.Services;

namespace VehicleInsurance.Shared.Extensions;

public static class RedisCacheExtensions
{
    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConfig = configuration.GetConnectionString("Redis") ??
                          throw new ArgumentNullException("Redis", "Redis connection string cannot be null.");

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConfig;
            options.InstanceName = configuration.GetValue<string>("RedisInstanceName");
        });
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            return ConnectionMultiplexer.Connect(redisConfig);
        });

        services.AddScoped<ICacheService, RedisCacheService>();
        return services;
    }
}
