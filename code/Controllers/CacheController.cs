using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Services.Interfaces;

namespace PersonalManagerAPI.Controllers;

/// <summary>
/// 快取管理控制器 - 提供快取管理與監控功能
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CacheController : BaseController
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheController> _logger;

    public CacheController(ICacheService cacheService, ILogger<CacheController> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// 獲取快取統計資訊
    /// </summary>
    /// <returns>快取統計資訊</returns>
    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<object>>> GetCacheStats()
    {
        try
        {
            var stats = await _cacheService.GetCacheStatsAsync();
            return Ok(ApiResponse<object>.Success(stats, "快取統計資訊獲取成功"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "獲取快取統計資訊時發生錯誤");
            return StatusCode(500, ApiResponse<object>.Failure("獲取快取統計資訊失敗"));
        }
    }

    /// <summary>
    /// 檢查特定快取項目是否存在
    /// </summary>
    /// <param name="key">快取鍵</param>
    /// <returns>是否存在</returns>
    [HttpGet("exists/{key}")]
    public async Task<ActionResult<ApiResponse<bool>>> CheckCacheExists(string key)
    {
        try
        {
            var exists = await _cacheService.ExistsAsync(key);
            return Ok(ApiResponse<bool>.Success(exists, $"快取項目 {key} " + (exists ? "存在" : "不存在")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檢查快取項目存在性時發生錯誤: {Key}", key);
            return StatusCode(500, ApiResponse<bool>.Failure("檢查快取項目失敗"));
        }
    }

    /// <summary>
    /// 獲取快取項目的過期時間
    /// </summary>
    /// <param name="key">快取鍵</param>
    /// <returns>過期時間</returns>
    [HttpGet("expiration/{key}")]
    public async Task<ActionResult<ApiResponse<TimeSpan?>>> GetCacheExpiration(string key)
    {
        try
        {
            var expiration = await _cacheService.GetExpirationAsync(key);
            return Ok(ApiResponse<TimeSpan?>.Success(expiration, "快取項目過期時間獲取成功"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "獲取快取項目過期時間時發生錯誤: {Key}", key);
            return StatusCode(500, ApiResponse<TimeSpan?>.Failure("獲取快取項目過期時間失敗"));
        }
    }

    /// <summary>
    /// 設定快取項目的過期時間
    /// </summary>
    /// <param name="key">快取鍵</param>
    /// <param name="request">過期時間設定請求</param>
    /// <returns>操作結果</returns>
    [HttpPut("expiration/{key}")]
    public async Task<ActionResult<ApiResponse<string>>> SetCacheExpiration(string key, [FromBody] SetExpirationRequest request)
    {
        try
        {
            var expiration = TimeSpan.FromSeconds(request.ExpirationSeconds);
            await _cacheService.SetExpirationAsync(key, expiration);
            return Ok(ApiResponse<string>.Success("OK", "快取項目過期時間設定成功"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "設定快取項目過期時間時發生錯誤: {Key}", key);
            return StatusCode(500, ApiResponse<string>.Failure("設定快取項目過期時間失敗"));
        }
    }

    /// <summary>
    /// 移除特定快取項目
    /// </summary>
    /// <param name="key">快取鍵</param>
    /// <returns>操作結果</returns>
    [HttpDelete("{key}")]
    public async Task<ActionResult<ApiResponse<string>>> RemoveCache(string key)
    {
        try
        {
            await _cacheService.RemoveAsync(key);
            return Ok(ApiResponse<string>.Success("OK", $"快取項目 {key} 移除成功"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "移除快取項目時發生錯誤: {Key}", key);
            return StatusCode(500, ApiResponse<string>.Failure("移除快取項目失敗"));
        }
    }

    /// <summary>
    /// 批量移除快取項目（根據模式）
    /// </summary>
    /// <param name="pattern">快取鍵模式</param>
    /// <returns>操作結果</returns>
    [HttpDelete("pattern/{pattern}")]
    public async Task<ActionResult<ApiResponse<string>>> RemoveCacheByPattern(string pattern)
    {
        try
        {
            await _cacheService.RemoveByPatternAsync(pattern);
            return Ok(ApiResponse<string>.Success("OK", $"快取項目模式 {pattern} 移除成功"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量移除快取項目時發生錯誤: {Pattern}", pattern);
            return StatusCode(500, ApiResponse<string>.Failure("批量移除快取項目失敗"));
        }
    }

    /// <summary>
    /// 清空所有快取（慎用）
    /// </summary>
    /// <returns>操作結果</returns>
    [HttpDelete("all")]
    [Authorize(Roles = "Admin")] // 只有管理員可以清空所有快取
    public async Task<ActionResult<ApiResponse<string>>> ClearAllCache()
    {
        try
        {
            await _cacheService.ClearAllAsync();
            _logger.LogWarning("管理員清空了所有快取");
            return Ok(ApiResponse<string>.Success("OK", "所有快取已清空"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清空所有快取時發生錯誤");
            return StatusCode(500, ApiResponse<string>.Failure("清空所有快取失敗"));
        }
    }

    /// <summary>
    /// 刷新特定快取項目（強制重新載入）
    /// </summary>
    /// <param name="key">快取鍵</param>
    /// <param name="request">刷新請求</param>
    /// <returns>操作結果</returns>
    [HttpPost("refresh/{key}")]
    public async Task<ActionResult<ApiResponse<string>>> RefreshCache(string key, [FromBody] RefreshCacheRequest request)
    {
        try
        {
            // 這裡應該根據快取類型來決定如何刷新
            // 為了示例，我們先移除快取項目
            await _cacheService.RemoveAsync(key);
            
            var expiration = request.ExpirationSeconds.HasValue 
                ? TimeSpan.FromSeconds(request.ExpirationSeconds.Value) 
                : (TimeSpan?)null;

            // 如果有提供資料，則設定新的快取
            if (!string.IsNullOrEmpty(request.Data))
            {
                await _cacheService.SetAsync(key, request.Data, expiration);
            }

            return Ok(ApiResponse<string>.Success("OK", $"快取項目 {key} 刷新成功"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刷新快取項目時發生錯誤: {Key}", key);
            return StatusCode(500, ApiResponse<string>.Failure("刷新快取項目失敗"));
        }
    }

    /// <summary>
    /// 測試快取功能
    /// </summary>
    /// <param name="request">測試請求</param>
    /// <returns>測試結果</returns>
    [HttpPost("test")]
    public async Task<ActionResult<ApiResponse<object>>> TestCache([FromBody] TestCacheRequest request)
    {
        try
        {
            var testKey = $"test:{Guid.NewGuid()}";
            var testData = new
            {
                Message = request.TestData ?? "Hello Cache!",
                Timestamp = DateTime.UtcNow,
                Random = new Random().Next(1000, 9999)
            };

            var expiration = TimeSpan.FromSeconds(request.ExpirationSeconds ?? 300); // 預設 5 分鐘

            // 設定快取
            await _cacheService.SetAsync(testKey, testData, expiration);

            // 立即獲取快取
            var cachedData = await _cacheService.GetAsync<object>(testKey);

            // 檢查是否存在
            var exists = await _cacheService.ExistsAsync(testKey);

            // 獲取過期時間
            var remainingTime = await _cacheService.GetExpirationAsync(testKey);

            var result = new
            {
                TestKey = testKey,
                OriginalData = testData,
                CachedData = cachedData,
                Exists = exists,
                RemainingTime = remainingTime,
                TestPassed = cachedData != null && exists
            };

            return Ok(ApiResponse<object>.Success(result, "快取功能測試完成"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "快取功能測試時發生錯誤");
            return StatusCode(500, ApiResponse<object>.Failure("快取功能測試失敗"));
        }
    }
}

/// <summary>
/// 設定過期時間請求
/// </summary>
public class SetExpirationRequest
{
    public int ExpirationSeconds { get; set; }
}

/// <summary>
/// 刷新快取請求
/// </summary>
public class RefreshCacheRequest
{
    public string? Data { get; set; }
    public int? ExpirationSeconds { get; set; }
}

/// <summary>
/// 測試快取請求
/// </summary>
public class TestCacheRequest
{
    public string? TestData { get; set; }
    public int? ExpirationSeconds { get; set; }
}