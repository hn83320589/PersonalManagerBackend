namespace PersonalManagerAPI.DTOs;

public class FileUploadResponseDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }
    public string FileUrl { get; set; } = string.Empty; // 完整的訪問URL
}

public class FileUploadRequestDto
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPublic { get; set; } = false;
    public int? UserId { get; set; }
}

public class ImageResizeRequestDto
{
    public int FileId { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool KeepAspectRatio { get; set; } = true;
}