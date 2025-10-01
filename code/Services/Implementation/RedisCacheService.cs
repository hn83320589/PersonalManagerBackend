using Microsoft.Extensions.Caching.Distributed;
using PersonalManagerAPI.Services.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace PersonalManagerAPI.Services.Implementation;

/// <summary>
/// Redis 快取服務實作 - 提供高效能分散式快取功能
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisCacheService(
        IDistributedCache distributedCache,
        IConnectionMultiplexer connectionMultiplexer,
        ILogger<RedisCacheService> logger)
    {
        _distributedCache = distributedCache;
        _connectionMultiplexer = connectionMultiplexer;
        _database = connectionMultiplexer.GetDatabase();
        _logger = logger;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <summary>
    /// 設定快取項目
    /// </summary>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            
            var options = new DistributedCacheEntryOptions();
            if (expiration.HasValue)
            {
                options.SetAbsoluteExpiration(expiration.Value);
            }
            else
            {
                // 預設過期時間 1 小時
                options.SetAbsoluteExpiration(TimeSpan.FromHours(1));
            }

            await _distributedCache.SetStringAsync(key, serializedValue, options);
            
            _logger.LogDebug("快取項目已設定: {Key}, 過期時間: {Expiration}", key, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "設定快取項目時發生錯誤: {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// 獲取快取項目
    /// </summary>
    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var serializedValue = await _distributedCache.GetStringAsync(key);
            
            if (string.IsNullOrEmpty(serializedValue))
            {
                _logger.LogDebug("快取項目未找到: {Key}", key);
                return default(T);
            }

            var value = JsonSerializer.Deserialize<T>(serializedValue, _jsonOptions);
            _logger.LogDebug("快取項目已獲取: {Key}", key);
            
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "獲取快取項目時發生錯誤: {Key}", key);
            return default(T);
        }
    }

    /// <summary>
    /// 檢查快取項目是否存在
    /// </summary>
    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            return await _database.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檢查快取項目存在性時發生錯誤: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// 移除快取項目
    /// </summary>
    public async Task RemoveAsync(string key)
    {
        try
        {
            await _distributedCache.RemoveAsync(key);
            _logger.LogDebug("快取項目已移除: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "移除快取項目時發生錯誤: {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// 批量移除快取項目（根據模式）
    /// </summary>
    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());
            var keys = server.Keys(pattern: pattern).ToArray();
            
            if (keys.Length > 0)
            {
                await _database.KeyDeleteAsync(keys);
                _logger.LogDebug("批量移除快取項目: {Pattern}, 數量: {Count}", pattern, keys.Length);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量移除快取項目時發生錯誤: {Pattern}", pattern);
            throw;
        }
    }

    /// <summary>
    /// 設定快取項目的過期時間
    /// </summary>
    public async Task SetExpirationAsync(string key, TimeSpan expiration)
    {
        try
        {
            await _database.KeyExpireAsync(key, expiration);
            _logger.LogDebug("快取項目過期時間已設定: {Key}, 過期時間: {Expiration}", key, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "設定快取項目過期時間時發生錯誤: {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// 獲取快取項目的過期時間
    /// </summary>
    public async Task<TimeSpan?> GetExpirationAsync(string key)
    {
        try
        {
            return await _database.KeyTimeToLiveAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "獲取快取項目過期時間時發生錯誤: {Key}", key);
            return null;
        }
    }

    /// <summary>
    /// 清空所有快取
    /// </summary>
    public async Task ClearAllAsync()
    {
        try
        {
            var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());
            await server.FlushDatabaseAsync();
            _logger.LogWarning("所有快取已清空");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清空所有快取時發生錯誤");
            throw;
        }
    }

    /// <summary>
    /// 獲取快取統計資訊
    /// </summary>
    public Task<object> GetCacheStatsAsync()
    {
        try
        {
            var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());
            
            var stats = new
            {
                CacheType = "Redis",
                IsConnected = _connectionMultiplexer.IsConnected,
                EndPoints = _connectionMultiplexer.GetEndPoints().Select(ep => ep.ToString()).ToArray(),
                DatabaseInfo = "Redis cache is connected and ready"
            };

            return Task.FromResult<object>(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "獲取快取統計資訊時發生錯誤");
            return Task.FromResult<object>(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// 獲取或設定快取項目 (快取穿透模式)
    /// </summary>
    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        try
        {
            var cachedValue = await GetAsync<T>(key);
            if (cachedValue != null)
            {
                _logger.LogDebug("快取命中: {Key}", key);
                return cachedValue;
            }

            _logger.LogDebug("快取未命中，從資料源載入: {Key}", key);
            var newValue = await factory();
            
            if (newValue != null)
            {
                await SetAsync(key, newValue, expiration);
            }

            return newValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "獲取或設定快取項目時發生錯誤: {Key}", key);
            // 如果快取失敗，至少要回傳資料源的資料
            return await factory();
        }
    }

    /// <summary>
    /// 刷新快取項目 (重新載入資料)
    /// </summary>
    public async Task<T> RefreshAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        try
        {
            _logger.LogDebug("刷新快取項目: {Key}", key);
            
            var newValue = await factory();
            
            if (newValue != null)
            {
                await SetAsync(key, newValue, expiration);
            }
            else
            {
                await RemoveAsync(key);
            }

            return newValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刷新快取項目時發生錯誤: {Key}", key);
            throw;
        }
    }
}