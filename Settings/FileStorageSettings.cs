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
}
