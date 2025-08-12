using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services;

namespace PersonalManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : BaseController
{
    private readonly JsonDataService _dataService;

    public UsersController(JsonDataService dataService)
    {
        _dataService = dataService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<User>>>> GetUsers()
    {
        try
        {
            var users = await _dataService.GetUsersAsync();
            return Ok(ApiResponse<List<User>>.SuccessResult(users, "成功取得使用者列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<User>>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<User>>> GetUser(int id)
    {
        try
        {
            var user = await _dataService.GetByIdAsync<User>("users.json", id);
            
            if (user == null)
            {
                return NotFound(ApiResponse<User>.ErrorResult("找不到指定的使用者"));
            }

            return Ok(ApiResponse<User>.SuccessResult(user, "成功取得使用者資料"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<User>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<User>>> CreateUser([FromBody] User user)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                            .Select(e => e.ErrorMessage)
                                            .ToList();
                return BadRequest(ApiResponse<User>.ErrorResult("資料驗證失敗", errors));
            }

            var users = await _dataService.GetUsersAsync();
            
            // Check if username or email already exists
            if (users.Any(u => u.Username == user.Username))
            {
                return BadRequest(ApiResponse<User>.ErrorResult("使用者名稱已存在"));
            }
            
            if (users.Any(u => u.Email == user.Email))
            {
                return BadRequest(ApiResponse<User>.ErrorResult("電子郵件已存在"));
            }

            // Generate new ID
            user.Id = users.Any() ? users.Max(u => u.Id) + 1 : 1;
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            
            users.Add(user);
            await _dataService.SaveUsersAsync(users);

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, 
                ApiResponse<User>.SuccessResult(user, "使用者建立成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<User>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<User>>> UpdateUser(int id, [FromBody] User user)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                            .Select(e => e.ErrorMessage)
                                            .ToList();
                return BadRequest(ApiResponse<User>.ErrorResult("資料驗證失敗", errors));
            }

            var users = await _dataService.GetUsersAsync();
            var existingUser = users.FirstOrDefault(u => u.Id == id);
            
            if (existingUser == null)
            {
                return NotFound(ApiResponse<User>.ErrorResult("找不到指定的使用者"));
            }

            // Check for duplicate username/email (excluding current user)
            if (users.Any(u => u.Id != id && u.Username == user.Username))
            {
                return BadRequest(ApiResponse<User>.ErrorResult("使用者名稱已存在"));
            }
            
            if (users.Any(u => u.Id != id && u.Email == user.Email))
            {
                return BadRequest(ApiResponse<User>.ErrorResult("電子郵件已存在"));
            }

            // Update user properties
            existingUser.Username = user.Username;
            existingUser.Email = user.Email;
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.Phone = user.Phone;
            existingUser.IsActive = user.IsActive;
            existingUser.UpdatedAt = DateTime.UtcNow;

            await _dataService.SaveUsersAsync(users);

            return Ok(ApiResponse<User>.SuccessResult(existingUser, "使用者更新成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<User>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteUser(int id)
    {
        try
        {
            var users = await _dataService.GetUsersAsync();
            var user = users.FirstOrDefault(u => u.Id == id);
            
            if (user == null)
            {
                return NotFound(ApiResponse.ErrorResult("找不到指定的使用者"));
            }

            users.Remove(user);
            await _dataService.SaveUsersAsync(users);

            return Ok(ApiResponse.SuccessResult("使用者刪除成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }
}