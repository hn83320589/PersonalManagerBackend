using PersonalManagerAPI.DTOs.WorkExperience;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services;
using PersonalManagerAPI.Services.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace PersonalManagerAPI.Services.Implementation;

/// <summary>
/// 工作經歷服務實作
/// </summary>
public class WorkExperienceService : IWorkExperienceService
{
    private readonly JsonDataService _dataService;
    private readonly IMapper _mapper;
    private readonly ILogger<WorkExperienceService> _logger;

    public WorkExperienceService(JsonDataService dataService, IMapper mapper, ILogger<WorkExperienceService> logger)
    {
        _dataService = dataService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<WorkExperienceResponseDto>> GetAllWorkExperiencesAsync(int skip = 0, int take = 50)
    {
        try
        {
            var workExperiences = await _dataService.GetWorkExperiencesAsync();
            var pagedWorkExperiences = workExperiences
                .OrderBy(w => w.SortOrder)
                .ThenByDescending(w => w.StartDate)
                .Skip(skip)
                .Take(take);
            
            return _mapper.Map<IEnumerable<WorkExperienceResponseDto>>(pagedWorkExperiences);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all work experiences");
            throw;
        }
    }

    public async Task<WorkExperienceResponseDto?> GetWorkExperienceByIdAsync(int id)
    {
        try
        {
            var workExperiences = await _dataService.GetWorkExperiencesAsync();
            var workExperience = workExperiences.FirstOrDefault(w => w.Id == id);
            return _mapper.Map<WorkExperienceResponseDto?>(workExperience);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting work experience with ID {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<WorkExperienceResponseDto>> GetWorkExperiencesByUserIdAsync(int userId)
    {
        try
        {
            var workExperiences = await _dataService.GetWorkExperiencesAsync();
            var userWorkExperiences = workExperiences
                .Where(w => w.UserId == userId)
                .OrderBy(w => w.SortOrder)
                .ThenByDescending(w => w.StartDate);
            
            return _mapper.Map<IEnumerable<WorkExperienceResponseDto>>(userWorkExperiences);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting work experiences for user ID {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<WorkExperienceResponseDto>> GetPublicWorkExperiencesAsync(int skip = 0, int take = 50)
    {
        try
        {
            var workExperiences = await _dataService.GetWorkExperiencesAsync();
            var publicWorkExperiences = workExperiences
                .Where(w => w.IsPublic)
                .OrderBy(w => w.SortOrder)
                .ThenByDescending(w => w.StartDate)
                .Skip(skip)
                .Take(take);
            
            return _mapper.Map<IEnumerable<WorkExperienceResponseDto>>(publicWorkExperiences);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting public work experiences");
            throw;
        }
    }

    public async Task<IEnumerable<WorkExperienceResponseDto>> GetCurrentWorkExperiencesAsync(bool publicOnly = true)
    {
        try
        {
            var workExperiences = await _dataService.GetWorkExperiencesAsync();
            var currentWorkExperiences = workExperiences
                .Where(w => (!publicOnly || w.IsPublic) && w.IsCurrent)
                .OrderBy(w => w.SortOrder)
                .ThenByDescending(w => w.StartDate);
            
            return _mapper.Map<IEnumerable<WorkExperienceResponseDto>>(currentWorkExperiences);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting current work experiences");
            throw;
        }
    }

    public async Task<IEnumerable<WorkExperienceResponseDto>> SearchByCompanyAsync(string company, bool publicOnly = true)
    {
        try
        {
            var workExperiences = await _dataService.GetWorkExperiencesAsync();
            var searchResults = workExperiences
                .Where(w => (!publicOnly || w.IsPublic) && 
                           w.Company.Contains(company, StringComparison.OrdinalIgnoreCase))
                .OrderBy(w => w.SortOrder)
                .ThenByDescending(w => w.StartDate);
            
            return _mapper.Map<IEnumerable<WorkExperienceResponseDto>>(searchResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching work experiences by company {Company}", company);
            throw;
        }
    }

    public async Task<IEnumerable<WorkExperienceResponseDto>> SearchByPositionAsync(string position, bool publicOnly = true)
    {
        try
        {
            var workExperiences = await _dataService.GetWorkExperiencesAsync();
            var searchResults = workExperiences
                .Where(w => (!publicOnly || w.IsPublic) && 
                           w.Position.Contains(position, StringComparison.OrdinalIgnoreCase))
                .OrderBy(w => w.SortOrder)
                .ThenByDescending(w => w.StartDate);
            
            return _mapper.Map<IEnumerable<WorkExperienceResponseDto>>(searchResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching work experiences by position {Position}", position);
            throw;
        }
    }

    public async Task<IEnumerable<WorkExperienceResponseDto>> SearchWorkExperiencesAsync(string keyword, bool publicOnly = true)
    {
        try
        {
            var workExperiences = await _dataService.GetWorkExperiencesAsync();
            var searchResults = workExperiences.Where(w =>
                (!publicOnly || w.IsPublic) &&
                (w.Company.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                 w.Position.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                 (!string.IsNullOrEmpty(w.Description) && w.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                 (!string.IsNullOrEmpty(w.Department) && w.Department.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                 (!string.IsNullOrEmpty(w.Location) && w.Location.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            ).OrderBy(w => w.SortOrder).ThenByDescending(w => w.StartDate);
            
            return _mapper.Map<IEnumerable<WorkExperienceResponseDto>>(searchResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching work experiences with keyword {Keyword}", keyword);
            throw;
        }
    }

    public async Task<IEnumerable<WorkExperienceResponseDto>> GetWorkExperiencesByDateRangeAsync(DateTime? startDate, DateTime? endDate, bool publicOnly = true)
    {
        try
        {
            var workExperiences = await _dataService.GetWorkExperiencesAsync();
            var filteredWorkExperiences = workExperiences.Where(w =>
            {
                if (publicOnly && !w.IsPublic) return false;
                
                // 檢查工作經歷的時間範圍是否與查詢範圍重疊
                if (startDate.HasValue && w.EndDate.HasValue && w.EndDate < startDate) return false;
                if (endDate.HasValue && w.StartDate > endDate) return false;
                
                return true;
            }).OrderBy(w => w.SortOrder).ThenByDescending(w => w.StartDate);
            
            return _mapper.Map<IEnumerable<WorkExperienceResponseDto>>(filteredWorkExperiences);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting work experiences by date range");
            throw;
        }
    }

    public async Task<WorkExperienceResponseDto> CreateWorkExperienceAsync(CreateWorkExperienceDto createWorkExperienceDto)
    {
        try
        {
            // 驗證日期
            if (!ValidateWorkExperienceDates(createWorkExperienceDto.StartDate, createWorkExperienceDto.EndDate))
            {
                throw new InvalidOperationException("工作經歷日期不正確：結束日期不能早於開始日期");
            }

            var workExperiences = await _dataService.GetWorkExperiencesAsync();
            
            // 檢查同一使用者是否已有相同公司和職位的工作經歷
            var existingWorkExperience = workExperiences.FirstOrDefault(w => 
                w.UserId == createWorkExperienceDto.UserId &&
                w.Company.Equals(createWorkExperienceDto.Company, StringComparison.OrdinalIgnoreCase) &&
                w.Position.Equals(createWorkExperienceDto.Position, StringComparison.OrdinalIgnoreCase) &&
                Math.Abs((w.StartDate - createWorkExperienceDto.StartDate).TotalDays) < 30); // 30天內算重複
                
            if (existingWorkExperience != null)
            {
                throw new InvalidOperationException("該使用者已有相似的工作經歷記錄");
            }

            var workExperience = _mapper.Map<Models.WorkExperience>(createWorkExperienceDto);
            
            // 生成新ID
            workExperience.Id = workExperiences.Any() ? workExperiences.Max(w => w.Id) + 1 : 1;
            
            // 設定時間戳記
            workExperience.CreatedAt = DateTime.UtcNow;
            workExperience.UpdatedAt = DateTime.UtcNow;

            // 如果設定為當前工作，取消同一使用者的其他當前工作狀態
            if (workExperience.IsCurrent)
            {
                var userCurrentJobs = workExperiences.Where(w => w.UserId == workExperience.UserId && w.IsCurrent);
                foreach (var job in userCurrentJobs)
                {
                    job.IsCurrent = false;
                    job.UpdatedAt = DateTime.UtcNow;
                }
            }

            workExperiences.Add(workExperience);
            await _dataService.SaveWorkExperiencesAsync(workExperiences);

            _logger.LogInformation("Work experience created with ID {Id} for user {UserId}", workExperience.Id, workExperience.UserId);
            return _mapper.Map<WorkExperienceResponseDto>(workExperience);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating work experience");
            throw;
        }
    }

    public async Task<WorkExperienceResponseDto?> UpdateWorkExperienceAsync(int id, UpdateWorkExperienceDto updateWorkExperienceDto)
    {
        try
        {
            var workExperiences = await _dataService.GetWorkExperiencesAsync();
            var existingWorkExperience = workExperiences.FirstOrDefault(w => w.Id == id);
            
            if (existingWorkExperience == null)
            {
                _logger.LogWarning("Work experience with ID {Id} not found for update", id);
                return null;
            }

            // 驗證日期（如果有提供）
            var startDate = updateWorkExperienceDto.StartDate ?? existingWorkExperience.StartDate;
            var endDate = updateWorkExperienceDto.EndDate ?? existingWorkExperience.EndDate;
            
            if (!ValidateWorkExperienceDates(startDate, endDate))
            {
                throw new InvalidOperationException("工作經歷日期不正確：結束日期不能早於開始日期");
            }

            // 檢查重複（如果有提供新值）
            var newCompany = updateWorkExperienceDto.Company ?? existingWorkExperience.Company;
            var newPosition = updateWorkExperienceDto.Position ?? existingWorkExperience.Position;
            var newStartDate = updateWorkExperienceDto.StartDate ?? existingWorkExperience.StartDate;
            
            var duplicateWorkExperience = workExperiences.FirstOrDefault(w => 
                w.Id != id &&
                w.UserId == existingWorkExperience.UserId &&
                w.Company.Equals(newCompany, StringComparison.OrdinalIgnoreCase) &&
                w.Position.Equals(newPosition, StringComparison.OrdinalIgnoreCase) &&
                Math.Abs((w.StartDate - newStartDate).TotalDays) < 30);
                
            if (duplicateWorkExperience != null)
            {
                throw new InvalidOperationException("該使用者已有相似的工作經歷記錄");
            }

            _mapper.Map(updateWorkExperienceDto, existingWorkExperience);
            existingWorkExperience.UpdatedAt = DateTime.UtcNow;

            // 如果更新為當前工作，取消同一使用者的其他當前工作狀態
            if (updateWorkExperienceDto.IsCurrent == true)
            {
                var userCurrentJobs = workExperiences.Where(w => w.UserId == existingWorkExperience.UserId && w.Id != id && w.IsCurrent);
                foreach (var job in userCurrentJobs)
                {
                    job.IsCurrent = false;
                    job.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _dataService.SaveWorkExperiencesAsync(workExperiences);
            _logger.LogInformation("Work experience with ID {Id} updated", id);
            
            return _mapper.Map<WorkExperienceResponseDto>(existingWorkExperience);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating work experience with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteWorkExperienceAsync(int id)
    {
        try
        {
            var workExperiences = await _dataService.GetWorkExperiencesAsync();
            var workExperience = workExperiences.FirstOrDefault(w => w.Id == id);
            
            if (workExperience == null)
            {
                _logger.LogWarning("Work experience with ID {Id} not found for deletion", id);
                return false;
            }

            workExperiences.Remove(workExperience);
            await _dataService.SaveWorkExperiencesAsync(workExperiences);
            _logger.LogInformation("Work experience with ID {Id} deleted", id);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting work experience with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> UpdateWorkExperienceOrderAsync(int id, int newSortOrder)
    {
        try
        {
            var workExperiences = await _dataService.GetWorkExperiencesAsync();
            var workExperience = workExperiences.FirstOrDefault(w => w.Id == id);
            
            if (workExperience == null)
            {
                _logger.LogWarning("Work experience with ID {Id} not found for order update", id);
                return false;
            }

            workExperience.SortOrder = newSortOrder;
            workExperience.UpdatedAt = DateTime.UtcNow;

            await _dataService.SaveWorkExperiencesAsync(workExperiences);
            _logger.LogInformation("Work experience with ID {Id} order updated to {Order}", id, newSortOrder);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating work experience order for ID {Id}", id);
            throw;
        }
    }

    public async Task<object> GetWorkExperienceStatsAsync()
    {
        try
        {
            var workExperiences = await _dataService.GetWorkExperiencesAsync();
            var today = DateTime.Today;
            var thisMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            var stats = new
            {
                TotalWorkExperiences = workExperiences.Count(),
                PublicWorkExperiences = workExperiences.Count(w => w.IsPublic),
                PrivateWorkExperiences = workExperiences.Count(w => !w.IsPublic),
                CurrentJobs = workExperiences.Count(w => w.IsCurrent),
                
                WorkExperiencesWithSalary = workExperiences.Count(w => w.Salary.HasValue && w.Salary > 0),
                WorkExperiencesWithDescription = workExperiences.Count(w => !string.IsNullOrEmpty(w.Description)),
                
                TodayCreated = workExperiences.Count(w => w.CreatedAt.Date == today),
                ThisMonthCreated = workExperiences.Count(w => w.CreatedAt >= thisMonth),
                RecentlyUpdated = workExperiences.Count(w => w.UpdatedAt >= today.AddDays(-7)),
                
                TopCompanies = workExperiences
                    .GroupBy(w => w.Company)
                    .Select(g => new { Company = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10),
                
                TopPositions = workExperiences
                    .GroupBy(w => w.Position)
                    .Select(g => new { Position = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10),
                
                AverageJobDuration = workExperiences
                    .Where(w => w.EndDate.HasValue)
                    .Select(w => (w.EndDate!.Value - w.StartDate).TotalDays)
                    .DefaultIfEmpty(0)
                    .Average(),
                
                JobsByYear = workExperiences
                    .GroupBy(w => w.StartDate.Year)
                    .Select(g => new { Year = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Year),
                
                SalaryStats = new
                {
                    Count = workExperiences.Count(w => w.Salary.HasValue && w.Salary > 0),
                    Average = workExperiences.Where(w => w.Salary.HasValue && w.Salary > 0).Select(w => w.Salary!.Value).DefaultIfEmpty(0).Average(),
                    Min = workExperiences.Where(w => w.Salary.HasValue && w.Salary > 0).Select(w => w.Salary!.Value).DefaultIfEmpty(0).Min(),
                    Max = workExperiences.Where(w => w.Salary.HasValue && w.Salary > 0).Select(w => w.Salary!.Value).DefaultIfEmpty(0).Max()
                }
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting work experience statistics");
            throw;
        }
    }

    public bool ValidateWorkExperienceDates(DateTime startDate, DateTime? endDate)
    {
        try
        {
            // 開始日期不能是未來
            if (startDate > DateTime.Today)
                return false;

            // 如果有結束日期，結束日期不能早於開始日期
            if (endDate.HasValue && endDate.Value < startDate)
                return false;

            // 結束日期不能是未來（除非是今天，代表剛結束）
            if (endDate.HasValue && endDate.Value > DateTime.Today)
                return false;

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while validating work experience dates");
            return false;
        }
    }
}