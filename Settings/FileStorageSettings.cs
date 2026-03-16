namespace PersonalManager.Api.Settings;

public class FileStorageSettings
{
    public string RootPath { get; set; } = "files";
    public long MaxFileSizeMB { get; set; } = 50;
    public string[] AllowedExtensions { get; set; } =
    [
        ".jpg", ".jpeg", ".png", ".gif", ".webp",
        ".pdf", ".doc", ".docx", ".ppt", ".pptx"
    ];
    public S3StorageSettings? S3 { get; set; }
}

public class S3StorageSettings
{
    public string BucketName { get; set; } = "";
    /// <summary>S3-compatible endpoint (e.g. Zeabur Object Storage or Cloudflare R2)</summary>
    public string ServiceUrl { get; set; } = "";
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
    /// <summary>Public base URL for serving files (no trailing slash)</summary>
    public string PublicBaseUrl { get; set; } = "";
    public bool ForcePathStyle { get; set; } = true;

    public bool IsConfigured =>
        !string.IsNullOrEmpty(BucketName) &&
        !string.IsNullOrEmpty(ServiceUrl) &&
        !string.IsNullOrEmpty(AccessKey) &&
        !string.IsNullOrEmpty(SecretKey) &&
        !string.IsNullOrEmpty(PublicBaseUrl);
}
