using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Models;
using PersonalManager.Api.Services;

namespace PersonalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TodoItemsController : ControllerBase
{
    private readonly ITodoItemService _service;
    public TodoItemsController(ITodoItemService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(ApiResponse<List<TodoItemResponse>>.Ok(await _service.GetAllAsync()));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item != null ? Ok(ApiResponse<TodoItemResponse>.Ok(item)) : NotFound(ApiResponse.Fail("Todo item not found"));
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(int userId)
        => Ok(ApiResponse<List<TodoItemResponse>>.Ok(await _service.GetByUserIdAsync(userId)));

    [HttpGet("user/{userId}/status/{status}")]
    public async Task<IActionResult> GetByStatus(int userId, TodoStatus status)
        => Ok(ApiResponse<List<TodoItemResponse>>.Ok(await _service.GetByStatusAsync(userId, status)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTodoItemDto dto)
        => Ok(ApiResponse<TodoItemResponse>.Ok(await _service.CreateAsync(dto), "Todo created"));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTodoItemDto dto)
    {
        var item = await _service.UpdateAsync(id, dto);
        return item != null ? Ok(ApiResponse<TodoItemResponse>.Ok(item, "Todo updated")) : NotFound(ApiResponse.Fail("Todo not found"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => await _service.DeleteAsync(id) ? Ok(ApiResponse.Ok("Todo deleted")) : NotFound(ApiResponse.Fail("Todo not found"));
}
