using API.Data;
using API.DTOs;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly JwtService _jwtService;
    private readonly ClinicDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AuthService authService, JwtService jwtService, ClinicDbContext context, ILogger<AuthController> logger)
    {
        _authService = authService;
        _jwtService = jwtService;
        _context = context;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);

            if (!response.Success)
            {
                return Unauthorized(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new LoginResponse
            {
                Success = false,
                Error = "An error occurred during login"
            });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var response = await _authService.RegisterAsync(request);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Created(string.Empty, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, new LoginResponse
            {
                Success = false,
                Error = "An error occurred during registration"
            });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<object>>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            // Always return success to prevent email enumeration
            // In production, send actual reset email here

            if (user != null)
            {
                // Generate reset token (in production, this would be saved with expiry)
                var resetToken = Guid.NewGuid().ToString();
                _logger.LogInformation($"Password reset requested for {user.Email}. Token: {resetToken}");
            }

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                message = "If an account with this email exists, a password reset link has been sent."
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forgot password");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while processing your request"));
        }
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<object>>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            // In production, validate the reset token
            // For school project, we'll just check that passwords match
            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "PASSWORD_MISMATCH", "Passwords do not match"));
            }

            if (request.NewPassword.Length < 6)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "PASSWORD_TOO_SHORT", "Password must be at least 6 characters"));
            }

            // For demo purposes - find user by email and reset
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "USER_NOT_FOUND", "User not found"));
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                message = "Password has been reset successfully"
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while resetting password"));
        }
    }

    [HttpPost("refresh")]
    [Authorize]
    public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    Error = "Invalid token"
                });
            }

            var user = await _context.Users
                .Include(u => u.Patient)
                .Include(u => u.Doctor)
                .Include(u => u.Clerk)
                .FirstOrDefaultAsync(u => u.Id == userGuid);

            if (user == null || !user.IsActive)
            {
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    Error = "User not found or inactive"
                });
            }

            // Get role-specific ID
            var roleSpecificId = user.Role switch
            {
                UserRole.Patient => user.Patient?.Id.ToString() ?? string.Empty,
                UserRole.Doctor => user.Doctor?.Id.ToString() ?? string.Empty,
                UserRole.Clerk => user.Clerk?.Id.ToString() ?? string.Empty,
                _ => string.Empty
            };

            // Generate new token
            var token = _jwtService.GenerateToken(user, roleSpecificId);

            return Ok(new LoginResponse
            {
                Success = true,
                Data = new UserData
                {
                    UserId = user.Id.ToString(),
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    Token = token,
                    RoleSpecificId = roleSpecificId
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new LoginResponse
            {
                Success = false,
                Error = "An error occurred during token refresh"
            });
        }
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
    }
}
