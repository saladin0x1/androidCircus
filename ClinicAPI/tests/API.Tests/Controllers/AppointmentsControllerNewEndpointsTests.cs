using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using API.DTOs;
using API.Models;
using API.Tests.Helpers;
using Xunit;

namespace API.Tests.Controllers;

public class AppointmentsControllerNewEndpointsTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly HttpClient _client;

    public AppointmentsControllerNewEndpointsTests(TestFixture fixture)
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
    public async Task GetAppointments_WithDateFilter_ReturnsFilteredResults()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);

        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        _fixture.DbContext.Appointments.AddRange(
            new Appointment
            {
                PatientId = patient.Patient!.Id,
                DoctorId = doctor.Doctor!.Id,
                AppointmentDate = today.AddHours(10),
                Status = AppointmentStatus.Scheduled,
                Reason = "Today appointment",
                CreatedBy = patient.Id
            },
            new Appointment
            {
                PatientId = patient.Patient!.Id,
                DoctorId = doctor.Doctor!.Id,
                AppointmentDate = tomorrow.AddHours(10),
                Status = AppointmentStatus.Scheduled,
                Reason = "Tomorrow appointment",
                CreatedBy = patient.Id
            }
        );
        await _fixture.DbContext.SaveChangesAsync();

        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync($"/api/appointments?date={today:yyyy-MM-dd}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<List<AppointmentDTO>>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Single(result.Data);
        Assert.Equal("Today appointment", result.Data[0].Reason);
    }

    [Fact]
    public async Task GetAppointments_AsClerkWithDoctorFilter_ReturnsFilteredResults()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var clerk = await _fixture.CreateTestUserAsync("clerk@example.com", UserRole.Clerk);
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var doctor1 = await _fixture.CreateTestUserAsync("doctor1@example.com", UserRole.Doctor);
        var doctor2 = await _fixture.CreateTestUserAsync("doctor2@example.com", UserRole.Doctor);

        _fixture.DbContext.Appointments.AddRange(
            new Appointment
            {
                PatientId = patient.Patient!.Id,
                DoctorId = doctor1.Doctor!.Id,
                AppointmentDate = DateTime.UtcNow.AddHours(2),
                Status = AppointmentStatus.Scheduled,
                Reason = "With doctor1",
                CreatedBy = clerk.Id
            },
            new Appointment
            {
                PatientId = patient.Patient!.Id,
                DoctorId = doctor2.Doctor!.Id,
                AppointmentDate = DateTime.UtcNow.AddHours(3),
                Status = AppointmentStatus.Scheduled,
                Reason = "With doctor2",
                CreatedBy = clerk.Id
            }
        );
        await _fixture.DbContext.SaveChangesAsync();

        var token = _fixture.GenerateTestToken(clerk);
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync($"/api/appointments?doctorId={doctor1.Doctor!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<List<AppointmentDTO>>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Single(result.Data);
        Assert.Equal("With doctor1", result.Data[0].Reason);
    }

    [Fact]
    public async Task GetAvailableSlots_ReturnsTimeSlots()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);

        var targetDate = DateTime.UtcNow.Date.AddDays(1);

        // Book one slot
        _fixture.DbContext.Appointments.Add(new Appointment
        {
            PatientId = patient.Patient!.Id,
            DoctorId = doctor.Doctor!.Id,
            AppointmentDate = targetDate.AddHours(9),
            Status = AppointmentStatus.Scheduled,
            Reason = "Booked",
            CreatedBy = patient.Id
        });
        await _fixture.DbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/appointments/available-slots?doctorId={doctor.Doctor!.Id}&date={targetDate:yyyy-MM-dd}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<List<TimeSlotDTO>>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);

        // Should have 20 slots (8 AM to 6 PM, 30-min intervals)
        Assert.Equal(20, result.Data.Count);

        // The 9:00 slot should be booked
        var nineAmSlot = result.Data.FirstOrDefault(s => s.Time == "09:00");
        Assert.NotNull(nineAmSlot);
        Assert.False(nineAmSlot.Available);

        // Other slots should be available
        var tenAmSlot = result.Data.FirstOrDefault(s => s.Time == "10:00");
        Assert.NotNull(tenAmSlot);
        Assert.True(tenAmSlot.Available);
    }

    [Fact]
    public async Task CreateAppointment_WithConflict_ReturnsBadRequest()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);

        var appointmentTime = DateTime.UtcNow.AddHours(24);

        // Create first appointment
        _fixture.DbContext.Appointments.Add(new Appointment
        {
            PatientId = patient.Patient!.Id,
            DoctorId = doctor.Doctor!.Id,
            AppointmentDate = appointmentTime,
            Status = AppointmentStatus.Scheduled,
            Reason = "First appointment",
            CreatedBy = patient.Id
        });
        await _fixture.DbContext.SaveChangesAsync();

        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);

        // Try to create overlapping appointment
        var request = new CreateAppointmentRequest
        {
            PatientId = patient.Patient!.Id.ToString(),
            DoctorId = doctor.Doctor!.Id.ToString(),
            AppointmentDate = appointmentTime,  // Same time - conflict!
            Reason = "Conflicting appointment"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/appointments", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<AppointmentDTO>>(response);
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("SLOT_UNAVAILABLE", result.Error?.Code);
    }

    [Fact]
    public async Task RescheduleAppointment_AsPatient_ReturnsSuccess()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);

        var originalTime = DateTime.UtcNow.AddHours(24);
        var newTime = DateTime.UtcNow.AddHours(48);

        var appointment = new Appointment
        {
            PatientId = patient.Patient!.Id,
            DoctorId = doctor.Doctor!.Id,
            AppointmentDate = originalTime,
            Status = AppointmentStatus.Scheduled,
            Reason = "Checkup",
            CreatedBy = patient.Id
        };
        _fixture.DbContext.Appointments.Add(appointment);
        await _fixture.DbContext.SaveChangesAsync();

        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);

        var request = new RescheduleAppointmentRequest
        {
            AppointmentDate = newTime
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/appointments/{appointment.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<AppointmentDTO>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);

        // Verify time changed
        Assert.Equal(newTime, result.Data.AppointmentDate);
    }

    [Fact]
    public async Task RescheduleAppointment_WithConflict_ReturnsBadRequest()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);

        var time1 = DateTime.UtcNow.AddHours(24);
        var time2 = DateTime.UtcNow.AddHours(26);

        var appointment1 = new Appointment
        {
            PatientId = patient.Patient!.Id,
            DoctorId = doctor.Doctor!.Id,
            AppointmentDate = time1,
            Status = AppointmentStatus.Scheduled,
            Reason = "First",
            CreatedBy = patient.Id
        };

        var appointment2 = new Appointment
        {
            PatientId = patient.Patient!.Id,
            DoctorId = doctor.Doctor!.Id,
            AppointmentDate = time2,
            Status = AppointmentStatus.Scheduled,
            Reason = "Second",
            CreatedBy = patient.Id
        };

        _fixture.DbContext.Appointments.AddRange(appointment1, appointment2);
        await _fixture.DbContext.SaveChangesAsync();

        // Reload appointment1 to get its ID
        await _fixture.DbContext.Entry(appointment1).ReloadAsync();

        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);

        // Try to reschedule first appointment to conflict with second
        var request = new RescheduleAppointmentRequest
        {
            AppointmentDate = time2  // This conflicts with the second appointment
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/appointments/{appointment1.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<AppointmentDTO>>(response);
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("SLOT_UNAVAILABLE", result.Error?.Code);
    }

    [Fact]
    public async Task RescheduleCompletedAppointment_ReturnsBadRequest()
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
            Status = AppointmentStatus.Completed,
            Reason = "Completed appointment",
            CreatedBy = patient.Id
        };
        _fixture.DbContext.Appointments.Add(appointment);
        await _fixture.DbContext.SaveChangesAsync();

        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);

        var request = new RescheduleAppointmentRequest
        {
            AppointmentDate = DateTime.UtcNow.AddHours(24)
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/appointments/{appointment.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<AppointmentDTO>>(response);
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("ALREADY_COMPLETED", result.Error?.Code);
    }

    [Fact]
    public async Task UpdateAppointmentStatus_AsDoctor_ReturnsSuccess()
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
            Reason = "No-show",
            CreatedBy = patient.Id
        };
        _fixture.DbContext.Appointments.Add(appointment);
        await _fixture.DbContext.SaveChangesAsync();

        var token = _fixture.GenerateTestToken(doctor);
        SetAuthHeader(token);

        var request = new UpdateAppointmentStatusRequest
        {
            Status = "NoShow"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/appointments/{appointment.Id}/status", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<AppointmentDTO>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("NoShow", result.Data.Status);
    }

    [Fact]
    public async Task UpdateAppointmentStatus_AsClerk_ReturnsSuccess()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);
        var clerk = await _fixture.CreateTestUserAsync("clerk@example.com", UserRole.Clerk);

        var appointment = new Appointment
        {
            PatientId = patient.Patient!.Id,
            DoctorId = doctor.Doctor!.Id,
            AppointmentDate = DateTime.UtcNow.AddHours(1),
            Status = AppointmentStatus.Scheduled,
            Reason = "To be cancelled",
            CreatedBy = patient.Id
        };
        _fixture.DbContext.Appointments.Add(appointment);
        await _fixture.DbContext.SaveChangesAsync();

        var token = _fixture.GenerateTestToken(clerk);
        SetAuthHeader(token);

        var request = new UpdateAppointmentStatusRequest
        {
            Status = "Cancelled"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/appointments/{appointment.Id}/status", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<AppointmentDTO>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("Cancelled", result.Data.Status);
    }

    [Fact]
    public async Task GetDoctors_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        _client.DefaultRequestHeaders.Clear();

        // Act
        var response = await _client.GetAsync("/api/doctors");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDoctors_WithAuth_ReturnsSuccess()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync("/api/doctors");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
