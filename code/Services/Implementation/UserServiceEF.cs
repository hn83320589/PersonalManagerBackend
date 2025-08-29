using PersonalManagerAPI.DTOs.User;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services.Interfaces;
using PersonalManagerAPI.Data;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace PersonalManagerAPI.Services.Implementation;

/// <summary>
/// 使用者服務實作 - Entity Framework 版本
/// </summary>
public class UserServiceEF : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<UserServiceEF> _logger;

    public UserServiceEF(
        ApplicationDbContext context, 
        IMapper mapper, 
        ILogger<UserServiceEF> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(int skip = 0, int take = 50)
    {
        try
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
            
            return _mapper.Map<IEnumerable<UserResponseDto>>(users);
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
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);
            
            return user != null ? _mapper.Map<UserResponseDto>(user) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting user by ID: {Id}", id);
            throw;
        }
    }

    public async Task<UserResponseDto?> GetUserByUsernameAsync(string username)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
            
            return user != null ? _mapper.Map<UserResponseDto>(user) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting user by username: {Username}", username);
            throw;
        }
    }

    public async Task<UserResponseDto?> GetUserByEmailAsync(string email)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
            
            return user != null ? _mapper.Map<UserResponseDto>(user) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting user by email: {Email}", email);
            throw;
        }
    }

    public async Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        try
        {
            // 檢查使用者名稱是否已存在
            var existingUserByUsername = await _context.Users
                .AnyAsync(u => u.Username == createUserDto.Username);
            
            if (existingUserByUsername)
            {
                throw new InvalidOperationException($"Username '{createUserDto.Username}' already exists");
            }

            // 檢查電子郵件是否已存在
            var existingUserByEmail = await _context.Users
                .AnyAsync(u => u.Email == createUserDto.Email);
            
            if (existingUserByEmail)
            {
                throw new InvalidOperationException($"Email '{createUserDto.Email}' already exists");
            }

            var user = _mapper.Map<User>(createUserDto);
            
            // 加密密碼
            if (!string.IsNullOrEmpty(createUserDto.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password, 12);
            }

            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User created successfully with ID: {Id}", user.Id);
            
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
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return null;
            }

            // 檢查使用者名稱唯一性 (排除自己)
            if (!string.IsNullOrEmpty(updateUserDto.Username) && 
                updateUserDto.Username != user.Username)
            {
                var existingUser = await _context.Users
                    .AnyAsync(u => u.Username == updateUserDto.Username && u.Id != id);
                
                if (existingUser)
                {
                    throw new InvalidOperationException($"Username '{updateUserDto.Username}' already exists");
                }
            }

            // 檢查電子郵件唯一性 (排除自己)
            if (!string.IsNullOrEmpty(updateUserDto.Email) && 
                updateUserDto.Email != user.Email)
            {
                var existingUser = await _context.Users
                    .AnyAsync(u => u.Email == updateUserDto.Email && u.Id != id);
                
                if (existingUser)
                {
                    throw new InvalidOperationException($"Email '{updateUserDto.Email}' already exists");
                }
            }

            // 只更新非空值
            if (!string.IsNullOrEmpty(updateUserDto.Username))
                user.Username = updateUserDto.Username;
            
            if (!string.IsNullOrEmpty(updateUserDto.Email))
                user.Email = updateUserDto.Email;
            
            if (!string.IsNullOrEmpty(updateUserDto.FirstName))
                user.FirstName = updateUserDto.FirstName;
            
            if (!string.IsNullOrEmpty(updateUserDto.LastName))
                user.LastName = updateUserDto.LastName;
            
            if (!string.IsNullOrEmpty(updateUserDto.Role))
                user.Role = updateUserDto.Role;
            
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("User updated successfully with ID: {Id}", user.Id);
            
            return _mapper.Map<UserResponseDto>(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating user with ID: {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User deleted successfully with ID: {Id}", id);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting user with ID: {Id}", id);
            throw;
        }
    }

    public async Task<bool> VerifyPasswordAsync(string username, string password)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
            
            if (user == null)
            {
                return false;
            }

            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while verifying password for user: {Username}", username);
            throw;
        }
    }

    public async Task<bool> ChangePasswordAsync(int id, ChangePasswordDto changePasswordDto)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }

            // 驗證舊密碼
            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Current password is incorrect");
            }

            // 設定新密碼
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword, 12);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Password changed successfully for user ID: {Id}", id);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while changing password for user ID: {Id}", id);
            throw;
        }
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        try
        {
            return await _context.Users
                .AnyAsync(u => u.Username == username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if username exists: {Username}", username);
            throw;
        }
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        try
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if email exists: {Email}", email);
            throw;
        }
    }

    public async Task<int> GetTotalCountAsync()
    {
        try
        {
            return await _context.Users.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting total user count");
            throw;
        }
    }

    public async Task<IEnumerable<UserResponseDto>> SearchUsersAsync(string searchTerm, int skip = 0, int take = 50)
    {
        try
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(u => 
                    u.Username.ToLower().Contains(searchTerm) ||
                    u.Email.ToLower().Contains(searchTerm) ||
                    (u.FirstName != null && u.FirstName.ToLower().Contains(searchTerm)) ||
                    (u.LastName != null && u.LastName.ToLower().Contains(searchTerm)));
            }

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return _mapper.Map<IEnumerable<UserResponseDto>>(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching users with term: {SearchTerm}", searchTerm);
            throw;
        }
    }

    public async Task<object> GetUserStatisticsAsync()
    {
        try
        {
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.LastLoginDate > DateTime.UtcNow.AddDays(-30));
            var adminUsers = await _context.Users.CountAsync(u => u.Role == "Admin");
            var recentUsers = await _context.Users.CountAsync(u => u.CreatedAt > DateTime.UtcNow.AddDays(-7));

            return new
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                AdminUsers = adminUsers,
                RecentUsers = recentUsers,
                UserGrowthRate = totalUsers > 0 ? (double)recentUsers / totalUsers * 100 : 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting user statistics");
            throw;
        }
    }

    public async Task<bool> ValidatePasswordAsync(int id, string password)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }

            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating password for user {UserId}", id);
            throw;
        }
    }

    public async Task<bool> IsUsernameExistAsync(string username, int? excludeId = null)
    {
        try
        {
            var query = _context.Users.Where(u => u.Username == username);
            
            if (excludeId.HasValue)
            {
                query = query.Where(u => u.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking username existence: {Username}", username);
            throw;
        }
    }

    public async Task<bool> IsEmailExistAsync(string email, int? excludeId = null)
    {
        try
        {
            var query = _context.Users.Where(u => u.Email == email);
            
            if (excludeId.HasValue)
            {
                query = query.Where(u => u.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email existence: {Email}", email);
            throw;
        }
    }

    public async Task<bool> UpdateLastLoginAsync(int id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }

            user.LastLoginDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated last login for user {UserId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last login for user {UserId}", id);
            throw;
        }
    }

    public async Task<object> GetUserStatsAsync()
    {
        // 重用現有的 GetUserStatisticsAsync 方法
        return await GetUserStatisticsAsync();
    }
}