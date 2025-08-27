using Microsoft.Extensions.Primitives;
using PersonalManagerAPI.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace PersonalManagerAPI.Middleware;

/// <summary>
/// JWT Token 驗證中間件
/// 檢查 Token 是否在黑名單中
/// </summary>
public class JwtTokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtTokenValidationMiddleware> _logger;

    public JwtTokenValidationMiddleware(RequestDelegate next, ILogger<JwtTokenValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITokenBlacklistService tokenBlacklistService)
    {
        try
        {
            // 檢查是否有 Authorization header
            if (context.Request.Headers.TryGetValue("Authorization", out StringValues authHeader))
            {
                var token = authHeader.FirstOrDefault()?.Split(" ").Last();
                
                if (!string.IsNullOrEmpty(token))
                {
                    // 解析 JWT Token 以取得 JTI (JWT ID)
                    var jwtHandler = new JwtSecurityTokenHandler();
                    if (jwtHandler.CanReadToken(token))
                    {
                        var jwtToken = jwtHandler.ReadJwtToken(token);
                        var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
                        
                        if (!string.IsNullOrEmpty(jti))
                        {
                            // 檢查 Token 是否在黑名單中
                            var isBlacklisted = await tokenBlacklistService.IsBlacklistedAsync(jti);
                            if (isBlacklisted)
                            {
                                _logger.LogWarning("拒絕黑名單 Token 訪問: {Jti}", jti);
                                context.Response.StatusCode = 401;
                                await context.Response.WriteAsync("Token has been revoked");
                                return;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "JWT Token 驗證中間件執行錯誤");
        }

        await _next(context);
    }
}