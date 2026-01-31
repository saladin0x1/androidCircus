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
public class DoctorsController : ControllerBase
{
    private readonly ClinicDbContext _context;
    private readonly ILogger<DoctorsController> _logger;

    public DoctorsController(ClinicDbContext context, ILogger<DoctorsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// List doctors filtered by role:
    /// - Doctors see only themselves
    /// - Patients see all doctors (for booking)
    /// - Clerks see all doctors (admin access)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<DoctorDTO>>>> GetDoctors()
    {
        try
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            List<DoctorDTO> doctors;

            if (userRole == "Doctor")
            {
                // Doctor sees only themselves
                if (!Guid.TryParse(userId, out var userGuid))
                {
                    return BadRequest(ApiResponse<List<DoctorDTO>>.ErrorResponse(
                        "INVALID_TOKEN", "Invalid token"));
                }

                var doctor = await _context.Doctors
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.UserId == userGuid);

                doctors = doctor != null ? new List<DoctorDTO>
                {
                    new DoctorDTO
                    {
                        Id = doctor.Id.ToString(),
                        Email = doctor.User.Email,
                        FirstName = doctor.User.FirstName,
                        LastName = doctor.User.LastName,
                        Phone = doctor.User.Phone ?? "",
                        Specialization = doctor.Specialization,
                        LicenseNumber = doctor.LicenseNumber,
                        YearsOfExperience = doctor.YearsOfExperience,
                        JoinedDate = doctor.JoinedDate,
                        IsActive = doctor.User.IsActive
                    }
                } : new List<DoctorDTO>();
            }
            else
            {
                // Patient and Clerk see all active doctors
                doctors = await _context.Doctors
                    .Include(d => d.User)
                    .Where(d => d.User.IsActive)
                    .Select(d => new DoctorDTO
                    {
                        Id = d.Id.ToString(),
                        Email = d.User.Email,
                        FirstName = d.User.FirstName,
                        LastName = d.User.LastName,
                        Phone = d.User.Phone ?? "",
                        Specialization = d.Specialization,
                        LicenseNumber = d.LicenseNumber,
                        YearsOfExperience = d.YearsOfExperience,
                        JoinedDate = d.JoinedDate,
                        IsActive = d.User.IsActive
                    })
                    .OrderBy(d => d.LastName)
                    .ThenBy(d => d.FirstName)
                    .ToListAsync();
            }

            return Ok(ApiResponse<List<DoctorDTO>>.SuccessResponse(doctors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching doctors");
            return StatusCode(500, ApiResponse<List<DoctorDTO>>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while fetching doctors"));
        }
    }

    /// <summary>
    /// Get a specific doctor by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<DoctorDTO>>> GetDoctor(string id)
    {
        try
        {
            if (!Guid.TryParse(id, out var doctorId))
            {
                return BadRequest(ApiResponse<DoctorDTO>.ErrorResponse(
                    "INVALID_ID", "Invalid doctor ID"));
            }

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
            {
                return NotFound(ApiResponse<DoctorDTO>.ErrorResponse(
                    "NOT_FOUND", "Doctor not found"));
            }

            var dto = new DoctorDTO
            {
                Id = doctor.Id.ToString(),
                Email = doctor.User.Email,
                FirstName = doctor.User.FirstName,
                LastName = doctor.User.LastName,
                Phone = doctor.User.Phone ?? "",
                Specialization = doctor.Specialization,
                LicenseNumber = doctor.LicenseNumber,
                YearsOfExperience = doctor.YearsOfExperience,
                JoinedDate = doctor.JoinedDate,
                IsActive = doctor.User.IsActive
            };

            return Ok(ApiResponse<DoctorDTO>.SuccessResponse(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching doctor");
            return StatusCode(500, ApiResponse<DoctorDTO>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while fetching doctor"));
        }
    }

    /// <summary>
    /// Create a new doctor (Clerk only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Clerk")]
    public async Task<ActionResult<ApiResponse<DoctorDTO>>> CreateDoctor([FromBody] CreateDoctorRequest request)
    {
        try
        {
            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest(ApiResponse<DoctorDTO>.ErrorResponse(
                    "EMAIL_EXISTS", "Email already registered"));
            }

            // Create user
            var user = new User
            {
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Phone = request.Phone,
                Role = UserRole.Doctor,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create doctor
            var doctor = new Doctor
            {
                UserId = user.Id,
                Specialization = request.Specialization,
                LicenseNumber = request.LicenseNumber,
                YearsOfExperience = request.YearsOfExperience
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            var dto = new DoctorDTO
            {
                Id = doctor.Id.ToString(),
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone ?? "",
                Specialization = doctor.Specialization,
                LicenseNumber = doctor.LicenseNumber,
                YearsOfExperience = doctor.YearsOfExperience,
                JoinedDate = doctor.JoinedDate,
                IsActive = user.IsActive
            };

            return Created(string.Empty, ApiResponse<DoctorDTO>.SuccessResponse(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating doctor");
            return StatusCode(500, ApiResponse<DoctorDTO>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while creating doctor"));
        }
    }

    /// <summary>
    /// Update a doctor (Clerk only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Clerk")]
    public async Task<ActionResult<ApiResponse<DoctorDTO>>> UpdateDoctor(
        string id, [FromBody] UpdateDoctorRequest request)
    {
        try
        {
            if (!Guid.TryParse(id, out var doctorId))
            {
                return BadRequest(ApiResponse<DoctorDTO>.ErrorResponse(
                    "INVALID_ID", "Invalid doctor ID"));
            }

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
            {
                return NotFound(ApiResponse<DoctorDTO>.ErrorResponse(
                    "NOT_FOUND", "Doctor not found"));
            }

            // Update user fields
            if (!string.IsNullOrWhiteSpace(request.FirstName))
                doctor.User.FirstName = request.FirstName;

            if (!string.IsNullOrWhiteSpace(request.LastName))
                doctor.User.LastName = request.LastName;

            if (request.Phone != null)
                doctor.User.Phone = request.Phone;

            if (request.IsActive.HasValue)
                doctor.User.IsActive = request.IsActive.Value;

            // Update doctor fields
            if (!string.IsNullOrWhiteSpace(request.Specialization))
                doctor.Specialization = request.Specialization;

            if (request.LicenseNumber != null)
                doctor.LicenseNumber = request.LicenseNumber;

            if (request.YearsOfExperience.HasValue)
                doctor.YearsOfExperience = request.YearsOfExperience.Value;

            doctor.User.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var dto = new DoctorDTO
            {
                Id = doctor.Id.ToString(),
                Email = doctor.User.Email,
                FirstName = doctor.User.FirstName,
                LastName = doctor.User.LastName,
                Phone = doctor.User.Phone ?? "",
                Specialization = doctor.Specialization,
                LicenseNumber = doctor.LicenseNumber,
                YearsOfExperience = doctor.YearsOfExperience,
                JoinedDate = doctor.JoinedDate,
                IsActive = doctor.User.IsActive
            };

            return Ok(ApiResponse<DoctorDTO>.SuccessResponse(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating doctor");
            return StatusCode(500, ApiResponse<DoctorDTO>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while updating doctor"));
        }
    }

    /// <summary>
    /// Delete a doctor (Clerk only) - soft delete by deactivating
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Clerk")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteDoctor(string id)
    {
        try
        {
            if (!Guid.TryParse(id, out var doctorId))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "INVALID_ID", "Invalid doctor ID"));
            }

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "NOT_FOUND", "Doctor not found"));
            }

            // Soft delete - deactivate user
            doctor.User.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                message = "Doctor deactivated successfully"
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting doctor");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while deleting doctor"));
        }
    }

    /// <summary>
    /// Get doctor dashboard statistics
    /// </summary>
    [HttpGet("dashboard")]
    [Authorize(Roles = "Doctor")]
    public async Task<ActionResult<ApiResponse<object>>> GetDashboard()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "INVALID_TOKEN", "Invalid token"));
            }

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userGuid);

