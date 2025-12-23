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
public class AppointmentsController : ControllerBase
{
    private readonly ClinicDbContext _context;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(ClinicDbContext context, ILogger<AppointmentsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    private string GetUserRole() => User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
    private string GetRoleSpecificId() => User.FindFirst("RoleSpecificId")?.Value ?? string.Empty;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AppointmentDTO>>>> GetAppointments([FromQuery] string? status = null)
    {
        try
        {
            var role = GetUserRole();
            var roleSpecificId = GetRoleSpecificId();

            IQueryable<Appointment> query = _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User);

            // Filter by role
            query = role switch
            {
                "Patient" => query.Where(a => a.PatientId.ToString() == roleSpecificId),
                "Doctor" => query.Where(a => a.DoctorId.ToString() == roleSpecificId),
                _ => query // Clerk sees all
            };

            // Filter by status if provided
            if (!string.IsNullOrEmpty(status) && status.ToLower() != "all")
            {
                if (Enum.TryParse<AppointmentStatus>(status, true, out var statusEnum))
                {
                    query = query.Where(a => a.Status == statusEnum);
                }
            }

            var appointments = await query
                .OrderBy(a => a.AppointmentDate)
                .Select(a => new AppointmentDTO
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
                })
                .ToListAsync();

            return Ok(ApiResponse<List<AppointmentDTO>>.SuccessResponse(appointments));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching appointments");
            return StatusCode(500, ApiResponse<List<AppointmentDTO>>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while fetching appointments"));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AppointmentDTO>>> GetAppointment(string id)
    {
        try
        {
            if (!Guid.TryParse(id, out var appointmentId))
            {
                return BadRequest(ApiResponse<AppointmentDTO>.ErrorResponse(
                    "INVALID_ID", "Invalid appointment ID"));
            }

            var appointment = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                return NotFound(ApiResponse<AppointmentDTO>.ErrorResponse(
                    "NOT_FOUND", "Appointment not found"));
            }

            // Check authorization
            var role = GetUserRole();
            var roleSpecificId = GetRoleSpecificId();

            if (role == "Patient" && appointment.PatientId.ToString() != roleSpecificId)
            {
                return Forbid();
            }
            else if (role == "Doctor" && appointment.DoctorId.ToString() != roleSpecificId)
            {
                return Forbid();
            }

            var dto = new AppointmentDTO
            {
                Id = appointment.Id.ToString(),
                PatientId = appointment.PatientId.ToString(),
                DoctorId = appointment.DoctorId.ToString(),
                AppointmentDate = appointment.AppointmentDate,
                Reason = appointment.Reason,
                Notes = appointment.Notes,
                DoctorNotes = appointment.DoctorNotes,
                Status = appointment.Status.ToString(),
                PatientName = $"{appointment.Patient.User.FirstName} {appointment.Patient.User.LastName}",
                DoctorName = $"{appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}",
                DoctorSpecialization = appointment.Doctor.Specialization
            };

            return Ok(ApiResponse<AppointmentDTO>.SuccessResponse(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching appointment");
            return StatusCode(500, ApiResponse<AppointmentDTO>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while fetching the appointment"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AppointmentDTO>>> CreateAppointment([FromBody] CreateAppointmentRequest request)
    {
        try
        {
            if (!Guid.TryParse(request.PatientId, out var patientId) ||
                !Guid.TryParse(request.DoctorId, out var doctorId))
            {
                return BadRequest(ApiResponse<AppointmentDTO>.ErrorResponse(
                    "INVALID_ID", "Invalid patient or doctor ID"));
            }

            // Check if patient and doctor exist
            var patientExists = await _context.Patients.AnyAsync(p => p.Id == patientId);
            var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == doctorId);

            if (!patientExists || !doctorExists)
            {
                return BadRequest(ApiResponse<AppointmentDTO>.ErrorResponse(
                    "INVALID_REFERENCE", "Patient or doctor not found"));
            }

            // Check for conflicts (same doctor, same time)
            var hasConflict = await _context.Appointments.AnyAsync(a =>
                a.DoctorId == doctorId &&
                a.AppointmentDate == request.AppointmentDate &&
                a.Status != AppointmentStatus.Cancelled);

            if (hasConflict)
            {
                return Conflict(ApiResponse<AppointmentDTO>.ErrorResponse(
                    "APPOINTMENT_CONFLICT", "This time slot is already booked"));
            }

            var appointment = new Appointment
            {
                PatientId = patientId,
                DoctorId = doctorId,
                AppointmentDate = request.AppointmentDate,
                Reason = request.Reason,
                Notes = request.Notes,
                CreatedBy = Guid.Parse(GetUserId()),
                Status = AppointmentStatus.Scheduled
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Fetch with related data for response
            var created = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .FirstAsync(a => a.Id == appointment.Id);

            var dto = new AppointmentDTO
            {
                Id = created.Id.ToString(),
                PatientId = created.PatientId.ToString(),
                DoctorId = created.DoctorId.ToString(),
                AppointmentDate = created.AppointmentDate,
                Reason = created.Reason,
                Notes = created.Notes,
                Status = created.Status.ToString(),
                PatientName = $"{created.Patient.User.FirstName} {created.Patient.User.LastName}",
                DoctorName = $"{created.Doctor.User.FirstName} {created.Doctor.User.LastName}",
                DoctorSpecialization = created.Doctor.Specialization
            };

            return CreatedAtAction(nameof(GetAppointment), new { id = dto.Id },
                ApiResponse<AppointmentDTO>.SuccessResponse(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment");
            return StatusCode(500, ApiResponse<AppointmentDTO>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while creating the appointment"));
        }
    }

    [HttpPut("{id}/complete")]
    [Authorize(Roles = "Doctor")]
    public async Task<ActionResult<ApiResponse<AppointmentDTO>>> CompleteAppointment(
        string id, [FromBody] CompleteAppointmentRequest request)
    {
        try
        {
            if (!Guid.TryParse(id, out var appointmentId))
            {
                return BadRequest(ApiResponse<AppointmentDTO>.ErrorResponse(
                    "INVALID_ID", "Invalid appointment ID"));
            }

            var appointment = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                return NotFound(ApiResponse<AppointmentDTO>.ErrorResponse(
                    "NOT_FOUND", "Appointment not found"));
            }

            // Verify doctor owns this appointment
            var roleSpecificId = GetRoleSpecificId();
            if (appointment.DoctorId.ToString() != roleSpecificId)
            {
                return Forbid();
            }

            appointment.Status = AppointmentStatus.Completed;
            appointment.DoctorNotes = request.DoctorNotes;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var dto = new AppointmentDTO
            {
                Id = appointment.Id.ToString(),
                PatientId = appointment.PatientId.ToString(),
                DoctorId = appointment.DoctorId.ToString(),
                AppointmentDate = appointment.AppointmentDate,
                Reason = appointment.Reason,
                Notes = appointment.Notes,
                DoctorNotes = appointment.DoctorNotes,
                Status = appointment.Status.ToString(),
                PatientName = $"{appointment.Patient.User.FirstName} {appointment.Patient.User.LastName}",
                DoctorName = $"{appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}",
                DoctorSpecialization = appointment.Doctor.Specialization
            };

            return Ok(ApiResponse<AppointmentDTO>.SuccessResponse(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing appointment");
            return StatusCode(500, ApiResponse<AppointmentDTO>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while completing the appointment"));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> CancelAppointment(string id)
    {
        try
        {
            if (!Guid.TryParse(id, out var appointmentId))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "INVALID_ID", "Invalid appointment ID"));
            }

            var appointment = await _context.Appointments.FindAsync(appointmentId);

            if (appointment == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "NOT_FOUND", "Appointment not found"));
            }

            // Check authorization
            var role = GetUserRole();
            var roleSpecificId = GetRoleSpecificId();

            if (role == "Patient" && appointment.PatientId.ToString() != roleSpecificId)
            {
                return Forbid();
            }
            else if (role == "Doctor")
            {
                return Forbid(); // Doctors can't cancel, only complete
            }

            appointment.Status = AppointmentStatus.Cancelled;
            appointment.CancelledAt = DateTime.UtcNow;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(new { Message = "Appointment cancelled successfully" }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling appointment");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while cancelling the appointment"));
        }
    }
}
