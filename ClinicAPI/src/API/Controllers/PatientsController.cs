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
public class PatientsController : ControllerBase
{
    private readonly ClinicDbContext _context;
    private readonly ILogger<PatientsController> _logger;

    public PatientsController(ClinicDbContext context, ILogger<PatientsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    private string GetUserRole() => User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

    /// <summary>
    /// List patients with optional search (Doctor and Clerk only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Doctor,Clerk")]
    public async Task<ActionResult<ApiResponse<List<PatientDTO>>>> GetPatients([FromQuery] string? search = null)
    {
        try
        {
            IQueryable<Patient> query = _context.Patients
                .Include(p => p.User);

            // Search by name if provided
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(p =>
                    (p.User.FirstName != null && p.User.FirstName.ToLower().Contains(searchLower)) ||
                    (p.User.LastName != null && p.User.LastName.ToLower().Contains(searchLower)) ||
                    (p.User.Email != null && p.User.Email.ToLower().Contains(searchLower)));
            }

            var patients = await query
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
            _logger.LogError(ex, "Error fetching patients");
            return StatusCode(500, ApiResponse<List<PatientDTO>>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while fetching patients"));
        }
    }

    /// <summary>
    /// Get patient details by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PatientDTO>>> GetPatient(string id)
    {
        try
        {
            if (!Guid.TryParse(id, out var patientId))
            {
                return BadRequest(ApiResponse<PatientDTO>.ErrorResponse(
                    "INVALID_ID", "Invalid patient ID"));
            }

            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null)
            {
                return NotFound(ApiResponse<PatientDTO>.ErrorResponse(
                    "NOT_FOUND", "Patient not found"));
            }

            // Authorization: Doctors and Clerks can access any patient, Patients only their own
            var role = GetUserRole();
            var userId = GetUserId();

            if (role == "Patient")
            {
                if (!Guid.TryParse(userId, out var userGuid) || userGuid != patient.UserId)
                {
                    return Forbid();
                }
            }

            var dto = new PatientDTO
            {
                Id = patient.Id.ToString(),
                Email = patient.User.Email,
                FirstName = patient.User.FirstName,
                LastName = patient.User.LastName,
                Phone = patient.User.Phone ?? "",
                DateOfBirth = patient.DateOfBirth,
                Address = patient.Address,
                EmergencyContactName = patient.EmergencyContactName,
                EmergencyContactPhone = patient.EmergencyContactPhone,
                DoctorNotes = patient.DoctorNotes,
                RegistrationDate = patient.RegistrationDate
            };

            return Ok(ApiResponse<PatientDTO>.SuccessResponse(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching patient");
            return StatusCode(500, ApiResponse<PatientDTO>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while fetching patient"));
        }
    }

    /// <summary>
    /// Update patient profile (Patient can update own, Doctor/Clerk can update any)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Patient,Doctor,Clerk")]
    public async Task<ActionResult<ApiResponse<PatientDTO>>> UpdatePatient(
        string id, [FromBody] UpdatePatientRequest request)
    {
        try
        {
            if (!Guid.TryParse(id, out var patientId))
            {
                return BadRequest(ApiResponse<PatientDTO>.ErrorResponse(
                    "INVALID_ID", "Invalid patient ID"));
            }

            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null)
            {
                return NotFound(ApiResponse<PatientDTO>.ErrorResponse(
                    "NOT_FOUND", "Patient not found"));
            }

            // Authorization check
            var role = GetUserRole();
            var userId = GetUserId();

            if (role == "Patient")
            {
                if (!Guid.TryParse(userId, out var userGuid) || userGuid != patient.UserId)
                {
                    return Forbid();
                }
            }

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(request.FirstName))
                patient.User.FirstName = request.FirstName;

            if (!string.IsNullOrWhiteSpace(request.LastName))
                patient.User.LastName = request.LastName;

            if (request.Phone != null)
                patient.User.Phone = request.Phone;

            if (request.DateOfBirth.HasValue)
                patient.DateOfBirth = request.DateOfBirth.Value;

            patient.User.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var dto = new PatientDTO
            {
                Id = patient.Id.ToString(),
                Email = patient.User.Email,
                FirstName = patient.User.FirstName,
                LastName = patient.User.LastName,
                Phone = patient.User.Phone ?? "",
                DateOfBirth = patient.DateOfBirth,
                Address = patient.Address,
                EmergencyContactName = patient.EmergencyContactName,
                EmergencyContactPhone = patient.EmergencyContactPhone,
                DoctorNotes = patient.DoctorNotes,
                RegistrationDate = patient.RegistrationDate
            };

            return Ok(ApiResponse<PatientDTO>.SuccessResponse(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating patient");
            return StatusCode(500, ApiResponse<PatientDTO>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while updating patient"));
        }
    }

    /// <summary>
    /// Get doctor's notes for a patient (Doctor and Clerk only)
    /// </summary>
    [HttpGet("{id}/notes")]
    [Authorize(Roles = "Doctor,Clerk")]
    public async Task<ActionResult<ApiResponse<string>>> GetPatientNotes(string id)
    {
        try
        {
            if (!Guid.TryParse(id, out var patientId))
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(
                    "INVALID_ID", "Invalid patient ID"));
            }

            var patient = await _context.Patients.FindAsync(patientId);

            if (patient == null)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(
                    "NOT_FOUND", "Patient not found"));
            }

            return Ok(ApiResponse<string>.SuccessResponse(patient.DoctorNotes ?? ""));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching patient notes");
            return StatusCode(500, ApiResponse<string>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while fetching patient notes"));
        }
    }

    /// <summary>
    /// Update doctor's notes for a patient (Doctor and Clerk only)
    /// </summary>
    [HttpPut("{id}/notes")]
    [Authorize(Roles = "Doctor,Clerk")]
    public async Task<ActionResult<ApiResponse<string>>> UpdatePatientNotes(
        string id, [FromBody] UpdatePatientNotesRequest request)
    {
        try
        {
            if (!Guid.TryParse(id, out var patientId))
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(
                    "INVALID_ID", "Invalid patient ID"));
            }

            var patient = await _context.Patients.FindAsync(patientId);

            if (patient == null)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(
                    "NOT_FOUND", "Patient not found"));
            }

            patient.DoctorNotes = request.Notes;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<string>.SuccessResponse(patient.DoctorNotes ?? ""));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating patient notes");
            return StatusCode(500, ApiResponse<string>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while updating patient notes"));
        }
    }
}
