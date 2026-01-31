using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeadFlowAI.Infrastructure.Configuration;

public static class CachingConfiguration
{
    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "LeadFlowAI:";
        });

        // Adiciona serviço de cache distribuído
        services.AddSingleton<IDistributedCache>(sp =>
            new RedisCache(new RedisCacheOptions
            {
                Configuration = redisConnectionString,
                InstanceName = "LeadFlowAI:"
            }));

        return services;
    }
}