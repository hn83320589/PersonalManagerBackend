using PersonalManagerAPI.DTOs.User;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services;
using PersonalManagerAPI.Services.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;
using BCrypt.Net;

namespace PersonalManagerAPI.Services.Implementation;

/// <summary>
/// 使用者服務實作
/// </summary>
public class UserService : IUserService
{
    private readonly JsonDataService _dataService;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(JsonDataService dataService, IMapper mapper, ILogger<UserService> logger)
    {
        _dataService = dataService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(int skip = 0, int take = 50)
    {
        try
        {
            var users = await _dataService.GetUsersAsync();
            var pagedUsers = users
                .OrderByDescending(u => u.CreatedAt)
                .Skip(skip)
                .Take(take);
            
            return _mapper.Map<IEnumerable<UserResponseDto>>(pagedUsers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all users");
            throw;
        }
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(int id)
    {
        try
        {
            var users = await _dataService.GetUsersAsync();
            var user = users.FirstOrDefault(u => u.Id == id);
            return _mapper.Map<UserResponseDto?>(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting user with ID {Id}", id);
            throw;
        }
    }

    public async Task<UserResponseDto?> GetUserByUsernameAsync(string username)
    {
        try
        {
            var users = await _dataService.GetUsersAsync();
            var user = users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            return _mapper.Map<UserResponseDto?>(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting user with username {Username}", username);
            throw;
        }
    }

    public async Task<UserResponseDto?> GetUserByEmailAsync(string email)
    {
        try
        {
            var users = await _dataService.GetUsersAsync();
            var user = users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            return _mapper.Map<UserResponseDto?>(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting user with email {Email}", email);
            throw;
        }
    }

    public async Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        try
        {
            // 檢查使用者名稱是否已存在
            if (await IsUsernameExistAsync(createUserDto.Username))
            {
                throw new InvalidOperationException("使用者名稱已存在");
            }

            // 檢查電子郵件是否已存在
            if (await IsEmailExistAsync(createUserDto.Email))
            {
                throw new InvalidOperationException("電子郵件已存在");
            }

            var users = await _dataService.GetUsersAsync();
            var user = _mapper.Map<User>(createUserDto);
            
            // 生成新ID
            user.Id = users.Any() ? users.Max(u => u.Id) + 1 : 1;
            
            // 設定時間戳記
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            
            // 雜湊密碼
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password, 12);
            
            // 設定全名
            user.FullName = $"{createUserDto.FirstName} {createUserDto.LastName}".Trim();

            users.Add(user);
            await _dataService.SaveUsersAsync(users);

            _logger.LogInformation("User created with ID {Id}", user.Id);
            return _mapper.Map<UserResponseDto>(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating user");
            throw;
        }
    }

    public async Task<UserResponseDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
    {
        try
        {
            var users = await _dataService.GetUsersAsync();
            var existingUser = users.FirstOrDefault(u => u.Id == id);
            
            if (existingUser == null)
            {
                _logger.LogWarning("User with ID {Id} not found for update", id);
                return null;
            }

            // 檢查使用者名稱重複（排除自己）
            if (!string.IsNullOrEmpty(updateUserDto.Username) && 
                await IsUsernameExistAsync(updateUserDto.Username, id))
            {
                throw new InvalidOperationException("使用者名稱已存在");
            }

            // 檢查電子郵件重複（排除自己）
            if (!string.IsNullOrEmpty(updateUserDto.Email) && 
                await IsEmailExistAsync(updateUserDto.Email, id))
            {
                throw new InvalidOperationException("電子郵件已存在");
            }

            _mapper.Map(updateUserDto, existingUser);
            existingUser.UpdatedAt = DateTime.UtcNow;
            
            // 更新全名
            if (!string.IsNullOrEmpty(updateUserDto.FirstName) || !string.IsNullOrEmpty(updateUserDto.LastName))
            {
                existingUser.FullName = $"{existingUser.FirstName} {existingUser.LastName}".Trim();
            }

            await _dataService.SaveUsersAsync(users);
            _logger.LogInformation("User with ID {Id} updated", id);
            
            return _mapper.Map<UserResponseDto>(existingUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating user with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        try
        {
            var users = await _dataService.GetUsersAsync();
            var user = users.FirstOrDefault(u => u.Id == id);
            
            if (user == null)
            {
                _logger.LogWarning("User with ID {Id} not found for deletion", id);
                return false;
            }

            users.Remove(user);
            await _dataService.SaveUsersAsync(users);
            _logger.LogInformation("User with ID {Id} deleted", id);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting user with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> ChangePasswordAsync(int id, ChangePasswordDto changePasswordDto)
    {
        try
        {
            var users = await _dataService.GetUsersAsync();
            var user = users.FirstOrDefault(u => u.Id == id);
            
            if (user == null)
            {
                _logger.LogWarning("User with ID {Id} not found for password change", id);
                return false;
            }

            // 驗證舊密碼
            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
            {
                _logger.LogWarning("Invalid current password for user with ID {Id}", id);
                return false;
            }

            // 設定新密碼
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword, 12);
            user.UpdatedAt = DateTime.UtcNow;

            await _dataService.SaveUsersAsync(users);
            _logger.LogInformation("Password changed for user with ID {Id}", id);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while changing password for user with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> ValidatePasswordAsync(int id, string password)
    {
        try
        {
            var users = await _dataService.GetUsersAsync();
            var user = users.FirstOrDefault(u => u.Id == id);
            
            if (user == null)
            {
                return false;
            }

            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while validating password for user with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> IsUsernameExistAsync(string username, int? excludeId = null)
    {
        try
        {
            var users = await _dataService.GetUsersAsync();
            return users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                                 (!excludeId.HasValue || u.Id != excludeId.Value));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking username existence");
            throw;
        }
    }

    public async Task<bool> IsEmailExistAsync(string email, int? excludeId = null)
    {
        try
        {
            var users = await _dataService.GetUsersAsync();
            return users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                                 (!excludeId.HasValue || u.Id != excludeId.Value));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking email existence");
            throw;
        }
    }

    public async Task<bool> UpdateLastLoginAsync(int id)
    {
        try
        {
            var users = await _dataService.GetUsersAsync();
            var user = users.FirstOrDefault(u => u.Id == id);
            
            if (user == null)
            {
                return false;
            }

            user.LastLoginDate = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _dataService.SaveUsersAsync(users);
            _logger.LogInformation("Last login updated for user with ID {Id}", id);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating last login for user with ID {Id}", id);
            throw;
        }
    }

    public async Task<object> GetUserStatsAsync()
    {
        try
        {
            var users = await _dataService.GetUsersAsync();
            var now = DateTime.UtcNow;
            var today = DateTime.Today;
            var thisMonth = new DateTime(now.Year, now.Month, 1);

            var stats = new
            {
                TotalUsers = users.Count(),
                ActiveUsers = users.Count(u => u.IsActive),
                InactiveUsers = users.Count(u => !u.IsActive),
                AdminUsers = users.Count(u => u.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase)),
                RegularUsers = users.Count(u => u.Role.Equals("User", StringComparison.OrdinalIgnoreCase)),
                
                TodayRegistrations = users.Count(u => u.CreatedAt.Date == today),
                ThisMonthRegistrations = users.Count(u => u.CreatedAt >= thisMonth),
                RecentLogins = users.Count(u => u.LastLoginDate.HasValue && u.LastLoginDate.Value >= today.AddDays(-7)),
                
                UsersWithPhone = users.Count(u => !string.IsNullOrEmpty(u.Phone)),
                UsersWithFullName = users.Count(u => !string.IsNullOrEmpty(u.FullName)),
                
                MonthlyRegistrations = users
                    .Where(u => u.CreatedAt >= DateTime.Today.AddMonths(-12))
                    .GroupBy(u => u.CreatedAt.ToString("yyyy-MM"))
                    .Select(g => new { Month = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Month)
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting user statistics");
            throw;
        }
    }
}