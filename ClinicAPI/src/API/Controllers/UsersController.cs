using API.Data;
using API.DTOs;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ClinicDbContext _context;
    private readonly ILogger<UsersController> _logger;

    public UsersController(ClinicDbContext context, ILogger<UsersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

    /// <summary>
    /// Get current user's profile
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserProfileDTO>>> GetMyProfile()
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Unauthorized(ApiResponse<UserProfileDTO>.ErrorResponse(
                    "INVALID_TOKEN", "Invalid token"));
            }

            var user = await _context.Users
                .Include(u => u.Patient)
                .Include(u => u.Doctor)
                .Include(u => u.Clerk)
                .FirstOrDefaultAsync(u => u.Id == userGuid);

            if (user == null)
            {
                return NotFound(ApiResponse<UserProfileDTO>.ErrorResponse(
                    "NOT_FOUND", "User not found"));
            }

            var dto = new UserProfileDTO
            {
                UserId = user.Id.ToString(),
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                Role = user.Role.ToString(),
                RoleSpecificId = user.Role switch
                {
                    UserRole.Patient => user.Patient?.Id.ToString(),
                    UserRole.Doctor => user.Doctor?.Id.ToString(),
                    UserRole.Clerk => user.Clerk?.Id.ToString(),
                    _ => null
                },
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };

            // Add role-specific information
            if (user.Role == UserRole.Patient && user.Patient != null)
            {
                dto.PatientInfo = new PatientInfoDTO
                {
                    PatientId = user.Patient.Id.ToString(),
                    DateOfBirth = user.Patient.DateOfBirth,
                    Address = user.Patient.Address,
                    EmergencyContactName = user.Patient.EmergencyContactName,
                    EmergencyContactPhone = user.Patient.EmergencyContactPhone
                };
            }
            else if (user.Role == UserRole.Doctor && user.Doctor != null)
            {
                dto.DoctorInfo = new DoctorInfoDTO
                {
                    DoctorId = user.Doctor.Id.ToString(),
                    Specialization = user.Doctor.Specialization,
                    LicenseNumber = user.Doctor.LicenseNumber,
                    YearsOfExperience = user.Doctor.YearsOfExperience
                };
            }

            return Ok(ApiResponse<UserProfileDTO>.SuccessResponse(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user profile");
            return StatusCode(500, ApiResponse<UserProfileDTO>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while fetching profile"));
        }
    }

    /// <summary>
    /// Update current user's profile
    /// </summary>
    [HttpPut("me")]
    public async Task<ActionResult<ApiResponse<UserProfileDTO>>> UpdateMyProfile(
        [FromBody] UpdateUserProfileRequest request)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Unauthorized(ApiResponse<UserProfileDTO>.ErrorResponse(
                    "INVALID_TOKEN", "Invalid token"));
            }

            var user = await _context.Users
                .Include(u => u.Patient)
                .Include(u => u.Doctor)
                .Include(u => u.Clerk)
                .FirstOrDefaultAsync(u => u.Id == userGuid);

            if (user == null)
            {
                return NotFound(ApiResponse<UserProfileDTO>.ErrorResponse(
                    "NOT_FOUND", "User not found"));
            }

            // Update user fields
            if (!string.IsNullOrWhiteSpace(request.FirstName))
                user.FirstName = request.FirstName;

            if (!string.IsNullOrWhiteSpace(request.LastName))
                user.LastName = request.LastName;

            if (request.Phone != null)
                user.Phone = request.Phone;

            // Update patient-specific fields if applicable
            if (user.Role == UserRole.Patient && user.Patient != null)
            {
                if (request.DateOfBirth.HasValue)
                    user.Patient.DateOfBirth = request.DateOfBirth.Value;

                if (request.Address != null)
                    user.Patient.Address = request.Address;

                if (request.EmergencyContactName != null)
                    user.Patient.EmergencyContactName = request.EmergencyContactName;

                if (request.EmergencyContactPhone != null)
                    user.Patient.EmergencyContactPhone = request.EmergencyContactPhone;
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var dto = new UserProfileDTO
            {
                UserId = user.Id.ToString(),
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                Role = user.Role.ToString(),
                RoleSpecificId = user.Role switch
                {
                    UserRole.Patient => user.Patient?.Id.ToString(),
                    UserRole.Doctor => user.Doctor?.Id.ToString(),
                    UserRole.Clerk => user.Clerk?.Id.ToString(),
                    _ => null
                },
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };

            return Ok(ApiResponse<UserProfileDTO>.SuccessResponse(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return StatusCode(500, ApiResponse<UserProfileDTO>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while updating profile"));
        }
    }

    /// <summary>
    /// Update current user's password
    /// </summary>
    [HttpPut("me/password")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateMyPassword(
        [FromBody] UpdatePasswordRequest request)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    "INVALID_TOKEN", "Invalid token"));
            }

            var user = await _context.Users.FindAsync(userGuid);
            if (user == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "NOT_FOUND", "User not found"));
            }

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "INVALID_PASSWORD", "Current password is incorrect"));
            }

            // Validate new password
            if (request.NewPassword.Length < 6)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "PASSWORD_TOO_SHORT", "New password must be at least 6 characters"));
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "PASSWORD_MISMATCH", "New passwords do not match"));
            }

            // Update password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                message = "Password updated successfully"
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating password");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while updating password"));
        }
    }

    /// <summary>
    /// Get pending users (Clerk only)
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Clerk")]
    public async Task<ActionResult<ApiResponse<List<PendingUserDTO>>>> GetPendingUsers()
    {
        try
        {
            var pendingUsers = await _context.Users
                .Where(u => !u.IsActive)
                .OrderBy(u => u.CreatedAt)
                .ToListAsync();

            var dtoList = pendingUsers.Select(u => new PendingUserDTO
            {
                Id = u.Id.ToString(),
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Role = u.Role.ToString(),
                CreatedAt = u.CreatedAt
            }).ToList();

            return Ok(ApiResponse<List<PendingUserDTO>>.SuccessResponse(dtoList));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pending users");
            return StatusCode(500, ApiResponse<List<PendingUserDTO>>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while fetching pending users"));
        }
    }

    /// <summary>
    /// Approve a pending user (Clerk only)
    /// </summary>
    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Clerk")]
    public async Task<ActionResult<ApiResponse<object>>> ApproveUser(string id)
    {
        try
        {
            if (!Guid.TryParse(id, out var userGuid))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "INVALID_ID", "Invalid user ID"));
            }

            var user = await _context.Users.FindAsync(userGuid);

            if (user == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "NOT_FOUND", "User not found"));
            }

            if (user.IsActive)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "ALREADY_ACTIVE", "User is already active"));
            }

            // Activate the user
            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                message = "User approved successfully",
                userId = user.Id.ToString()
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving user");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while approving user"));
        }
    }

    /// <summary>
    /// Reject a pending user (Clerk only)
    /// </summary>
    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Clerk")]
    public async Task<ActionResult<ApiResponse<object>>> RejectUser(string id)
    {
        try
        {
            if (!Guid.TryParse(id, out var userGuid))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "INVALID_ID", "Invalid user ID"));
            }

            var user = await _context.Users
                .Include(u => u.Patient)
                .Include(u => u.Doctor)
                .Include(u => u.Clerk)
                .FirstOrDefaultAsync(u => u.Id == userGuid);

            if (user == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "NOT_FOUND", "User not found"));
            }

            // Delete role-specific entity
            if (user.Patient != null)
            {
                _context.Patients.Remove(user.Patient);
            }
            else if (user.Doctor != null)
            {
                _context.Doctors.Remove(user.Doctor);
            }
            else if (user.Clerk != null)
            {
                _context.Clerks.Remove(user.Clerk);
            }

            // Delete the user
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                message = "User rejected successfully",
                userId = userGuid.ToString()
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting user");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while rejecting user"));
        }
    }
}
