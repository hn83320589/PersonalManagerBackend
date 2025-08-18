using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services;

namespace PersonalManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoItemsController : BaseController
{
    private readonly JsonDataService _dataService;

    public TodoItemsController(JsonDataService dataService)
    {
        _dataService = dataService;
    }

    /// <summary>
    /// 取得所有待辦事項
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTodoItems([FromQuery] Models.TaskStatus? status = null, [FromQuery] TodoPriority? priority = null)
    {
        try
        {
            var todoItems = await _dataService.GetTodoItemsAsync();
            var query = todoItems.AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            if (priority.HasValue)
            {
                query = query.Where(t => t.Priority == priority.Value);
            }

            var filteredItems = query
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.DueDate)
                .ToList();
                
            return Ok(ApiResponse<List<TodoItem>>.SuccessResult(filteredItems, "成功取得待辦事項列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<TodoItem>>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 取得指定待辦事項
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTodoItem(int id)
    {
        try
        {
            var todoItems = await _dataService.GetTodoItemsAsync();
            var todoItem = todoItems.FirstOrDefault(t => t.Id == id);
            
            if (todoItem == null)
            {
                return NotFound(ApiResponse<TodoItem>.ErrorResult("找不到指定的待辦事項"));
            }

            return Ok(ApiResponse<TodoItem>.SuccessResult(todoItem, "成功取得待辦事項資料"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<TodoItem>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 取得指定使用者的待辦事項
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetTodoItemsByUser(int userId, [FromQuery] Models.TaskStatus? status = null)
    {
        try
        {
            var todoItems = await _dataService.GetTodoItemsAsync();
            var query = todoItems.Where(t => t.UserId == userId);

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            var userTodoItems = query
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.DueDate)
                .ToList();
                
            return Ok(ApiResponse<List<TodoItem>>.SuccessResult(userTodoItems, "成功取得使用者待辦事項列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<TodoItem>>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 取得今日到期的待辦事項
    /// </summary>
    [HttpGet("due-today")]
    public async Task<IActionResult> GetDueTodayItems()
    {
        try
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            
            var todoItems = await _dataService.GetTodoItemsAsync();
            var dueTodayItems = todoItems
                .Where(t => t.Status != Models.TaskStatus.Completed && 
                           t.DueDate.HasValue && 
                           t.DueDate.Value >= today && 
                           t.DueDate.Value < tomorrow)
                .OrderByDescending(t => t.Priority)
                .ToList();
                
            return Ok(ApiResponse<List<TodoItem>>.SuccessResult(dueTodayItems, "成功取得今日到期待辦事項"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<TodoItem>>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 取得逾期的待辦事項
    /// </summary>
    [HttpGet("overdue")]
    public async Task<IActionResult> GetOverdueItems()
    {
        try
        {
            var today = DateTime.Today;
            
            var todoItems = await _dataService.GetTodoItemsAsync();
            var overdueItems = todoItems
                .Where(t => t.Status != Models.TaskStatus.Completed && 
                           t.DueDate.HasValue && 
                           t.DueDate.Value < today)
                .OrderBy(t => t.DueDate)
                .ToList();
                
            return Ok(ApiResponse<List<TodoItem>>.SuccessResult(overdueItems, "成功取得逾期待辦事項"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<TodoItem>>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 建立待辦事項
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTodoItem([FromBody] TodoItem todoItem)
    {
        try
        {
            var todoItems = await _dataService.GetTodoItemsAsync();
            
            todoItem.Id = todoItems.Count > 0 ? todoItems.Max(t => t.Id) + 1 : 1;
            todoItem.Status = Models.TaskStatus.Pending;
            todoItem.CreatedAt = DateTime.UtcNow;
            todoItem.UpdatedAt = DateTime.UtcNow;
            
            todoItems.Add(todoItem);
            await _dataService.SaveTodoItemsAsync(todoItems);

            return Ok(ApiResponse<TodoItem>.SuccessResult(todoItem, "待辦事項建立成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<TodoItem>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 更新待辦事項
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTodoItem(int id, [FromBody] TodoItem updatedItem)
    {
        try
        {
            var todoItems = await _dataService.GetTodoItemsAsync();
            var todoItem = todoItems.FirstOrDefault(t => t.Id == id);
            
            if (todoItem == null)
            {
                return NotFound(ApiResponse<TodoItem>.ErrorResult("找不到指定的待辦事項"));
            }

            todoItem.Title = updatedItem.Title;
            todoItem.Description = updatedItem.Description;
            todoItem.Priority = updatedItem.Priority;
            todoItem.Status = updatedItem.Status;
            todoItem.DueDate = updatedItem.DueDate;
            todoItem.Category = updatedItem.Category;
            todoItem.UpdatedAt = DateTime.UtcNow;

            if (updatedItem.Status == Models.TaskStatus.Completed && todoItem.CompletedAt == null)
            {
                todoItem.CompletedAt = DateTime.UtcNow;
            }

            await _dataService.SaveTodoItemsAsync(todoItems);

            return Ok(ApiResponse<TodoItem>.SuccessResult(todoItem, "待辦事項更新成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<TodoItem>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 標記待辦事項為完成
    /// </summary>
    [HttpPut("{id}/complete")]
    public async Task<IActionResult> CompleteTodoItem(int id)
    {
        try
        {
            var todoItems = await _dataService.GetTodoItemsAsync();
            var todoItem = todoItems.FirstOrDefault(t => t.Id == id);
            
            if (todoItem == null)
            {
                return NotFound(ApiResponse<TodoItem>.ErrorResult("找不到指定的待辦事項"));
            }

            todoItem.Status = Models.TaskStatus.Completed;
            todoItem.CompletedAt = DateTime.UtcNow;
            todoItem.UpdatedAt = DateTime.UtcNow;

            await _dataService.SaveTodoItemsAsync(todoItems);

            return Ok(ApiResponse<TodoItem>.SuccessResult(todoItem, "待辦事項已標記為完成"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<TodoItem>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 刪除待辦事項
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTodoItem(int id)
    {
        try
        {
            var todoItems = await _dataService.GetTodoItemsAsync();
            var todoItem = todoItems.FirstOrDefault(t => t.Id == id);
            
            if (todoItem == null)
            {
                return NotFound(ApiResponse.ErrorResult("找不到指定的待辦事項"));
            }

            todoItems.Remove(todoItem);
            await _dataService.SaveTodoItemsAsync(todoItems);

            return Ok(ApiResponse.SuccessResult("待辦事項刪除成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }
}