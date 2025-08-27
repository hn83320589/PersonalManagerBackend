using PersonalManagerAPI.DTOs.ContactMethod;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services;
using PersonalManagerAPI.Services.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace PersonalManagerAPI.Services.Implementation;

/// <summary>
/// 聯絡方式服務實作
/// </summary>
public class ContactMethodService : IContactMethodService
{
    private readonly JsonDataService _dataService;
    private readonly IMapper _mapper;
    private readonly ILogger<ContactMethodService> _logger;

    // 電話號碼驗證正規表達式
    private readonly Regex _phoneRegex = new(@"^[\+]?[1-9][\d]{0,15}$", RegexOptions.Compiled);
    
    // URL 驗證正規表達式
    private readonly Regex _urlRegex = new(@"^https?://[^\s/$.?#].[^\s]*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public ContactMethodService(JsonDataService dataService, IMapper mapper, ILogger<ContactMethodService> logger)
    {
        _dataService = dataService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<ContactMethodResponseDto>> GetAllContactMethodsAsync(int skip = 0, int take = 50)
    {
        try
        {
            var contactMethods = await _dataService.GetContactMethodsAsync();
            var pagedMethods = contactMethods
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Type)
                .Skip(skip)
                .Take(take);

            var result = _mapper.Map<IEnumerable<ContactMethodResponseDto>>(pagedMethods);
            
            // 添加類型名稱
            foreach (var method in result)
            {
                method.TypeName = method.Type.ToString();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all contact methods");
            throw;
        }
    }

    public async Task<ContactMethodResponseDto?> GetContactMethodByIdAsync(int id)
    {
        try
        {
            var contactMethods = await _dataService.GetContactMethodsAsync();
            var contactMethod = contactMethods.FirstOrDefault(c => c.Id == id);
            
            if (contactMethod == null)
                return null;

            var result = _mapper.Map<ContactMethodResponseDto>(contactMethod);
            result.TypeName = result.Type.ToString();
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting contact method with ID {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<ContactMethodResponseDto>> GetContactMethodsByUserIdAsync(int userId)
    {
        try
        {
            var contactMethods = await _dataService.GetContactMethodsAsync();
            var userMethods = contactMethods
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Type);

            var result = _mapper.Map<IEnumerable<ContactMethodResponseDto>>(userMethods);
            
            foreach (var method in result)
            {
                method.TypeName = method.Type.ToString();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting contact methods for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<ContactMethodResponseDto>> GetPublicContactMethodsAsync(int? userId = null)
    {
        try
        {
            var contactMethods = await _dataService.GetContactMethodsAsync();
            var query = contactMethods.Where(c => c.IsPublic);
            
            if (userId.HasValue)
            {
                query = query.Where(c => c.UserId == userId.Value);
            }

            var publicMethods = query
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Type);

            var result = _mapper.Map<IEnumerable<ContactMethodResponseDto>>(publicMethods);
            
            foreach (var method in result)
            {
                method.TypeName = method.Type.ToString();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting public contact methods");
            throw;
        }
    }

    public async Task<IEnumerable<ContactMethodResponseDto>> GetPreferredContactMethodsAsync(int? userId = null)
    {
        try
        {
            var contactMethods = await _dataService.GetContactMethodsAsync();
            var query = contactMethods.Where(c => c.IsPreferred);
            
            if (userId.HasValue)
            {
                query = query.Where(c => c.UserId == userId.Value);
            }

            var preferredMethods = query
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Type);

            var result = _mapper.Map<IEnumerable<ContactMethodResponseDto>>(preferredMethods);
            
            foreach (var method in result)
            {
                method.TypeName = method.Type.ToString();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting preferred contact methods");
            throw;
        }
    }

    public async Task<IEnumerable<ContactMethodResponseDto>> GetContactMethodsByTypeAsync(ContactType type, int? userId = null)
    {
        try
        {
            var contactMethods = await _dataService.GetContactMethodsAsync();
            var query = contactMethods.Where(c => c.Type == type);
            
            if (userId.HasValue)
            {
                query = query.Where(c => c.UserId == userId.Value);
            }

            var typedMethods = query
                .OrderBy(c => c.SortOrder)
                .ThenByDescending(c => c.IsPreferred);

            var result = _mapper.Map<IEnumerable<ContactMethodResponseDto>>(typedMethods);
            
            foreach (var method in result)
            {
                method.TypeName = method.Type.ToString();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting contact methods by type {Type}", type);
            throw;
        }
    }

    public async Task<ContactMethodResponseDto> CreateContactMethodAsync(CreateContactMethodDto createContactMethodDto)
    {
        try
        {
            // 驗證聯絡方式值的格式
            if (!await ValidateContactMethodValueAsync(createContactMethodDto.Type, createContactMethodDto.Value))
            {
                throw new ArgumentException($"聯絡方式值格式不正確: {createContactMethodDto.Value}");
            }

            var contactMethods = await _dataService.GetContactMethodsAsync();
            var contactMethod = _mapper.Map<ContactMethod>(createContactMethodDto);
            
            // 生成新ID
            contactMethod.Id = contactMethods.Any() ? contactMethods.Max(c => c.Id) + 1 : 1;
            
            // 設定時間戳記
            contactMethod.CreatedAt = DateTime.UtcNow;
            contactMethod.UpdatedAt = DateTime.UtcNow;

            // 如果設定為偏好聯絡方式，取消同一使用者的其他偏好設定
            if (contactMethod.IsPreferred)
            {
                var userMethods = contactMethods.Where(c => c.UserId == contactMethod.UserId).ToList();
                foreach (var method in userMethods)
                {
                    method.IsPreferred = false;
                }
            }

            contactMethods.Add(contactMethod);
            await _dataService.SaveContactMethodsAsync(contactMethods);

            _logger.LogInformation("Contact method created with ID {Id} for user {UserId}", contactMethod.Id, contactMethod.UserId);
            
            var result = _mapper.Map<ContactMethodResponseDto>(contactMethod);
            result.TypeName = result.Type.ToString();
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating contact method");
            throw;
        }
    }

    public async Task<ContactMethodResponseDto?> UpdateContactMethodAsync(int id, UpdateContactMethodDto updateContactMethodDto)
    {
        try
        {
            var contactMethods = await _dataService.GetContactMethodsAsync();
            var existingMethod = contactMethods.FirstOrDefault(c => c.Id == id);
            
            if (existingMethod == null)
            {
                _logger.LogWarning("Contact method with ID {Id} not found for update", id);
                return null;
            }

            // 如果要更新聯絡方式值，進行格式驗證
            if (!string.IsNullOrEmpty(updateContactMethodDto.Value))
            {
                var typeToValidate = updateContactMethodDto.Type ?? existingMethod.Type;
                if (!await ValidateContactMethodValueAsync(typeToValidate, updateContactMethodDto.Value))
                {
                    throw new ArgumentException($"聯絡方式值格式不正確: {updateContactMethodDto.Value}");
                }
            }

            // 如果設定為偏好聯絡方式，取消同一使用者的其他偏好設定
            if (updateContactMethodDto.IsPreferred == true)
            {
                var userMethods = contactMethods.Where(c => c.UserId == existingMethod.UserId && c.Id != id).ToList();
                foreach (var method in userMethods)
                {
                    method.IsPreferred = false;
                }
            }

            _mapper.Map(updateContactMethodDto, existingMethod);
            existingMethod.UpdatedAt = DateTime.UtcNow;

            await _dataService.SaveContactMethodsAsync(contactMethods);
            _logger.LogInformation("Contact method with ID {Id} updated", id);
            
            var result = _mapper.Map<ContactMethodResponseDto>(existingMethod);
            result.TypeName = result.Type.ToString();
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating contact method with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteContactMethodAsync(int id)
    {
        try
        {
            var contactMethods = await _dataService.GetContactMethodsAsync();
            var contactMethod = contactMethods.FirstOrDefault(c => c.Id == id);
            
            if (contactMethod == null)
            {
                _logger.LogWarning("Contact method with ID {Id} not found for deletion", id);
                return false;
            }

            contactMethods.Remove(contactMethod);
            await _dataService.SaveContactMethodsAsync(contactMethods);
            _logger.LogInformation("Contact method with ID {Id} deleted", id);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting contact method with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> SetPreferredContactMethodAsync(int id, int userId)
    {
        try
        {
            var contactMethods = await _dataService.GetContactMethodsAsync();
            var contactMethod = contactMethods.FirstOrDefault(c => c.Id == id && c.UserId == userId);
            
            if (contactMethod == null)
            {
                return false;
            }

            // 取消同一使用者的其他偏好設定
            var userMethods = contactMethods.Where(c => c.UserId == userId).ToList();
            foreach (var method in userMethods)
            {
                method.IsPreferred = method.Id == id;
                method.UpdatedAt = DateTime.UtcNow;
            }

            await _dataService.SaveContactMethodsAsync(contactMethods);
            _logger.LogInformation("Contact method with ID {Id} set as preferred for user {UserId}", id, userId);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while setting preferred contact method");
            throw;
        }
    }

    public async Task<bool> UpdateContactMethodOrderAsync(int id, int newSortOrder)
    {
        try
        {
            var contactMethods = await _dataService.GetContactMethodsAsync();
            var contactMethod = contactMethods.FirstOrDefault(c => c.Id == id);
            
            if (contactMethod == null)
            {
                return false;
            }

            contactMethod.SortOrder = newSortOrder;
            contactMethod.UpdatedAt = DateTime.UtcNow;

            await _dataService.SaveContactMethodsAsync(contactMethods);
            _logger.LogInformation("Contact method with ID {Id} sort order updated to {SortOrder}", id, newSortOrder);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating contact method order");
            throw;
        }
    }

    public async Task<bool> BatchUpdateContactMethodOrderAsync(Dictionary<int, int> contactMethodOrders)
    {
        try
        {
            var contactMethods = await _dataService.GetContactMethodsAsync();
            var methodsToUpdate = contactMethods.Where(c => contactMethodOrders.ContainsKey(c.Id)).ToList();
            
            foreach (var method in methodsToUpdate)
            {
                method.SortOrder = contactMethodOrders[method.Id];
                method.UpdatedAt = DateTime.UtcNow;
            }

            await _dataService.SaveContactMethodsAsync(contactMethods);
            _logger.LogInformation("Batch updated {Count} contact methods order", methodsToUpdate.Count);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while batch updating contact methods order");
            throw;
        }
    }

    public async Task<bool> ToggleContactMethodVisibilityAsync(int id)
    {
        try
        {
            var contactMethods = await _dataService.GetContactMethodsAsync();
            var contactMethod = contactMethods.FirstOrDefault(c => c.Id == id);
            
            if (contactMethod == null)
            {
                return false;
            }

            contactMethod.IsPublic = !contactMethod.IsPublic;
            contactMethod.UpdatedAt = DateTime.UtcNow;

            await _dataService.SaveContactMethodsAsync(contactMethods);
            _logger.LogInformation("Contact method with ID {Id} visibility toggled to {IsPublic}", id, contactMethod.IsPublic);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while toggling contact method visibility");
            throw;
        }
    }

    public async Task<bool> ValidateContactMethodValueAsync(ContactType type, string value)
    {
        return await Task.FromResult(type switch
        {
            ContactType.Email => IsValidEmail(value),
            ContactType.Phone => _phoneRegex.IsMatch(value),
            ContactType.LinkedIn => IsValidUrl(value) && value.Contains("linkedin.com"),
            ContactType.GitHub => IsValidUrl(value) && value.Contains("github.com"),
            ContactType.Facebook => IsValidUrl(value) && value.Contains("facebook.com"),
            ContactType.Twitter => IsValidUrl(value) && (value.Contains("twitter.com") || value.Contains("x.com")),
            ContactType.Instagram => IsValidUrl(value) && value.Contains("instagram.com"),
            ContactType.Other => !string.IsNullOrWhiteSpace(value),
            _ => false
        });
    }

    public async Task<IEnumerable<ContactMethodResponseDto>> SearchContactMethodsAsync(string searchTerm)
    {
        try
        {
            var contactMethods = await _dataService.GetContactMethodsAsync();
            var searchResults = contactMethods
                .Where(c => c.Value.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                           (c.Label?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false))
                .OrderBy(c => c.SortOrder);

            var result = _mapper.Map<IEnumerable<ContactMethodResponseDto>>(searchResults);
            
            foreach (var method in result)
            {
                method.TypeName = method.Type.ToString();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching contact methods with term {SearchTerm}", searchTerm);
            throw;
        }
    }

    public async Task<Dictionary<ContactType, int>> GetContactMethodTypeStatsAsync(int? userId = null)
    {
        try
        {
            var contactMethods = await _dataService.GetContactMethodsAsync();
            var query = contactMethods.AsQueryable();
            
            if (userId.HasValue)
            {
                query = query.Where(c => c.UserId == userId.Value);
            }

            return query.GroupBy(c => c.Type)
                       .ToDictionary(g => g.Key, g => g.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting contact method type statistics");
            throw;
        }
    }

    public async Task<bool> ContactMethodExistsAsync(int id)
    {
        try
        {
            var contactMethods = await _dataService.GetContactMethodsAsync();
            return contactMethods.Any(c => c.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking contact method existence");
            throw;
        }
    }

    public async Task<bool> UserHasContactMethodTypeAsync(int userId, ContactType type)
    {
        try
        {
            var contactMethods = await _dataService.GetContactMethodsAsync();
            return contactMethods.Any(c => c.UserId == userId && c.Type == type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking user contact method type");
            throw;
        }
    }

    public async Task<object> GetContactMethodStatsAsync()
    {
        try
        {
            var contactMethods = await _dataService.GetContactMethodsAsync();
            var now = DateTime.UtcNow;
            var today = DateTime.Today;
            var thisMonth = new DateTime(now.Year, now.Month, 1);

            var stats = new
            {
                TotalContactMethods = contactMethods.Count(),
                PublicContactMethods = contactMethods.Count(c => c.IsPublic),
                PrivateContactMethods = contactMethods.Count(c => !c.IsPublic),
                PreferredContactMethods = contactMethods.Count(c => c.IsPreferred),
                
                TodayContactMethods = contactMethods.Count(c => c.CreatedAt.Date == today),
                ThisMonthContactMethods = contactMethods.Count(c => c.CreatedAt >= thisMonth),
                
                ContactMethodsByType = contactMethods
                    .GroupBy(c => c.Type)
                    .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
                    .OrderByDescending(x => x.Count),
                    
                ContactMethodsByUser = contactMethods
                    .GroupBy(c => c.UserId)
                    .Select(g => new { UserId = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10),
                    
                RecentActivity = contactMethods
                    .OrderByDescending(c => c.UpdatedAt)
                    .Take(10)
                    .Select(c => new { c.Id, c.Type, c.Value, c.CreatedAt, c.UpdatedAt })
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting contact method statistics");
            throw;
        }
    }

    #region Private Methods

    /// <summary>
    /// 驗證電子郵件格式
    /// </summary>
    private bool IsValidEmail(string email)
    {
        try
        {
            var mailAddress = new MailAddress(email);
            return mailAddress.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 驗證 URL 格式
    /// </summary>
    private bool IsValidUrl(string url)
    {
        return _urlRegex.IsMatch(url) || Uri.TryCreate(url, UriKind.Absolute, out _);
    }

    #endregion
}