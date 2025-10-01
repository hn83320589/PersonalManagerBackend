using System.Text.Json;

namespace PersonalManagerAPI.Services.Interfaces;

/// <summary>
/// 快取服務介面 - 提供分散式快取功能
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// 設定快取項目
    /// </summary>
    /// <param name="key">快取鍵</param>
    /// <param name="value">快取值</param>
    /// <param name="expiration">過期時間</param>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

    /// <summary>
    /// 獲取快取項目
    /// </summary>
    /// <param name="key">快取鍵</param>
    /// <returns>快取值</returns>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// 檢查快取項目是否存在
    /// </summary>
    /// <param name="key">快取鍵</param>
    /// <returns>是否存在</returns>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// 移除快取項目
    /// </summary>
    /// <param name="key">快取鍵</param>
    Task RemoveAsync(string key);

    /// <summary>
    /// 批量移除快取項目（根據模式）
    /// </summary>
    /// <param name="pattern">快取鍵模式</param>
    Task RemoveByPatternAsync(string pattern);

    /// <summary>
    /// 設定快取項目的過期時間
    /// </summary>
    /// <param name="key">快取鍵</param>
    /// <param name="expiration">過期時間</param>
    Task SetExpirationAsync(string key, TimeSpan expiration);

    /// <summary>
    /// 獲取快取項目的過期時間
    /// </summary>
    /// <param name="key">快取鍵</param>
    /// <returns>過期時間</returns>
    Task<TimeSpan?> GetExpirationAsync(string key);

    /// <summary>
    /// 清空所有快取
    /// </summary>
    Task ClearAllAsync();

    /// <summary>
    /// 獲取快取統計資訊
    /// </summary>
    /// <returns>快取統計資訊</returns>
    Task<object> GetCacheStatsAsync();

    /// <summary>
    /// 獲取或設定快取項目 (快取穿透模式)
    /// </summary>
    /// <param name="key">快取鍵</param>
    /// <param name="factory">資料工廠方法</param>
    /// <param name="expiration">過期時間</param>
    /// <returns>快取值</returns>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);

    /// <summary>
    /// 刷新快取項目 (重新載入資料)
    /// </summary>
    /// <param name="key">快取鍵</param>
    /// <param name="factory">資料工廠方法</param>
    /// <param name="expiration">過期時間</param>
    /// <returns>快取值</returns>
    Task<T> RefreshAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
}