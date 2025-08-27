using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services;
using PersonalManagerAPI.Services.Interfaces;
using PersonalManagerAPI.DTOs.GuestBookEntry;

namespace PersonalManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GuestBookEntriesController : BaseController
{
    private readonly IGuestBookService _guestBookService;

    public GuestBookEntriesController(IGuestBookService guestBookService)
    {
        _guestBookService = guestBookService;
    }

    /// <summary>
    /// 取得所有留言（分頁）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetGuestBookEntries([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool includeReplies = true)
    {
        try
        {
            var skip = (page - 1) * pageSize;
            var entries = await _guestBookService.GetAllEntriesAsync(skip, pageSize, includeReplies);
            var totalEntries = await _guestBookService.GetAllEntriesAsync(0, int.MaxValue, includeReplies);
            var totalCount = totalEntries.Count();

            var result = new
            {
                Entries = entries,
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
            var entry = await _guestBookService.GetEntryByIdAsync(id);
            
            if (entry == null)
            {
                return NotFound(ApiResponse<object>.ErrorResult("找不到指定的留言"));
            }

            return Ok(ApiResponse<object>.SuccessResult(entry, "成功取得留言資料"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResult("伺服器錯誤: " + ex.Message));
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
            var replies = await _guestBookService.GetRepliesAsync(parentId);
            
            return Ok(ApiResponse<object>.SuccessResult(replies, "成功取得回覆列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResult("伺服器錯誤: " + ex.Message));
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
            var recentEntries = await _guestBookService.GetRecentEntriesAsync(limit);
            
            return Ok(ApiResponse<object>.SuccessResult(recentEntries, "成功取得最新留言"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 建立留言
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateGuestBookEntry([FromBody] CreateGuestBookEntryDto createEntryDto)
    {
        try
        {
            var ipAddress = GetClientIpAddress();
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            
            var entry = await _guestBookService.CreateEntryAsync(createEntryDto, ipAddress, userAgent);

            return Ok(ApiResponse<object>.SuccessResult(entry, "留言建立成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 更新留言（僅限編輯內容，不能修改作者資訊）
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGuestBookEntry(int id, [FromBody] UpdateGuestBookEntryDto updateEntryDto)
    {
        try
        {
            var entry = await _guestBookService.UpdateEntryAsync(id, updateEntryDto);
            
            if (entry == null)
            {
                return NotFound(ApiResponse<object>.ErrorResult("找不到指定的留言"));
            }

            return Ok(ApiResponse<object>.SuccessResult(entry, "留言更新成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResult("伺服器錯誤: " + ex.Message));
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
            var success = await _guestBookService.DeleteEntryAsync(id);
            
            if (!success)
            {
                return NotFound(ApiResponse.ErrorResult("找不到指定的留言"));
            }

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
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest(ApiResponse<object>.ErrorResult("搜尋關鍵字不能為空"));
            }

            var searchResults = await _guestBookService.SearchEntriesAsync(keyword);
            
            return Ok(ApiResponse<object>.SuccessResult(searchResults, $"搜尋「{keyword}」找到 {searchResults.Count()} 則留言"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    private string GetClientIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}