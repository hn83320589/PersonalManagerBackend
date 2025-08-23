using PersonalManagerAPI.DTOs.Portfolio;
using PersonalManagerAPI.Models;

namespace PersonalManagerAPI.Services.Interfaces;

/// <summary>
/// 作品集服務介面
/// </summary>
public interface IPortfolioService
{
    /// <summary>
    /// 取得所有作品（分頁）
    /// </summary>
    Task<IEnumerable<PortfolioResponseDto>> GetAllPortfoliosAsync(int skip = 0, int take = 50);

    /// <summary>
    /// 根據ID取得作品
    /// </summary>
    Task<PortfolioResponseDto?> GetPortfolioByIdAsync(int id);

    /// <summary>
    /// 根據使用者ID取得作品列表
    /// </summary>
    Task<IEnumerable<PortfolioResponseDto>> GetPortfoliosByUserIdAsync(int userId);

    /// <summary>
    /// 取得公開的作品（分頁）
    /// </summary>
    Task<IEnumerable<PortfolioResponseDto>> GetPublicPortfoliosAsync(int skip = 0, int take = 50);

    /// <summary>
    /// 取得特色作品
    /// </summary>
    Task<IEnumerable<PortfolioResponseDto>> GetFeaturedPortfoliosAsync(bool publicOnly = true);

    /// <summary>
    /// 根據技術標籤搜尋作品
    /// </summary>
    Task<IEnumerable<PortfolioResponseDto>> SearchByTechnologyAsync(string technology, bool publicOnly = true);

    /// <summary>
    /// 根據類型搜尋作品
    /// </summary>
    Task<IEnumerable<PortfolioResponseDto>> SearchByTypeAsync(string type, bool publicOnly = true);

    /// <summary>
    /// 根據關鍵字搜尋作品（標題、描述、技術）
    /// </summary>
    Task<IEnumerable<PortfolioResponseDto>> SearchPortfoliosAsync(string keyword, bool publicOnly = true);

    /// <summary>
    /// 根據日期範圍查詢作品
    /// </summary>
    Task<IEnumerable<PortfolioResponseDto>> GetPortfoliosByDateRangeAsync(DateTime? startDate, DateTime? endDate, bool publicOnly = true);

    /// <summary>
    /// 建立新的作品
    /// </summary>
    Task<PortfolioResponseDto> CreatePortfolioAsync(CreatePortfolioDto createPortfolioDto);

    /// <summary>
    /// 更新作品
    /// </summary>
    Task<PortfolioResponseDto?> UpdatePortfolioAsync(int id, UpdatePortfolioDto updatePortfolioDto);

    /// <summary>
    /// 刪除作品
    /// </summary>
    Task<bool> DeletePortfolioAsync(int id);

    /// <summary>
    /// 更新作品排序
    /// </summary>
    Task<bool> UpdatePortfolioOrderAsync(int id, int newSortOrder);

    /// <summary>
    /// 批量更新作品排序
    /// </summary>
    Task<bool> BatchUpdatePortfolioOrderAsync(Dictionary<int, int> portfolioOrders);

    /// <summary>
    /// 取得作品統計資訊
    /// </summary>
    Task<object> GetPortfolioStatsAsync();

    /// <summary>
    /// 取得所有技術標籤
    /// </summary>
    Task<IEnumerable<string>> GetTechnologiesAsync(bool publicOnly = true);

    /// <summary>
    /// 取得所有作品類型
    /// </summary>
    Task<IEnumerable<string>> GetPortfolioTypesAsync(bool publicOnly = true);
}