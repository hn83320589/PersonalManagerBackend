using PersonalManagerAPI.DTOs.TodoItem;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services;
using PersonalManagerAPI.Services.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace PersonalManagerAPI.Services.Implementation;

/// <summary>
/// 待辦事項服務實作
/// </summary>
public class TodoItemService : ITodoItemService
{
    private readonly JsonDataService _dataService;
    private readonly IMapper _mapper;
    private readonly ILogger<TodoItemService> _logger;

    // 預定義的狀態列表
    private readonly HashSet<string> _validStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Pending", "InProgress", "Completed", "Cancelled", "OnHold"
    };

    // 預定義的優先級列表
    private readonly HashSet<string> _validPriorities = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Low", "Medium", "High", "Urgent"
    };

    public TodoItemService(JsonDataService dataService, IMapper mapper, ILogger<TodoItemService> logger)
    {
        _dataService = dataService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<TodoItemResponseDto>> GetAllTodoItemsAsync(int skip = 0, int take = 50)
    {
        try
        {
            var todoItems = await _dataService.GetTodoItemsAsync();
            var pagedItems = todoItems
                .OrderBy(t => t.SortOrder)
                .ThenByDescending(t => t.CreatedAt)
                .Skip(skip)
                .Take(take);
            
            return _mapper.Map<IEnumerable<TodoItemResponseDto>>(pagedItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all todo items");
            throw;
        }
    }

    public async Task<TodoItemResponseDto?> GetTodoItemByIdAsync(int id)
    {
        try
        {
            var todoItems = await _dataService.GetTodoItemsAsync();
            var todoItem = todoItems.FirstOrDefault(t => t.Id == id);
            return _mapper.Map<TodoItemResponseDto?>(todoItem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting todo item with ID {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<TodoItemResponseDto>> GetTodoItemsByUserIdAsync(int userId)
    {
        try
        {
            var todoItems = await _dataService.GetTodoItemsAsync();
            var userTodoItems = todoItems
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.SortOrder)
                .ThenByDescending(t => t.CreatedAt);
            
            return _mapper.Map<IEnumerable<TodoItemResponseDto>>(userTodoItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting todo items for user ID {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<TodoItemResponseDto>> GetTodoItemsByStatusAsync(string status, int userId)
    {
        try
        {
            var todoItems = await _dataService.GetTodoItemsAsync();
            var filteredItems = todoItems
                .Where(t => t.UserId == userId && 
                           t.Status.ToString().Equals(status, StringComparison.OrdinalIgnoreCase))
                .OrderBy(t => t.SortOrder)
                .ThenByDescending(t => t.CreatedAt);
            
            return _mapper.Map<IEnumerable<TodoItemResponseDto>>(filteredItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting todo items by status {Status} for user {UserId}", status, userId);
            throw;
        }
    }

    public async Task<IEnumerable<TodoItemResponseDto>> GetTodoItemsByPriorityAsync(string priority, int userId)
    {
        try
        {
            var todoItems = await _dataService.GetTodoItemsAsync();
            var filteredItems = todoItems
                .Where(t => t.UserId == userId && 
                           t.Priority.ToString().Equals(priority, StringComparison.OrdinalIgnoreCase))
                .OrderBy(t => t.SortOrder)
                .ThenByDescending(t => t.CreatedAt);
            
            return _mapper.Map<IEnumerable<TodoItemResponseDto>>(filteredItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting todo items by priority {Priority} for user {UserId}", priority, userId);
            throw;
        }
    }

    public async Task<IEnumerable<TodoItemResponseDto>> GetTodoItemsByCategoryAsync(string category, int userId)
    {
        try
        {
            var todoItems = await _dataService.GetTodoItemsAsync();
            var filteredItems = todoItems
                .Where(t => t.UserId == userId && 
                           !string.IsNullOrEmpty(t.Category) &&
                           t.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .OrderBy(t => t.SortOrder)
                .ThenByDescending(t => t.CreatedAt);
            
            return _mapper.Map<IEnumerable<TodoItemResponseDto>>(filteredItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting todo items by category {Category} for user {UserId}", category, userId);
            throw;
        }
    }

    public async Task<IEnumerable<TodoItemResponseDto>> GetOverdueTodoItemsAsync(int userId)
    {
        try
        {
            var todoItems = await _dataService.GetTodoItemsAsync();
            var now = DateTime.Now;
            var overdueItems = todoItems
                .Where(t => t.UserId == userId && 
                           t.DueDate.HasValue && 
                           t.DueDate.Value < now && 
                           !t.IsCompleted)
                .OrderBy(t => t.DueDate)
                .ThenBy(t => t.SortOrder);
            
            return _mapper.Map<IEnumerable<TodoItemResponseDto>>(overdueItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting overdue todo items for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<TodoItemResponseDto>> GetDueSoonTodoItemsAsync(int userId, int days = 3)
    {
        try
        {
            var todoItems = await _dataService.GetTodoItemsAsync();
            var now = DateTime.Now;
            var cutoffDate = now.AddDays(days);
            
            var dueSoonItems = todoItems
                .Where(t => t.UserId == userId && 
                           t.DueDate.HasValue && 
                           t.DueDate.Value >= now && 
                           t.DueDate.Value <= cutoffDate && 
                           !t.IsCompleted)
                .OrderBy(t => t.DueDate)
                .ThenBy(t => t.SortOrder);
            
            return _mapper.Map<IEnumerable<TodoItemResponseDto>>(dueSoonItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting due soon todo items for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<TodoItemResponseDto>> GetTodayTodoItemsAsync(int userId)
    {
        try
        {
            var todoItems = await _dataService.GetTodoItemsAsync();
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            
            var todayItems = todoItems
                .Where(t => t.UserId == userId && 
                           t.DueDate.HasValue && 
                           t.DueDate.Value >= today && 
                           t.DueDate.Value < tomorrow)
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.DueDate);
            
            return _mapper.Map<IEnumerable<TodoItemResponseDto>>(todayItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting today's todo items for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<TodoItemResponseDto>> SearchTodoItemsAsync(string keyword, int userId)
    {
        try
        {
            var todoItems = await _dataService.GetTodoItemsAsync();
            var searchResults = todoItems.Where(t =>
                t.UserId == userId &&
                (t.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                 (!string.IsNullOrEmpty(t.Description) && t.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                 (!string.IsNullOrEmpty(t.Category) && t.Category.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            ).OrderBy(t => t.SortOrder)
             .ThenByDescending(t => t.CreatedAt);
            
            return _mapper.Map<IEnumerable<TodoItemResponseDto>>(searchResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching todo items with keyword {Keyword} for user {UserId}", keyword, userId);
            throw;
        }
    }

    public async Task<TodoItemResponseDto> CreateTodoItemAsync(CreateTodoItemDto createTodoItemDto)
    {
        try
        {
            // 驗證到期日期
            if (!ValidateDueDate(createTodoItemDto.DueDate))
            {
                throw new InvalidOperationException("到期日期不能早於今天");
            }

            // 驗證狀態
            if (!string.IsNullOrEmpty(createTodoItemDto.Status) && 
                !_validStatuses.Contains(createTodoItemDto.Status))
            {
                _logger.LogWarning("Invalid status provided: {Status}", createTodoItemDto.Status);
            }

            // 驗證優先級
            if (!string.IsNullOrEmpty(createTodoItemDto.Priority) && 
                !_validPriorities.Contains(createTodoItemDto.Priority))
            {
                _logger.LogWarning("Invalid priority provided: {Priority}", createTodoItemDto.Priority);
            }

            var todoItems = await _dataService.GetTodoItemsAsync();
            var todoItem = _mapper.Map<Models.TodoItem>(createTodoItemDto);
            
            // 生成新ID
            todoItem.Id = todoItems.Any() ? todoItems.Max(t => t.Id) + 1 : 1;
            
            // 設定時間戳記
            todoItem.CreatedAt = DateTime.UtcNow;
            todoItem.UpdatedAt = DateTime.UtcNow;

            todoItems.Add(todoItem);
            await _dataService.SaveTodoItemsAsync(todoItems);

            _logger.LogInformation("Todo item created with ID {Id} for user {UserId}", todoItem.Id, todoItem.UserId);
            return _mapper.Map<TodoItemResponseDto>(todoItem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating todo item");
            throw;
        }
    }

    public async Task<TodoItemResponseDto?> UpdateTodoItemAsync(int id, UpdateTodoItemDto updateTodoItemDto)
    {
        try
        {
            var todoItems = await _dataService.GetTodoItemsAsync();
            var existingItem = todoItems.FirstOrDefault(t => t.Id == id);
            
            if (existingItem == null)
            {
                _logger.LogWarning("Todo item with ID {Id} not found for update", id);
                return null;
            }

            // 驗證到期日期（如果有提供）
            if (updateTodoItemDto.DueDate.HasValue && !ValidateDueDate(updateTodoItemDto.DueDate))
            {
                throw new InvalidOperationException("到期日期不能早於今天");
            }

            // 驗證狀態（如果有提供）
            if (!string.IsNullOrEmpty(updateTodoItemDto.Status) && 
                !_validStatuses.Contains(updateTodoItemDto.Status))
            {
                _logger.LogWarning("Invalid status provided for update: {Status}", updateTodoItemDto.Status);
            }

            // 驗證優先級（如果有提供）
            if (!string.IsNullOrEmpty(updateTodoItemDto.Priority) && 
                !_validPriorities.Contains(updateTodoItemDto.Priority))
            {
                _logger.LogWarning("Invalid priority provided for update: {Priority}", updateTodoItemDto.Priority);
            }

            // 處理完成狀態變更
            if (updateTodoItemDto.IsCompleted.HasValue)
            {
                if (updateTodoItemDto.IsCompleted.Value && !existingItem.IsCompleted)
                {
                    existingItem.CompletedAt = DateTime.UtcNow;
                    existingItem.CompletedDate = DateTime.UtcNow;
                }
                else if (!updateTodoItemDto.IsCompleted.Value && existingItem.IsCompleted)
                {
                    existingItem.CompletedAt = null;
                    existingItem.CompletedDate = null;
                }
            }

            _mapper.Map(updateTodoItemDto, existingItem);
            existingItem.UpdatedAt = DateTime.UtcNow;

            await _dataService.SaveTodoItemsAsync(todoItems);
            _logger.LogInformation("Todo item with ID {Id} updated", id);
            
            return _mapper.Map<TodoItemResponseDto>(existingItem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating todo item with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> MarkTodoItemAsCompletedAsync(int id)
    {
        try
        {
            var todoItems = await _dataService.GetTodoItemsAsync();
            var todoItem = todoItems.FirstOrDefault(t => t.Id == id);
            
            if (todoItem == null)
            {
                _logger.LogWarning("Todo item with ID {Id} not found for completion", id);
                return false;
            }

            if (!todoItem.IsCompleted)
            {
                todoItem.IsCompleted = true;
                todoItem.CompletedAt = DateTime.UtcNow;
                todoItem.CompletedDate = DateTime.UtcNow;
                todoItem.Status = Models.TaskStatus.Completed;
                todoItem.UpdatedAt = DateTime.UtcNow;

                await _dataService.SaveTodoItemsAsync(todoItems);
                _logger.LogInformation("Todo item with ID {Id} marked as completed", id);
            }
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while marking todo item {Id} as completed", id);
            throw;
        }
    }

    public async Task<bool> MarkTodoItemAsIncompleteAsync(int id)
    {
        try
        {
            var todoItems = await _dataService.GetTodoItemsAsync();
            var todoItem = todoItems.FirstOrDefault(t => t.Id == id);
            
            if (todoItem == null)
            {
                _logger.LogWarning("Todo item with ID {Id} not found for incompletion", id);
                return false;
            }

            if (todoItem.IsCompleted)
            {
                todoItem.IsCompleted = false;
                todoItem.CompletedAt = null;
                todoItem.CompletedDate = null;
                todoItem.Status = Models.TaskStatus.Pending;
                todoItem.UpdatedAt = DateTime.UtcNow;

                await _dataService.SaveTodoItemsAsync(todoItems);
                _logger.LogInformation("Todo item with ID {Id} marked as incomplete", id);
            }
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while marking todo item {Id} as incomplete", id);
            throw;
        }
    }

    public async Task<bool> DeleteTodoItemAsync(int id)
    {
        try
        {
            var todoItems = await _dataService.GetTodoItemsAsync();
            var todoItem = todoItems.FirstOrDefault(t => t.Id == id);
            
            if (todoItem == null)
            {
                _logger.LogWarning("Todo item with ID {Id} not found for deletion", id);
                return false;
            }

            todoItems.Remove(todoItem);
            await _dataService.SaveTodoItemsAsync(todoItems);
            _logger.LogInformation("Todo item with ID {Id} deleted", id);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting todo item with ID {Id}", id);
            throw;
        }
    }

    public async Task<int> BatchUpdateStatusAsync(List<int> todoItemIds, string status)
    {
        try
        {
            if (!_validStatuses.Contains(status))
            {
                throw new InvalidOperationException($"無效的狀態: {status}");
            }

            var todoItems = await _dataService.GetTodoItemsAsync();
            var itemsToUpdate = todoItems.Where(t => todoItemIds.Contains(t.Id)).ToList();
            
            var updatedCount = 0;
            foreach (var item in itemsToUpdate)
            {
                if (Enum.TryParse<Models.TaskStatus>(status, true, out var taskStatus))
                {
                    item.Status = taskStatus;
                    item.UpdatedAt = DateTime.UtcNow;
                    
                    // 處理完成狀態
                    if (taskStatus == Models.TaskStatus.Completed && !item.IsCompleted)
                    {
                        item.IsCompleted = true;
                        item.CompletedAt = DateTime.UtcNow;
                        item.CompletedDate = DateTime.UtcNow;
                    }
                    else if (taskStatus != Models.TaskStatus.Completed && item.IsCompleted)
                    {
                        item.IsCompleted = false;
                        item.CompletedAt = null;
                        item.CompletedDate = null;
                    }
                    
                    updatedCount++;
                }
            }

            if (updatedCount > 0)
            {
                await _dataService.SaveTodoItemsAsync(todoItems);
                _logger.LogInformation("Batch updated {Count} todo items to status {Status}", updatedCount, status);
            }
            
            return updatedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while batch updating todo items status");
            throw;
        }
    }

    public async Task<int> BatchDeleteTodoItemsAsync(List<int> todoItemIds)
    {
        try
        {
            var todoItems = await _dataService.GetTodoItemsAsync();
            var itemsToDelete = todoItems.Where(t => todoItemIds.Contains(t.Id)).ToList();
            
            foreach (var item in itemsToDelete)
            {
                todoItems.Remove(item);
            }

            if (itemsToDelete.Count > 0)
            {
                await _dataService.SaveTodoItemsAsync(todoItems);
                _logger.LogInformation("Batch deleted {Count} todo items", itemsToDelete.Count);
            }
            
            return itemsToDelete.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while batch deleting todo items");
            throw;
        }
    }

    public async Task<object> GetTodoItemStatsAsync(int userId)
    {
        try
        {
            var todoItems = await _dataService.GetTodoItemsAsync();
            var userTodoItems = todoItems.Where(t => t.UserId == userId);
            var now = DateTime.Now;
            var today = DateTime.Today;
            var thisWeek = today.AddDays(-(int)today.DayOfWeek);
            var thisMonth = new DateTime(now.Year, now.Month, 1);

            var stats = new
            {
                TotalItems = userTodoItems.Count(),
                CompletedItems = userTodoItems.Count(t => t.IsCompleted),
                PendingItems = userTodoItems.Count(t => !t.IsCompleted),
                
                OverdueItems = userTodoItems.Count(t => 
                    t.DueDate.HasValue && t.DueDate.Value < now && !t.IsCompleted),
                DueTodayItems = userTodoItems.Count(t => 
                    t.DueDate.HasValue && t.DueDate.Value.Date == today && !t.IsCompleted),
                DueThisWeekItems = userTodoItems.Count(t => 
                    t.DueDate.HasValue && t.DueDate.Value >= thisWeek && t.DueDate.Value < thisWeek.AddDays(7) && !t.IsCompleted),
                
                CompletionRate = userTodoItems.Any() ? 
                    Math.Round((double)userTodoItems.Count(t => t.IsCompleted) / userTodoItems.Count() * 100, 2) : 0,
                
                ItemsByStatus = userTodoItems
                    .GroupBy(t => t.Status.ToString())
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count),
                
                ItemsByPriority = userTodoItems
                    .GroupBy(t => t.Priority.ToString())
                    .Select(g => new { Priority = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count),
                
                ItemsByCategory = userTodoItems
                    .Where(t => !string.IsNullOrEmpty(t.Category))
                    .GroupBy(t => t.Category)
                    .Select(g => new { Category = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10),
                
                CreatedThisWeek = userTodoItems.Count(t => t.CreatedAt >= thisWeek),
                CreatedThisMonth = userTodoItems.Count(t => t.CreatedAt >= thisMonth),
                CompletedThisWeek = userTodoItems.Count(t => 
                    t.CompletedAt.HasValue && t.CompletedAt.Value >= thisWeek),
                CompletedThisMonth = userTodoItems.Count(t => 
                    t.CompletedAt.HasValue && t.CompletedAt.Value >= thisMonth),
                
                AverageCompletionTime = userTodoItems
                    .Where(t => t.IsCompleted && t.CompletedAt.HasValue)
                    .Select(t => (t.CompletedAt!.Value - t.CreatedAt).TotalDays)
                    .DefaultIfEmpty(0)
                    .Average(),
                
                UniqueCategories = userTodoItems
                    .Where(t => !string.IsNullOrEmpty(t.Category))
                    .Select(t => t.Category)
                    .Distinct()
                    .Count()
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting todo item statistics for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync(int userId)
    {
        try
        {
            var todoItems = await _dataService.GetTodoItemsAsync();
            var categories = todoItems
                .Where(t => t.UserId == userId && !string.IsNullOrEmpty(t.Category))
                .Select(t => t.Category!)
                .Distinct()
                .OrderBy(c => c);
            
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting categories for user {UserId}", userId);
            throw;
        }
    }

    public bool ValidateDueDate(DateTime? dueDate)
    {
        try
        {
            // 允許空值
            if (!dueDate.HasValue)
                return true;

            // 到期日期不能早於今天
            if (dueDate.Value.Date < DateTime.Today)
                return false;

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while validating due date");
            return false;
        }
    }
}