namespace PersonalManagerAPI.Services
{
    /// <summary>
    /// 檔案隔離服務介面
    /// </summary>
    public interface IFileQuarantineService
    {
        /// <summary>
        /// 將檔案移至隔離區
        /// </summary>
        Task<QuarantineResult> QuarantineFileAsync(IFormFile file, string reason);

        /// <summary>
        /// 從隔離區移除檔案
        /// </summary>
        Task<bool> RemoveFromQuarantineAsync(string quarantineId);

        /// <summary>
        /// 取得隔離檔案清單
        /// </summary>
        Task<List<QuarantineInfo>> GetQuarantinedFilesAsync();

        /// <summary>
        /// 清理過期的隔離檔案
        /// </summary>
        Task<int> CleanupExpiredFilesAsync(TimeSpan retentionPeriod);
    }
}