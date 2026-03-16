using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Models;
using PersonalManager.Api.Repositories;

namespace PersonalManager.Api.Auth;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<UserResponse?> GetCurrentUserAsync(int userId);
    Task<AuthResponse?> RefreshAsync(string refreshToken);
    Task<bool> RevokeAsync(string refreshToken);
    int? GetUserIdFromToken(ClaimsPrincipal principal);
}

public class AuthService : IAuthService
{
    private readonly IRepository<User> _users;
    private readonly IRepository<RefreshToken> _refreshTokens;
    private readonly JwtSettings _jwt;
    private const int RefreshTokenExpiryDays = 7;

    public AuthService(IRepository<User> users, IRepository<RefreshToken> refreshTokens, IOptions<JwtSettings> jwt)
    {
        _users = users;
        _refreshTokens = refreshTokens;
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
