using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using API.DTOs;
using API.Models;
using API.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace API.Tests.Controllers;

public class AppointmentsControllerTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly HttpClient _client;

    public AppointmentsControllerTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    private static async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    private void SetAuthHeader(string token)
    {
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    }

    [Fact]
    public async Task GetAppointments_AsPatient_ReturnsOwnAppointments()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient1 = await _fixture.CreateTestUserAsync("patient1@example.com", UserRole.Patient);
        var patient2 = await _fixture.CreateTestUserAsync("patient2@example.com", UserRole.Patient);
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);

        // Create appointments for both patients
        _fixture.DbContext.Appointments.AddRange(
            new Appointment
            {
                PatientId = patient1.Patient!.Id,
                DoctorId = doctor.Doctor!.Id,
                AppointmentDate = DateTime.UtcNow.AddHours(2),
                Status = AppointmentStatus.Scheduled,
                Reason = "Checkup",
                CreatedBy = patient1.Id
            },
            new Appointment
            {
                PatientId = patient2.Patient!.Id,
                DoctorId = doctor.Doctor!.Id,
                AppointmentDate = DateTime.UtcNow.AddHours(3),
                Status = AppointmentStatus.Scheduled,
                Reason = "Follow-up",
                CreatedBy = patient2.Id
            }
        );
        await _fixture.DbContext.SaveChangesAsync();

        var token = _fixture.GenerateTestToken(patient1);
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync("/api/appointments");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<List<AppointmentDTO>>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Single(result.Data);
        Assert.Equal(patient1.Patient.Id.ToString(), result.Data[0].PatientId);
    }

    [Fact]
    public async Task GetAppointments_AsDoctor_ReturnsAssignedAppointments()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var doctor1 = await _fixture.CreateTestUserAsync("doctor1@example.com", UserRole.Doctor);
        var doctor2 = await _fixture.CreateTestUserAsync("doctor2@example.com", UserRole.Doctor);

        // Create appointments with different doctors
        _fixture.DbContext.Appointments.AddRange(
            new Appointment
            {
                PatientId = patient.Patient!.Id,
                DoctorId = doctor1.Doctor!.Id,
                AppointmentDate = DateTime.UtcNow.AddHours(2),
                Status = AppointmentStatus.Scheduled,
                Reason = "Checkup",
                CreatedBy = patient.Id
            },
            new Appointment
            {
                PatientId = patient.Patient!.Id,
                DoctorId = doctor2.Doctor!.Id,
                AppointmentDate = DateTime.UtcNow.AddHours(3),
                Status = AppointmentStatus.Scheduled,
                Reason = "Follow-up",
                CreatedBy = patient.Id
            }
        );
        await _fixture.DbContext.SaveChangesAsync();

        var token = _fixture.GenerateTestToken(doctor1);
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync("/api/appointments");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<List<AppointmentDTO>>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Single(result.Data);
        Assert.Equal(doctor1.Doctor.Id.ToString(), result.Data[0].DoctorId);
    }

    [Fact]
    public async Task GetAppointments_AsClerk_ReturnsAllAppointments()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var clerk = await _fixture.CreateTestUserAsync("clerk@example.com", UserRole.Clerk);
        var patient1 = await _fixture.CreateTestUserAsync("patient1@example.com", UserRole.Patient);
        var patient2 = await _fixture.CreateTestUserAsync("patient2@example.com", UserRole.Patient);
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);

        // Create multiple appointments
        _fixture.DbContext.Appointments.AddRange(
            new Appointment
            {
                PatientId = patient1.Patient!.Id,
                DoctorId = doctor.Doctor!.Id,
                AppointmentDate = DateTime.UtcNow.AddHours(2),
                Status = AppointmentStatus.Scheduled,
                Reason = "Checkup",
                CreatedBy = clerk.Id
            },
            new Appointment
            {
                PatientId = patient2.Patient!.Id,
                DoctorId = doctor.Doctor!.Id,
                AppointmentDate = DateTime.UtcNow.AddHours(3),
                Status = AppointmentStatus.Completed,
                Reason = "Follow-up",
                CreatedBy = clerk.Id
            }
        );
        await _fixture.DbContext.SaveChangesAsync();

        var token = _fixture.GenerateTestToken(clerk);
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync("/api/appointments");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<List<AppointmentDTO>>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(2, result.Data.Count);
    }

    [Fact]
    public async Task GetAppointments_WithStatusFilter_ReturnsFilteredResults()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);

        _fixture.DbContext.Appointments.AddRange(
            new Appointment
            {
                PatientId = patient.Patient!.Id,
                DoctorId = doctor.Doctor!.Id,
                AppointmentDate = DateTime.UtcNow.AddHours(2),
                Status = AppointmentStatus.Scheduled,
                Reason = "Upcoming",
                CreatedBy = patient.Id
            },
            new Appointment
            {
                PatientId = patient.Patient!.Id,
                DoctorId = doctor.Doctor!.Id,
                AppointmentDate = DateTime.UtcNow.AddHours(-2),
                Status = AppointmentStatus.Completed,
                Reason = "Past",
                CreatedBy = patient.Id
            }
        );
        await _fixture.DbContext.SaveChangesAsync();

        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync("/api/appointments?status=Scheduled");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<List<AppointmentDTO>>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Single(result.Data);
        Assert.Equal("Scheduled", result.Data[0].Status);
    }

    [Fact]
    public async Task GetAppointment_ById_ReturnsAppointment()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);

        var appointment = new Appointment
        {
            PatientId = patient.Patient!.Id,
            DoctorId = doctor.Doctor!.Id,
            AppointmentDate = DateTime.UtcNow.AddHours(2),
            Status = AppointmentStatus.Scheduled,
            Reason = "Checkup",
            CreatedBy = patient.Id
        };
        _fixture.DbContext.Appointments.Add(appointment);
        await _fixture.DbContext.SaveChangesAsync();

        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync($"/api/appointments/{appointment.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<AppointmentDTO>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(appointment.Id.ToString(), result.Data.Id);
        Assert.NotNull(result.Data.PatientName);
        Assert.NotNull(result.Data.DoctorName);
    }

    [Fact]
    public async Task GetAppointment_WithInvalidId_ReturnsBadRequest()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync("/api/appointments/invalid-guid");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateAppointment_AsPatient_ReturnsCreated()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);

        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);

        var request = new CreateAppointmentRequest
        {
            PatientId = patient.Patient!.Id.ToString(),
            DoctorId = doctor.Doctor!.Id.ToString(),
            AppointmentDate = DateTime.UtcNow.AddHours(24),
            Reason = "Annual checkup"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/appointments", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<AppointmentDTO>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("Annual checkup", result.Data.Reason);
        Assert.Equal("Scheduled", result.Data.Status);
    }

    [Fact]
    public async Task CreateAppointment_AsPatientForAnotherPatient_ReturnsForbidden()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient1 = await _fixture.CreateTestUserAsync("patient1@example.com", UserRole.Patient);
        var patient2 = await _fixture.CreateTestUserAsync("patient2@example.com", UserRole.Patient);
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);

        var token = _fixture.GenerateTestToken(patient1);
        SetAuthHeader(token);

        var request = new CreateAppointmentRequest
        {
            PatientId = patient2.Patient!.Id.ToString(),  // Different patient
            DoctorId = doctor.Doctor!.Id.ToString(),
            AppointmentDate = DateTime.UtcNow.AddHours(24),
            Reason = "Trying to book for someone else"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/appointments", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateAppointment_AsClerk_ReturnsCreated()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var clerk = await _fixture.CreateTestUserAsync("clerk@example.com", UserRole.Clerk);
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);

        var token = _fixture.GenerateTestToken(clerk);
        SetAuthHeader(token);

        var request = new CreateAppointmentRequest
        {
            PatientId = patient.Patient!.Id.ToString(),
            DoctorId = doctor.Doctor!.Id.ToString(),
            AppointmentDate = DateTime.UtcNow.AddHours(24),
            Reason = "Scheduled by clerk"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/appointments", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<AppointmentDTO>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
    }

    [Fact]
    public async Task CreateAppointment_AsDoctor_ReturnsForbidden()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);

        var token = _fixture.GenerateTestToken(doctor);
        SetAuthHeader(token);

        var request = new CreateAppointmentRequest
        {
            PatientId = patient.Patient!.Id.ToString(),
            DoctorId = doctor.Doctor!.Id.ToString(),
            AppointmentDate = DateTime.UtcNow.AddHours(24),
            Reason = "Doctor trying to create"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/appointments", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CompleteAppointment_AsAssignedDoctor_ReturnsCompleted()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);

        var appointment = new Appointment
        {
            PatientId = patient.Patient!.Id,
            DoctorId = doctor.Doctor!.Id,
            AppointmentDate = DateTime.UtcNow.AddHours(-1),
            Status = AppointmentStatus.Scheduled,
            Reason = "Checkup",
            CreatedBy = patient.Id
        };
        _fixture.DbContext.Appointments.Add(appointment);
        await _fixture.DbContext.SaveChangesAsync();

        var token = _fixture.GenerateTestToken(doctor);
        SetAuthHeader(token);

        var request = new CompleteAppointmentRequest
        {
            DoctorNotes = "Patient responded well to treatment"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/appointments/{appointment.Id}/complete", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<AppointmentDTO>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("Completed", result.Data.Status);
        Assert.Equal("Patient responded well to treatment", result.Data.DoctorNotes);
    }

    [Fact]
    public async Task CompleteAppointment_AsNonAssignedDoctor_ReturnsForbidden()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var doctor1 = await _fixture.CreateTestUserAsync("doctor1@example.com", UserRole.Doctor);
        var doctor2 = await _fixture.CreateTestUserAsync("doctor2@example.com", UserRole.Doctor);

        var appointment = new Appointment
        {
            PatientId = patient.Patient!.Id,
            DoctorId = doctor1.Doctor!.Id,  // Assigned to doctor1
            AppointmentDate = DateTime.UtcNow.AddHours(-1),
            Status = AppointmentStatus.Scheduled,
            Reason = "Checkup",
            CreatedBy = patient.Id
        };
        _fixture.DbContext.Appointments.Add(appointment);
        await _fixture.DbContext.SaveChangesAsync();

        var token = _fixture.GenerateTestToken(doctor2);  // doctor2 trying to complete
        SetAuthHeader(token);

        var request = new CompleteAppointmentRequest
        {
            DoctorNotes = "Trying to complete another doctor's appointment"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/appointments/{appointment.Id}/complete", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CancelAppointment_AsPatient_ReturnsSuccess()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);

        var appointment = new Appointment
        {
            PatientId = patient.Patient!.Id,
            DoctorId = doctor.Doctor!.Id,
            AppointmentDate = DateTime.UtcNow.AddHours(24),
            Status = AppointmentStatus.Scheduled,
            Reason = "Checkup",
            CreatedBy = patient.Id
        };
        _fixture.DbContext.Appointments.Add(appointment);
        await _fixture.DbContext.SaveChangesAsync();

        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);

        // Act
        var response = await _client.DeleteAsync($"/api/appointments/{appointment.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<object>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);

        // Verify status in database
        _fixture.DbContext.Entry(appointment).Reload();
        Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
    }

    [Fact]
    public async Task CancelAppointment_AsPatientForAnotherPatient_ReturnsForbidden()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient1 = await _fixture.CreateTestUserAsync("patient1@example.com", UserRole.Patient);
        var patient2 = await _fixture.CreateTestUserAsync("patient2@example.com", UserRole.Patient);
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);

        var appointment = new Appointment
        {
            PatientId = patient1.Patient!.Id,
            DoctorId = doctor.Doctor!.Id,
            AppointmentDate = DateTime.UtcNow.AddHours(24),
            Status = AppointmentStatus.Scheduled,
            Reason = "Checkup",
            CreatedBy = patient1.Id
        };
        _fixture.DbContext.Appointments.Add(appointment);
        await _fixture.DbContext.SaveChangesAsync();

        var token = _fixture.GenerateTestToken(patient2);  // patient2 trying to cancel patient1's appointment
        SetAuthHeader(token);

        // Act
        var response = await _client.DeleteAsync($"/api/appointments/{appointment.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CancelCompletedAppointment_ReturnsBadRequest()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);

        var appointment = new Appointment
        {
            PatientId = patient.Patient!.Id,
            DoctorId = doctor.Doctor!.Id,
            AppointmentDate = DateTime.UtcNow.AddHours(-24),
            Status = AppointmentStatus.Completed,  // Already completed
            Reason = "Checkup",
            CreatedBy = patient.Id
        };
        _fixture.DbContext.Appointments.Add(appointment);
        await _fixture.DbContext.SaveChangesAsync();

        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);

        // Act
        var response = await _client.DeleteAsync($"/api/appointments/{appointment.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<object>>(response);
        Assert.NotNull(result);
        Assert.False(result.Success);
    }

    [Fact]
    public async Task CancelAppointment_AsClerk_ReturnsSuccess()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var clerk = await _fixture.CreateTestUserAsync("clerk@example.com", UserRole.Clerk);
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);

        var appointment = new Appointment
        {
            PatientId = patient.Patient!.Id,
            DoctorId = doctor.Doctor!.Id,
            AppointmentDate = DateTime.UtcNow.AddHours(24),
            Status = AppointmentStatus.Scheduled,
            Reason = "Checkup",
            CreatedBy = clerk.Id
        };
        _fixture.DbContext.Appointments.Add(appointment);
        await _fixture.DbContext.SaveChangesAsync();

        var token = _fixture.GenerateTestToken(clerk);
        SetAuthHeader(token);

        // Act
        var response = await _client.DeleteAsync($"/api/appointments/{appointment.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<object>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
    }
}
