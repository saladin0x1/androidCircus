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

    /// <summary>
    /// Get appointments filtered by user role
    /// - Patient: sees own appointments
    /// - Doctor: sees assigned appointments
    /// - Clerk: sees all appointments
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AppointmentDTO>>>> GetAppointments(
        [FromQuery] string? status = null,
        [FromQuery] DateTime? date = null,
        [FromQuery] string? doctorId = null)
    {
        try
        {
            var role = GetUserRole();
            var userId = GetUserId();

            IQueryable<Appointment> query = _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User);

            // Filter by role
            switch (role)
            {
                case "Patient":
                    if (!Guid.TryParse(userId, out var patientUserId))
                    {
                        return BadRequest(ApiResponse<List<AppointmentDTO>>.ErrorResponse(
                            "INVALID_USER", "Invalid user ID"));
                    }
                    var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == patientUserId);
                    if (patient == null)
                    {
                        return NotFound(ApiResponse<List<AppointmentDTO>>.ErrorResponse(
                            "PATIENT_NOT_FOUND", "Patient profile not found"));
                    }
                    query = query.Where(a => a.PatientId == patient.Id);
                    break;

                case "Doctor":
                    if (!Guid.TryParse(userId, out var doctorUserId))
                    {
                        return BadRequest(ApiResponse<List<AppointmentDTO>>.ErrorResponse(
                            "INVALID_USER", "Invalid user ID"));
                    }
                    var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == doctorUserId);
                    if (doctor == null)
                    {
                        return NotFound(ApiResponse<List<AppointmentDTO>>.ErrorResponse(
                            "DOCTOR_NOT_FOUND", "Doctor profile not found"));
                    }
                    query = query.Where(a => a.DoctorId == doctor.Id);
                    break;

                case "Clerk":
                    // Clerks see all appointments, can filter by doctor
                    if (!string.IsNullOrEmpty(doctorId) && Guid.TryParse(doctorId, out var doctorGuid))
                    {
                        query = query.Where(a => a.DoctorId == doctorGuid);
                    }
                    break;
            }

            // Optional status filter
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<AppointmentStatus>(status, true, out var statusEnum))
            {
                query = query.Where(a => a.Status == statusEnum);
            }

            // Optional date filter (appointments on specific date)
            if (date.HasValue)
            {
                var targetDate = date.Value.Date;
                var nextDay = targetDate.AddDays(1);
                query = query.Where(a => a.AppointmentDate >= targetDate && a.AppointmentDate < nextDay);
            }

            var appointments = await query
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
                DoctorName = $"Dr. {a.Doctor.User.FirstName} {a.Doctor.User.LastName}",
                DoctorSpecialization = a.Doctor.Specialization
            }).ToList();

            return Ok(ApiResponse<List<AppointmentDTO>>.SuccessResponse(dtoList));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching appointments");
            return StatusCode(500, ApiResponse<List<AppointmentDTO>>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while fetching appointments"));
        }
    }

    /// <summary>
    /// Get a specific appointment by ID
    /// </summary>
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
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                return NotFound(ApiResponse<AppointmentDTO>.ErrorResponse(
                    "NOT_FOUND", "Appointment not found"));
            }

            // Authorization check
            var role = GetUserRole();
            var userId = GetUserId();

            switch (role)
            {
                case "Patient":
                    if (!Guid.TryParse(userId, out var patientUserId))
                        return Forbid();
                    var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == patientUserId);
                    if (patient == null || appointment.PatientId != patient.Id)
                        return Forbid();
                    break;

                case "Doctor":
                    if (!Guid.TryParse(userId, out var doctorUserId))
                        return Forbid();
                    var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == doctorUserId);
                    if (doctor == null || appointment.DoctorId != doctor.Id)
                        return Forbid();
                    break;

                case "Clerk":
                    // Clerks can access any appointment
                    break;
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
                DoctorName = $"Dr. {appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}",
                DoctorSpecialization = appointment.Doctor.Specialization
            };

            return Ok(ApiResponse<AppointmentDTO>.SuccessResponse(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching appointment");
            return StatusCode(500, ApiResponse<AppointmentDTO>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while fetching appointment"));
        }
    }

    /// <summary>
    /// Get available time slots for a doctor on a specific date
    /// </summary>
    [HttpGet("available-slots")]
    public async Task<ActionResult<ApiResponse<List<TimeSlotDTO>>>> GetAvailableSlots(
        [FromQuery] string doctorId,
        [FromQuery] DateTime date)
    {
        try
        {
            if (!Guid.TryParse(doctorId, out var doctorGuid))
            {
                return BadRequest(ApiResponse<List<TimeSlotDTO>>.ErrorResponse(
                    "INVALID_DOCTOR_ID", "Invalid doctor ID"));
            }

            // Verify doctor exists
            var doctor = await _context.Doctors.FindAsync(doctorGuid);
            if (doctor == null)
            {
                return NotFound(ApiResponse<List<TimeSlotDTO>>.ErrorResponse(
                    "DOCTOR_NOT_FOUND", "Doctor not found"));
            }

            var targetDate = date.Date;
            var startOfDay = targetDate;
            var endOfDay = targetDate.AddDays(1);

            // Get existing appointments for this doctor on this date
            var existingAppointments = await _context.Appointments
                .Where(a => a.DoctorId == doctorGuid
                    && a.AppointmentDate >= startOfDay
                    && a.AppointmentDate < endOfDay
                    && a.Status == AppointmentStatus.Scheduled)
                .Select(a => a.AppointmentDate)
                .ToListAsync();

            // Generate time slots from 8 AM to 6 PM (30-minute intervals)
            var slots = new List<TimeSlotDTO>();
            var startHour = 8;
            var endHour = 18;

            for (int hour = startHour; hour < endHour; hour++)
            {
                for (int minute = 0; minute < 60; minute += 30)
                {
                    var slotTime = new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, hour, minute, 0);

                    // Check if this slot is booked
                    var isBooked = existingAppointments.Any(a =>
                        a.Hour == slotTime.Hour && a.Minute == slotTime.Minute);

                    slots.Add(new TimeSlotDTO
                    {
                        Time = slotTime.ToString("HH:mm"),
                        Available = !isBooked
                    });
                }
            }

            return Ok(ApiResponse<List<TimeSlotDTO>>.SuccessResponse(slots));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching available slots");
            return StatusCode(500, ApiResponse<List<TimeSlotDTO>>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while fetching available slots"));
        }
    }

    /// <summary>
    /// Create a new appointment (Patient and Clerk only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Patient,Clerk")]
    public async Task<ActionResult<ApiResponse<AppointmentDTO>>> CreateAppointment([FromBody] CreateAppointmentRequest request)
    {
        try
        {
            // Validate GUIDs
            if (!Guid.TryParse(request.PatientId, out var patientGuid))
            {
                return BadRequest(ApiResponse<AppointmentDTO>.ErrorResponse(
                    "INVALID_PATIENT_ID", "Invalid patient ID"));
            }
            if (!Guid.TryParse(request.DoctorId, out var doctorGuid))
            {
                return BadRequest(ApiResponse<AppointmentDTO>.ErrorResponse(
                    "INVALID_DOCTOR_ID", "Invalid doctor ID"));
            }

            // Verify patient exists
            var patient = await _context.Patients.FindAsync(patientGuid);
            if (patient == null)
            {
                return NotFound(ApiResponse<AppointmentDTO>.ErrorResponse(
                    "PATIENT_NOT_FOUND", "Patient not found"));
            }

            // Verify doctor exists
            var doctor = await _context.Doctors.FindAsync(doctorGuid);
            if (doctor == null)
            {
                return NotFound(ApiResponse<AppointmentDTO>.ErrorResponse(
                    "DOCTOR_NOT_FOUND", "Doctor not found"));
            }

            // If patient is creating, verify they're creating for themselves
            var role = GetUserRole();
            if (role == "Patient")
            {
                var userId = GetUserId();
                if (!Guid.TryParse(userId, out var userGuid))
                    return Forbid();

                var currentPatient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userGuid);
                if (currentPatient == null || currentPatient.Id != patientGuid)
                    return Forbid();
            }

            // Check for conflicting appointments (double-booking prevention)
            var appointmentTime = request.AppointmentDate;
            var appointmentEndTime = appointmentTime.AddMinutes(30); // Assuming 30-minute slots

            var conflictingAppointments = await _context.Appointments
                .Where(a => a.DoctorId == doctorGuid
                    && a.Status == AppointmentStatus.Scheduled
                    && a.AppointmentDate < appointmentEndTime
                    && a.AppointmentDate.AddMinutes(30) > appointmentTime)
                .ToListAsync();

            if (conflictingAppointments.Any())
            {
                return BadRequest(ApiResponse<AppointmentDTO>.ErrorResponse(
                    "SLOT_UNAVAILABLE", "This time slot is already booked. Please choose a different time."));
            }

            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                PatientId = patientGuid,
                DoctorId = doctorGuid,
                AppointmentDate = request.AppointmentDate,
                Reason = request.Reason,
                Notes = request.Notes,
                Status = AppointmentStatus.Scheduled,
                CreatedBy = Guid.Parse(GetUserId())
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            await _context.Entry(appointment)
                .Reference(a => a.Patient).Query()
                .Include(p => p.User).LoadAsync();
            await _context.Entry(appointment)
                .Reference(a => a.Doctor).Query()
                .Include(d => d.User).LoadAsync();

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
                DoctorName = $"Dr. {appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}",
                DoctorSpecialization = appointment.Doctor.Specialization
            };

            return Created(string.Empty, ApiResponse<AppointmentDTO>.SuccessResponse(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment");
            return StatusCode(500, ApiResponse<AppointmentDTO>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while creating appointment"));
        }
    }

    /// <summary>
    /// Reschedule an appointment (Patient, Doctor, Clerk)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Patient,Doctor,Clerk")]
    public async Task<ActionResult<ApiResponse<AppointmentDTO>>> RescheduleAppointment(
        string id, [FromBody] RescheduleAppointmentRequest request)
    {
        try
        {
            if (!Guid.TryParse(id, out var appointmentId))
            {
                return BadRequest(ApiResponse<AppointmentDTO>.ErrorResponse(
                    "INVALID_ID", "Invalid appointment ID"));
            }

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                return NotFound(ApiResponse<AppointmentDTO>.ErrorResponse(
                    "NOT_FOUND", "Appointment not found"));
            }

            // Authorization check
            var role = GetUserRole();
            var userId = GetUserId();

            switch (role)
            {
                case "Patient":
                    if (!Guid.TryParse(userId, out var patientUserId))
                        return Forbid();
                    var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == patientUserId);
                    if (patient == null || appointment.PatientId != patient.Id)
                        return Forbid();
                    break;

                case "Doctor":
                    if (!Guid.TryParse(userId, out var doctorUserId))
                        return Forbid();
                    var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == doctorUserId);
                    if (doctor == null || appointment.DoctorId != doctor.Id)
                        return Forbid();
                    break;
            }

            // Check if can be rescheduled (not completed or cancelled)
            if (appointment.Status == AppointmentStatus.Completed)
            {
                return BadRequest(ApiResponse<AppointmentDTO>.ErrorResponse(
                    "ALREADY_COMPLETED", "Cannot reschedule a completed appointment"));
            }

            if (appointment.Status == AppointmentStatus.Cancelled)
            {
                return BadRequest(ApiResponse<AppointmentDTO>.ErrorResponse(
                    "ALREADY_CANCELLED", "Cannot reschedule a cancelled appointment"));
            }

            // Check for conflicts with new time
            var newTime = request.AppointmentDate;
            var newEndTime = newTime.AddMinutes(30);

            var conflictingAppointments = await _context.Appointments
                .Where(a => a.DoctorId == appointment.DoctorId
                    && a.Id != appointment.Id
                    && a.Status == AppointmentStatus.Scheduled
                    && a.AppointmentDate < newEndTime
                    && a.AppointmentDate.AddMinutes(30) > newTime)
                .ToListAsync();

            if (conflictingAppointments.Any())
            {
                return BadRequest(ApiResponse<AppointmentDTO>.ErrorResponse(
                    "SLOT_UNAVAILABLE", "The requested time slot is already booked."));
            }

            // Update appointment time
            appointment.AppointmentDate = newTime;
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
                DoctorName = $"Dr. {appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}",
                DoctorSpecialization = appointment.Doctor.Specialization
            };

            return Ok(ApiResponse<AppointmentDTO>.SuccessResponse(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rescheduling appointment");
            return StatusCode(500, ApiResponse<AppointmentDTO>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while rescheduling appointment"));
        }
    }

    /// <summary>
    /// Update appointment status (Doctor and Clerk only)
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Doctor,Clerk")]
    public async Task<ActionResult<ApiResponse<AppointmentDTO>>> UpdateAppointmentStatus(
        string id, [FromBody] UpdateAppointmentStatusRequest request)
    {
        try
        {
            if (!Guid.TryParse(id, out var appointmentId))
            {
                return BadRequest(ApiResponse<AppointmentDTO>.ErrorResponse(
                    "INVALID_ID", "Invalid appointment ID"));
            }

            if (!Enum.TryParse<AppointmentStatus>(request.Status, true, out var newStatus))
            {
                return BadRequest(ApiResponse<AppointmentDTO>.ErrorResponse(
                    "INVALID_STATUS", "Invalid status value"));
            }

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                return NotFound(ApiResponse<AppointmentDTO>.ErrorResponse(
                    "NOT_FOUND", "Appointment not found"));
            }

            // If doctor, verify they're assigned to this appointment
            var role = GetUserRole();
            if (role == "Doctor")
            {
                var userId = GetUserId();
                if (!Guid.TryParse(userId, out var userGuid))
                    return Forbid();

                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userGuid);
                if (doctor == null || appointment.DoctorId != doctor.Id)
                    return Forbid();
            }

            appointment.Status = newStatus;
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
                DoctorName = $"Dr. {appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}",
                DoctorSpecialization = appointment.Doctor.Specialization
            };

            return Ok(ApiResponse<AppointmentDTO>.SuccessResponse(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating appointment status");
            return StatusCode(500, ApiResponse<AppointmentDTO>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while updating appointment status"));
        }
    }

    /// <summary>
    /// Complete an appointment (Doctor only)
    /// </summary>
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
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                return NotFound(ApiResponse<AppointmentDTO>.ErrorResponse(
                    "NOT_FOUND", "Appointment not found"));
            }

            // Verify doctor is assigned to this appointment
            var userId = GetUserId();
            if (!Guid.TryParse(userId, out var userGuid))
                return Forbid();

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userGuid);
            if (doctor == null || appointment.DoctorId != doctor.Id)
                return Forbid();

            // Update appointment
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
                DoctorName = $"Dr. {appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}",
                DoctorSpecialization = appointment.Doctor.Specialization
            };

            return Ok(ApiResponse<AppointmentDTO>.SuccessResponse(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing appointment");
            return StatusCode(500, ApiResponse<AppointmentDTO>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while completing appointment"));
        }
    }

    /// <summary>
    /// Cancel an appointment (Patient - own only, Clerk - any)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Patient,Clerk")]
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

            // Authorization check
            var role = GetUserRole();
            var userId = GetUserId();

            if (role == "Patient")
            {
                if (!Guid.TryParse(userId, out var userGuid))
                    return Forbid();

                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userGuid);
                if (patient == null || appointment.PatientId != patient.Id)
                    return Forbid();
            }

            // Check if already cancelled or completed
            if (appointment.Status == AppointmentStatus.Cancelled)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "ALREADY_CANCELLED", "Appointment is already cancelled"));
            }

            if (appointment.Status == AppointmentStatus.Completed)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "ALREADY_COMPLETED", "Cannot cancel a completed appointment"));
            }

            // Cancel the appointment
            appointment.Status = AppointmentStatus.Cancelled;
            appointment.CancelledAt = DateTime.UtcNow;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                message = "Appointment cancelled successfully",
                appointmentId = appointment.Id.ToString()
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling appointment");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while cancelling appointment"));
        }
    }
}
