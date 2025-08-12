using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services;

namespace PersonalManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GuestBookEntriesController : BaseController
{
    private readonly JsonDataService _dataService;

    public GuestBookEntriesController(JsonDataService dataService)
    {
        _dataService = dataService;
    }

    /// <summary>
    /// 取得所有留言（分頁）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetGuestBookEntries([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool includeReplies = true)
    {
        try
        {
            var entries = await _dataService.GetGuestBookEntriesAsync();
            var query = entries.AsQueryable();

            if (!includeReplies)
            {
                query = query.Where(e => e.ParentId == null);
            }

            var totalCount = query.Count();
            var pagedEntries = query
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new
            {
                Entries = pagedEntries,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
                
            return Ok(ApiResponse<object>.SuccessResult(result, "成功取得留言列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 取得指定留言
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetGuestBookEntry(int id)
    {
        try
        {
            var entries = await _dataService.GetGuestBookEntriesAsync();
            var entry = entries.FirstOrDefault(e => e.Id == id);
            
            if (entry == null)
            {
                return NotFound(ApiResponse<GuestBookEntry>.ErrorResult("找不到指定的留言"));
            }

            return Ok(ApiResponse<GuestBookEntry>.SuccessResult(entry, "成功取得留言資料"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<GuestBookEntry>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 取得指定留言的回覆
    /// </summary>
    [HttpGet("{parentId}/replies")]
    public async Task<IActionResult> GetReplies(int parentId)
    {
        try
        {
            var entries = await _dataService.GetGuestBookEntriesAsync();
            var replies = entries
                .Where(e => e.ParentId == parentId)
                .OrderBy(e => e.CreatedAt)
                .ToList();
                
            return Ok(ApiResponse<List<GuestBookEntry>>.SuccessResult(replies, "成功取得回覆列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<GuestBookEntry>>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 取得最新留言
    /// </summary>
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecentEntries([FromQuery] int limit = 5)
    {
        try
        {
            var entries = await _dataService.GetGuestBookEntriesAsync();
            var recentEntries = entries
                .Where(e => e.ParentId == null) // 只取主留言，不包含回覆
                .OrderByDescending(e => e.CreatedAt)
                .Take(limit)
                .ToList();
                
            return Ok(ApiResponse<List<GuestBookEntry>>.SuccessResult(recentEntries, "成功取得最新留言"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<GuestBookEntry>>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 建立留言
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateGuestBookEntry([FromBody] GuestBookEntry entry)
    {
        try
        {
            var entries = await _dataService.GetGuestBookEntriesAsync();

            // 驗證必要欄位
            if (string.IsNullOrEmpty(entry.Name) || string.IsNullOrEmpty(entry.Message))
            {
                return BadRequest(ApiResponse<GuestBookEntry>.ErrorResult("姓名和留言內容為必填項目"));
            }

            // 驗證Email格式（如果有提供）
            if (!string.IsNullOrEmpty(entry.Email) && !IsValidEmail(entry.Email))
            {
                return BadRequest(ApiResponse<GuestBookEntry>.ErrorResult("Email格式不正確"));
            }

            // 如果是回覆，檢查父留言是否存在
            if (entry.ParentId.HasValue)
            {
                var parentEntry = entries.FirstOrDefault(e => e.Id == entry.ParentId.Value);
                if (parentEntry == null)
                {
                    return BadRequest(ApiResponse<GuestBookEntry>.ErrorResult("找不到要回覆的留言"));
                }
            }
            
            entry.Id = entries.Count > 0 ? entries.Max(e => e.Id) + 1 : 1;
            entry.CreatedAt = DateTime.UtcNow;
            entry.UpdatedAt = DateTime.UtcNow;
            entry.IpAddress = GetClientIpAddress(); // 記錄IP地址
            
            entries.Add(entry);
            await _dataService.SaveGuestBookEntriesAsync(entries);

            return Ok(ApiResponse<GuestBookEntry>.SuccessResult(entry, "留言建立成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<GuestBookEntry>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 更新留言（僅限編輯內容，不能修改作者資訊）
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGuestBookEntry(int id, [FromBody] GuestBookEntry updatedEntry)
    {
        try
        {
            var entries = await _dataService.GetGuestBookEntriesAsync();
            var entry = entries.FirstOrDefault(e => e.Id == id);
            
            if (entry == null)
            {
                return NotFound(ApiResponse<GuestBookEntry>.ErrorResult("找不到指定的留言"));
            }

            // 只允許更新留言內容，不允許修改作者資訊
            entry.Message = updatedEntry.Message;
            entry.UpdatedAt = DateTime.UtcNow;

            await _dataService.SaveGuestBookEntriesAsync(entries);

            return Ok(ApiResponse<GuestBookEntry>.SuccessResult(entry, "留言更新成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<GuestBookEntry>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 刪除留言
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGuestBookEntry(int id)
    {
        try
        {
            var entries = await _dataService.GetGuestBookEntriesAsync();
            var entry = entries.FirstOrDefault(e => e.Id == id);
            
            if (entry == null)
            {
                return NotFound(ApiResponse.ErrorResult("找不到指定的留言"));
            }

            // 如果是主留言，同時刪除所有回覆
            if (entry.ParentId == null)
            {
                var replies = entries.Where(e => e.ParentId == id).ToList();
                foreach (var reply in replies)
                {
                    entries.Remove(reply);
                }
            }

            entries.Remove(entry);
            await _dataService.SaveGuestBookEntriesAsync(entries);

            return Ok(ApiResponse.SuccessResult("留言刪除成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 依關鍵字搜尋留言
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchGuestBookEntries([FromQuery] string keyword)
    {
        try
        {
            var entries = await _dataService.GetGuestBookEntriesAsync();
            var searchResults = entries
                .Where(e => e.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                           e.Message.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(e => e.CreatedAt)
                .ToList();
                
            return Ok(ApiResponse<List<GuestBookEntry>>.SuccessResult(searchResults, $"搜尋「{keyword}」找到 {searchResults.Count} 則留言"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<GuestBookEntry>>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private string GetClientIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}