using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services.Interfaces;

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
        private readonly ITokenBlacklistService _tokenBlacklistService;
        private readonly IUserSessionService _userSessionService;

        public AuthService(JsonDataService dataService, IConfiguration configuration, ILogger<AuthService> logger, ITokenBlacklistService tokenBlacklistService, IUserSessionService userSessionService)
        {
            _dataService = dataService;
            _configuration = configuration;
            _logger = logger;
            _tokenBlacklistService = tokenBlacklistService;
            _userSessionService = userSessionService;
        }

        /// <summary>
        /// 使用者登入
        /// </summary>
        public async Task<TokenResponseDto?> LoginAsync(LoginDto loginDto)
        {
            return await LoginAsync(loginDto, null, null, null);
        }

        /// <summary>
        /// 使用者登入 (含設備資訊)
        /// </summary>
        public async Task<TokenResponseDto?> LoginAsync(LoginDto loginDto, DeviceInfoDto? deviceInfo = null, string? userAgent = null, string? ipAddress = null)
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

                // 檢測可疑活動
                var suspiciousCheck = await _userSessionService.DetectSuspiciousActivityAsync(user.Id, deviceInfo, ipAddress);
                if (suspiciousCheck != null)
                {
                    _logger.LogWarning("檢測到可疑登入活動: {Username}, IP: {IpAddress}", user.Username, ipAddress);
                }

                // 強制執行設備數量限制
                await _userSessionService.EnforceDeviceLimitAsync(user.Id, 5);

                // 產生令牌和會話ID
                var sessionId = Guid.NewGuid().ToString();
                var accessToken = GenerateAccessToken(user, sessionId);
                var refreshToken = GenerateRefreshToken();

                // 儲存重新整理令牌
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _dataService.UpdateAsync(user);

                // 創建用戶會話
                var expiresAt = DateTime.UtcNow.AddDays(7); // 與 refresh token 同步
                await _userSessionService.CreateSessionAsync(user.Id, sessionId, refreshToken, expiresAt, deviceInfo, userAgent, ipAddress);

                _logger.LogInformation("使用者登入成功並創建會話: {Username}, SessionId: {SessionId}", user.Username, sessionId);

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

                // 從現有的 session 中取得 sessionId
                var existingSession = await _userSessionService.GetSessionByRefreshTokenAsync(refreshToken);
                var sessionId = existingSession?.SessionId ?? Guid.NewGuid().ToString();

                // 產生新的令牌
                var accessToken = GenerateAccessToken(user, sessionId);
                var newRefreshToken = GenerateRefreshToken();

                // 更新重新整理令牌
                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _dataService.UpdateAsync(user);

                // 更新會話的最後活躍時間
                if (existingSession != null)
                {
                    await _userSessionService.UpdateLastActiveAsync(sessionId);
                }

                _logger.LogInformation("令牌重新整理成功: {Username}, SessionId: {SessionId}", user.Username, sessionId);

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
        public string GenerateAccessToken(User user, string? sessionId = null)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            var issuer = jwtSettings["Issuer"] ?? "PersonalManagerAPI";
            var audience = jwtSettings["Audience"] ?? "PersonalManagerClient";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jti = sessionId ?? Guid.NewGuid().ToString();

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("userId", user.Id.ToString()),
                new Claim("username", user.Username),
                new Claim("fullName", user.FullName ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, jti), // 使用 sessionId 作為 JTI
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

        /// <summary>
        /// 撤銷 Access Token (加入黑名單)
        /// </summary>
        public async Task<bool> RevokeAccessTokenAsync(string accessToken)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                if (!handler.CanReadToken(accessToken))
                {
                    _logger.LogWarning("無效的 Access Token 格式");
                    return false;
                }

                var jwtToken = handler.ReadJwtToken(accessToken);
                var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
                
                if (string.IsNullOrEmpty(jti))
                {
                    _logger.LogWarning("Access Token 缺少 JTI 聲明");
                    return false;
                }

                var expiryTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Exp).Value)).DateTime;
                
                await _tokenBlacklistService.AddToBlacklistAsync(jti, expiryTime);
                _logger.LogInformation("Access Token 已加入黑名單: {Jti}", jti);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "撤銷 Access Token 失敗");
                return false;
            }
        }

        /// <summary>
        /// 登出並撤銷所有使用者 Token
        /// </summary>
        public async Task<bool> LogoutAsync(int userId, string? accessToken = null)
        {
            try
            {
                string? sessionId = null;

                // 從 Access Token 中提取 sessionId (JTI)
                if (!string.IsNullOrEmpty(accessToken))
                {
                    var handler = new JwtSecurityTokenHandler();
                    if (handler.CanReadToken(accessToken))
                    {
                        var jwtToken = handler.ReadJwtToken(accessToken);
                        sessionId = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
                    }
                }

                // 1. 結束用戶會話
                if (!string.IsNullOrEmpty(sessionId))
                {
                    await _userSessionService.EndSessionAsync(sessionId, "UserLogout");
                }

                // 2. 撤銷 Refresh Token
                var users = await _dataService.GetAllAsync<User>();
                var user = users.FirstOrDefault(u => u.Id == userId);
                
                if (user != null)
                {
                    user.RefreshToken = null;
                    user.RefreshTokenExpiryTime = null;
                    await _dataService.UpdateAsync(user);
                }

                // 3. 將 Access Token 加入黑名單
                if (!string.IsNullOrEmpty(accessToken))
                {
                    await RevokeAccessTokenAsync(accessToken);
                }

                _logger.LogInformation("使用者登出成功: {UserId}, SessionId: {SessionId}", userId, sessionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "使用者登出失敗: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// 檢查 Refresh Token 是否需要自動續期
        /// </summary>
        public async Task<bool> ShouldAutoRenewRefreshTokenAsync(string refreshToken)
        {
            try
            {
                var users = await _dataService.GetAllAsync<User>();
                var user = users.FirstOrDefault(u => u.RefreshToken == refreshToken && u.IsActive);

                if (user == null || user.RefreshTokenExpiryTime == null)
                {
                    return false;
                }

                // 如果 Refresh Token 在24小時內過期，則需要自動續期
                var remainingTime = user.RefreshTokenExpiryTime.Value - DateTime.UtcNow;
                var autoRenewThreshold = TimeSpan.FromHours(24);

                _logger.LogInformation("Refresh Token 剩餘時間: {RemainingTime}, 續期閾值: {Threshold}", 
                    remainingTime, autoRenewThreshold);

                return remainingTime <= autoRenewThreshold && remainingTime > TimeSpan.Zero;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查 Refresh Token 自動續期狀態失敗");
                return false;
            }
        }

        /// <summary>
        /// 自動續期 Refresh Token
        /// </summary>
        public async Task<TokenResponseDto?> AutoRenewRefreshTokenAsync(string refreshToken)
        {
            try
            {
                var users = await _dataService.GetAllAsync<User>();
                var user = users.FirstOrDefault(u => u.RefreshToken == refreshToken && u.IsActive);

                if (user == null || user.RefreshTokenExpiryTime == null)
                {
                    _logger.LogWarning("自動續期失敗 - 無效的 Refresh Token");
                    return null;
                }

                // 檢查是否仍在有效期內
                if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    _logger.LogWarning("自動續期失敗 - Refresh Token 已過期");
                    return null;
                }

                // 產生新的 Token 組
                var newAccessToken = GenerateAccessToken(user);
                var newRefreshToken = GenerateRefreshToken();

                // 更新使用者的 Refresh Token，延長有效期
                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // 重新延長7天
                user.LastLoginDate = DateTime.UtcNow; // 更新最後活躍時間
                await _dataService.UpdateAsync(user);

                _logger.LogInformation("Refresh Token 自動續期成功: {Username}", user.Username);

                return new TokenResponseDto
                {
                    AccessToken = newAccessToken,
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
                _logger.LogError(ex, "Refresh Token 自動續期過程發生錯誤");
                return null;
            }
        }

        /// <summary>
        /// 取得 Refresh Token 剩餘有效時間
        /// </summary>
        public async Task<TimeSpan?> GetRefreshTokenRemainingTimeAsync(string refreshToken)
        {
            try
            {
                var users = await _dataService.GetAllAsync<User>();
                var user = users.FirstOrDefault(u => u.RefreshToken == refreshToken && u.IsActive);

                if (user == null || user.RefreshTokenExpiryTime == null)
                {
                    return null;
                }

                var remainingTime = user.RefreshTokenExpiryTime.Value - DateTime.UtcNow;
                return remainingTime > TimeSpan.Zero ? remainingTime : TimeSpan.Zero;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得 Refresh Token 剩餘時間失敗");
                return null;
            }
        }
    }
}