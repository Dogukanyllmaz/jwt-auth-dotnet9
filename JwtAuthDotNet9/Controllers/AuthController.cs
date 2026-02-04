using JwtAuthDotNet9.Entities;
using JwtAuthDotNet9.Models;
using JwtAuthDotNet9.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;


namespace JwtAuthDotNet9.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        public static User user = new User();

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserDto request)
        {
            Log.Information("Register endpoint called for user: {UserName}", request.UserName, DateTime.Now);
            var user = await authService.RegisterAsync(request);
            if (user is null)
            {
                Log.Error("Registration failed for user: {UserName}", request.UserName, DateTime.Now);
                return BadRequest("User already exists.");
            }
            Log.Information("User registered successfully: {UserName}", request.UserName, DateTime.Now);
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDto request)
        {
            Log.Information("Login endpoint called for user: {UserName}", request.UserName, DateTime.Now);
            var result = await authService.LoginAsync(request);
            if (result is null)
            {
                Log.Error("Login failed for user: {UserName}", request.UserName, DateTime.Now);
                return BadRequest("Invalid username or password.");
            }
            Log.Information("User logged in successfully: {UserName}", request.UserName, DateTime.Now);
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken(RefreshTokenRequestDto request)
        {
            Log.Information("Refresh token endpoint called for user ID: {UserId}", request.UserId, DateTime.Now);
            var result = await authService.RefreshTokensAsync(request);
            if (result is null || result.AccessToken is null || result.RefreshToken is null)
            {
                Log.Error("Refresh token failed for user ID: {UserId}", request.UserId, DateTime.Now);
                return Unauthorized("Invalid refresh token.");
            }
            Log.Information("Tokens refreshed successfully for user ID: {UserId}", request.UserId, DateTime.Now);
            return Ok(result);
        }

        [Authorize]
        [HttpGet]
        public IActionResult AuthenticatedOnlyEndpoint()
        {
            Log.Information("Authenticated endpoint accessed by user: {UserName}", User.Identity?.Name, DateTime.Now);
            return Ok("You are authenticated!");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnlyEndpoint()
        {
            Log.Information("Admin endpoint accessed by user: {UserName}", User.Identity?.Name, DateTime.Now);
            return Ok("You are Admin!");
        }

    }
}
