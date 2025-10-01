using System.Security.Cryptography;
using System.Text;
using PersonalManagerAPI.Middleware.Exceptions;

namespace PersonalManagerAPI.Services
{
    /// <summary>
    /// 檔案安全服務，提供檔案安全驗證、病毒掃描、惡意檔案檢測等功能
    /// </summary>
    public class FileSecurityService : IFileSecurityService
    {
        private readonly ILogger<FileSecurityService> _logger;
        
        // 危險的檔案副檔名
        private static readonly HashSet<string> DangerousExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".exe", ".bat", ".cmd", ".com", ".scr", ".pif", ".vbs", ".js", ".jar", ".app", ".deb", ".pkg", ".dmg",
            ".asp", ".aspx", ".php", ".jsp", ".ps1", ".sh", ".py", ".rb", ".perl", ".pl"
        };

        // 已知的惡意檔案簽名 (Magic Numbers)
        private static readonly Dictionary<string, byte[]> MaliciousSignatures = new()
        {
            // PE執行檔 (Windows .exe)
            { "PE", new byte[] { 0x4D, 0x5A } }, // MZ
            // ELF執行檔 (Linux)
            { "ELF", new byte[] { 0x7F, 0x45, 0x4C, 0x46 } }, // .ELF
            // Java Class檔案
            { "Java", new byte[] { 0xCA, 0xFE, 0xBA, 0xBE } },
            // Shell Script
            { "Shell", Encoding.ASCII.GetBytes("#!/bin/sh") },
            { "Bash", Encoding.ASCII.GetBytes("#!/bin/bash") }
        };

        // 有效的檔案類型簽名
        private static readonly Dictionary<string, (byte[] signature, string[] extensions)> ValidFileSignatures = new()
        {
            // 圖片檔案
            { "JPEG", (new byte[] { 0xFF, 0xD8, 0xFF }, new[] { ".jpg", ".jpeg" }) },
            { "PNG", (new byte[] { 0x89, 0x50, 0x4E, 0x47 }, new[] { ".png" }) },
            { "GIF87a", (Encoding.ASCII.GetBytes("GIF87a"), new[] { ".gif" }) },
            { "GIF89a", (Encoding.ASCII.GetBytes("GIF89a"), new[] { ".gif" }) },
            { "WEBP", (new byte[] { 0x52, 0x49, 0x46, 0x46 }, new[] { ".webp" }) },
            
            // PDF檔案
            { "PDF", (Encoding.ASCII.GetBytes("%PDF"), new[] { ".pdf" }) },
            
            // Office檔案 (Office 2007+)
            { "DOCX", (new byte[] { 0x50, 0x4B, 0x03, 0x04 }, new[] { ".docx", ".xlsx", ".pptx" }) },
            
            // 純文字檔案
            { "TXT", (new byte[] { }, new[] { ".txt" }) }, // 純文字沒有固定簽名
            
            // 影片檔案
            { "MP4", (new byte[] { 0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70 }, new[] { ".mp4" }) },
            { "AVI", (Encoding.ASCII.GetBytes("RIFF"), new[] { ".avi" }) }
        };

