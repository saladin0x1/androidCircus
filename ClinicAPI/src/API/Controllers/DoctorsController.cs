using API.Data;
using API.DTOs;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly ClinicDbContext _context;

    public DoctorsController(ClinicDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetDoctors()
    {
        var doctors = await _context.Doctors
            .Include(d => d.User)
            .Select(d => new
            {
                Id = d.Id,
                Name = d.User.FirstName + " " + d.User.LastName,
                Specialization = d.Specialization
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.SuccessResponse(doctors));
    }
}
