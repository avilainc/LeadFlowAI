using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace LeadFlowAI.Infrastructure.Services;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly JsonSerializerOptions _jsonOptions;

    public CacheService(IDistributedCache cache)
    {
        _cache = cache;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var cachedValue = await _cache.GetStringAsync(key, cancellationToken);
        if (string.IsNullOrEmpty(cachedValue))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(cachedValue, _jsonOptions);
        }
        catch (JsonException)
        {
            // Se não conseguir deserializar, remove do cache
            await RemoveAsync(key, cancellationToken);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions();
        if (expiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = expiration.Value;
        }
        else
        {
            // Expiração padrão de 1 hora
            options.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
        }

        var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
        await _cache.SetStringAsync(key, serializedValue, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var value = await _cache.GetAsync(key, cancellationToken);
        return value != null;
    }
}