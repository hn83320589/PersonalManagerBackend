using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.DTOs.TodoItem;
using PersonalManagerAPI.Services.Interfaces;

namespace PersonalManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoItemsController : BaseController
{
    private readonly ITodoItemService _todoItemService;

    public TodoItemsController(ITodoItemService todoItemService)
    {
        _todoItemService = todoItemService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<TodoItemResponseDto>>>> GetTodoItems([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var todoItems = await _todoItemService.GetAllTodoItemsAsync(skip, take);
        return Ok(ApiResponse<IEnumerable<TodoItemResponseDto>>.SuccessResult(todoItems, "成功取得待辦事項列表"));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TodoItemResponseDto>>> GetTodoItem(int id)
    {
        var todoItem = await _todoItemService.GetTodoItemByIdAsync(id);
        
        if (todoItem == null)
        {
            return NotFound(ApiResponse<TodoItemResponseDto>.ErrorResult("找不到指定的待辦事項"));
        }

        return Ok(ApiResponse<TodoItemResponseDto>.SuccessResult(todoItem, "成功取得待辦事項資料"));
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TodoItemResponseDto>>>> GetTodoItemsByUserId(int userId)
    {
        var todoItems = await _todoItemService.GetTodoItemsByUserIdAsync(userId);
        return Ok(ApiResponse<IEnumerable<TodoItemResponseDto>>.SuccessResult(todoItems, "成功取得使用者待辦事項列表"));
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TodoItemResponseDto>>>> GetTodoItemsByStatus(string status, [FromQuery] int userId)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return BadRequest(ApiResponse<IEnumerable<TodoItemResponseDto>>.ErrorResult("狀態不能為空"));
        }

        var todoItems = await _todoItemService.GetTodoItemsByStatusAsync(status, userId);
        return Ok(ApiResponse<IEnumerable<TodoItemResponseDto>>.SuccessResult(todoItems, $"成功取得{status}狀態的待辦事項"));
    }

    [HttpGet("priority/{priority}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TodoItemResponseDto>>>> GetTodoItemsByPriority(string priority, [FromQuery] int userId)
    {
        if (string.IsNullOrWhiteSpace(priority))
        {
            return BadRequest(ApiResponse<IEnumerable<TodoItemResponseDto>>.ErrorResult("優先級不能為空"));
        }

        var todoItems = await _todoItemService.GetTodoItemsByPriorityAsync(priority, userId);
        return Ok(ApiResponse<IEnumerable<TodoItemResponseDto>>.SuccessResult(todoItems, $"成功取得{priority}優先級的待辦事項"));
    }

    [HttpGet("category/{category}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TodoItemResponseDto>>>> GetTodoItemsByCategory(string category, [FromQuery] int userId)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return BadRequest(ApiResponse<IEnumerable<TodoItemResponseDto>>.ErrorResult("分類不能為空"));
        }

        var todoItems = await _todoItemService.GetTodoItemsByCategoryAsync(category, userId);
        return Ok(ApiResponse<IEnumerable<TodoItemResponseDto>>.SuccessResult(todoItems, $"成功取得{category}分類的待辦事項"));
    }

    [HttpGet("overdue")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TodoItemResponseDto>>>> GetOverdueTodoItems([FromQuery] int userId)
    {
        var todoItems = await _todoItemService.GetOverdueTodoItemsAsync(userId);
        return Ok(ApiResponse<IEnumerable<TodoItemResponseDto>>.SuccessResult(todoItems, "成功取得逾期待辦事項"));
    }

    [HttpGet("due-soon")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TodoItemResponseDto>>>> GetDueSoonTodoItems([FromQuery] int userId, [FromQuery] int days = 3)
    {
        var todoItems = await _todoItemService.GetDueSoonTodoItemsAsync(userId, days);
        return Ok(ApiResponse<IEnumerable<TodoItemResponseDto>>.SuccessResult(todoItems, $"成功取得未來{days}天到期的待辦事項"));
    }

    [HttpGet("today")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TodoItemResponseDto>>>> GetTodayTodoItems([FromQuery] int userId)
    {
        var todoItems = await _todoItemService.GetTodayTodoItemsAsync(userId);
        return Ok(ApiResponse<IEnumerable<TodoItemResponseDto>>.SuccessResult(todoItems, "成功取得今日待辦事項"));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TodoItemResponseDto>>>> SearchTodoItems([FromQuery] string keyword, [FromQuery] int userId)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return BadRequest(ApiResponse<IEnumerable<TodoItemResponseDto>>.ErrorResult("搜尋關鍵字不能為空"));
        }

        var todoItems = await _todoItemService.SearchTodoItemsAsync(keyword, userId);
        return Ok(ApiResponse<IEnumerable<TodoItemResponseDto>>.SuccessResult(todoItems, "搜尋完成"));
    }

    [HttpGet("categories")]
    public async Task<ActionResult<ApiResponse<IEnumerable<string>>>> GetCategories([FromQuery] int userId)
    {
        var categories = await _todoItemService.GetCategoriesAsync(userId);
        return Ok(ApiResponse<IEnumerable<string>>.SuccessResult(categories, "成功取得分類列表"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TodoItemResponseDto>>> CreateTodoItem([FromBody] CreateTodoItemDto createTodoItemDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
            return BadRequest(ApiResponse<TodoItemResponseDto>.ErrorResult("資料驗證失敗", errors));
        }

        var todoItem = await _todoItemService.CreateTodoItemAsync(createTodoItemDto);

        return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, 
            ApiResponse<TodoItemResponseDto>.SuccessResult(todoItem, "待辦事項建立成功"));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<TodoItemResponseDto>>> UpdateTodoItem(int id, [FromBody] UpdateTodoItemDto updateTodoItemDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
            return BadRequest(ApiResponse<TodoItemResponseDto>.ErrorResult("資料驗證失敗", errors));
        }

        var todoItem = await _todoItemService.UpdateTodoItemAsync(id, updateTodoItemDto);
        
        if (todoItem == null)
        {
            return NotFound(ApiResponse<TodoItemResponseDto>.ErrorResult("找不到指定的待辦事項"));
        }

        return Ok(ApiResponse<TodoItemResponseDto>.SuccessResult(todoItem, "待辦事項更新成功"));
    }

    [HttpPatch("{id}/complete")]
    public async Task<ActionResult<ApiResponse>> MarkTodoItemAsCompleted(int id)
    {
        var result = await _todoItemService.MarkTodoItemAsCompletedAsync(id);
        
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResult("找不到指定的待辦事項"));
        }

        return Ok(ApiResponse.SuccessResult("待辦事項標記為完成"));
    }

    [HttpPatch("{id}/incomplete")]
    public async Task<ActionResult<ApiResponse>> MarkTodoItemAsIncomplete(int id)
    {
        var result = await _todoItemService.MarkTodoItemAsIncompleteAsync(id);
        
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResult("找不到指定的待辦事項"));
        }

        return Ok(ApiResponse.SuccessResult("待辦事項標記為未完成"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteTodoItem(int id)
    {
        var result = await _todoItemService.DeleteTodoItemAsync(id);
        
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResult("找不到指定的待辦事項"));
        }

        return Ok(ApiResponse.SuccessResult("待辦事項刪除成功"));
    }

    [HttpPatch("batch/status")]
    public async Task<ActionResult<ApiResponse<int>>> BatchUpdateStatus([FromBody] BatchUpdateStatusRequest request)
    {
        if (request.TodoItemIds == null || !request.TodoItemIds.Any())
        {
            return BadRequest(ApiResponse<int>.ErrorResult("待辦事項ID列表不能為空"));
        }

        if (string.IsNullOrWhiteSpace(request.Status))
        {
            return BadRequest(ApiResponse<int>.ErrorResult("狀態不能為空"));
        }

        var updatedCount = await _todoItemService.BatchUpdateStatusAsync(request.TodoItemIds, request.Status);
        return Ok(ApiResponse<int>.SuccessResult(updatedCount, $"批量更新了{updatedCount}個待辦事項的狀態"));
    }

    [HttpDelete("batch")]
    public async Task<ActionResult<ApiResponse<int>>> BatchDeleteTodoItems([FromBody] BatchDeleteRequest request)
    {
        if (request.TodoItemIds == null || !request.TodoItemIds.Any())
        {
            return BadRequest(ApiResponse<int>.ErrorResult("待辦事項ID列表不能為空"));
        }

        var deletedCount = await _todoItemService.BatchDeleteTodoItemsAsync(request.TodoItemIds);
        return Ok(ApiResponse<int>.SuccessResult(deletedCount, $"批量刪除了{deletedCount}個待辦事項"));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<object>>> GetTodoItemStats([FromQuery] int userId)
    {
        var stats = await _todoItemService.GetTodoItemStatsAsync(userId);
        return Ok(ApiResponse<object>.SuccessResult(stats, "成功取得待辦事項統計"));
    }
}

// 批量操作的請求模型
public class BatchUpdateStatusRequest
{
    public List<int> TodoItemIds { get; set; } = new();
    public string Status { get; set; } = string.Empty;
}

public class BatchDeleteRequest
{
    public List<int> TodoItemIds { get; set; } = new();
}