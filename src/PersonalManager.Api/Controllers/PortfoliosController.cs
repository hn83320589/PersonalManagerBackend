using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Services;

namespace PersonalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortfoliosController : ControllerBase
{
    private readonly IPortfolioService _service;
    public PortfoliosController(IPortfolioService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(ApiResponse<List<PortfolioResponse>>.Ok(await _service.GetAllAsync()));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item != null ? Ok(ApiResponse<PortfolioResponse>.Ok(item)) : NotFound(ApiResponse.Fail("Portfolio not found"));
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(int userId)
        => Ok(ApiResponse<List<PortfolioResponse>>.Ok(await _service.GetByUserIdAsync(userId)));

    [HttpGet("user/{userId}/public")]
    public async Task<IActionResult> GetPublicByUserId(int userId)
        => Ok(ApiResponse<List<PortfolioResponse>>.Ok(await _service.GetPublicByUserIdAsync(userId)));

    [HttpGet("user/{userId}/featured")]
    public async Task<IActionResult> GetFeatured(int userId)
        => Ok(ApiResponse<List<PortfolioResponse>>.Ok(await _service.GetFeaturedAsync(userId)));

    [Authorize] [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePortfolioDto dto)
        => Ok(ApiResponse<PortfolioResponse>.Ok(await _service.CreateAsync(dto), "Portfolio created"));

    [Authorize] [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePortfolioDto dto)
    {
        var item = await _service.UpdateAsync(id, dto);
        return item != null ? Ok(ApiResponse<PortfolioResponse>.Ok(item, "Portfolio updated")) : NotFound(ApiResponse.Fail("Portfolio not found"));
    }

    [Authorize] [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => await _service.DeleteAsync(id) ? Ok(ApiResponse.Ok("Portfolio deleted")) : NotFound(ApiResponse.Fail("Portfolio not found"));
}
