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

            var result = await _authService.LoginAsync(loginDto);
            if (result == null)
            {
                return Unauthorized(ApiResponse<TokenResponseDto>.Failure(
                    "登入失敗，請檢查使用者名稱和密碼"));
            }

            _logger.LogInformation("使用者登入成功: {Username}", loginDto.Username);
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
            return Ok(ApiResponse<object>.Success(null, "登出成功"));
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
    }
}