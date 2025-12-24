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

    public ClerkController(ClinicDbContext context)
    {
        _context = context;
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
}
