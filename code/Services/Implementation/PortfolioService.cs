using PersonalManagerAPI.DTOs.Portfolio;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services;
using PersonalManagerAPI.Services.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace PersonalManagerAPI.Services.Implementation;

/// <summary>
/// 作品集服務實作
/// </summary>
public class PortfolioService : IPortfolioService
{
    private readonly JsonDataService _dataService;
    private readonly IMapper _mapper;
    private readonly ILogger<PortfolioService> _logger;

    // 預定義的作品類型
    private readonly HashSet<string> _validTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Web Application", "Mobile App", "Desktop Application", "API/Backend",
        "Library/Framework", "Database", "DevOps/Infrastructure", "Data Analysis",
        "Machine Learning", "Game", "IoT", "Blockchain", "Other"
    };

    public PortfolioService(JsonDataService dataService, IMapper mapper, ILogger<PortfolioService> logger)
    {
        _dataService = dataService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<PortfolioResponseDto>> GetAllPortfoliosAsync(int skip = 0, int take = 50)
    {
        try
        {
            var portfolios = await _dataService.GetPortfoliosAsync();
            var pagedPortfolios = portfolios
                .OrderBy(p => p.SortOrder)
                .ThenByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Take(take);
            
            return _mapper.Map<IEnumerable<PortfolioResponseDto>>(pagedPortfolios);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all portfolios");
            throw;
        }
    }

    public async Task<PortfolioResponseDto?> GetPortfolioByIdAsync(int id)
    {
        try
        {
            var portfolios = await _dataService.GetPortfoliosAsync();
            var portfolio = portfolios.FirstOrDefault(p => p.Id == id);
            return _mapper.Map<PortfolioResponseDto?>(portfolio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting portfolio with ID {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<PortfolioResponseDto>> GetPortfoliosByUserIdAsync(int userId)
    {
        try
        {
            var portfolios = await _dataService.GetPortfoliosAsync();
            var userPortfolios = portfolios
                .Where(p => p.UserId == userId)
                .OrderBy(p => p.SortOrder)
                .ThenByDescending(p => p.CreatedAt);
            
            return _mapper.Map<IEnumerable<PortfolioResponseDto>>(userPortfolios);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting portfolios for user ID {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<PortfolioResponseDto>> GetPublicPortfoliosAsync(int skip = 0, int take = 50)
    {
        try
        {
            var portfolios = await _dataService.GetPortfoliosAsync();
            var publicPortfolios = portfolios
                .Where(p => p.IsPublic)
                .OrderBy(p => p.SortOrder)
                .ThenByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Take(take);
            
            return _mapper.Map<IEnumerable<PortfolioResponseDto>>(publicPortfolios);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting public portfolios");
            throw;
        }
    }

    public async Task<IEnumerable<PortfolioResponseDto>> GetFeaturedPortfoliosAsync(bool publicOnly = true)
    {
        try
        {
            var portfolios = await _dataService.GetPortfoliosAsync();
            var featuredPortfolios = portfolios
                .Where(p => (!publicOnly || p.IsPublic) && p.IsFeatured)
                .OrderBy(p => p.SortOrder)
                .ThenByDescending(p => p.CreatedAt);
            
            return _mapper.Map<IEnumerable<PortfolioResponseDto>>(featuredPortfolios);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting featured portfolios");
            throw;
        }
    }

    public async Task<IEnumerable<PortfolioResponseDto>> SearchByTechnologyAsync(string technology, bool publicOnly = true)
    {
        try
        {
            var portfolios = await _dataService.GetPortfoliosAsync();
            var searchResults = portfolios
                .Where(p => (!publicOnly || p.IsPublic) && 
                           !string.IsNullOrEmpty(p.Technologies) &&
                           p.Technologies.Contains(technology, StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p.SortOrder)
                .ThenByDescending(p => p.CreatedAt);
            
            return _mapper.Map<IEnumerable<PortfolioResponseDto>>(searchResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching portfolios by technology {Technology}", technology);
            throw;
        }
    }

    public async Task<IEnumerable<PortfolioResponseDto>> SearchByTypeAsync(string type, bool publicOnly = true)
    {
        try
        {
            var portfolios = await _dataService.GetPortfoliosAsync();
            var searchResults = portfolios
                .Where(p => (!publicOnly || p.IsPublic) && 
                           !string.IsNullOrEmpty(p.Type) &&
                           p.Type.Contains(type, StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p.SortOrder)
                .ThenByDescending(p => p.CreatedAt);
            
            return _mapper.Map<IEnumerable<PortfolioResponseDto>>(searchResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching portfolios by type {Type}", type);
            throw;
        }
    }

    public async Task<IEnumerable<PortfolioResponseDto>> SearchPortfoliosAsync(string keyword, bool publicOnly = true)
    {
        try
        {
            var portfolios = await _dataService.GetPortfoliosAsync();
            var searchResults = portfolios.Where(p =>
                (!publicOnly || p.IsPublic) &&
                (p.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                 (!string.IsNullOrEmpty(p.Description) && p.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                 (!string.IsNullOrEmpty(p.Type) && p.Type.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                 (!string.IsNullOrEmpty(p.Technologies) && p.Technologies.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            ).OrderBy(p => p.SortOrder).ThenByDescending(p => p.CreatedAt);
            
            return _mapper.Map<IEnumerable<PortfolioResponseDto>>(searchResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching portfolios with keyword {Keyword}", keyword);
            throw;
        }
    }

    public async Task<IEnumerable<PortfolioResponseDto>> GetPortfoliosByDateRangeAsync(DateTime? startDate, DateTime? endDate, bool publicOnly = true)
    {
        try
        {
            var portfolios = await _dataService.GetPortfoliosAsync();
            var filteredPortfolios = portfolios.Where(p =>
            {
                if (publicOnly && !p.IsPublic) return false;
                
                // 檢查作品的時間範圍是否與查詢範圍重疊
                if (startDate.HasValue && p.EndDate.HasValue && p.EndDate < startDate) return false;
                if (endDate.HasValue && p.StartDate.HasValue && p.StartDate > endDate) return false;
                
                return true;
            }).OrderBy(p => p.SortOrder).ThenByDescending(p => p.CreatedAt);
            
            return _mapper.Map<IEnumerable<PortfolioResponseDto>>(filteredPortfolios);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting portfolios by date range");
            throw;
        }
    }

    public async Task<PortfolioResponseDto> CreatePortfolioAsync(CreatePortfolioDto createPortfolioDto)
    {
        try
        {
            // 驗證作品類型
            if (!string.IsNullOrEmpty(createPortfolioDto.Type) && 
                !_validTypes.Contains(createPortfolioDto.Type))
            {
                _logger.LogWarning("Invalid portfolio type provided: {Type}", createPortfolioDto.Type);
                // 仍然允許創建，但記錄警告
            }

            var portfolios = await _dataService.GetPortfoliosAsync();
            
            // 檢查同一使用者是否已有相同標題的作品
            var existingPortfolio = portfolios.FirstOrDefault(p => 
                p.UserId == createPortfolioDto.UserId &&
                p.Title.Equals(createPortfolioDto.Title, StringComparison.OrdinalIgnoreCase));
                
            if (existingPortfolio != null)
            {
                throw new InvalidOperationException("該使用者已有相同標題的作品");
            }

            var portfolio = _mapper.Map<Models.Portfolio>(createPortfolioDto);
            
            // 生成新ID
            portfolio.Id = portfolios.Any() ? portfolios.Max(p => p.Id) + 1 : 1;
            
            // 設定時間戳記
            portfolio.CreatedAt = DateTime.UtcNow;
            portfolio.UpdatedAt = DateTime.UtcNow;

            portfolios.Add(portfolio);
            await _dataService.SavePortfoliosAsync(portfolios);

            _logger.LogInformation("Portfolio created with ID {Id} for user {UserId}", portfolio.Id, portfolio.UserId);
            return _mapper.Map<PortfolioResponseDto>(portfolio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating portfolio");
            throw;
        }
    }

    public async Task<PortfolioResponseDto?> UpdatePortfolioAsync(int id, UpdatePortfolioDto updatePortfolioDto)
    {
        try
        {
            var portfolios = await _dataService.GetPortfoliosAsync();
            var existingPortfolio = portfolios.FirstOrDefault(p => p.Id == id);
            
            if (existingPortfolio == null)
            {
                _logger.LogWarning("Portfolio with ID {Id} not found for update", id);
                return null;
            }

            // 驗證作品類型（如果有提供）
            if (!string.IsNullOrEmpty(updatePortfolioDto.Type) && 
                !_validTypes.Contains(updatePortfolioDto.Type))
            {
                _logger.LogWarning("Invalid portfolio type provided for update: {Type}", updatePortfolioDto.Type);
            }

            // 檢查標題重複（如果有提供新標題）
            var newTitle = updatePortfolioDto.Title ?? existingPortfolio.Title;
            
            var duplicatePortfolio = portfolios.FirstOrDefault(p => 
                p.Id != id &&
                p.UserId == existingPortfolio.UserId &&
                p.Title.Equals(newTitle, StringComparison.OrdinalIgnoreCase));
                
            if (duplicatePortfolio != null)
            {
                throw new InvalidOperationException("該使用者已有相同標題的作品");
            }

            _mapper.Map(updatePortfolioDto, existingPortfolio);
            existingPortfolio.UpdatedAt = DateTime.UtcNow;

            await _dataService.SavePortfoliosAsync(portfolios);
            _logger.LogInformation("Portfolio with ID {Id} updated", id);
            
            return _mapper.Map<PortfolioResponseDto>(existingPortfolio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating portfolio with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeletePortfolioAsync(int id)
    {
        try
        {
            var portfolios = await _dataService.GetPortfoliosAsync();
            var portfolio = portfolios.FirstOrDefault(p => p.Id == id);
            
            if (portfolio == null)
            {
                _logger.LogWarning("Portfolio with ID {Id} not found for deletion", id);
                return false;
            }

            portfolios.Remove(portfolio);
            await _dataService.SavePortfoliosAsync(portfolios);
            _logger.LogInformation("Portfolio with ID {Id} deleted", id);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting portfolio with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> UpdatePortfolioOrderAsync(int id, int newSortOrder)
    {
        try
        {
            var portfolios = await _dataService.GetPortfoliosAsync();
            var portfolio = portfolios.FirstOrDefault(p => p.Id == id);
            
            if (portfolio == null)
            {
                _logger.LogWarning("Portfolio with ID {Id} not found for order update", id);
                return false;
            }

            portfolio.SortOrder = newSortOrder;
            portfolio.UpdatedAt = DateTime.UtcNow;

            await _dataService.SavePortfoliosAsync(portfolios);
            _logger.LogInformation("Portfolio with ID {Id} order updated to {Order}", id, newSortOrder);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating portfolio order for ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> BatchUpdatePortfolioOrderAsync(Dictionary<int, int> portfolioOrders)
    {
        try
        {
            var portfolios = await _dataService.GetPortfoliosAsync();
            var updated = false;
            
            foreach (var portfolioOrder in portfolioOrders)
            {
                var portfolio = portfolios.FirstOrDefault(p => p.Id == portfolioOrder.Key);
                if (portfolio != null)
                {
                    portfolio.SortOrder = portfolioOrder.Value;
                    portfolio.UpdatedAt = DateTime.UtcNow;
                    updated = true;
                }
            }

            if (updated)
            {
                await _dataService.SavePortfoliosAsync(portfolios);
                _logger.LogInformation("Batch updated portfolio orders for {Count} portfolios", portfolioOrders.Count);
            }
            
            return updated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while batch updating portfolio orders");
            throw;
        }
    }

    public async Task<object> GetPortfolioStatsAsync()
    {
        try
        {
            var portfolios = await _dataService.GetPortfoliosAsync();
            var today = DateTime.Today;
            var thisMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            var stats = new
            {
                TotalPortfolios = portfolios.Count(),
                PublicPortfolios = portfolios.Count(p => p.IsPublic),
                PrivatePortfolios = portfolios.Count(p => !p.IsPublic),
                FeaturedPortfolios = portfolios.Count(p => p.IsFeatured),
                
                PortfoliosWithProjectUrl = portfolios.Count(p => !string.IsNullOrEmpty(p.ProjectUrl)),
                PortfoliosWithRepositoryUrl = portfolios.Count(p => !string.IsNullOrEmpty(p.RepositoryUrl)),
                PortfoliosWithImage = portfolios.Count(p => !string.IsNullOrEmpty(p.ImageUrl)),
                PortfoliosWithDescription = portfolios.Count(p => !string.IsNullOrEmpty(p.Description)),
                
                TodayCreated = portfolios.Count(p => p.CreatedAt.Date == today),
                ThisMonthCreated = portfolios.Count(p => p.CreatedAt >= thisMonth),
                RecentlyUpdated = portfolios.Count(p => p.UpdatedAt >= today.AddDays(-7)),
                
                TopTypes = portfolios
                    .Where(p => !string.IsNullOrEmpty(p.Type))
                    .GroupBy(p => p.Type)
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10),
                
                TopTechnologies = portfolios
                    .Where(p => !string.IsNullOrEmpty(p.Technologies))
                    .SelectMany(p => p.Technologies!.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim()))
                    .GroupBy(t => t, StringComparer.OrdinalIgnoreCase)
                    .Select(g => new { Technology = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(15),
                
                ProjectDurationStats = new
                {
                    Count = portfolios.Count(p => p.StartDate.HasValue && p.EndDate.HasValue),
                    AverageDays = portfolios
                        .Where(p => p.StartDate.HasValue && p.EndDate.HasValue)
                        .Select(p => (p.EndDate!.Value - p.StartDate!.Value).TotalDays)
                        .DefaultIfEmpty(0)
                        .Average(),
                    MinDays = portfolios
                        .Where(p => p.StartDate.HasValue && p.EndDate.HasValue)
                        .Select(p => (p.EndDate!.Value - p.StartDate!.Value).TotalDays)
                        .DefaultIfEmpty(0)
                        .Min(),
                    MaxDays = portfolios
                        .Where(p => p.StartDate.HasValue && p.EndDate.HasValue)
                        .Select(p => (p.EndDate!.Value - p.StartDate!.Value).TotalDays)
                        .DefaultIfEmpty(0)
                        .Max()
                },
                
                PortfoliosByYear = portfolios
                    .Where(p => p.StartDate.HasValue)
                    .GroupBy(p => p.StartDate!.Value.Year)
                    .Select(g => new { Year = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Year),
                
                UniqueTypes = portfolios
                    .Where(p => !string.IsNullOrEmpty(p.Type))
                    .Select(p => p.Type)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count(),
                
                UniqueTechnologies = portfolios
                    .Where(p => !string.IsNullOrEmpty(p.Technologies))
                    .SelectMany(p => p.Technologies!.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim()))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count()
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting portfolio statistics");
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetTechnologiesAsync(bool publicOnly = true)
    {
        try
        {
            var portfolios = await _dataService.GetPortfoliosAsync();
            var technologies = portfolios
                .Where(p => !publicOnly || p.IsPublic)
                .Where(p => !string.IsNullOrEmpty(p.Technologies))
                .SelectMany(p => p.Technologies!.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim()))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(t => t);
            
            return technologies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting technologies");
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetPortfolioTypesAsync(bool publicOnly = true)
    {
        try
        {
            var portfolios = await _dataService.GetPortfoliosAsync();
            var types = portfolios
                .Where(p => !publicOnly || p.IsPublic)
                .Where(p => !string.IsNullOrEmpty(p.Type))
                .Select(p => p.Type!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(t => t);
            
            return types;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting portfolio types");
            throw;
        }
    }
}