using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Services;

namespace PersonalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GuestBookEntriesController : BaseApiController
{
    private readonly IGuestBookEntryService _service;
    public GuestBookEntriesController(IGuestBookEntryService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetApproved()
        => Ok(ApiResponse<List<GuestBookEntryResponse>>.Ok(await _service.GetApprovedAsync()));

    [HttpGet("user/{targetUserId}")]
    public async Task<IActionResult> GetApprovedByUser(int targetUserId)
        => Ok(ApiResponse<List<GuestBookEntryResponse>>.Ok(await _service.GetApprovedByTargetUserIdAsync(targetUserId)));

    [Authorize]
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized(ApiResponse.Fail("Unauthorized"));
        var all = await _service.GetAllAsync();
        var entries = all.Where(e => e.TargetUserId == currentUserId.Value).ToList();
        return Ok(ApiResponse<List<GuestBookEntryResponse>>.Ok(entries));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item != null ? Ok(ApiResponse<GuestBookEntryResponse>.Ok(item)) : NotFound(ApiResponse.Fail("Entry not found"));
    }

    [EnableRateLimiting("public_write")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGuestBookEntryDto dto)
        => Ok(ApiResponse<GuestBookEntryResponse>.Ok(await _service.CreateAsync(dto), "Message submitted"));

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateGuestBookEntryDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized(ApiResponse.Fail("Unauthorized"));
        var existing = await _service.GetByIdAsync(id);
        if (existing == null) return NotFound(ApiResponse.Fail("Entry not found"));
        if (existing.TargetUserId != currentUserId.Value) return StatusCode(403, ApiResponse.Fail("Forbidden"));
        var item = await _service.UpdateAsync(id, dto);
        return item != null ? Ok(ApiResponse<GuestBookEntryResponse>.Ok(item, "Entry updated")) : NotFound(ApiResponse.Fail("Entry not found"));
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized(ApiResponse.Fail("Unauthorized"));
        var existing = await _service.GetByIdAsync(id);
        if (existing == null) return NotFound(ApiResponse.Fail("Entry not found"));
        if (existing.TargetUserId != currentUserId.Value) return StatusCode(403, ApiResponse.Fail("Forbidden"));
        return await _service.DeleteAsync(id) ? Ok(ApiResponse.Ok("Entry deleted")) : NotFound(ApiResponse.Fail("Entry not found"));
    }
}
