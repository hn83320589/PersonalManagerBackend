using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Models;
using System.Text.Json;

namespace PersonalManagerAPI.Services;

public class FileService : IFileService
{
    private readonly JsonDataService _dataService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileService> _logger;
    private readonly IFileSecurityService _fileSecurityService;
    private readonly string[] _allowedImageTypes = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private readonly string[] _allowedVideoTypes = { ".mp4", ".avi", ".mov", ".wmv", ".webm" };
    private readonly string[] _allowedDocumentTypes = { ".pdf", ".doc", ".docx", ".txt", ".xlsx", ".pptx" };
    private const long MaxFileSize = 50 * 1024 * 1024; // 50MB
    private const long MaxImageSize = 10 * 1024 * 1024; // 10MB for images

    public FileService(JsonDataService dataService, IWebHostEnvironment environment, ILogger<FileService> logger, IFileSecurityService fileSecurityService)
    {
        _dataService = dataService;
        _environment = environment;
        _logger = logger;
        _fileSecurityService = fileSecurityService;
    }

    public async Task<ApiResponse<FileUploadResponseDto>> UploadFileAsync(IFormFile file, FileUploadRequestDto request)
    {
        try
        {
            // 使用增強的檔案安全驗證
            var securityResult = await _fileSecurityService.ValidateFileSecurityAsync(file);
            if (!securityResult.IsValid)
            {
                var errors = securityResult.Errors.Any() ? securityResult.Errors : new List<string> { "檔案安全驗證失敗" };
                return ApiResponse<FileUploadResponseDto>.ErrorResult("檔案安全驗證失敗", errors);
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var category = DetermineFileCategory(fileExtension);
            var fileName = GenerateUniqueFileName(file.FileName);
            var relativePath = GetRelativePath(category, fileName);
            var fullPath = Path.Combine(_environment.WebRootPath, relativePath);

            // 確保目錄存在
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            // 儲存檔案
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 建立檔案記錄
            var fileUpload = new FileUpload
            {
                FileName = fileName,
                OriginalFileName = file.FileName,
                FilePath = relativePath,
                ContentType = file.ContentType,
                FileSize = file.Length,
                UserId = request.UserId,
                Category = category,
                Description = request.Description,
                IsPublic = request.IsPublic,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // 儲存到JSON資料
            var files = await GetAllFilesAsync();
            fileUpload.Id = files.Count > 0 ? files.Max(f => f.Id) + 1 : 1;
            files.Add(fileUpload);
            await SaveAllFilesAsync(files);

            var responseDto = MapToResponseDto(fileUpload);
            return ApiResponse<FileUploadResponseDto>.SuccessResult(responseDto, "檔案上傳成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檔案上傳失敗: {FileName}", file.FileName);
            return ApiResponse<FileUploadResponseDto>.ErrorResult("檔案上傳失敗");
        }
    }

    public async Task<ApiResponse<FileUploadResponseDto>> GetFileAsync(int id)
    {
        try
        {
            var files = await GetAllFilesAsync();
            var file = files.FirstOrDefault(f => f.Id == id);
            
            if (file == null)
            {
                return ApiResponse<FileUploadResponseDto>.ErrorResult("找不到指定的檔案");
            }

            var responseDto = MapToResponseDto(file);
            return ApiResponse<FileUploadResponseDto>.SuccessResult(responseDto, "成功取得檔案資訊");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得檔案失敗: {FileId}", id);
            return ApiResponse<FileUploadResponseDto>.ErrorResult("取得檔案失敗");
        }
    }

    public async Task<ApiResponse<List<FileUploadResponseDto>>> GetFilesByUserAsync(int userId)
    {
        try
        {
            var files = await GetAllFilesAsync();
            var userFiles = files.Where(f => f.UserId == userId).ToList();
            
            var responseDtos = userFiles.Select(MapToResponseDto).ToList();
            return ApiResponse<List<FileUploadResponseDto>>.SuccessResult(responseDtos, "成功取得使用者檔案列表");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得使用者檔案失敗: {UserId}", userId);
            return ApiResponse<List<FileUploadResponseDto>>.ErrorResult("取得使用者檔案失敗");
        }
    }

    public async Task<ApiResponse<List<FileUploadResponseDto>>> GetPublicFilesAsync(string? category = null)
    {
        try
        {
            var files = await GetAllFilesAsync();
            var query = files.Where(f => f.IsPublic);
            
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(f => f.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            }
            
            var publicFiles = query.OrderByDescending(f => f.CreatedAt).ToList();
            var responseDtos = publicFiles.Select(MapToResponseDto).ToList();
            
            return ApiResponse<List<FileUploadResponseDto>>.SuccessResult(responseDtos, "成功取得公開檔案列表");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得公開檔案失敗");
            return ApiResponse<List<FileUploadResponseDto>>.ErrorResult("取得公開檔案失敗");
        }
    }

    public async Task<ApiResponse> DeleteFileAsync(int id)
    {
        try
        {
            var files = await GetAllFilesAsync();
            var file = files.FirstOrDefault(f => f.Id == id);
            
            if (file == null)
            {
                return ApiResponse.ErrorResult("找不到指定的檔案");
            }

            // 刪除實體檔案
            var fullPath = Path.Combine(_environment.WebRootPath, file.FilePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            // 從資料中移除
            files.Remove(file);
            await SaveAllFilesAsync(files);

            return ApiResponse.SuccessResult("檔案刪除成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刪除檔案失敗: {FileId}", id);
            return ApiResponse.ErrorResult("刪除檔案失敗");
        }
    }

    public async Task<ApiResponse<FileUploadResponseDto>> UpdateFileInfoAsync(int id, FileUploadRequestDto request)
    {
        try
        {
            var files = await GetAllFilesAsync();
            var file = files.FirstOrDefault(f => f.Id == id);
            
            if (file == null)
            {
                return ApiResponse<FileUploadResponseDto>.ErrorResult("找不到指定的檔案");
            }

            file.Description = request.Description;
            file.IsPublic = request.IsPublic;
            file.UpdatedAt = DateTime.UtcNow;

            await SaveAllFilesAsync(files);

            var responseDto = MapToResponseDto(file);
            return ApiResponse<FileUploadResponseDto>.SuccessResult(responseDto, "檔案資訊更新成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新檔案資訊失敗: {FileId}", id);
            return ApiResponse<FileUploadResponseDto>.ErrorResult("更新檔案資訊失敗");
        }
    }

    public async Task<ApiResponse<FileUploadResponseDto>> ResizeImageAsync(int fileId, int width, int height, bool keepAspectRatio = true)
    {
        // 這裡可以實作圖片縮放功能，但需要額外的圖片處理套件如 ImageSharp
        // 目前先回傳未實作的訊息
        await Task.CompletedTask;
        return ApiResponse<FileUploadResponseDto>.ErrorResult("圖片縮放功能尚未實作，需要安裝 ImageSharp 套件");
    }

    public Task<bool> ValidateFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Task.FromResult(false);
        }

        if (file.Length > MaxFileSize)
        {
            return Task.FromResult(false);
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = _allowedImageTypes.Concat(_allowedVideoTypes).Concat(_allowedDocumentTypes);
        
        if (!allowedExtensions.Contains(extension))
        {
            return Task.FromResult(false);
        }

        // 圖片檔案大小限制
        if (_allowedImageTypes.Contains(extension) && file.Length > MaxImageSize)
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    public string GetFileUrl(string filePath)
    {
        return $"/uploads/{filePath.Replace('\\', '/')}";
    }

    private string DetermineFileCategory(string extension)
    {
        if (_allowedImageTypes.Contains(extension))
            return "images";
        if (_allowedVideoTypes.Contains(extension))
            return "videos";
        if (_allowedDocumentTypes.Contains(extension))
            return "documents";
        return "other";
    }

    private string GenerateUniqueFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var guid = Guid.NewGuid().ToString("N")[..8];
        
        return $"{fileNameWithoutExtension}_{timestamp}_{guid}{extension}";
    }

    private string GetRelativePath(string category, string fileName)
    {
        return Path.Combine("uploads", category, fileName);
    }

    private FileUploadResponseDto MapToResponseDto(FileUpload file)
    {
        return new FileUploadResponseDto
        {
            Id = file.Id,
            FileName = file.FileName,
            OriginalFileName = file.OriginalFileName,
            FilePath = file.FilePath,
            ContentType = file.ContentType,
            FileSize = file.FileSize,
            Category = file.Category,
            Description = file.Description,
            IsPublic = file.IsPublic,
            CreatedAt = file.CreatedAt,
            FileUrl = GetFileUrl(file.FilePath)
        };
    }

    private async Task<List<FileUpload>> GetAllFilesAsync()
    {
        try
        {
            return await _dataService.ReadJsonAsync<FileUpload>("fileUploads.json");
        }
        catch
        {
            return new List<FileUpload>();
        }
    }

    private async Task SaveAllFilesAsync(List<FileUpload> files)
    {
        await _dataService.WriteJsonAsync("fileUploads.json", files);
    }
}