using Microsoft.Extensions.Caching.Memory;
using PersonalManagerAPI.Services.Interfaces;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace PersonalManagerAPI.Services.Implementation;

/// <summary>
/// 記憶體快取服務實作 - 作為 Redis 的備援方案
/// </summary>
public class InMemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<InMemoryCacheService> _logger;
    private readonly ConcurrentDictionary<string, DateTime> _keyExpiration;

    public InMemoryCacheService(IMemoryCache memoryCache, ILogger<InMemoryCacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
        _keyExpiration = new ConcurrentDictionary<string, DateTime>();
    }

    /// <summary>
    /// 設定快取項目
    /// </summary>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var options = new MemoryCacheEntryOptions();
            
            if (expiration.HasValue)
            {
                options.SetAbsoluteExpiration(expiration.Value);
                _keyExpiration[key] = DateTime.UtcNow.Add(expiration.Value);
            }
            else
            {
                // 預設過期時間 1 小時
                var defaultExpiration = TimeSpan.FromHours(1);
                options.SetAbsoluteExpiration(defaultExpiration);
                _keyExpiration[key] = DateTime.UtcNow.Add(defaultExpiration);
            }

            // 設定快取項目移除回調
            options.RegisterPostEvictionCallback((key, value, reason, state) =>
            {
                _keyExpiration.TryRemove(key.ToString()!, out _);
                _logger.LogDebug("快取項目已移除: {Key}, 原因: {Reason}", key, reason);
            });

            _memoryCache.Set(key, value, options);
            
            _logger.LogDebug("快取項目已設定: {Key}, 過期時間: {Expiration}", key, expiration);
            
            await Task.CompletedTask;
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
            if (_memoryCache.TryGetValue(key, out var value))
            {
                _logger.LogDebug("快取項目已獲取: {Key}", key);
                return (T?)value;
            }

            _logger.LogDebug("快取項目未找到: {Key}", key);
            return default(T);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "獲取快取項目時發生錯誤: {Key}", key);
            return default(T);
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// 檢查快取項目是否存在
    /// </summary>
    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            return _memoryCache.TryGetValue(key, out _);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檢查快取項目存在性時發生錯誤: {Key}", key);
            return false;
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// 移除快取項目
    /// </summary>
    public async Task RemoveAsync(string key)
    {
        try
        {
            _memoryCache.Remove(key);
            _keyExpiration.TryRemove(key, out _);
            _logger.LogDebug("快取項目已移除: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "移除快取項目時發生錯誤: {Key}", key);
            throw;
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// 批量移除快取項目（根據模式）
    /// </summary>
    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            // 將 Redis 模式轉換為正規表達式
            var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);

            var keysToRemove = _keyExpiration.Keys
                .Where(key => regex.IsMatch(key))
                .ToList();

            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
                _keyExpiration.TryRemove(key, out _);
            }

            _logger.LogDebug("批量移除快取項目: {Pattern}, 數量: {Count}", pattern, keysToRemove.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量移除快取項目時發生錯誤: {Pattern}", pattern);
            throw;
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// 設定快取項目的過期時間
    /// </summary>
    public async Task SetExpirationAsync(string key, TimeSpan expiration)
    {
        try
        {
            if (_memoryCache.TryGetValue(key, out var value))
            {
                // 重新設定快取項目與新的過期時間
                await SetAsync(key, value, expiration);
            }
            
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
            if (_keyExpiration.TryGetValue(key, out var expirationTime))
            {
                var remainingTime = expirationTime - DateTime.UtcNow;
                return remainingTime > TimeSpan.Zero ? remainingTime : null;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "獲取快取項目過期時間時發生錯誤: {Key}", key);
            return null;
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// 清空所有快取
    /// </summary>
    public async Task ClearAllAsync()
    {
        try
        {
            // IMemoryCache 沒有直接清空的方法，需要透過反射或重新建立
            // 這裡使用一個變通方法：移除所有已知的快取鍵
            var keysToRemove = _keyExpiration.Keys.ToList();
            
            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
            }
            
            _keyExpiration.Clear();
            
            _logger.LogWarning("所有快取已清空 (記憶體快取)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清空所有快取時發生錯誤");
            throw;
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// 獲取快取統計資訊
    /// </summary>
    public async Task<object> GetCacheStatsAsync()
    {
        try
        {
            var activeKeys = _keyExpiration.Keys.Count;
            var expiredKeys = _keyExpiration
                .Where(kvp => kvp.Value <= DateTime.UtcNow)
                .Count();

            var stats = new
            {
                CacheType = "InMemory",
                ActiveKeys = activeKeys,
                ExpiredKeys = expiredKeys,
                TotalKeys = activeKeys + expiredKeys,
                MemoryPressure = GC.GetTotalMemory(false),
                Generation0Collections = GC.CollectionCount(0),
                Generation1Collections = GC.CollectionCount(1),
                Generation2Collections = GC.CollectionCount(2)
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "獲取快取統計資訊時發生錯誤");
            return new { Error = ex.Message };
        }
        finally
        {
            await Task.CompletedTask;
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