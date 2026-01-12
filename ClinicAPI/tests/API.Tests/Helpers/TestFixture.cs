using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading;
using System.Text;
using System.Text.Json;
using API.Data;
using API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace API.Tests.Helpers;

public class TestFixture : WebApplicationFactory<Program>
{
    public ClinicDbContext DbContext { get; private set; } = null!;
    private static int _testRunId = 0;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<ClinicDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database with unique name for each test run
            var dbName = $"TestDb_{Interlocked.Increment(ref _testRunId)}";
            services.AddDbContext<ClinicDbContext>(options =>
            {
                options.UseInMemoryDatabase(dbName);
                options.EnableSensitiveDataLogging();
            });

            // Create the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Get the DbContext and ensure it's created
            DbContext = serviceProvider.GetRequiredService<ClinicDbContext>();
            DbContext.Database.EnsureCreated();
        });
    }

    public async Task ResetDatabaseAsync()
    {
        // Clear all entities individually to avoid cascade issues
        var appointments = await DbContext.Appointments.ToListAsync();
        DbContext.Appointments.RemoveRange(appointments);

        var patients = await DbContext.Patients.ToListAsync();
        DbContext.Patients.RemoveRange(patients);

        var doctors = await DbContext.Doctors.ToListAsync();
        DbContext.Doctors.RemoveRange(doctors);

        var clerks = await DbContext.Clerks.ToListAsync();
        DbContext.Clerks.RemoveRange(clerks);

        var users = await DbContext.Users.ToListAsync();
        DbContext.Users.RemoveRange(users);

        await DbContext.SaveChangesAsync();
    }

    public async Task<User> CreateTestUserAsync(string email, UserRole role, string password = "Password123!")
    {
        var user = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FirstName = "Test",
            LastName = "User",
            Phone = "1234567890",
            Role = role,
            IsActive = true
        };

        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        // Create role-specific entity
        switch (role)
        {
            case UserRole.Patient:
                var patient = new Patient
                {
                    UserId = user.Id,
                    DateOfBirth = DateTime.UtcNow.AddYears(-30)
                };
                DbContext.Patients.Add(patient);
                await DbContext.SaveChangesAsync();

                // Reload to get navigation property
                await DbContext.Entry(user).Reference(u => u.Patient).LoadAsync();
                break;

            case UserRole.Doctor:
                var doctor = new Doctor
                {
                    UserId = user.Id,
                    Specialization = "General Practitioner"
                };
                DbContext.Doctors.Add(doctor);
                await DbContext.SaveChangesAsync();

                // Reload to get navigation property
                await DbContext.Entry(user).Reference(u => u.Doctor).LoadAsync();
                break;

            case UserRole.Clerk:
                var clerk = new Clerk
                {
                    UserId = user.Id
                };
                DbContext.Clerks.Add(clerk);
                await DbContext.SaveChangesAsync();

                // Reload to get navigation property
                await DbContext.Entry(user).Reference(u => u.Clerk).LoadAsync();
                break;
        }

        return user;
    }

    public string GenerateTestToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            "YourSuperSecretKeyForJWTMustBeAtLeast32CharactersLong!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("RoleSpecificId", user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: "ClinicAPI",
            audience: "ClinicApp",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }
}
