using PersonalManagerAPI.DTOs.Skill;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services;
using PersonalManagerAPI.Services.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace PersonalManagerAPI.Services.Implementation;

/// <summary>
/// 技能服務實作
/// </summary>
public class SkillService : ISkillService
{
    private readonly JsonDataService _dataService;
    private readonly IMapper _mapper;
    private readonly ILogger<SkillService> _logger;

    // 預定義的技能分類
    private readonly HashSet<string> _validCategories = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Programming Languages", "Web Development", "Mobile Development", 
        "Database", "DevOps", "Cloud Platforms", "Design", "Data Science",
        "Machine Learning", "Project Management", "Soft Skills", "Languages",
        "Tools", "Frameworks", "Operating Systems", "Other"
    };

    public SkillService(JsonDataService dataService, IMapper mapper, ILogger<SkillService> logger)
    {
        _dataService = dataService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<SkillResponseDto>> GetAllSkillsAsync(int skip = 0, int take = 50)
    {
        try
        {
            var skills = await _dataService.GetSkillsAsync();
            var pagedSkills = skills
                .OrderBy(s => s.SortOrder)
                .ThenBy(s => s.Category)
                .ThenBy(s => s.Name)
                .Skip(skip)
                .Take(take);
            
            return _mapper.Map<IEnumerable<SkillResponseDto>>(pagedSkills);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all skills");
            throw;
        }
    }

    public async Task<SkillResponseDto?> GetSkillByIdAsync(int id)
    {
        try
        {
            var skills = await _dataService.GetSkillsAsync();
            var skill = skills.FirstOrDefault(s => s.Id == id);
            return _mapper.Map<SkillResponseDto?>(skill);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting skill with ID {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<SkillResponseDto>> GetSkillsByUserIdAsync(int userId)
    {
        try
        {
            var skills = await _dataService.GetSkillsAsync();
            var userSkills = skills
                .Where(s => s.UserId == userId)
                .OrderBy(s => s.SortOrder)
                .ThenBy(s => s.Category)
                .ThenBy(s => s.Name);
            
            return _mapper.Map<IEnumerable<SkillResponseDto>>(userSkills);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting skills for user ID {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<SkillResponseDto>> GetPublicSkillsAsync(int skip = 0, int take = 50)
    {
        try
        {
            var skills = await _dataService.GetSkillsAsync();
            var publicSkills = skills
                .Where(s => s.IsPublic)
                .OrderBy(s => s.SortOrder)
                .ThenBy(s => s.Category)
                .ThenBy(s => s.Name)
                .Skip(skip)
                .Take(take);
            
            return _mapper.Map<IEnumerable<SkillResponseDto>>(publicSkills);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting public skills");
            throw;
        }
    }

    public async Task<SkillResponseDto> CreateSkillAsync(CreateSkillDto createSkillDto)
    {
        try
        {
            // 驗證分類
            if (!string.IsNullOrEmpty(createSkillDto.Category) && 
                !ValidateSkillCategory(createSkillDto.Category))
            {
                _logger.LogWarning("Invalid skill category provided: {Category}", createSkillDto.Category);
                // 仍然允許創建，但記錄警告
            }

            var skills = await _dataService.GetSkillsAsync();
            
            // 檢查同一使用者是否已有相同名稱和分類的技能
            var existingSkill = skills.FirstOrDefault(s => 
                s.UserId == createSkillDto.UserId &&
                s.Name.Equals(createSkillDto.Name, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(s.Category, createSkillDto.Category, StringComparison.OrdinalIgnoreCase));
                
            if (existingSkill != null)
            {
                throw new InvalidOperationException("該使用者已有相同名稱和分類的技能");
            }

            var skill = _mapper.Map<Models.Skill>(createSkillDto);
            
            // 生成新ID
            skill.Id = skills.Any() ? skills.Max(s => s.Id) + 1 : 1;
            
            // 設定時間戳記
            skill.CreatedAt = DateTime.UtcNow;
            skill.UpdatedAt = DateTime.UtcNow;

            skills.Add(skill);
            await _dataService.SaveSkillsAsync(skills);

            _logger.LogInformation("Skill created with ID {Id} for user {UserId}", skill.Id, skill.UserId);
            return _mapper.Map<SkillResponseDto>(skill);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating skill");
            throw;
        }
    }

    public async Task<SkillResponseDto?> UpdateSkillAsync(int id, UpdateSkillDto updateSkillDto)
    {
        try
        {
            var skills = await _dataService.GetSkillsAsync();
            var existingSkill = skills.FirstOrDefault(s => s.Id == id);
            
            if (existingSkill == null)
            {
                _logger.LogWarning("Skill with ID {Id} not found for update", id);
                return null;
            }

            // 驗證分類（如果有提供）
            if (!string.IsNullOrEmpty(updateSkillDto.Category) && 
                !ValidateSkillCategory(updateSkillDto.Category))
            {
                _logger.LogWarning("Invalid skill category provided for update: {Category}", updateSkillDto.Category);
            }

            // 檢查名稱和分類重複（如果有提供新值）
            var newName = updateSkillDto.Name ?? existingSkill.Name;
            var newCategory = updateSkillDto.Category ?? existingSkill.Category;
            
            var duplicateSkill = skills.FirstOrDefault(s => 
                s.Id != id &&
                s.UserId == existingSkill.UserId &&
                s.Name.Equals(newName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(s.Category, newCategory, StringComparison.OrdinalIgnoreCase));
                
            if (duplicateSkill != null)
            {
                throw new InvalidOperationException("該使用者已有相同名稱和分類的技能");
            }

            _mapper.Map(updateSkillDto, existingSkill);
            existingSkill.UpdatedAt = DateTime.UtcNow;

            await _dataService.SaveSkillsAsync(skills);
            _logger.LogInformation("Skill with ID {Id} updated", id);
            
            return _mapper.Map<SkillResponseDto>(existingSkill);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating skill with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteSkillAsync(int id)
    {
        try
        {
            var skills = await _dataService.GetSkillsAsync();
            var skill = skills.FirstOrDefault(s => s.Id == id);
            
            if (skill == null)
            {
                _logger.LogWarning("Skill with ID {Id} not found for deletion", id);
                return false;
            }

            skills.Remove(skill);
            await _dataService.SaveSkillsAsync(skills);
            _logger.LogInformation("Skill with ID {Id} deleted", id);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting skill with ID {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<SkillResponseDto>> GetSkillsByLevelAsync(SkillLevel level, bool publicOnly = true)
    {
        try
        {
            var skills = await _dataService.GetSkillsAsync();
            var levelSkills = skills
                .Where(s => (!publicOnly || s.IsPublic) && s.Level == level)
                .OrderBy(s => s.SortOrder)
                .ThenBy(s => s.Category)
                .ThenBy(s => s.Name);
            
            return _mapper.Map<IEnumerable<SkillResponseDto>>(levelSkills);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting skills by level {Level}", level);
            throw;
        }
    }

    public async Task<IEnumerable<SkillResponseDto>> GetSkillsByCategoryAsync(string category, bool publicOnly = true)
    {
        try
        {
            var skills = await _dataService.GetSkillsAsync();
            var categorySkills = skills
                .Where(s => (!publicOnly || s.IsPublic) && 
                           string.Equals(s.Category, category, StringComparison.OrdinalIgnoreCase))
                .OrderBy(s => s.SortOrder)
                .ThenBy(s => s.Name);
            
            return _mapper.Map<IEnumerable<SkillResponseDto>>(categorySkills);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting skills by category {Category}", category);
            throw;
        }
    }

    public async Task<IEnumerable<SkillResponseDto>> SearchSkillsAsync(string keyword, bool publicOnly = true)
    {
        try
        {
            var skills = await _dataService.GetSkillsAsync();
            var searchResults = skills.Where(s =>
                (!publicOnly || s.IsPublic) &&
                (s.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                 (!string.IsNullOrEmpty(s.Category) && s.Category.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                 (!string.IsNullOrEmpty(s.Description) && s.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            ).OrderBy(s => s.SortOrder).ThenBy(s => s.Category).ThenBy(s => s.Name);
            
            return _mapper.Map<IEnumerable<SkillResponseDto>>(searchResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching skills with keyword {Keyword}", keyword);
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetSkillCategoriesAsync(bool publicOnly = true)
    {
        try
        {
            var skills = await _dataService.GetSkillsAsync();
            var categories = skills
                .Where(s => !publicOnly || s.IsPublic)
                .Where(s => !string.IsNullOrEmpty(s.Category))
                .Select(s => s.Category!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c);
            
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting skill categories");
            throw;
        }
    }

    public async Task<bool> UpdateSkillOrderAsync(int id, int newSortOrder)
    {
        try
        {
            var skills = await _dataService.GetSkillsAsync();
            var skill = skills.FirstOrDefault(s => s.Id == id);
            
            if (skill == null)
            {
                _logger.LogWarning("Skill with ID {Id} not found for order update", id);
                return false;
            }

            skill.SortOrder = newSortOrder;
            skill.UpdatedAt = DateTime.UtcNow;

            await _dataService.SaveSkillsAsync(skills);
            _logger.LogInformation("Skill with ID {Id} order updated to {Order}", id, newSortOrder);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating skill order for ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> BatchUpdateSkillOrderAsync(Dictionary<int, int> skillOrders)
    {
        try
        {
            var skills = await _dataService.GetSkillsAsync();
            var updated = false;
            
            foreach (var skillOrder in skillOrders)
            {
                var skill = skills.FirstOrDefault(s => s.Id == skillOrder.Key);
                if (skill != null)
                {
                    skill.SortOrder = skillOrder.Value;
                    skill.UpdatedAt = DateTime.UtcNow;
                    updated = true;
                }
            }

            if (updated)
            {
                await _dataService.SaveSkillsAsync(skills);
                _logger.LogInformation("Batch updated skill orders for {Count} skills", skillOrders.Count);
            }
            
            return updated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while batch updating skill orders");
            throw;
        }
    }

    public async Task<object> GetSkillStatsAsync()
    {
        try
        {
            var skills = await _dataService.GetSkillsAsync();
            var today = DateTime.Today;
            var thisMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            var stats = new
            {
                TotalSkills = skills.Count(),
                PublicSkills = skills.Count(s => s.IsPublic),
                PrivateSkills = skills.Count(s => !s.IsPublic),
                
                SkillsByLevel = new
                {
                    Beginner = skills.Count(s => s.Level == SkillLevel.Beginner),
                    Intermediate = skills.Count(s => s.Level == SkillLevel.Intermediate),
                    Advanced = skills.Count(s => s.Level == SkillLevel.Advanced),
                    Expert = skills.Count(s => s.Level == SkillLevel.Expert)
                },
                
                SkillsWithDescription = skills.Count(s => !string.IsNullOrEmpty(s.Description)),
                SkillsWithCategory = skills.Count(s => !string.IsNullOrEmpty(s.Category)),
                
                TodayCreated = skills.Count(s => s.CreatedAt.Date == today),
                ThisMonthCreated = skills.Count(s => s.CreatedAt >= thisMonth),
                RecentlyUpdated = skills.Count(s => s.UpdatedAt >= today.AddDays(-7)),
                
                TopCategories = skills
                    .Where(s => !string.IsNullOrEmpty(s.Category))
                    .GroupBy(s => s.Category)
                    .Select(g => new { Category = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10),
                
                SkillLevelDistribution = skills
                    .GroupBy(s => s.Level)
                    .Select(g => new { Level = g.Key.ToString(), Count = g.Count(), Percentage = Math.Round((double)g.Count() / skills.Count * 100, 1) })
                    .OrderBy(x => x.Level),
                
                UniqueCategories = skills
                    .Where(s => !string.IsNullOrEmpty(s.Category))
                    .Select(s => s.Category)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count()
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting skill statistics");
            throw;
        }
    }

    public bool ValidateSkillCategory(string category)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(category))
                return false;

            // 檢查是否在預定義分類中，或者是合理的自訂分類
            if (_validCategories.Contains(category))
                return true;

            // 檢查自訂分類是否符合基本規則
            if (category.Length > 50 || category.Length < 2)
                return false;

            // 檢查是否包含特殊字元（允許字母、數字、空格、連字符、底線）
            if (!System.Text.RegularExpressions.Regex.IsMatch(category, @"^[a-zA-Z0-9\s\-_]+$"))
                return false;

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while validating skill category");
            return false;
        }
    }
}