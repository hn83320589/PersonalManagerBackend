using PersonalManagerAPI.DTOs.Education;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services;
using PersonalManagerAPI.Services.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace PersonalManagerAPI.Services.Implementation;

/// <summary>
/// 學歷服務實作
/// </summary>
public class EducationService : IEducationService
{
    private readonly JsonDataService _dataService;
    private readonly IMapper _mapper;
    private readonly ILogger<EducationService> _logger;

    public EducationService(JsonDataService dataService, IMapper mapper, ILogger<EducationService> logger)
    {
        _dataService = dataService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<EducationResponseDto>> GetAllEducationsAsync(int skip = 0, int take = 50)
    {
        try
        {
            var educations = await _dataService.GetEducationsAsync();
            var pagedEducations = educations
                .OrderBy(e => e.SortOrder)
                .ThenByDescending(e => e.StartDate)
                .Skip(skip)
                .Take(take);
            
            return _mapper.Map<IEnumerable<EducationResponseDto>>(pagedEducations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all educations");
            throw;
        }
    }

    public async Task<EducationResponseDto?> GetEducationByIdAsync(int id)
    {
        try
        {
            var educations = await _dataService.GetEducationsAsync();
            var education = educations.FirstOrDefault(e => e.Id == id);
            return _mapper.Map<EducationResponseDto?>(education);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting education with ID {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<EducationResponseDto>> GetEducationsByUserIdAsync(int userId)
    {
        try
        {
            var educations = await _dataService.GetEducationsAsync();
            var userEducations = educations
                .Where(e => e.UserId == userId)
                .OrderBy(e => e.SortOrder)
                .ThenByDescending(e => e.StartDate);
            
            return _mapper.Map<IEnumerable<EducationResponseDto>>(userEducations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting educations for user ID {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<EducationResponseDto>> GetPublicEducationsAsync(int skip = 0, int take = 50)
    {
        try
        {
            var educations = await _dataService.GetEducationsAsync();
            var publicEducations = educations
                .Where(e => e.IsPublic)
                .OrderBy(e => e.SortOrder)
                .ThenByDescending(e => e.StartDate)
                .Skip(skip)
                .Take(take);
            
            return _mapper.Map<IEnumerable<EducationResponseDto>>(publicEducations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting public educations");
            throw;
        }
    }

    public async Task<EducationResponseDto> CreateEducationAsync(CreateEducationDto createEducationDto)
    {
        try
        {
            // 驗證日期邏輯（如果提供了開始和結束日期）
            if (createEducationDto.StartDate.HasValue && createEducationDto.EndDate.HasValue &&
                !ValidateEducationDates(createEducationDto.StartDate.Value, createEducationDto.EndDate))
            {
                throw new ArgumentException("結束日期不能早於開始日期");
            }

            var educations = await _dataService.GetEducationsAsync();
            var education = _mapper.Map<Education>(createEducationDto);
            
            // 生成新ID
            education.Id = educations.Any() ? educations.Max(e => e.Id) + 1 : 1;
            
            // 設定時間戳記
            education.CreatedAt = DateTime.UtcNow;
            education.UpdatedAt = DateTime.UtcNow;

            educations.Add(education);
            await _dataService.SaveEducationsAsync(educations);

            _logger.LogInformation("Education created with ID {Id} for user {UserId}", education.Id, education.UserId);
            return _mapper.Map<EducationResponseDto>(education);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating education");
            throw;
        }
    }

    public async Task<EducationResponseDto?> UpdateEducationAsync(int id, UpdateEducationDto updateEducationDto)
    {
        try
        {
            var educations = await _dataService.GetEducationsAsync();
            var existingEducation = educations.FirstOrDefault(e => e.Id == id);
            
            if (existingEducation == null)
            {
                _logger.LogWarning("Education with ID {Id} not found for update", id);
                return null;
            }

            // 驗證更新後的日期邏輯（如果都有值）
            var newStartDate = updateEducationDto.StartDate ?? existingEducation.StartDate;
            var newEndDate = updateEducationDto.EndDate ?? existingEducation.EndDate;
            
            if (newStartDate.HasValue && newEndDate.HasValue && !ValidateEducationDates(newStartDate.Value, newEndDate))
            {
                throw new ArgumentException("結束日期不能早於開始日期");
            }

            _mapper.Map(updateEducationDto, existingEducation);
            existingEducation.UpdatedAt = DateTime.UtcNow;

            await _dataService.SaveEducationsAsync(educations);
            _logger.LogInformation("Education with ID {Id} updated", id);
            
            return _mapper.Map<EducationResponseDto>(existingEducation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating education with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteEducationAsync(int id)
    {
        try
        {
            var educations = await _dataService.GetEducationsAsync();
            var education = educations.FirstOrDefault(e => e.Id == id);
            
            if (education == null)
            {
                _logger.LogWarning("Education with ID {Id} not found for deletion", id);
                return false;
            }

            educations.Remove(education);
            await _dataService.SaveEducationsAsync(educations);
            _logger.LogInformation("Education with ID {Id} deleted", id);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting education with ID {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<EducationResponseDto>> SearchBySchoolAsync(string schoolName, bool publicOnly = true)
    {
        try
        {
            var educations = await _dataService.GetEducationsAsync();
            var searchResults = educations.Where(e =>
                (!publicOnly || e.IsPublic) &&
                e.School.Contains(schoolName, StringComparison.OrdinalIgnoreCase)
            ).OrderBy(e => e.SortOrder).ThenByDescending(e => e.StartDate);
            
            return _mapper.Map<IEnumerable<EducationResponseDto>>(searchResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching educations by school {SchoolName}", schoolName);
            throw;
        }
    }

    public async Task<IEnumerable<EducationResponseDto>> SearchByDegreeAsync(string degree, bool publicOnly = true)
    {
        try
        {
            var educations = await _dataService.GetEducationsAsync();
            var searchResults = educations.Where(e =>
                (!publicOnly || e.IsPublic) &&
                e.Degree.Contains(degree, StringComparison.OrdinalIgnoreCase)
            ).OrderBy(e => e.SortOrder).ThenByDescending(e => e.StartDate);
            
            return _mapper.Map<IEnumerable<EducationResponseDto>>(searchResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching educations by degree {Degree}", degree);
            throw;
        }
    }

    public async Task<IEnumerable<EducationResponseDto>> GetEducationsByPeriodAsync(int startYear, int? endYear = null)
    {
        try
        {
            var educations = await _dataService.GetEducationsAsync();
            var periodEducations = educations.Where(e =>
                e.StartDate.HasValue && e.StartDate.Value.Year >= startYear &&
                (!endYear.HasValue || (e.EndDate.HasValue && e.EndDate.Value.Year <= endYear.Value))
            ).OrderBy(e => e.SortOrder).ThenByDescending(e => e.StartDate);
            
            return _mapper.Map<IEnumerable<EducationResponseDto>>(periodEducations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting educations by period {StartYear}-{EndYear}", startYear, endYear);
            throw;
        }
    }

    public async Task<object> GetEducationStatsAsync()
    {
        try
        {
            var educations = await _dataService.GetEducationsAsync();
            var now = DateTime.UtcNow;
            var today = DateTime.Today;
            var thisMonth = new DateTime(now.Year, now.Month, 1);

            var stats = new
            {
                TotalEducations = educations.Count(),
                PublicEducations = educations.Count(e => e.IsPublic),
                PrivateEducations = educations.Count(e => !e.IsPublic),
                
                CompletedEducations = educations.Count(e => e.EndDate.HasValue && e.EndDate.Value <= now),
                OngoingEducations = educations.Count(e => !e.EndDate.HasValue || e.EndDate.Value > now),
                
                EducationsWithDescription = educations.Count(e => !string.IsNullOrEmpty(e.Description)),
                EducationsWithStartDate = educations.Count(e => e.StartDate.HasValue),
                EducationsWithEndDate = educations.Count(e => e.EndDate.HasValue),
                
                TodayCreated = educations.Count(e => e.CreatedAt.Date == today),
                ThisMonthCreated = educations.Count(e => e.CreatedAt >= thisMonth),
                RecentlyUpdated = educations.Count(e => e.UpdatedAt >= today.AddDays(-7)),
                
                EducationsByDecade = educations
                    .Where(e => e.StartDate.HasValue)
                    .GroupBy(e => (e.StartDate!.Value.Year / 10) * 10)
                    .Select(g => new { Decade = $"{g.Key}s", Count = g.Count() })
                    .OrderBy(x => x.Decade),
                
                TopSchools = educations
                    .GroupBy(e => e.School)
                    .Select(g => new { School = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10),
                
                TopDegrees = educations
                    .GroupBy(e => e.Degree)
                    .Select(g => new { Degree = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting education statistics");
            throw;
        }
    }

    public bool ValidateEducationDates(DateTime startDate, DateTime? endDate)
    {
        try
        {
            // 如果沒有結束日期，則視為進行中，允許
            if (!endDate.HasValue)
                return true;
                
            // 結束日期不能早於開始日期
            if (endDate.Value < startDate)
                return false;
                
            // 開始日期不能是未來日期（超過一年）
            if (startDate > DateTime.UtcNow.AddYears(1))
                return false;
                
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while validating education dates");
            return false;
        }
    }
}