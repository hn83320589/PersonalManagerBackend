using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Services;

namespace PersonalManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : BaseController
{
    private readonly IFileService _fileService;

    public FilesController(IFileService fileService)
    {
        _fileService = fileService;
    }

    /// <summary>
    /// 上傳檔案
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile([FromForm] IFormFile file, [FromForm] FileUploadRequestDto request)
    {
        if (file == null)
        {
            return BadRequest(ApiResponse.ErrorResult("請選擇要上傳的檔案"));
        }

        var result = await _fileService.UploadFileAsync(file, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// 取得檔案資訊
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetFile(int id)
    {
        var result = await _fileService.GetFileAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// 取得使用者的所有檔案
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetFilesByUser(int userId)
    {
        var result = await _fileService.GetFilesByUserAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// 取得公開檔案列表
    /// </summary>
    [HttpGet("public")]
    public async Task<IActionResult> GetPublicFiles([FromQuery] string? category = null)
    {
        var result = await _fileService.GetPublicFilesAsync(category);
        return Ok(result);
    }

    /// <summary>
    /// 更新檔案資訊
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFileInfo(int id, [FromBody] FileUploadRequestDto request)
    {
        var result = await _fileService.UpdateFileInfoAsync(id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// 刪除檔案
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFile(int id)
    {
        var result = await _fileService.DeleteFileAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// 取得圖片檔案（依分類）
    /// </summary>
    [HttpGet("images")]
    public async Task<IActionResult> GetImages()
    {
        var result = await _fileService.GetPublicFilesAsync("images");
        return Ok(result);
    }

    /// <summary>
    /// 取得影片檔案
    /// </summary>
    [HttpGet("videos")]
    public async Task<IActionResult> GetVideos()
    {
        var result = await _fileService.GetPublicFilesAsync("videos");
        return Ok(result);
    }

    /// <summary>
    /// 取得文件檔案
    /// </summary>
    [HttpGet("documents")]
    public async Task<IActionResult> GetDocuments()
    {
        var result = await _fileService.GetPublicFilesAsync("documents");
        return Ok(result);
    }

    /// <summary>
    /// 縮放圖片（預留功能）
    /// </summary>
    [HttpPost("{id}/resize")]
    public async Task<IActionResult> ResizeImage(int id, [FromBody] ImageResizeRequestDto request)
    {
        var result = await _fileService.ResizeImageAsync(id, request.Width, request.Height, request.KeepAspectRatio);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}