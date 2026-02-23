using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Services;

namespace PersonalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;
    public UsersController(IUserService service) => _service = service;

    [AllowAnonymous]
    [HttpGet("public")]
    public async Task<IActionResult> GetAllPublic()
    {
        var users = await _service.GetAllAsync();
        var result = users.Where(u => u.IsActive).Select(u => new PublicUserDto
        {
            Id = u.Id,
            Username = u.Username,
            FullName = u.FullName
        }).ToList();
        return Ok(ApiResponse<List<PublicUserDto>>.Ok(result));
    }

    [AllowAnonymous]
    [HttpGet("public/{username}")]
    public async Task<IActionResult> GetPublicByUsername(string username)
    {
        var users = await _service.GetAllAsync();
        var user = users.FirstOrDefault(u => u.Username == username && u.IsActive);
        if (user == null) return NotFound(ApiResponse.Fail("User not found"));
        return Ok(ApiResponse<PublicUserDto>.Ok(new PublicUserDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName
        }));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(ApiResponse<List<UserResponse>>.Ok(await _service.GetAllAsync()));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item != null ? Ok(ApiResponse<UserResponse>.Ok(item)) : NotFound(ApiResponse.Fail("User not found"));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        => Ok(ApiResponse<UserResponse>.Ok(await _service.CreateAsync(dto), "User created"));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        var item = await _service.UpdateAsync(id, dto);
        return item != null ? Ok(ApiResponse<UserResponse>.Ok(item, "User updated")) : NotFound(ApiResponse.Fail("User not found"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => await _service.DeleteAsync(id) ? Ok(ApiResponse.Ok("User deleted")) : NotFound(ApiResponse.Fail("User not found"));
}
