using PersonalManagerAPI.DTOs.PersonalProfile;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services;
using PersonalManagerAPI.Services.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace PersonalManagerAPI.Services.Implementation;

/// <summary>
/// 個人資料服務實作
/// </summary>
public class PersonalProfileService : IPersonalProfileService
{
    private readonly JsonDataService _dataService;
    private readonly IMapper _mapper;
    private readonly ILogger<PersonalProfileService> _logger;

    public PersonalProfileService(JsonDataService dataService, IMapper mapper, ILogger<PersonalProfileService> logger)
    {
        _dataService = dataService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<PersonalProfileResponseDto>> GetAllProfilesAsync(int skip = 0, int take = 50)
    {
        try
        {
            var profiles = await _dataService.GetPersonalProfilesAsync();
            var pagedProfiles = profiles
                .OrderByDescending(p => p.UpdatedAt)
                .Skip(skip)
                .Take(take);
            
            return _mapper.Map<IEnumerable<PersonalProfileResponseDto>>(pagedProfiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all profiles");
            throw;
        }
    }

    public async Task<PersonalProfileResponseDto?> GetProfileByIdAsync(int id)
    {
        try
        {
            var profiles = await _dataService.GetPersonalProfilesAsync();
            var profile = profiles.FirstOrDefault(p => p.Id == id);
            return _mapper.Map<PersonalProfileResponseDto?>(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting profile with ID {Id}", id);
            throw;
        }
    }

    public async Task<PersonalProfileResponseDto?> GetProfileByUserIdAsync(int userId)
    {
        try
        {
            var profiles = await _dataService.GetPersonalProfilesAsync();
            var profile = profiles.FirstOrDefault(p => p.UserId == userId);
            return _mapper.Map<PersonalProfileResponseDto?>(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting profile for user ID {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<PersonalProfileResponseDto>> GetPublicProfilesAsync(int skip = 0, int take = 50)
    {
        try
        {
            var profiles = await _dataService.GetPersonalProfilesAsync();
            var publicProfiles = profiles
                .Where(p => p.IsPublic)
                .OrderByDescending(p => p.UpdatedAt)
                .Skip(skip)
                .Take(take);
            
            return _mapper.Map<IEnumerable<PersonalProfileResponseDto>>(publicProfiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting public profiles");
            throw;
        }
    }

    public async Task<PersonalProfileResponseDto> CreateProfileAsync(CreatePersonalProfileDto createProfileDto)
    {
        try
        {
            // 檢查使用者是否已有個人資料
            if (await HasUserProfileAsync(createProfileDto.UserId))
            {
                throw new InvalidOperationException("該使用者已有個人資料");
            }

            // 驗證社群媒體連結格式
            if (!ValidateSocialMediaUrls(createProfileDto))
            {
                throw new ArgumentException("社群媒體連結格式不正確");
            }

            var profiles = await _dataService.GetPersonalProfilesAsync();
            var profile = _mapper.Map<PersonalProfile>(createProfileDto);
            
            // 生成新ID
            profile.Id = profiles.Any() ? profiles.Max(p => p.Id) + 1 : 1;
            
            // 設定時間戳記
            profile.CreatedAt = DateTime.UtcNow;
            profile.UpdatedAt = DateTime.UtcNow;

            profiles.Add(profile);
            await _dataService.SavePersonalProfilesAsync(profiles);

            _logger.LogInformation("Personal profile created with ID {Id} for user {UserId}", profile.Id, profile.UserId);
            return _mapper.Map<PersonalProfileResponseDto>(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating personal profile");
            throw;
        }
    }

    public async Task<PersonalProfileResponseDto?> UpdateProfileAsync(int id, UpdatePersonalProfileDto updateProfileDto)
    {
        try
        {
            var profiles = await _dataService.GetPersonalProfilesAsync();
            var existingProfile = profiles.FirstOrDefault(p => p.Id == id);
            
            if (existingProfile == null)
            {
                _logger.LogWarning("Personal profile with ID {Id} not found for update", id);
                return null;
            }

            _mapper.Map(updateProfileDto, existingProfile);
            existingProfile.UpdatedAt = DateTime.UtcNow;

            await _dataService.SavePersonalProfilesAsync(profiles);
            _logger.LogInformation("Personal profile with ID {Id} updated", id);
            
            return _mapper.Map<PersonalProfileResponseDto>(existingProfile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating personal profile with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteProfileAsync(int id)
    {
        try
        {
            var profiles = await _dataService.GetPersonalProfilesAsync();
            var profile = profiles.FirstOrDefault(p => p.Id == id);
            
            if (profile == null)
            {
                _logger.LogWarning("Personal profile with ID {Id} not found for deletion", id);
                return false;
            }

            profiles.Remove(profile);
            await _dataService.SavePersonalProfilesAsync(profiles);
            _logger.LogInformation("Personal profile with ID {Id} deleted", id);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting personal profile with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> TogglePublicStatusAsync(int id)
    {
        try
        {
            var profiles = await _dataService.GetPersonalProfilesAsync();
            var profile = profiles.FirstOrDefault(p => p.Id == id);
            
            if (profile == null)
            {
                _logger.LogWarning("Personal profile with ID {Id} not found for toggle", id);
                return false;
            }

            profile.IsPublic = !profile.IsPublic;
            profile.UpdatedAt = DateTime.UtcNow;

            await _dataService.SavePersonalProfilesAsync(profiles);
            _logger.LogInformation("Personal profile with ID {Id} public status toggled to {Status}", id, profile.IsPublic);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while toggling public status for profile with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> HasUserProfileAsync(int userId)
    {
        try
        {
            var profiles = await _dataService.GetPersonalProfilesAsync();
            return profiles.Any(p => p.UserId == userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if user {UserId} has profile", userId);
            throw;
        }
    }

    public async Task<IEnumerable<PersonalProfileResponseDto>> SearchProfilesAsync(string keyword, bool publicOnly = true)
    {
        try
        {
            var profiles = await _dataService.GetPersonalProfilesAsync();
            var searchResults = profiles.Where(p =>
                (!publicOnly || p.IsPublic) &&
                ((!string.IsNullOrEmpty(p.Title) && p.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                 (!string.IsNullOrEmpty(p.Summary) && p.Summary.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                 (!string.IsNullOrEmpty(p.Description) && p.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                 (!string.IsNullOrEmpty(p.Location) && p.Location.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            ).OrderByDescending(p => p.UpdatedAt);
            
            return _mapper.Map<IEnumerable<PersonalProfileResponseDto>>(searchResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching profiles with keyword {Keyword}", keyword);
            throw;
        }
    }

    public async Task<object> GetProfileStatsAsync()
    {
        try
        {
            var profiles = await _dataService.GetPersonalProfilesAsync();
            var now = DateTime.UtcNow;
            var today = DateTime.Today;
            var thisMonth = new DateTime(now.Year, now.Month, 1);

            var stats = new
            {
                TotalProfiles = profiles.Count(),
                PublicProfiles = profiles.Count(p => p.IsPublic),
                PrivateProfiles = profiles.Count(p => !p.IsPublic),
                
                ProfilesWithProfileImage = profiles.Count(p => !string.IsNullOrEmpty(p.ProfileImageUrl)),
                ProfilesWithWebsite = profiles.Count(p => !string.IsNullOrEmpty(p.Website)),
                ProfilesWithLocation = profiles.Count(p => !string.IsNullOrEmpty(p.Location)),
                
                TodayCreated = profiles.Count(p => p.CreatedAt.Date == today),
                ThisMonthCreated = profiles.Count(p => p.CreatedAt >= thisMonth),
                RecentlyUpdated = profiles.Count(p => p.UpdatedAt >= today.AddDays(-7)),
                
                ProfilesWithSummary = profiles.Count(p => !string.IsNullOrEmpty(p.Summary)),
                ProfilesWithDescription = profiles.Count(p => !string.IsNullOrEmpty(p.Description)),
                ProfilesWithBirthday = profiles.Count(p => p.Birthday.HasValue),
                
                MonthlyCreation = profiles
                    .Where(p => p.CreatedAt >= DateTime.Today.AddMonths(-12))
                    .GroupBy(p => p.CreatedAt.ToString("yyyy-MM"))
                    .Select(g => new { Month = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Month)
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting profile statistics");
            throw;
        }
    }

    public bool ValidateSocialMediaUrls(CreatePersonalProfileDto profileDto)
    {
        try
        {
            // 驗證網站URL格式（如果提供）
            if (!string.IsNullOrEmpty(profileDto.Website))
            {
                if (!Uri.TryCreate(profileDto.Website, UriKind.Absolute, out var websiteUri) || 
                    (websiteUri.Scheme != Uri.UriSchemeHttp && websiteUri.Scheme != Uri.UriSchemeHttps))
                {
                    _logger.LogWarning("Invalid website URL format: {Url}", profileDto.Website);
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while validating URLs");
            return false;
        }
    }
}