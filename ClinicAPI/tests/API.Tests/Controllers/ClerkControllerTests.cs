using System.Net;
using System.Text.Json;
using API.DTOs;
using API.Models;
using API.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace API.Tests.Controllers;

public class ClerkControllerTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly HttpClient _client;

    public ClerkControllerTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    private void SetAuthHeader(string token)
    {
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    }

    [Fact]
    public async Task GetDashboard_AsClerk_ReturnsDashboardStats()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var clerk = await _fixture.CreateTestUserAsync("clerk@example.com", UserRole.Clerk);
        var token = _fixture.GenerateTestToken(clerk);
        SetAuthHeader(token);

        await _fixture.CreateTestUserAsync("doctor1@example.com", UserRole.Doctor);
        await _fixture.CreateTestUserAsync("doctor2@example.com", UserRole.Doctor);
        await _fixture.CreateTestUserAsync("patient1@example.com", UserRole.Patient);
        await _fixture.CreateTestUserAsync("patient2@example.com", UserRole.Patient);
        await _fixture.CreateTestUserAsync("patient3@example.com", UserRole.Patient);

        // Act
        var response = await _client.GetAsync("/api/clerk/dashboard");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        Assert.True(root.GetProperty("success").GetBoolean());
        var data = root.GetProperty("data");

        Assert.Equal(2, data.GetProperty("totalDoctors").GetInt32());
        Assert.Equal(3, data.GetProperty("totalPatients").GetInt32());
        Assert.True(data.GetProperty("todayAppointments").GetInt32() >= 0);
        Assert.True(data.GetProperty("pendingAppointments").GetInt32() >= 0);
    }

    [Fact]
    public async Task GetDashboard_AsDoctor_ReturnsForbidden()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);
        var token = _fixture.GenerateTestToken(doctor);
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync("/api/clerk/dashboard");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetDashboard_AsPatient_ReturnsForbidden()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync("/api/clerk/dashboard");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetDashboard_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        _client.DefaultRequestHeaders.Clear();

        // Act
        var response = await _client.GetAsync("/api/clerk/dashboard");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDashboard_WithTodayAppointments_ReturnsCorrectCount()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var clerk = await _fixture.CreateTestUserAsync("clerk@example.com", UserRole.Clerk);
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);
        var patient1 = await _fixture.CreateTestUserAsync("patient1@example.com", UserRole.Patient);
        var patient2 = await _fixture.CreateTestUserAsync("patient2@example.com", UserRole.Patient);

        // Create appointments for today
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        _fixture.DbContext.Appointments.AddRange(
            new API.Models.Appointment
            {
                PatientId = patient1.Patient!.Id,
                DoctorId = doctor.Doctor!.Id,
                AppointmentDate = today.AddHours(10),
                Status = AppointmentStatus.Scheduled,
                Reason = "Checkup",
                CreatedBy = clerk.Id
            },
            new API.Models.Appointment
            {
                PatientId = patient2.Patient!.Id,
                DoctorId = doctor.Doctor!.Id,
                AppointmentDate = today.AddHours(14),
                Status = AppointmentStatus.Scheduled,
                Reason = "Follow-up",
                CreatedBy = clerk.Id
            },
            new API.Models.Appointment
            {
                PatientId = patient1.Patient!.Id,
                DoctorId = doctor.Doctor!.Id,
                AppointmentDate = tomorrow.AddHours(10),
                Status = AppointmentStatus.Scheduled,
                Reason = "Future appointment",
                CreatedBy = clerk.Id
            }
        );
        await _fixture.DbContext.SaveChangesAsync();

        var token = _fixture.GenerateTestToken(clerk);
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync("/api/clerk/dashboard");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;
        var data = root.GetProperty("data");

        Assert.Equal(2, data.GetProperty("todayAppointments").GetInt32());
    }

    [Fact]
    public async Task GetDashboard_WithPendingAppointments_ReturnsCorrectCount()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var clerk = await _fixture.CreateTestUserAsync("clerk@example.com", UserRole.Clerk);
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);

        // Create appointments with different statuses
        var today = DateTime.UtcNow.Date;

        _fixture.DbContext.Appointments.AddRange(
            new API.Models.Appointment
            {
                PatientId = patient.Patient!.Id,
                DoctorId = doctor.Doctor!.Id,
                AppointmentDate = today.AddDays(1),
                Status = AppointmentStatus.Scheduled,
                Reason = "Scheduled",
                CreatedBy = clerk.Id
            },
            new API.Models.Appointment
            {
                PatientId = patient.Patient!.Id,
                DoctorId = doctor.Doctor!.Id,
                AppointmentDate = today.AddDays(-1),
                Status = AppointmentStatus.Scheduled,
                Reason = "Another scheduled",
                CreatedBy = clerk.Id
            },
            new API.Models.Appointment
            {
                PatientId = patient.Patient!.Id,
                DoctorId = doctor.Doctor!.Id,
                AppointmentDate = today.AddDays(-2),
                Status = AppointmentStatus.Completed,
                Reason = "Completed",
                CreatedBy = clerk.Id
            },
            new API.Models.Appointment
            {
                PatientId = patient.Patient!.Id,
                DoctorId = doctor.Doctor!.Id,
                AppointmentDate = today.AddDays(-3),
                Status = AppointmentStatus.Cancelled,
                Reason = "Cancelled",
                CreatedBy = clerk.Id
            }
        );
        await _fixture.DbContext.SaveChangesAsync();

        var token = _fixture.GenerateTestToken(clerk);
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync("/api/clerk/dashboard");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;
        var data = root.GetProperty("data");

        Assert.Equal(2, data.GetProperty("pendingAppointments").GetInt32());
    }

    [Fact]
    public async Task GetDashboard_WithEmptyDatabase_ReturnsZeros()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var clerk = await _fixture.CreateTestUserAsync("clerk@example.com", UserRole.Clerk);
        var token = _fixture.GenerateTestToken(clerk);
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync("/api/clerk/dashboard");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;
        var data = root.GetProperty("data");

        // Note: The clerk themselves counts as 1 user, but doctors and patients should be 0
        // since we haven't created any other than the clerk
        Assert.Equal(0, data.GetProperty("totalDoctors").GetInt32());
        Assert.Equal(0, data.GetProperty("totalPatients").GetInt32());
        Assert.Equal(0, data.GetProperty("todayAppointments").GetInt32());
        Assert.Equal(0, data.GetProperty("pendingAppointments").GetInt32());
    }
}
