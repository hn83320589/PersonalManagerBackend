using PersonalManagerAPI.DTOs.GuestBookEntry;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services;
using PersonalManagerAPI.Services.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace PersonalManagerAPI.Services.Implementation;

/// <summary>
/// 留言板服務實作
/// </summary>
public class GuestBookService : IGuestBookService
{
    private readonly JsonDataService _dataService;
    private readonly IMapper _mapper;
    private readonly ILogger<GuestBookService> _logger;

    public GuestBookService(JsonDataService dataService, IMapper mapper, ILogger<GuestBookService> logger)
    {
        _dataService = dataService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<GuestBookEntryResponseDto>> GetAllEntriesAsync(int skip = 0, int take = 50, bool includeReplies = true)
    {
        try
        {
            var entries = await _dataService.GetGuestBookEntriesAsync();
            var query = entries.AsQueryable();

            if (!includeReplies)
            {
                query = query.Where(e => e.ParentId == null);
            }

            var pagedEntries = query
                .OrderByDescending(e => e.CreatedAt)
                .Skip(skip)
                .Take(take);

            var result = _mapper.Map<IEnumerable<GuestBookEntryResponseDto>>(pagedEntries);
            
            // 填充回覆資訊
            if (includeReplies)
            {
                await PopulateRepliesAsync(result, entries);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all entries");
            throw;
        }
    }

    public async Task<GuestBookEntryResponseDto?> GetEntryByIdAsync(int id)
    {
        try
        {
            var entries = await _dataService.GetGuestBookEntriesAsync();
            var entry = entries.FirstOrDefault(e => e.Id == id);
            
            if (entry == null)
                return null;

            var result = _mapper.Map<GuestBookEntryResponseDto>(entry);
            
            // 填充回覆資訊
            await PopulateRepliesAsync(new[] { result }, entries);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting entry with ID {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<GuestBookEntryResponseDto>> GetPublicEntriesAsync(int skip = 0, int take = 50)
    {
        try
        {
            var entries = await _dataService.GetGuestBookEntriesAsync();
            var publicEntries = entries
                .Where(e => e.IsPublic && e.IsApproved)
                .OrderByDescending(e => e.CreatedAt)
                .Skip(skip)
                .Take(take);

            var result = _mapper.Map<IEnumerable<GuestBookEntryResponseDto>>(publicEntries);
            await PopulateRepliesAsync(result, entries);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting public entries");
            throw;
        }
    }

    public async Task<IEnumerable<GuestBookEntryResponseDto>> GetPendingEntriesAsync()
    {
        try
        {
            var entries = await _dataService.GetGuestBookEntriesAsync();
            var pendingEntries = entries
                .Where(e => !e.IsApproved)
                .OrderByDescending(e => e.CreatedAt);

            return _mapper.Map<IEnumerable<GuestBookEntryResponseDto>>(pendingEntries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting pending entries");
            throw;
        }
    }

    public async Task<IEnumerable<GuestBookEntryResponseDto>> GetRepliesAsync(int parentId)
    {
        try
        {
            var entries = await _dataService.GetGuestBookEntriesAsync();
            var replies = entries
                .Where(e => e.ParentId == parentId)
                .OrderBy(e => e.CreatedAt);

            return _mapper.Map<IEnumerable<GuestBookEntryResponseDto>>(replies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting replies for parent ID {ParentId}", parentId);
            throw;
        }
    }

    public async Task<IEnumerable<GuestBookEntryResponseDto>> GetRecentEntriesAsync(int limit = 5)
    {
        try
        {
            var entries = await _dataService.GetGuestBookEntriesAsync();
            var recentEntries = entries
                .Where(e => e.ParentId == null && e.IsPublic && e.IsApproved)
                .OrderByDescending(e => e.CreatedAt)
                .Take(limit);

            return _mapper.Map<IEnumerable<GuestBookEntryResponseDto>>(recentEntries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting recent entries");
            throw;
        }
    }

    public async Task<GuestBookEntryResponseDto> CreateEntryAsync(CreateGuestBookEntryDto createEntryDto, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            var entries = await _dataService.GetGuestBookEntriesAsync();
            var entry = _mapper.Map<GuestBookEntry>(createEntryDto);
            
            // 生成新ID
            entry.Id = entries.Any() ? entries.Max(e => e.Id) + 1 : 1;
            
            // 設定時間戳記和其他資訊
            entry.CreatedAt = DateTime.UtcNow;
            entry.UpdatedAt = DateTime.UtcNow;
            entry.IpAddress = ipAddress;
            entry.UserAgent = userAgent;
            entry.IsApproved = false; // 新留言需要審核

            entries.Add(entry);
            await _dataService.SaveGuestBookEntriesAsync(entries);

            _logger.LogInformation("GuestBook entry created with ID {Id}", entry.Id);
            return _mapper.Map<GuestBookEntryResponseDto>(entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating entry");
            throw;
        }
    }

    public async Task<GuestBookEntryResponseDto?> CreateReplyAsync(int parentId, CreateGuestBookEntryDto createEntryDto, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            var entries = await _dataService.GetGuestBookEntriesAsync();
            var parentEntry = entries.FirstOrDefault(e => e.Id == parentId);
            
            if (parentEntry == null)
            {
                _logger.LogWarning("Parent entry with ID {ParentId} not found for reply", parentId);
                return null;
            }

            var replyDto = new CreateGuestBookEntryDto
            {
                Name = createEntryDto.Name,
                Email = createEntryDto.Email,
                Website = createEntryDto.Website,
                Message = createEntryDto.Message,
                ParentId = parentId,
                IsPublic = createEntryDto.IsPublic
            };
            return await CreateEntryAsync(replyDto, ipAddress, userAgent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating reply for parent ID {ParentId}", parentId);
            throw;
        }
    }

    public async Task<GuestBookEntryResponseDto?> UpdateEntryAsync(int id, UpdateGuestBookEntryDto updateEntryDto)
    {
        try
        {
            var entries = await _dataService.GetGuestBookEntriesAsync();
            var existingEntry = entries.FirstOrDefault(e => e.Id == id);
            
            if (existingEntry == null)
            {
                _logger.LogWarning("Entry with ID {Id} not found for update", id);
                return null;
            }

            _mapper.Map(updateEntryDto, existingEntry);
            existingEntry.UpdatedAt = DateTime.UtcNow;

            await _dataService.SaveGuestBookEntriesAsync(entries);
            _logger.LogInformation("Entry with ID {Id} updated", id);
            
            return _mapper.Map<GuestBookEntryResponseDto>(existingEntry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating entry with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteEntryAsync(int id)
    {
        try
        {
            var entries = await _dataService.GetGuestBookEntriesAsync();
            var entry = entries.FirstOrDefault(e => e.Id == id);
            
            if (entry == null)
            {
                _logger.LogWarning("Entry with ID {Id} not found for deletion", id);
                return false;
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
            _logger.LogInformation("Entry with ID {Id} deleted", id);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting entry with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> ApproveEntryAsync(int id)
    {
        try
        {
            var entries = await _dataService.GetGuestBookEntriesAsync();
            var entry = entries.FirstOrDefault(e => e.Id == id);
            
            if (entry == null)
            {
                return false;
            }

            entry.IsApproved = true;
            entry.UpdatedAt = DateTime.UtcNow;

            await _dataService.SaveGuestBookEntriesAsync(entries);
            _logger.LogInformation("Entry with ID {Id} approved", id);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while approving entry with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> RejectEntryAsync(int id)
    {
        try
        {
            var entries = await _dataService.GetGuestBookEntriesAsync();
            var entry = entries.FirstOrDefault(e => e.Id == id);
            
            if (entry == null)
            {
                return false;
            }

            entry.IsApproved = false;
            entry.UpdatedAt = DateTime.UtcNow;

            await _dataService.SaveGuestBookEntriesAsync(entries);
            _logger.LogInformation("Entry with ID {Id} rejected", id);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while rejecting entry with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> BatchApproveEntriesAsync(IEnumerable<int> entryIds)
    {
        try
        {
            var entries = await _dataService.GetGuestBookEntriesAsync();
            var entriesToApprove = entries.Where(e => entryIds.Contains(e.Id)).ToList();
            
            foreach (var entry in entriesToApprove)
            {
                entry.IsApproved = true;
                entry.UpdatedAt = DateTime.UtcNow;
            }

            await _dataService.SaveGuestBookEntriesAsync(entries);
            _logger.LogInformation("Batch approved {Count} entries", entriesToApprove.Count);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while batch approving entries");
            throw;
        }
    }

    public async Task<bool> BatchDeleteEntriesAsync(IEnumerable<int> entryIds)
    {
        try
        {
            var entries = await _dataService.GetGuestBookEntriesAsync();
            var entriesToDelete = entries.Where(e => entryIds.Contains(e.Id)).ToList();
            
            foreach (var entry in entriesToDelete)
            {
                // 如果是主留言，同時刪除所有回覆
                if (entry.ParentId == null)
                {
                    var replies = entries.Where(e => e.ParentId == entry.Id).ToList();
                    foreach (var reply in replies)
                    {
                        entries.Remove(reply);
                    }
                }
                
                entries.Remove(entry);
            }

            await _dataService.SaveGuestBookEntriesAsync(entries);
            _logger.LogInformation("Batch deleted {Count} entries", entriesToDelete.Count);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while batch deleting entries");
            throw;
        }
    }

    public async Task<IEnumerable<GuestBookEntryResponseDto>> SearchEntriesAsync(string keyword)
    {
        try
        {
            var entries = await _dataService.GetGuestBookEntriesAsync();
            var searchResults = entries
                .Where(e => e.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                           e.Message.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(e => e.CreatedAt);

            return _mapper.Map<IEnumerable<GuestBookEntryResponseDto>>(searchResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching entries with keyword {Keyword}", keyword);
            throw;
        }
    }

    public async Task<IEnumerable<GuestBookEntryResponseDto>> GetEntriesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var entries = await _dataService.GetGuestBookEntriesAsync();
            var filteredEntries = entries
                .Where(e => e.CreatedAt >= startDate && e.CreatedAt <= endDate)
                .OrderByDescending(e => e.CreatedAt);

            return _mapper.Map<IEnumerable<GuestBookEntryResponseDto>>(filteredEntries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting entries by date range");
            throw;
        }
    }

    public async Task<bool> EntryExistsAsync(int id)
    {
        try
        {
            var entries = await _dataService.GetGuestBookEntriesAsync();
            return entries.Any(e => e.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking entry existence");
            throw;
        }
    }

    public async Task<object> GetEntryStatsAsync()
    {
        try
        {
            var entries = await _dataService.GetGuestBookEntriesAsync();
            var now = DateTime.UtcNow;
            var today = DateTime.Today;
            var thisMonth = new DateTime(now.Year, now.Month, 1);

            var stats = new
            {
                TotalEntries = entries.Count(),
                MainEntries = entries.Count(e => e.ParentId == null),
                TotalReplies = entries.Count(e => e.ParentId != null),
                ApprovedEntries = entries.Count(e => e.IsApproved),
                PendingEntries = entries.Count(e => !e.IsApproved),
                PublicEntries = entries.Count(e => e.IsPublic),
                PrivateEntries = entries.Count(e => !e.IsPublic),
                
                TodayEntries = entries.Count(e => e.CreatedAt.Date == today),
                ThisMonthEntries = entries.Count(e => e.CreatedAt >= thisMonth),
                ThisWeekEntries = entries.Count(e => e.CreatedAt >= today.AddDays(-7)),
                
                MonthlyStats = entries
                    .Where(e => e.CreatedAt >= DateTime.Today.AddMonths(-12))
                    .GroupBy(e => e.CreatedAt.ToString("yyyy-MM"))
                    .Select(g => new { Month = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Month),
                    
                RecentActivity = entries
                    .OrderByDescending(e => e.CreatedAt)
                    .Take(10)
                    .Select(e => new { e.Id, e.Name, e.CreatedAt, e.IsApproved, ReplyCount = entries.Count(r => r.ParentId == e.Id) })
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting entry statistics");
            throw;
        }
    }

    #region Private Methods

    /// <summary>
    /// 填充留言的回覆資訊
    /// </summary>
    private async Task PopulateRepliesAsync(IEnumerable<GuestBookEntryResponseDto> entries, IEnumerable<GuestBookEntry> allEntries)
    {
        foreach (var entry in entries.Where(e => e.ParentId == null))
        {
            var replies = allEntries
                .Where(e => e.ParentId == entry.Id)
                .OrderBy(e => e.CreatedAt);
                
            entry.Replies = _mapper.Map<List<GuestBookEntryResponseDto>>(replies);
            entry.ReplyCount = entry.Replies.Count;
        }
        
        await Task.CompletedTask; // 保持非同步簽名一致性
    }

    #endregion
}