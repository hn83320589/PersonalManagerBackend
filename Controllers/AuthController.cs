using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PersonalManager.Api.Auth;
using PersonalManager.Api.DTOs;

namespace PersonalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _auth.LoginAsync(request);
        if (result == null)
            return Unauthorized(ApiResponse.Fail("Invalid username or password"));
        return Ok(ApiResponse<AuthResponse>.Ok(result, "Login successful"));
    }

    [EnableRateLimiting("auth")]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _auth.RegisterAsync(request);
        if (result == null)
            return BadRequest(ApiResponse.Fail("Username or email already exists"));
        return Ok(ApiResponse<AuthResponse>.Ok(result, "Registration successful"));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = _auth.GetUserIdFromToken(User);
        if (userId == null) return Unauthorized();
        var user = await _auth.GetCurrentUserAsync(userId.Value);
        if (user == null) return NotFound(ApiResponse.Fail("User not found"));
        return Ok(ApiResponse<UserResponse>.Ok(user));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var result = await _auth.RefreshAsync(request.RefreshToken);
        if (result == null)
            return Unauthorized(ApiResponse.Fail("Invalid or expired refresh token"));
        return Ok(ApiResponse<AuthResponse>.Ok(result, "Token refreshed"));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
    {
        await _auth.RevokeAsync(request.RefreshToken);
        return Ok(ApiResponse.Ok("Logged out"));
    }
}
