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
                // Parse the GUID from the role-specific claim
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

    [HttpPut("{id}/notes")]
    [Authorize(Roles = "Doctor")]
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
