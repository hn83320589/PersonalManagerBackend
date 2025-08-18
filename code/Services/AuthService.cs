using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Models;

namespace PersonalManagerAPI.Services
{
    /// <summary>
    /// 認證服務實作
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly JsonDataService _dataService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(JsonDataService dataService, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _dataService = dataService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// 使用者登入
        /// </summary>
        public async Task<TokenResponseDto?> LoginAsync(LoginDto loginDto)
        {
            try
            {
                // 尋找使用者
                var users = await _dataService.GetAllAsync<User>();
                var user = users.FirstOrDefault(u => u.Username == loginDto.Username && u.IsActive);

                if (user == null)
                {
                    _logger.LogWarning("登入失敗 - 使用者不存在: {Username}", loginDto.Username);
                    return null;
                }

                // 驗證密碼
                if (!VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    _logger.LogWarning("登入失敗 - 密碼錯誤: {Username}", loginDto.Username);
                    return null;
                }

                // 更新最後登入時間
                user.LastLoginDate = DateTime.UtcNow;
                await _dataService.UpdateAsync(user);

                // 產生令牌
                var accessToken = GenerateAccessToken(user);
                var refreshToken = GenerateRefreshToken();

                // 儲存重新整理令牌 (實際應用中應存到資料庫)
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // 7天有效期
                await _dataService.UpdateAsync(user);

                _logger.LogInformation("使用者登入成功: {Username}", loginDto.Username);

                return new TokenResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    TokenType = "Bearer",
                    ExpiresIn = ((DateTimeOffset)DateTime.UtcNow.AddHours(1)).ToUnixTimeSeconds(),
                    User = new UserInfoDto
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        FullName = user.FullName,
                        Role = user.Role,
                        IsActive = user.IsActive
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登入過程發生錯誤: {Username}", loginDto.Username);
                return null;
            }
        }

        /// <summary>
        /// 使用者註冊
        /// </summary>
        public async Task<TokenResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // 檢查使用者名稱是否已存在
                var users = await _dataService.GetAllAsync<User>();
                if (users.Any(u => u.Username == registerDto.Username))
                {
                    _logger.LogWarning("註冊失敗 - 使用者名稱已存在: {Username}", registerDto.Username);
                    return null;
                }

                // 檢查 Email 是否已存在
                if (users.Any(u => u.Email == registerDto.Email))
                {
                    _logger.LogWarning("註冊失敗 - Email已存在: {Email}", registerDto.Email);
                    return null;
                }

                // 建立新使用者
                var newUser = new User
                {
                    Id = users.Count > 0 ? users.Max(u => u.Id) + 1 : 1,
                    Username = registerDto.Username,
                    Email = registerDto.Email,
                    FullName = registerDto.FullName,
                    PasswordHash = HashPassword(registerDto.Password),
                    Role = "User",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    LastLoginDate = DateTime.UtcNow
                };

                // 儲存使用者
                await _dataService.CreateAsync(newUser);

                _logger.LogInformation("使用者註冊成功: {Username}", registerDto.Username);

                // 自動登入
                var loginDto = new LoginDto
                {
                    Username = registerDto.Username,
                    Password = registerDto.Password
                };

                return await LoginAsync(loginDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "註冊過程發生錯誤: {Username}", registerDto.Username);
                return null;
            }
        }

        /// <summary>
        /// 重新整理令牌
        /// </summary>
        public async Task<TokenResponseDto?> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                // 尋找擁有此重新整理令牌的使用者
                var users = await _dataService.GetAllAsync<User>();
                var user = users.FirstOrDefault(u => u.RefreshToken == refreshToken && u.IsActive);

                if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    _logger.LogWarning("重新整理令牌無效或已過期");
                    return null;
                }

                // 產生新的令牌
                var accessToken = GenerateAccessToken(user);
                var newRefreshToken = GenerateRefreshToken();

                // 更新重新整理令牌
                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _dataService.UpdateAsync(user);

                _logger.LogInformation("令牌重新整理成功: {Username}", user.Username);

                return new TokenResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = newRefreshToken,
                    TokenType = "Bearer",
                    ExpiresIn = ((DateTimeOffset)DateTime.UtcNow.AddHours(1)).ToUnixTimeSeconds(),
                    User = new UserInfoDto
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        FullName = user.FullName,
                        Role = user.Role,
                        IsActive = user.IsActive
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重新整理令牌過程發生錯誤");
                return null;
            }
        }

        /// <summary>
        /// 撤銷令牌
        /// </summary>
        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            try
            {
                var users = await _dataService.GetAllAsync<User>();
                var user = users.FirstOrDefault(u => u.RefreshToken == refreshToken);

                if (user != null)
                {
                    user.RefreshToken = null;
                    user.RefreshTokenExpiryTime = null;
                    await _dataService.UpdateAsync(user);

                    _logger.LogInformation("令牌撤銷成功: {Username}", user.Username);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "撤銷令牌過程發生錯誤");
                return false;
            }
        }

        /// <summary>
        /// 驗證密碼
        /// </summary>
        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        /// <summary>
        /// 雜湊密碼
        /// </summary>
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// 產生存取令牌
        /// </summary>
        public string GenerateAccessToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            var issuer = jwtSettings["Issuer"] ?? "PersonalManagerAPI";
            var audience = jwtSettings["Audience"] ?? "PersonalManagerClient";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("userId", user.Id.ToString()),
                new Claim("username", user.Username),
                new Claim("fullName", user.FullName ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, 
                    new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                    ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1), // 1小時有效期
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// 產生重新整理令牌
        /// </summary>
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        /// <summary>
        /// 從令牌取得使用者 ID
        /// </summary>
        public int? GetUserIdFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "userId");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}