using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.DTOs.User;
using PersonalManagerAPI.Services.Interfaces;

namespace PersonalManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : BaseController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserResponseDto>>>> GetUsers([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var users = await _userService.GetAllUsersAsync(skip, take);
        return Ok(ApiResponse<IEnumerable<UserResponseDto>>.SuccessResult(users, "成功取得使用者列表"));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetUser(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        
        if (user == null)
        {
            return NotFound(ApiResponse<UserResponseDto>.ErrorResult("找不到指定的使用者"));
        }

        return Ok(ApiResponse<UserResponseDto>.SuccessResult(user, "成功取得使用者資料"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
            return BadRequest(ApiResponse<UserResponseDto>.ErrorResult("資料驗證失敗", errors));
        }

        var user = await _userService.CreateUserAsync(createUserDto);

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, 
            ApiResponse<UserResponseDto>.SuccessResult(user, "使用者建立成功"));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
            return BadRequest(ApiResponse<UserResponseDto>.ErrorResult("資料驗證失敗", errors));
        }

        var user = await _userService.UpdateUserAsync(id, updateUserDto);
        
        if (user == null)
        {
            return NotFound(ApiResponse<UserResponseDto>.ErrorResult("找不到指定的使用者"));
        }

        return Ok(ApiResponse<UserResponseDto>.SuccessResult(user, "使用者更新成功"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteUser(int id)
    {
        var result = await _userService.DeleteUserAsync(id);
        
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResult("找不到指定的使用者"));
        }

        return Ok(ApiResponse.SuccessResult("使用者刪除成功"));
    }

    [HttpGet("username/{username}")]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetUserByUsername(string username)
    {
        var user = await _userService.GetUserByUsernameAsync(username);
        
        if (user == null)
        {
            return NotFound(ApiResponse<UserResponseDto>.ErrorResult("找不到指定的使用者"));
        }

        return Ok(ApiResponse<UserResponseDto>.SuccessResult(user, "成功取得使用者資料"));
    }

    [HttpGet("email/{email}")]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetUserByEmail(string email)
    {
        var user = await _userService.GetUserByEmailAsync(email);
        
        if (user == null)
        {
            return NotFound(ApiResponse<UserResponseDto>.ErrorResult("找不到指定的使用者"));
        }

        return Ok(ApiResponse<UserResponseDto>.SuccessResult(user, "成功取得使用者資料"));
    }

    [HttpPost("{id}/change-password")]
    public async Task<ActionResult<ApiResponse>> ChangePassword(int id, [FromBody] ChangePasswordDto changePasswordDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
            return BadRequest(ApiResponse.ErrorResult("資料驗證失敗", errors));
        }

        var result = await _userService.ChangePasswordAsync(id, changePasswordDto);
        
        if (!result)
        {
            return BadRequest(ApiResponse.ErrorResult("密碼變更失敗，請檢查舊密碼是否正確"));
        }

        return Ok(ApiResponse.SuccessResult("密碼變更成功"));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<object>>> GetUserStats()
    {
        var stats = await _userService.GetUserStatsAsync();
        return Ok(ApiResponse<object>.SuccessResult(stats, "成功取得使用者統計資料"));
    }
}