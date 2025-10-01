using System.Security.Cryptography;
using System.Text;

namespace PersonalManagerAPI.Services
{
    /// <summary>
    /// 檔案隔離服務，用於處理可疑或潛在危險的檔案
    /// </summary>
    public class FileQuarantineService : IFileQuarantineService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileQuarantineService> _logger;
        private readonly string _quarantinePath;

        public FileQuarantineService(IWebHostEnvironment environment, ILogger<FileQuarantineService> logger)
        {
            _environment = environment;
            _logger = logger;
            _quarantinePath = Path.Combine(_environment.ContentRootPath, "Quarantine");
            
            // 確保隔離目錄存在
            Directory.CreateDirectory(_quarantinePath);
        }

        /// <summary>
        /// 將檔案移至隔離區
        /// </summary>
        public async Task<QuarantineResult> QuarantineFileAsync(IFormFile file, string reason)
        {
            try
            {
                var quarantineId = Guid.NewGuid().ToString("N");
                var quarantineFileName = $"{quarantineId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.quarantine";
                var quarantineFilePath = Path.Combine(_quarantinePath, quarantineFileName);

                // 建立隔離檔案資訊
                var quarantineInfo = new QuarantineInfo
                {
                    Id = quarantineId,
                    OriginalFileName = file.FileName,
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    QuarantineReason = reason,
                    QuarantineDateTime = DateTime.UtcNow,
                    FileHash = await CalculateFileHashAsync(file)
                };

                // 加密保存檔案內容
                await SaveEncryptedFileAsync(file, quarantineFilePath);

                // 保存隔離資訊
                await SaveQuarantineInfoAsync(quarantineInfo);

                _logger.LogWarning("File quarantined: {FileName} -> {QuarantineId}, Reason: {Reason}", 
                    file.FileName, quarantineId, reason);

                return new QuarantineResult
                {
                    Success = true,
                    QuarantineId = quarantineId,
                    Message = "檔案已安全隔離"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to quarantine file: {FileName}", file.FileName);
                return new QuarantineResult
                {
                    Success = false,
                    Message = "檔案隔離失敗"
                };
            }
        }

        /// <summary>
        /// 從隔離區移除檔案
        /// </summary>
        public Task<bool> RemoveFromQuarantineAsync(string quarantineId)
        {
            try
            {
                var quarantineFile = Directory.GetFiles(_quarantinePath, $"{quarantineId}_*.quarantine").FirstOrDefault();
                var quarantineInfoFile = Path.Combine(_quarantinePath, $"{quarantineId}.json");

                if (quarantineFile != null && File.Exists(quarantineFile))
                {
                    File.Delete(quarantineFile);
                }

                if (File.Exists(quarantineInfoFile))
                {
                    File.Delete(quarantineInfoFile);
                }

                _logger.LogInformation("File removed from quarantine: {QuarantineId}", quarantineId);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove file from quarantine: {QuarantineId}", quarantineId);
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// 取得隔離檔案清單
        /// </summary>
        public async Task<List<QuarantineInfo>> GetQuarantinedFilesAsync()
        {
            var quarantinedFiles = new List<QuarantineInfo>();

            try
            {
                var infoFiles = Directory.GetFiles(_quarantinePath, "*.json");
                
                foreach (var infoFile in infoFiles)
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(infoFile);
                        var info = System.Text.Json.JsonSerializer.Deserialize<QuarantineInfo>(json);
                        if (info != null)
                        {
                            quarantinedFiles.Add(info);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to read quarantine info file: {InfoFile}", infoFile);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get quarantined files list");
            }

            return quarantinedFiles.OrderByDescending(q => q.QuarantineDateTime).ToList();
        }

        /// <summary>
        /// 清理過期的隔離檔案
        /// </summary>
        public async Task<int> CleanupExpiredFilesAsync(TimeSpan retentionPeriod)
        {
            var cleanedCount = 0;
            var cutoffDate = DateTime.UtcNow - retentionPeriod;

            try
            {
                var quarantinedFiles = await GetQuarantinedFilesAsync();
                
                foreach (var info in quarantinedFiles.Where(q => q.QuarantineDateTime < cutoffDate))
                {
                    if (await RemoveFromQuarantineAsync(info.Id))
                    {
                        cleanedCount++;
                    }
                }

                _logger.LogInformation("Cleaned up {Count} expired quarantine files", cleanedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup expired quarantine files");
            }

            return cleanedCount;
        }

        /// <summary>
        /// 計算檔案雜湊值
        /// </summary>
        private async Task<string> CalculateFileHashAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var sha256 = SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(stream);
            return Convert.ToHexString(hashBytes);
        }

        /// <summary>
        /// 加密保存檔案
        /// </summary>
        private async Task SaveEncryptedFileAsync(IFormFile file, string filePath)
        {
            // 簡單的XOR加密（生產環境應使用更強的加密）
            var key = Encoding.UTF8.GetBytes("QuarantineKey123"); // 在生產環境中應使用更安全的密鑰管理
            
            using var inputStream = file.OpenReadStream();
            using var outputStream = new FileStream(filePath, FileMode.Create);
            
            var buffer = new byte[8192];
            int bytesRead;
            int keyIndex = 0;

            while ((bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                // 簡單XOR加密
                for (int i = 0; i < bytesRead; i++)
                {
                    buffer[i] ^= key[keyIndex % key.Length];
                    keyIndex++;
                }

                await outputStream.WriteAsync(buffer, 0, bytesRead);
            }
        }

        /// <summary>
        /// 保存隔離資訊
        /// </summary>
        private async Task SaveQuarantineInfoAsync(QuarantineInfo info)
        {
            var infoPath = Path.Combine(_quarantinePath, $"{info.Id}.json");
            var json = System.Text.Json.JsonSerializer.Serialize(info, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
            
            await File.WriteAllTextAsync(infoPath, json);
        }
    }

    /// <summary>
    /// 隔離檔案資訊
    /// </summary>
    public class QuarantineInfo
    {
        public string Id { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string QuarantineReason { get; set; } = string.Empty;
        public DateTime QuarantineDateTime { get; set; }
        public string FileHash { get; set; } = string.Empty;
    }

    /// <summary>
    /// 隔離操作結果
    /// </summary>
    public class QuarantineResult
    {
        public bool Success { get; set; }
        public string QuarantineId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}