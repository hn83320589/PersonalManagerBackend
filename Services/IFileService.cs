using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Models;

namespace PersonalManagerAPI.Services;

public interface IFileService
{
    Task<ApiResponse<FileUploadResponseDto>> UploadFileAsync(IFormFile file, FileUploadRequestDto request);
    Task<ApiResponse<FileUploadResponseDto>> GetFileAsync(int id);
    Task<ApiResponse<List<FileUploadResponseDto>>> GetFilesByUserAsync(int userId);
    Task<ApiResponse<List<FileUploadResponseDto>>> GetPublicFilesAsync(string? category = null);
    Task<ApiResponse> DeleteFileAsync(int id);
    Task<ApiResponse<FileUploadResponseDto>> UpdateFileInfoAsync(int id, FileUploadRequestDto request);
    Task<ApiResponse<FileUploadResponseDto>> ResizeImageAsync(int fileId, int width, int height, bool keepAspectRatio = true);
    Task<bool> ValidateFileAsync(IFormFile file);
    string GetFileUrl(string filePath);
}