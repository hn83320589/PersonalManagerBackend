namespace PersonalManagerAPI.Services
{
    /// <summary>
    /// 檔案安全服務介面
    /// </summary>
    public interface IFileSecurityService
    {
        /// <summary>
        /// 全面的檔案安全驗證
        /// </summary>
        /// <param name="file">要驗證的檔案</param>
        /// <returns>驗證結果</returns>
        Task<FileSecurityResult> ValidateFileSecurityAsync(IFormFile file);
    }
}