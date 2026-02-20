using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Services;

namespace PersonalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactMethodsController : ControllerBase
{
    private readonly IContactMethodService _service;
    public ContactMethodsController(IContactMethodService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(ApiResponse<List<ContactMethodResponse>>.Ok(await _service.GetAllAsync()));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item != null ? Ok(ApiResponse<ContactMethodResponse>.Ok(item)) : NotFound(ApiResponse.Fail("Contact method not found"));
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(int userId)
        => Ok(ApiResponse<List<ContactMethodResponse>>.Ok(await _service.GetByUserIdAsync(userId)));

    [HttpGet("user/{userId}/public")]
    public async Task<IActionResult> GetPublicByUserId(int userId)
        => Ok(ApiResponse<List<ContactMethodResponse>>.Ok(await _service.GetPublicByUserIdAsync(userId)));

    [Authorize] [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateContactMethodDto dto)
        => Ok(ApiResponse<ContactMethodResponse>.Ok(await _service.CreateAsync(dto), "Contact method created"));

    [Authorize] [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateContactMethodDto dto)
    {
        var item = await _service.UpdateAsync(id, dto);
        return item != null ? Ok(ApiResponse<ContactMethodResponse>.Ok(item, "Contact method updated")) : NotFound(ApiResponse.Fail("Contact method not found"));
    }

    [Authorize] [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => await _service.DeleteAsync(id) ? Ok(ApiResponse.Ok("Contact method deleted")) : NotFound(ApiResponse.Fail("Contact method not found"));
}
