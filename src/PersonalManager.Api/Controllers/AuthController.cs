using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManager.Api.Auth;
using PersonalManager.Api.DTOs;

namespace PersonalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _auth.LoginAsync(request);
        if (result == null)
            return Unauthorized(ApiResponse.Fail("Invalid username or password"));
        return Ok(ApiResponse<AuthResponse>.Ok(result, "Login successful"));
    }

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
}
