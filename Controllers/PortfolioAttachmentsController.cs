using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Services;

namespace PersonalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortfolioAttachmentsController : ControllerBase
{
    private readonly IPortfolioAttachmentService _service;
    public PortfolioAttachmentsController(IPortfolioAttachmentService service) => _service = service;

    [HttpGet("portfolio/{portfolioId}")]
    public async Task<IActionResult> GetByPortfolio(int portfolioId)
        => Ok(ApiResponse<List<PortfolioAttachmentResponse>>.Ok(await _service.GetByPortfolioIdAsync(portfolioId)));

    [Authorize] [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePortfolioAttachmentDto dto)
        => Ok(ApiResponse<PortfolioAttachmentResponse>.Ok(await _service.CreateAsync(dto), "Attachment created"));

    [Authorize] [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => await _service.DeleteAsync(id) ? Ok(ApiResponse.Ok("Attachment deleted")) : NotFound(ApiResponse.Fail("Attachment not found"));
}