        public FileSecurityService(ILogger<FileSecurityService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 全面的檔案安全驗證
        /// </summary>
        public async Task<FileSecurityResult> ValidateFileSecurityAsync(IFormFile file)
        {
            var result = new FileSecurityResult { IsValid = true };

            try
            {
                // 1. 基本檔案驗證
                if (!ValidateBasicFile(file, result))
                    return result;

                // 2. 檔案副檔名安全檢查
                if (!ValidateFileExtension(file.FileName, result))
                    return result;

                // 3. 檔案內容類型驗證
                if (!await ValidateFileContentAsync(file, result))
                    return result;

                // 4. 檔案簽名驗證 (Magic Number)
                if (!await ValidateFileSignatureAsync(file, result))
                    return result;

                // 5. 惡意內容掃描
                if (!await ScanForMaliciousContentAsync(file, result))
                    return result;

                // 6. 檔案名稱安全檢查
                if (!ValidateFileName(file.FileName, result))
                    return result;

                _logger.LogInformation("File security validation passed for: {FileName}", file.FileName);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File security validation failed for: {FileName}", file.FileName);
                result.IsValid = false;
                result.Errors.Add("檔案安全驗證過程中發生錯誤");
                return result;
            }
        }

        /// <summary>
        /// 基本檔案驗證
        /// </summary>
        private bool ValidateBasicFile(IFormFile file, FileSecurityResult result)
        {
            if (file == null || file.Length == 0)
            {
                result.IsValid = false;
                result.Errors.Add("檔案不能為空");
                return false;
            }

            if (string.IsNullOrWhiteSpace(file.FileName))
            {
                result.IsValid = false;
                result.Errors.Add("檔案名稱不能為空");
                return false;
            }

            // 檔案大小限制 (50MB)
            if (file.Length > 50 * 1024 * 1024)
            {
                result.IsValid = false;
                result.Errors.Add("檔案大小不能超過50MB");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 檔案副檔名安全檢查
        /// </summary>
        private bool ValidateFileExtension(string fileName, FileSecurityResult result)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            // 檢查是否為危險的檔案類型
            if (DangerousExtensions.Contains(extension))
            {
                result.IsValid = false;
                result.Errors.Add($"不允許上傳 {extension} 檔案類型");
                _logger.LogWarning("Dangerous file extension detected: {Extension} in {FileName}", extension, fileName);
                return false;
            }

            // 檢查是否為允許的檔案類型
            var allowedExtensions = ValidFileSignatures.SelectMany(kvp => kvp.Value.extensions).ToHashSet();
            if (!allowedExtensions.Contains(extension) && extension != ".txt")
            {
                result.IsValid = false;
                result.Errors.Add($"不支援的檔案類型: {extension}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 檔案內容類型驗證
        /// </summary>
        private Task<bool> ValidateFileContentAsync(IFormFile file, FileSecurityResult result)
        {
            // 檢查Content-Type是否與檔案副檔名匹配
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var contentType = file.ContentType.ToLowerInvariant();

            var validContentTypes = GetValidContentTypes(extension);
            if (validContentTypes.Any() && !validContentTypes.Contains(contentType))
            {
                result.IsValid = false;
                result.Errors.Add($"檔案內容類型 {contentType} 與副檔名 {extension} 不匹配");
                _logger.LogWarning("Content-Type mismatch: {ContentType} for extension {Extension}", contentType, extension);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        /// <summary>
        /// 檔案簽名驗證 (Magic Number)
        /// </summary>
        private async Task<bool> ValidateFileSignatureAsync(IFormFile file, FileSecurityResult result)
        {
            var buffer = new byte[32]; // 讀取前32個位元組
            
            using var stream = file.OpenReadStream();
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            stream.Position = 0; // 重置流位置

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            // 對於已知的檔案類型，驗證檔案簽名
            foreach (var (signatureType, (signature, extensions)) in ValidFileSignatures)
            {
                if (extensions.Contains(extension) && signature.Length > 0)
                {
                    if (bytesRead >= signature.Length)
                    {
                        var fileSignature = buffer.Take(signature.Length).ToArray();
                        if (fileSignature.SequenceEqual(signature))
                        {
                            return true; // 簽名匹配
                        }
                    }
                    
                    // 簽名不匹配
                    result.IsValid = false;
                    result.Errors.Add($"檔案簽名與副檔名 {extension} 不匹配");
                    _logger.LogWarning("File signature mismatch for {Extension} file: {FileName}", extension, file.FileName);
                    return false;
                }
            }

            // 對於沒有簽名檢查的檔案類型（如.txt），允許通過
            return true;
        }

        /// <summary>
        /// 惡意內容掃描
        /// </summary>
        private async Task<bool> ScanForMaliciousContentAsync(IFormFile file, FileSecurityResult result)
        {
            var buffer = new byte[1024]; // 檢查前1KB
            
            using var stream = file.OpenReadStream();
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            stream.Position = 0;

            // 檢查是否包含已知的惡意檔案簽名
            foreach (var (signatureType, signature) in MaliciousSignatures)
            {
                if (ContainsSignature(buffer, signature, bytesRead))
                {
                    result.IsValid = false;
                    result.Errors.Add($"檢測到潛在的惡意內容: {signatureType}");
                    _logger.LogWarning("Malicious signature detected in {FileName}: {SignatureType}", file.FileName, signatureType);
                    return false;
                }
            }

            // 檢查是否包含可疑的腳本內容
            var content = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            if (ContainsSuspiciousScript(content))
            {
                result.IsValid = false;
                result.Errors.Add("檢測到可疑的腳本內容");
                _logger.LogWarning("Suspicious script content detected in {FileName}", file.FileName);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 檔案名稱安全檢查
        /// </summary>
        private bool ValidateFileName(string fileName, FileSecurityResult result)
        {
            // 檢查檔案名稱長度
            if (fileName.Length > 255)
            {
                result.IsValid = false;
                result.Errors.Add("檔案名稱長度不能超過255個字元");
                return false;
            }

            // 檢查是否包含危險字元
            var dangerousChars = new char[] { '/', '\\', ':', '*', '?', '"', '<', '>', '|', '\0' };
            if (fileName.IndexOfAny(dangerousChars) >= 0)
            {
                result.IsValid = false;
                result.Errors.Add("檔案名稱包含非法字元");
                return false;
            }

            // 檢查是否為系統保留名稱
            var reservedNames = new[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName).ToUpperInvariant();
            if (reservedNames.Contains(nameWithoutExtension))
            {
                result.IsValid = false;
                result.Errors.Add("檔案名稱為系統保留名稱");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 取得有效的Content-Type清單
        /// </summary>
        private string[] GetValidContentTypes(string extension)
        {
            return extension switch
            {
                ".jpg" or ".jpeg" => new[] { "image/jpeg" },
                ".png" => new[] { "image/png" },
                ".gif" => new[] { "image/gif" },
                ".webp" => new[] { "image/webp" },
                ".pdf" => new[] { "application/pdf" },
                ".txt" => new[] { "text/plain" },
                ".docx" => new[] { "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                ".xlsx" => new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                ".pptx" => new[] { "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
                ".mp4" => new[] { "video/mp4" },
                ".avi" => new[] { "video/x-msvideo" },
                _ => Array.Empty<string>()
            };
        }

        /// <summary>
        /// 檢查是否包含特定簽名
        /// </summary>
        private bool ContainsSignature(byte[] buffer, byte[] signature, int bufferLength)
        {
            if (signature.Length > bufferLength) return false;

            for (int i = 0; i <= bufferLength - signature.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < signature.Length; j++)
                {
                    if (buffer[i + j] != signature[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match) return true;
            }
            return false;
        }

        /// <summary>
        /// 檢查是否包含可疑的腳本內容
        /// </summary>
        private bool ContainsSuspiciousScript(string content)
        {
            var suspiciousPatterns = new[]
            {
                "<script", "javascript:", "vbscript:", "onload=", "onerror=", "onclick=",
                "eval(", "document.write", "innerHTML", "document.cookie",
                "<?php", "<%", "<jsp:", "<?=", 
                "exec(", "system(", "shell_exec", "passthru",
                "base64_decode", "gzinflate", "str_rot13"
            };

            var lowerContent = content.ToLowerInvariant();
            return suspiciousPatterns.Any(pattern => lowerContent.Contains(pattern.ToLowerInvariant()));
        }
    }

    /// <summary>
    /// 檔案安全驗證結果
    /// </summary>
    public class FileSecurityResult
    {
        public bool IsValid { get; set; } = true;
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
}