            if (doctor == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "NOT_FOUND", "Doctor not found"));
            }

            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var todayAppointments = await _context.Appointments
                .CountAsync(a => a.DoctorId == doctor.Id && a.AppointmentDate >= today && a.AppointmentDate < tomorrow);

            var pendingAppointments = await _context.Appointments
                .CountAsync(a => a.DoctorId == doctor.Id && a.Status == AppointmentStatus.Scheduled);

            var totalPatients = await _context.Appointments
                .Where(a => a.DoctorId == doctor.Id)
                .Select(a => a.PatientId)
                .Distinct()
                .CountAsync();

            var completedAppointments = await _context.Appointments
                .CountAsync(a => a.DoctorId == doctor.Id && a.Status == AppointmentStatus.Completed);

            var stats = new
            {
                TodayAppointments = todayAppointments,
                PendingAppointments = pendingAppointments,
                TotalPatients = totalPatients,
                CompletedAppointments = completedAppointments
            };

            return Ok(ApiResponse<object>.SuccessResponse(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching doctor dashboard");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while fetching dashboard"));
        }
    }

    /// <summary>
    /// Get doctor's patients (patients who have had appointments with this doctor)
    /// </summary>
    [HttpGet("patients")]
    [Authorize(Roles = "Doctor")]
    public async Task<ActionResult<ApiResponse<List<PatientDTO>>>> GetDoctorPatients()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest(ApiResponse<List<PatientDTO>>.ErrorResponse(
                    "INVALID_TOKEN", "Invalid token"));
            }

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userGuid);
            if (doctor == null)
            {
                return NotFound(ApiResponse<List<PatientDTO>>.ErrorResponse(
                    "NOT_FOUND", "Doctor not found"));
            }

            // Get unique patients who have had appointments with this doctor
            var patientIds = await _context.Appointments
                .Where(a => a.DoctorId == doctor.Id)
                .Select(a => a.PatientId)
                .Distinct()
                .ToListAsync();

            var patients = await _context.Patients
                .Include(p => p.User)
                .Where(p => patientIds.Contains(p.Id))
                .OrderBy(p => p.User.LastName)
                .ThenBy(p => p.User.FirstName)
                .ToListAsync();

            var dtoList = patients.Select(p => new PatientDTO
            {
                Id = p.Id.ToString(),
                Email = p.User.Email,
                FirstName = p.User.FirstName,
                LastName = p.User.LastName,
                Phone = p.User.Phone ?? "",
                DateOfBirth = p.DateOfBirth,
                Address = p.Address,
                EmergencyContactName = p.EmergencyContactName,
                EmergencyContactPhone = p.EmergencyContactPhone,
                DoctorNotes = p.DoctorNotes,
                RegistrationDate = p.RegistrationDate
            }).ToList();

            return Ok(ApiResponse<List<PatientDTO>>.SuccessResponse(dtoList));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching doctor's patients");
            return StatusCode(500, ApiResponse<List<PatientDTO>>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while fetching patients"));
        }
    }

    /// <summary>
    /// Get doctor's agenda (upcoming appointments)
    /// </summary>
    [HttpGet("agenda")]
    [Authorize(Roles = "Doctor")]
    public async Task<ActionResult<ApiResponse<List<AppointmentDTO>>>> GetDoctorAgenda(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest(ApiResponse<List<AppointmentDTO>>.ErrorResponse(
                    "INVALID_TOKEN", "Invalid token"));
            }

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userGuid);
            if (doctor == null)
            {
                return NotFound(ApiResponse<List<AppointmentDTO>>.ErrorResponse(
                    "NOT_FOUND", "Doctor not found"));
            }

            var start = startDate ?? DateTime.UtcNow.Date;
            var end = endDate ?? start.AddDays(7);

            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Where(a => a.DoctorId == doctor.Id && a.AppointmentDate >= start && a.AppointmentDate < end)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            var dtoList = appointments.Select(a => new AppointmentDTO
            {
                Id = a.Id.ToString(),
                PatientId = a.PatientId.ToString(),
                DoctorId = a.DoctorId.ToString(),
                AppointmentDate = a.AppointmentDate,
                Reason = a.Reason,
                Notes = a.Notes,
                DoctorNotes = a.DoctorNotes,
                Status = a.Status.ToString(),
                PatientName = $"{a.Patient.User.FirstName} {a.Patient.User.LastName}",
                DoctorName = $"{a.Doctor.User.FirstName} {a.Doctor.User.LastName}",
                DoctorSpecialization = a.Doctor.Specialization
            }).ToList();

            return Ok(ApiResponse<List<AppointmentDTO>>.SuccessResponse(dtoList));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching doctor's agenda");
            return StatusCode(500, ApiResponse<List<AppointmentDTO>>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while fetching agenda"));
        }
    }
}
