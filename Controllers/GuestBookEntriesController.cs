using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Services;

namespace PersonalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GuestBookEntriesController : ControllerBase
{
    private readonly IGuestBookEntryService _service;
    public GuestBookEntriesController(IGuestBookEntryService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetApproved()
        => Ok(ApiResponse<List<GuestBookEntryResponse>>.Ok(await _service.GetApprovedAsync()));

    [Authorize]
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
        => Ok(ApiResponse<List<GuestBookEntryResponse>>.Ok(await _service.GetAllAsync()));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item != null ? Ok(ApiResponse<GuestBookEntryResponse>.Ok(item)) : NotFound(ApiResponse.Fail("Entry not found"));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGuestBookEntryDto dto)
        => Ok(ApiResponse<GuestBookEntryResponse>.Ok(await _service.CreateAsync(dto), "Message submitted"));

    [Authorize] [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateGuestBookEntryDto dto)
    {
        var item = await _service.UpdateAsync(id, dto);
        return item != null ? Ok(ApiResponse<GuestBookEntryResponse>.Ok(item, "Entry updated")) : NotFound(ApiResponse.Fail("Entry not found"));
    }

    [Authorize] [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => await _service.DeleteAsync(id) ? Ok(ApiResponse.Ok("Entry deleted")) : NotFound(ApiResponse.Fail("Entry not found"));
}
