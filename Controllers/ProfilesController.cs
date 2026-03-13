using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Services;

namespace PersonalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfilesController : BaseApiController
{
    private readonly IProfileService _service;
    private readonly IUserService _userService;
    public ProfilesController(IProfileService service, IUserService userService)
    {
        _service = service;
        _userService = userService;
    }

    [HttpGet("directory")]
    public async Task<IActionResult> GetDirectory()
    {
        var profiles = await _service.GetAllAsync();
        var users = await _userService.GetAllAsync();
        var userMap = users.ToDictionary(u => u.Id);

        var result = profiles
            .Where(p => userMap.ContainsKey(p.UserId))
            .Select(p => new ProfileDirectoryDto
            {
                UserId = p.UserId,
                Username = userMap[p.UserId].Username,
                FullName = userMap[p.UserId].FullName,
                Title = p.Title,
                Summary = p.Summary,
                ProfileImageUrl = p.ProfileImageUrl,
                Location = p.Location,
                ThemeColor = p.ThemeColor
            }).ToList();

        return Ok(ApiResponse<List<ProfileDirectoryDto>>.Ok(result));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(ApiResponse<List<ProfileResponse>>.Ok(await _service.GetAllAsync()));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item != null ? Ok(ApiResponse<ProfileResponse>.Ok(item)) : NotFound(ApiResponse.Fail("Profile not found"));
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        var item = await _service.GetByUserIdAsync(userId);
        return item != null ? Ok(ApiResponse<ProfileResponse>.Ok(item)) : NotFound(ApiResponse.Fail("Profile not found"));
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProfileDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized(ApiResponse.Fail("Unauthorized"));
        dto.UserId = currentUserId.Value;
        return Ok(ApiResponse<ProfileResponse>.Ok(await _service.CreateAsync(dto), "Profile created"));
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProfileDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized(ApiResponse.Fail("Unauthorized"));
        var existing = await _service.GetByIdAsync(id);
        if (existing == null) return NotFound(ApiResponse.Fail("Profile not found"));
        if (existing.UserId != currentUserId.Value) return StatusCode(403, ApiResponse.Fail("Forbidden"));
        var item = await _service.UpdateAsync(id, dto);
        return item != null ? Ok(ApiResponse<ProfileResponse>.Ok(item, "Profile updated")) : NotFound(ApiResponse.Fail("Profile not found"));
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized(ApiResponse.Fail("Unauthorized"));
        var existing = await _service.GetByIdAsync(id);
        if (existing == null) return NotFound(ApiResponse.Fail("Profile not found"));
        if (existing.UserId != currentUserId.Value) return StatusCode(403, ApiResponse.Fail("Forbidden"));
        return await _service.DeleteAsync(id) ? Ok(ApiResponse.Ok("Profile deleted")) : NotFound(ApiResponse.Fail("Profile not found"));
    }
}
