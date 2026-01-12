using API.Data;
using API.DTOs;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Clerk")]
public class ClerkController : ControllerBase
{
    private readonly ClinicDbContext _context;
    private readonly ILogger<ClerkController> _logger;

    public ClerkController(ClinicDbContext context, ILogger<ClerkController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<object>>> GetDashboard()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var todayAppointments = await _context.Appointments
            .CountAsync(a => a.AppointmentDate >= today && a.AppointmentDate < tomorrow);

        var pendingAppointments = await _context.Appointments
            .CountAsync(a => a.Status == AppointmentStatus.Scheduled);

        var totalPatients = await _context.Patients.CountAsync();
        var totalDoctors = await _context.Doctors.CountAsync();

        var stats = new
        {
            TodayAppointments = todayAppointments,
            PendingAppointments = pendingAppointments,
            TotalPatients = totalPatients,
            TotalDoctors = totalDoctors
        };

        return Ok(ApiResponse<object>.SuccessResponse(stats));
    }

    [HttpGet("dashboard/today")]
    public async Task<ActionResult<ApiResponse<List<AppointmentDTO>>>> GetTodayAppointments()
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Where(a => a.AppointmentDate >= today && a.AppointmentDate < tomorrow)
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
            _logger.LogError(ex, "Error fetching today's appointments");
            return StatusCode(500, ApiResponse<List<AppointmentDTO>>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while fetching appointments"));
        }
    }

    [HttpGet("dashboard/pending")]
    public async Task<ActionResult<ApiResponse<List<AppointmentDTO>>>> GetPendingAppointments()
    {
        try
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Where(a => a.Status == AppointmentStatus.Scheduled)
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
            _logger.LogError(ex, "Error fetching pending appointments");
            return StatusCode(500, ApiResponse<List<AppointmentDTO>>.ErrorResponse(
                "SERVER_ERROR", "An error occurred while fetching appointments"));
        }
    }
}
