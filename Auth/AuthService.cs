using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Models;
using PersonalManager.Api.Repositories;
using PersonalManager.Api.Services;
using PersonalManager.Api.Settings;

namespace PersonalManager.Api.Auth;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<UserResponse?> GetCurrentUserAsync(int userId);
    Task<AuthResponse?> RefreshAsync(string refreshToken);
    Task<bool> RevokeAsync(string refreshToken);
    int? GetUserIdFromToken(ClaimsPrincipal principal);
    Task<bool> ForgotPasswordAsync(string email, string resetBaseUrl);
    Task<bool> ResetPasswordAsync(string token, string newPassword);
}

public class AuthService : IAuthService
{
    private readonly IRepository<User> _users;
    private readonly IRepository<RefreshToken> _refreshTokens;
    private readonly IRepository<PasswordResetToken> _resetTokens;
    private readonly IEmailService _email;
    private readonly JwtSettings _jwt;
    private const int RefreshTokenExpiryDays = 7;
    private const int ResetTokenExpiryHours = 1;

    public AuthService(
        IRepository<User> users,
        IRepository<RefreshToken> refreshTokens,
        IRepository<PasswordResetToken> resetTokens,
        IEmailService email,
        IOptions<JwtSettings> jwt)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _resetTokens = resetTokens;
        _email = email;
        _jwt = jwt.Value;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var users = await _users.FindAsync(u => u.Username == request.Username);
        var user = users.FirstOrDefault();
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        var existing = await _users.FindAsync(u => u.Username == request.Username || u.Email == request.Email);
        if (existing.Count > 0) return null;

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Role = "User"
        };

        user = await _users.AddAsync(user);
        return await GenerateAuthResponseAsync(user);
    }

    public async Task<UserResponse?> GetCurrentUserAsync(int userId)
    {
        var user = await _users.GetByIdAsync(userId);
        if (user == null) return null;

        return new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<AuthResponse?> RefreshAsync(string refreshToken)
    {
        var tokens = await _refreshTokens.FindAsync(t => t.Token == refreshToken);
        var token = tokens.FirstOrDefault();

        if (token == null || token.IsRevoked || token.ExpiresAt <= DateTime.UtcNow)
            return null;

        // Revoke the used token
        token.IsRevoked = true;
        await _refreshTokens.UpdateAsync(token);

        var user = await _users.GetByIdAsync(token.UserId);
        if (user == null) return null;

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<bool> RevokeAsync(string refreshToken)
    {
        var tokens = await _refreshTokens.FindAsync(t => t.Token == refreshToken);
        var token = tokens.FirstOrDefault();
        if (token == null) return false;

        token.IsRevoked = true;
        await _refreshTokens.UpdateAsync(token);
        return true;
    }

    public int? GetUserIdFromToken(ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && int.TryParse(claim.Value, out var id) ? id : null;
    }

    public async Task<bool> ForgotPasswordAsync(string email, string resetBaseUrl)
    {
        var users = await _users.FindAsync(u => u.Email == email);
        var user = users.FirstOrDefault();
        // Always return true to avoid email enumeration
        if (user == null) return true;

        var tokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        // URL-safe base64
        tokenValue = tokenValue.Replace('+', '-').Replace('/', '_').TrimEnd('=');

        await _resetTokens.AddAsync(new PasswordResetToken
        {
            UserId = user.Id,
            Token = tokenValue,
            ExpiresAt = DateTime.UtcNow.AddHours(ResetTokenExpiryHours)
        });

        var resetLink = $"{resetBaseUrl.TrimEnd('/')}/reset-password?token={tokenValue}";
        var subject = "Personal Manager — 密碼重設";
        var body = $"""
            <p>您好 {user.Username}，</p>
            <p>我們收到您的密碼重設請求，請點擊下方連結完成重設：</p>
            <p><a href="{resetLink}">{resetLink}</a></p>
            <p>此連結將在 1 小時後失效。若您沒有發出此請求，請忽略本信。</p>
            """;

        await _email.SendEmailAsync(user.Email, subject, body);
        return true;
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        var tokens = await _resetTokens.FindAsync(t => t.Token == token);
        var resetToken = tokens.FirstOrDefault();

        if (resetToken == null || resetToken.IsUsed || resetToken.ExpiresAt <= DateTime.UtcNow)
            return false;

        var user = await _users.GetByIdAsync(resetToken.UserId);
        if (user == null) return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _users.UpdateAsync(user);

        resetToken.IsUsed = true;
        await _resetTokens.UpdateAsync(resetToken);

        return true;
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(User user)
    {
        var expiry = DateTime.UtcNow.AddHours(_jwt.ExpiryHours);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(_jwt.Issuer, _jwt.Audience, claims, expires: expiry, signingCredentials: creds);

        // Generate and persist refresh token
        var refreshTokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays);

        await _refreshTokens.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = refreshTokenExpiry
        });

        return new AuthResponse
        {
            UserId = user.Id,
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            ExpiresAt = expiry,
            RefreshToken = refreshTokenValue,
            RefreshTokenExpiresAt = refreshTokenExpiry
        };
    }
}
