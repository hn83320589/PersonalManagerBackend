using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Services;
using PersonalManagerAPI.Middleware.Exceptions;

namespace PersonalManagerAPI.Controllers
{
    /// <summary>
    /// 認證控制器 - 處理使用者登入、註冊、令牌管理
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// 使用者登入
        /// </summary>
        /// <param name="loginDto">登入請求資料</param>
        /// <returns>JWT令牌和使用者資訊</returns>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<TokenResponseDto>>> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<TokenResponseDto>.Failure("資料驗證失敗", errors));
            }

            // 收集設備和連接資訊
            var userAgent = Request.Headers["User-Agent"].FirstOrDefault();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            
            // 如果是 EnhancedLoginDto，則取得設備資訊
            DeviceInfoDto? deviceInfo = null;
            if (loginDto is EnhancedLoginDto enhancedLogin)
            {
                deviceInfo = enhancedLogin.DeviceInfo;
            }

            var result = await _authService.LoginAsync(loginDto, deviceInfo, userAgent, ipAddress);
            if (result == null)
            {
                return Unauthorized(ApiResponse<TokenResponseDto>.Failure(
                    "登入失敗，請檢查使用者名稱和密碼"));
            }

            _logger.LogInformation("使用者登入成功: {Username}, IP: {IpAddress}", loginDto.Username, ipAddress);
            return Ok(ApiResponse<TokenResponseDto>.Success(result, "登入成功"));
        }

        /// <summary>
        /// 增強登入 (含設備資訊)
        /// </summary>
        /// <param name="enhancedLoginDto">增強登入請求資料</param>
        /// <returns>JWT令牌和使用者資訊</returns>
        [HttpPost("enhanced-login")]
        public async Task<ActionResult<ApiResponse<TokenResponseDto>>> EnhancedLogin([FromBody] EnhancedLoginDto enhancedLoginDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<TokenResponseDto>.Failure("資料驗證失敗", errors));
            }

            var userAgent = Request.Headers["User-Agent"].FirstOrDefault();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            var result = await _authService.LoginAsync(enhancedLoginDto, enhancedLoginDto.DeviceInfo, userAgent, ipAddress);
            if (result == null)
            {
                return Unauthorized(ApiResponse<TokenResponseDto>.Failure(
                    "登入失敗，請檢查使用者名稱和密碼"));
            }

            _logger.LogInformation("使用者增強登入成功: {Username}, Device: {DeviceName}, IP: {IpAddress}", 
                enhancedLoginDto.Username, enhancedLoginDto.DeviceInfo?.DeviceName, ipAddress);
            return Ok(ApiResponse<TokenResponseDto>.Success(result, "登入成功"));
        }

        /// <summary>
        /// 使用者註冊
        /// </summary>
        /// <param name="registerDto">註冊請求資料</param>
        /// <returns>JWT令牌和使用者資訊</returns>
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<TokenResponseDto>>> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<TokenResponseDto>.Failure("資料驗證失敗", errors));
            }

            var result = await _authService.RegisterAsync(registerDto);
            if (result == null)
            {
                return BadRequest(ApiResponse<TokenResponseDto>.Failure(
                    "註冊失敗，使用者名稱或Email可能已存在"));
            }

            _logger.LogInformation("使用者註冊成功: {Username}", registerDto.Username);
            return CreatedAtAction(nameof(GetCurrentUser), 
                ApiResponse<TokenResponseDto>.Success(result, "註冊成功"));
        }

        /// <summary>
        /// 重新整理令牌
        /// </summary>
        /// <param name="refreshTokenDto">重新整理令牌請求</param>
        /// <returns>新的JWT令牌</returns>
        [HttpPost("refresh")]
        public async Task<ActionResult<ApiResponse<TokenResponseDto>>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<TokenResponseDto>.Failure("資料驗證失敗", errors));
            }

            var result = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken);
            if (result == null)
            {
                return Unauthorized(ApiResponse<TokenResponseDto>.Failure(
                    "重新整理令牌無效或已過期"));
            }

            return Ok(ApiResponse<TokenResponseDto>.Success(result, "令牌重新整理成功"));
        }

        /// <summary>
        /// 撤銷令牌 (登出)
        /// </summary>
        /// <param name="refreshTokenDto">重新整理令牌</param>
        /// <returns>操作結果</returns>
        [HttpPost("revoke")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> RevokeToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            var result = await _authService.RevokeTokenAsync(refreshTokenDto.RefreshToken);
            if (!result)
            {
                return BadRequest(ApiResponse<object>.Failure("令牌撤銷失敗"));
            }

            _logger.LogInformation("令牌撤銷成功");
            return Ok(ApiResponse<object>.Success(null!, "登出成功"));
        }

        /// <summary>
        /// 完整登出 (撤銷所有使用者 Token)
        /// </summary>
        /// <returns>操作結果</returns>
        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> Logout()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(ApiResponse<object>.Failure("無效的令牌"));
            }

            // 取得當前的 Access Token
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            var accessToken = authHeader?.Split(" ").Last();

            var result = await _authService.LogoutAsync(userId, accessToken);
            if (!result)
            {
                return BadRequest(ApiResponse<object>.Failure("登出失敗"));
            }

            _logger.LogInformation("使用者完整登出成功: {UserId}", userId);
            return Ok(ApiResponse<object>.Success(null!, "完整登出成功"));
        }

        /// <summary>
        /// 取得當前使用者資訊
        /// </summary>
        /// <returns>使用者資訊</returns>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserInfoDto>>> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(ApiResponse<UserInfoDto>.Failure("無效的令牌"));
            }

            try
            {
                // 透過 JsonDataService 取得使用者資訊
                var jsonDataService = HttpContext.RequestServices.GetService<JsonDataService>();
                if (jsonDataService == null)
                {
                    throw new BusinessLogicException("無法取得資料服務");
                }

                var users = await jsonDataService.GetAllAsync<Models.User>();
                var user = users.FirstOrDefault(u => u.Id == userId && u.IsActive);

                if (user == null)
                {
                    return NotFound(ApiResponse<UserInfoDto>.Failure("使用者不存在"));
                }

                var userInfo = new UserInfoDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = user.Role,
                    IsActive = user.IsActive
                };

                return Ok(ApiResponse<UserInfoDto>.Success(userInfo, "成功取得使用者資訊"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得使用者資訊時發生錯誤: UserId={UserId}", userId);
                throw new BusinessLogicException("取得使用者資訊失敗");
            }
        }

        /// <summary>
        /// 驗證令牌有效性
        /// </summary>
        /// <returns>令牌驗證結果</returns>
        [HttpGet("validate")]
        [Authorize]
        public ActionResult<ApiResponse<object>> ValidateToken()
        {
            // 如果到達這裡表示令牌有效 (通過 [Authorize] 驗證)
            var username = User.FindFirst("username")?.Value;
            var userId = User.FindFirst("userId")?.Value;

            return Ok(ApiResponse<object>.Success(new
            {
                Valid = true,
                UserId = userId,
                Username = username,
                Message = "令牌有效"
            }, "令牌驗證成功"));
        }

        /// <summary>
        /// 測試受保護的端點
        /// </summary>
        /// <returns>測試訊息</returns>
        [HttpGet("protected")]
        [Authorize]
        public ActionResult<ApiResponse<object>> ProtectedEndpoint()
        {
            var username = User.FindFirst("username")?.Value;
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            return Ok(ApiResponse<object>.Success(new
            {
                Message = "您已成功存取受保護的端點",
                Username = username,
                Role = role,
                ServerTime = DateTime.UtcNow
            }, "受保護端點存取成功"));
        }

        /// <summary>
        /// 智慧 Token 刷新 (檢查是否需要自動續期)
        /// </summary>
        /// <param name="refreshTokenDto">重新整理令牌請求</param>
        /// <returns>Token 刷新結果</returns>
        [HttpPost("smart-refresh")]
        public async Task<ActionResult<ApiResponse<TokenResponseDto>>> SmartRefresh([FromBody] RefreshTokenDto refreshTokenDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<TokenResponseDto>.Failure("資料驗證失敗", errors));
            }

            // 檢查是否需要自動續期
            var shouldAutoRenew = await _authService.ShouldAutoRenewRefreshTokenAsync(refreshTokenDto.RefreshToken);
            
            TokenResponseDto? result;
            string message;

            if (shouldAutoRenew)
            {
                // 執行自動續期 (會產生新的 Refresh Token)
                result = await _authService.AutoRenewRefreshTokenAsync(refreshTokenDto.RefreshToken);
                message = result != null ? "Token 自動續期成功" : "Token 自動續期失敗";
                _logger.LogInformation("執行 Refresh Token 自動續期");
            }
            else
            {
                // 執行一般刷新 (保持相同的 Refresh Token)
                result = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken);
                message = result != null ? "Token 刷新成功" : "Token 刷新失敗";
                _logger.LogInformation("執行一般 Token 刷新");
            }

            if (result == null)
            {
                return Unauthorized(ApiResponse<TokenResponseDto>.Failure("Token 無效或已過期"));
            }

            return Ok(ApiResponse<TokenResponseDto>.Success(result, message));
        }

        /// <summary>
        /// 檢查 Token 狀態和剩餘時間
        /// </summary>
        /// <param name="refreshTokenDto">重新整理令牌請求</param>
        /// <returns>Token 狀態資訊</returns>
        [HttpPost("token-status")]
        public async Task<ActionResult<ApiResponse<object>>> GetTokenStatus([FromBody] RefreshTokenDto refreshTokenDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<object>.Failure("資料驗證失敗", errors));
            }

            var remainingTime = await _authService.GetRefreshTokenRemainingTimeAsync(refreshTokenDto.RefreshToken);
            if (remainingTime == null)
            {
                return BadRequest(ApiResponse<object>.Failure("無效的 Refresh Token"));
            }

            var shouldAutoRenew = await _authService.ShouldAutoRenewRefreshTokenAsync(refreshTokenDto.RefreshToken);

            return Ok(ApiResponse<object>.Success(new
            {
                IsValid = true,
                RemainingTime = remainingTime.Value,
                RemainingDays = remainingTime.Value.TotalDays,
                RemainingHours = remainingTime.Value.TotalHours,
                ShouldAutoRenew = shouldAutoRenew,
                AutoRenewThreshold = TimeSpan.FromHours(24),
                ServerTime = DateTime.UtcNow
            }, "Token 狀態查詢成功"));
        }
    }
}