namespace PersonalManager.Api.Models;

public class PortfolioAttachment
{
    public int Id { get; set; }
    public int PortfolioId { get; set; }
    public int? FileUploadId { get; set; }    // null = 直接 URL（非上傳檔案）
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}
