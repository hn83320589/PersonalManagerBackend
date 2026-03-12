namespace PersonalManager.Api.Models;

public class FileUpload
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FileName { get; set; } = string.Empty;      // 原始檔名
    public string StoredName { get; set; } = string.Empty;    // GUID + 副檔名
    public string FileUrl { get; set; } = string.Empty;       // /files/{StoredName}
    public string FileType { get; set; } = string.Empty;      // image/pdf/document/presentation/other
    public long FileSize { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
