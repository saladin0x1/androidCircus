using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class ClinicDbContext : DbContext
{
    public ClinicDbContext(DbContextOptions<ClinicDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Patient> Patients { get; set; } = null!;
    public DbSet<Doctor> Doctors { get; set; } = null!;
    public DbSet<Clerk> Clerks { get; set; } = null!;
    public DbSet<Appointment> Appointments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User entity configuration
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasOne(u => u.Patient)
            .WithOne(p => p.User)
            .HasForeignKey<Patient>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Doctor)
            .WithOne(d => d.User)
            .HasForeignKey<Doctor>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Clerk)
            .WithOne(c => c.User)
            .HasForeignKey<Clerk>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Appointment entity configuration
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasIndex(a => a.AppointmentDate);

        modelBuilder.Entity<Appointment>()
            .HasIndex(a => a.Status);

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Create user IDs
        var patientUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var doctorUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var clerkUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        // Password: "Password123!" hashed with BCrypt
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Password123!");

        // Seed Users
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = patientUserId,
                Email = "patient@clinic.com",
                PasswordHash = hashedPassword,
                FirstName = "John",
                LastName = "Doe",
                Phone = "+1234567890",
                Role = UserRole.Patient,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = doctorUserId,
                Email = "doctor@clinic.com",
                PasswordHash = hashedPassword,
                FirstName = "Dr. Sarah",
                LastName = "Smith",
                Phone = "+1234567891",
                Role = UserRole.Doctor,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = clerkUserId,
                Email = "clerk@clinic.com",
                PasswordHash = hashedPassword,
                FirstName = "Mary",
                LastName = "Johnson",
                Phone = "+1234567892",
                Role = UserRole.Clerk,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );

        // Seed Patient
        var patientId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        modelBuilder.Entity<Patient>().HasData(
            new Patient
            {
                Id = patientId,
                UserId = patientUserId,
                DateOfBirth = new DateTime(1990, 5, 15),
                Address = "123 Main St, City",
                EmergencyContactName = "Jane Doe",
                EmergencyContactPhone = "+1234567899",
                RegistrationDate = DateTime.UtcNow
            }
        );

        // Seed Doctor
        var doctorId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        modelBuilder.Entity<Doctor>().HasData(
            new Doctor
            {
                Id = doctorId,
                UserId = doctorUserId,
                Specialization = "General Practitioner",
                LicenseNumber = "MD12345",
                YearsOfExperience = 10,
                JoinedDate = DateTime.UtcNow
            }
        );

        // Seed Clerk
        var clerkId = Guid.Parse("66666666-6666-6666-6666-666666666666");
        modelBuilder.Entity<Clerk>().HasData(
            new Clerk
            {
                Id = clerkId,
                UserId = clerkUserId,
                Department = "Reception",
                HireDate = DateTime.UtcNow
            }
        );

        // Seed sample appointments
        modelBuilder.Entity<Appointment>().HasData(
            new Appointment
            {
                Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                PatientId = patientId,
                DoctorId = doctorId,
                AppointmentDate = DateTime.UtcNow.AddDays(2),
                Reason = "Annual checkup",
                Status = AppointmentStatus.Scheduled,
                CreatedBy = clerkUserId,
                CreatedAt = DateTime.UtcNow
            },
            new Appointment
            {
                Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                PatientId = patientId,
                DoctorId = doctorId,
                AppointmentDate = DateTime.UtcNow.AddDays(7),
                Reason = "Follow-up consultation",
                Status = AppointmentStatus.Scheduled,
                CreatedBy = patientUserId,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
