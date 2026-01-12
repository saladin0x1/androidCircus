using API.Data;
using API.DTOs;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class AuthService
{
    private readonly ClinicDbContext _context;
    private readonly JwtService _jwtService;

    public AuthService(ClinicDbContext context, JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Patient)
            .Include(u => u.Doctor)
            .Include(u => u.Clerk)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return new LoginResponse
            {
                Success = false,
                Error = "Invalid email or password"
            };
        }

        if (!user.IsActive)
        {
            return new LoginResponse
            {
                Success = false,
                Error = "Account is inactive"
            };
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Get role-specific ID
        var roleSpecificId = user.Role switch
        {
            UserRole.Patient => user.Patient?.Id.ToString() ?? string.Empty,
            UserRole.Doctor => user.Doctor?.Id.ToString() ?? string.Empty,
            UserRole.Clerk => user.Clerk?.Id.ToString() ?? string.Empty,
            _ => string.Empty
        };

        var token = _jwtService.GenerateToken(user, roleSpecificId);

        return new LoginResponse
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
        };
    }

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
    {
        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return new LoginResponse
            {
                Success = false,
                Error = "Email already registered"
            };
        }

        // Create user (inactive until approved by clerk)
        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            Role = request.Role,
            IsActive = false  // Requires clerk approval
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Create role-specific entity
        string roleSpecificId = string.Empty;

        switch (request.Role)
        {
            case UserRole.Patient:
                var patient = new Patient
                {
                    UserId = user.Id,
                    DateOfBirth = request.DateOfBirth
                };
                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();
                roleSpecificId = patient.Id.ToString();
                break;

            case UserRole.Doctor:
                var doctor = new Doctor
                {
                    UserId = user.Id,
                    Specialization = request.Specialization ?? "General Practitioner"
                };
                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();
                roleSpecificId = doctor.Id.ToString();
                break;

            case UserRole.Clerk:
                var clerk = new Clerk
                {
                    UserId = user.Id
                };
                _context.Clerks.Add(clerk);
                await _context.SaveChangesAsync();
                roleSpecificId = clerk.Id.ToString();
                break;
        }

        // Don't return token for inactive users - they need approval first
        return new LoginResponse
        {
            Success = true,
            Data = new UserData
            {
                UserId = user.Id.ToString(),
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role.ToString(),
                Token = null,  // No token until approved
                RoleSpecificId = roleSpecificId
            },
            Message = "Account created successfully. Please wait for clerk approval."
        };
    }
}
