using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Services;
using PersonalManager.Api.Settings;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace PersonalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FileUploadsController : ControllerBase
{
    private readonly IFileUploadService _service;
    private readonly FileStorageSettings _settings;

    public FileUploadsController(IFileUploadService service, IOptions<FileStorageSettings> settings)
    {
        _service = service;
        _settings = settings.Value;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(ApiResponse.Fail("Unauthorized"));
        return Ok(ApiResponse<List<FileUploadResponse>>.Ok(await _service.GetByUserIdAsync(userId.Value)));
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(ApiResponse.Fail("Unauthorized"));
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse.Fail("No file provided"));

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_settings.AllowedExtensions.Contains(ext))
            return BadRequest(ApiResponse.Fail($"File type '{ext}' is not allowed"));

        if (file.Length > _settings.MaxFileSizeMB * 1024 * 1024)
            return BadRequest(ApiResponse.Fail($"File size exceeds limit of {_settings.MaxFileSizeMB}MB"));

        var result = await _service.UploadAsync(file, userId.Value);
        return Ok(ApiResponse<FileUploadResponse>.Ok(result, "File uploaded successfully"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(ApiResponse.Fail("Unauthorized"));
        return await _service.DeleteAsync(id, userId.Value)
            ? Ok(ApiResponse.Ok("File deleted"))
            : NotFound(ApiResponse.Fail("File not found"));
    }

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub") ?? User.FindFirst("userId");
        if (claim != null && int.TryParse(claim.Value, out var id)) return id;
        return null;
    }
}
