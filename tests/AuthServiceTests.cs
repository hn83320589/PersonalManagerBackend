using Microsoft.Extensions.Options;
using Moq;
using PersonalManager.Api.Auth;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Models;
using PersonalManager.Api.Repositories;

namespace PersonalManager.Tests;

public class AuthServiceTests
{
    private readonly Mock<IRepository<User>> _userRepo;
    private readonly Mock<IRepository<RefreshToken>> _refreshTokenRepo;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _userRepo = new Mock<IRepository<User>>();
        _refreshTokenRepo = new Mock<IRepository<RefreshToken>>();

        var jwt = Options.Create(new JwtSettings
        {
            SecretKey = "test-secret-key-at-least-32-characters-long",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiryHours = 24
        });

        _sut = new AuthService(_userRepo.Object, _refreshTokenRepo.Object, jwt);
    }

    private User MakeUser(int id = 1, string username = "admin", string password = "password123") => new()
    {
        Id = id,
        Username = username,
        Email = $"{username}@test.com",
        FullName = "Test User",
        Role = "User",
        IsActive = true,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    private void SetupRefreshTokenAdd()
    {
        _refreshTokenRepo
            .Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken t) => { t.Id = 1; return t; });
    }

    // ===== LoginAsync =====

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        var user = MakeUser();
        _userRepo.Setup(r => r.FindAsync(It.IsAny<Func<User, bool>>()))
            .ReturnsAsync([user]);
        SetupRefreshTokenAdd();

        var result = await _sut.LoginAsync(new LoginRequest { Username = "admin", Password = "password123" });

        Assert.NotNull(result);
        Assert.Equal("admin", result.Username);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsNull()
    {
        var user = MakeUser();
        _userRepo.Setup(r => r.FindAsync(It.IsAny<Func<User, bool>>()))
            .ReturnsAsync([user]);

        var result = await _sut.LoginAsync(new LoginRequest { Username = "admin", Password = "wrongpass" });

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_NonExistentUser_ReturnsNull()
    {
        _userRepo.Setup(r => r.FindAsync(It.IsAny<Func<User, bool>>()))
            .ReturnsAsync([]);

        var result = await _sut.LoginAsync(new LoginRequest { Username = "nobody", Password = "pass" });

        Assert.Null(result);
    }

    // ===== RegisterAsync =====

    [Fact]
    public async Task RegisterAsync_NewUser_ReturnsAuthResponse()
    {
        _userRepo.Setup(r => r.FindAsync(It.IsAny<Func<User, bool>>()))
            .ReturnsAsync([]);
        _userRepo.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => { u.Id = 99; return u; });
        SetupRefreshTokenAdd();

        var result = await _sut.RegisterAsync(new RegisterRequest
        {
            Username = "newuser",
            Email = "newuser@test.com",
            Password = "password123",
            FullName = "New User"
        });

        Assert.NotNull(result);
        Assert.Equal("newuser", result.Username);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUsername_ReturnsNull()
    {
        var existing = MakeUser(username: "existing");
        _userRepo.Setup(r => r.FindAsync(It.IsAny<Func<User, bool>>()))
            .ReturnsAsync([existing]);

        var result = await _sut.RegisterAsync(new RegisterRequest
        {
            Username = "existing",
            Email = "other@test.com",
            Password = "password123",
            FullName = "Other"
        });

        Assert.Null(result);
    }

    // ===== RefreshAsync =====

    [Fact]
    public async Task RefreshAsync_ValidToken_ReturnsNewAuthResponse()
    {
        var user = MakeUser();
        var refreshToken = new RefreshToken
        {
            Id = 1,
            UserId = user.Id,
            Token = "valid-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        _refreshTokenRepo.Setup(r => r.FindAsync(It.IsAny<Func<RefreshToken, bool>>()))
            .ReturnsAsync([refreshToken]);
        _refreshTokenRepo.Setup(r => r.UpdateAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken t) => t);
        _userRepo.Setup(r => r.GetByIdAsync(user.Id))
            .ReturnsAsync(user);
        SetupRefreshTokenAdd();

        var result = await _sut.RefreshAsync("valid-token");

        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.True(refreshToken.IsRevoked); // old token should be revoked
    }

    [Fact]
    public async Task RefreshAsync_ExpiredToken_ReturnsNull()
    {
        var refreshToken = new RefreshToken
        {
            Id = 1,
            UserId = 1,
            Token = "expired-token",
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // expired
            IsRevoked = false
        };
        _refreshTokenRepo.Setup(r => r.FindAsync(It.IsAny<Func<RefreshToken, bool>>()))
            .ReturnsAsync([refreshToken]);

        var result = await _sut.RefreshAsync("expired-token");

        Assert.Null(result);
    }

    [Fact]
    public async Task RefreshAsync_RevokedToken_ReturnsNull()
    {
        var refreshToken = new RefreshToken
        {
            Id = 1,
            UserId = 1,
            Token = "revoked-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = true // already revoked
        };
        _refreshTokenRepo.Setup(r => r.FindAsync(It.IsAny<Func<RefreshToken, bool>>()))
            .ReturnsAsync([refreshToken]);

        var result = await _sut.RefreshAsync("revoked-token");

        Assert.Null(result);
    }

    [Fact]
    public async Task RefreshAsync_TokenNotFound_ReturnsNull()
    {
        _refreshTokenRepo.Setup(r => r.FindAsync(It.IsAny<Func<RefreshToken, bool>>()))
            .ReturnsAsync([]);

        var result = await _sut.RefreshAsync("nonexistent-token");

        Assert.Null(result);
    }

    // ===== RevokeAsync =====

    [Fact]
    public async Task RevokeAsync_ValidToken_ReturnsTrueAndRevokes()
    {
        var refreshToken = new RefreshToken
        {
            Id = 1,
            UserId = 1,
            Token = "my-token",
            IsRevoked = false
        };
        _refreshTokenRepo.Setup(r => r.FindAsync(It.IsAny<Func<RefreshToken, bool>>()))
            .ReturnsAsync([refreshToken]);
        _refreshTokenRepo.Setup(r => r.UpdateAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken t) => t);

        var result = await _sut.RevokeAsync("my-token");

        Assert.True(result);
        Assert.True(refreshToken.IsRevoked);
    }

    [Fact]
    public async Task RevokeAsync_TokenNotFound_ReturnsFalse()
    {
        _refreshTokenRepo.Setup(r => r.FindAsync(It.IsAny<Func<RefreshToken, bool>>()))
            .ReturnsAsync([]);

        var result = await _sut.RevokeAsync("nonexistent");

        Assert.False(result);
    }
}
