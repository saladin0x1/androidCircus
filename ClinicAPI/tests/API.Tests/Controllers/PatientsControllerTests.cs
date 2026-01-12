using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using API.DTOs;
using API.Models;
using API.Tests.Helpers;
using Xunit;

namespace API.Tests.Controllers;

public class PatientsControllerTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly HttpClient _client;

    public PatientsControllerTests(TestFixture fixture)
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
    public async Task GetPatient_AsDoctor_ReturnsPatientData()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);
        var token = _fixture.GenerateTestToken(doctor);
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync($"/api/patients/{patient.Patient?.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await _fixture.DeserializeResponse<ApiResponse<PatientDTO>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("patient@example.com", result.Data.Email);
    }

    [Fact]
    public async Task GetPatient_AsClerk_ReturnsPatientData()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var clerk = await _fixture.CreateTestUserAsync("clerk@example.com", UserRole.Clerk);
        var token = _fixture.GenerateTestToken(clerk);
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync($"/api/patients/{patient.Patient?.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await _fixture.DeserializeResponse<ApiResponse<PatientDTO>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("patient@example.com", result.Data.Email);
    }

    [Fact]
    public async Task GetPatient_AsSamePatient_ReturnsOwnData()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync($"/api/patients/{patient.Patient?.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await _fixture.DeserializeResponse<ApiResponse<PatientDTO>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("patient@example.com", result.Data.Email);
    }

    [Fact]
    public async Task GetPatient_AsDifferentPatient_ReturnsForbidden()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient1 = await _fixture.CreateTestUserAsync("patient1@example.com", UserRole.Patient);
        var patient2 = await _fixture.CreateTestUserAsync("patient2@example.com", UserRole.Patient);
        var token = _fixture.GenerateTestToken(patient2);
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync($"/api/patients/{patient1.Patient?.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetPatient_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        _client.DefaultRequestHeaders.Clear();

        // Act
        var response = await _client.GetAsync($"/api/patients/{patient.Patient?.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetPatient_WithInvalidId_ReturnsBadRequest()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);
        var token = _fixture.GenerateTestToken(doctor);
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync("/api/patients/invalid-guid");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await _fixture.DeserializeResponse<ApiResponse<PatientDTO>>(response);
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("INVALID_ID", result.Error?.Code);
    }

    [Fact]
    public async Task GetPatient_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);
        var token = _fixture.GenerateTestToken(doctor);
        SetAuthHeader(token);
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/patients/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var result = await _fixture.DeserializeResponse<ApiResponse<PatientDTO>>(response);
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("NOT_FOUND", result.Error?.Code);
    }

    [Fact]
    public async Task GetPatientNotes_AsDoctor_ReturnsNotes()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        patient.Patient!.DoctorNotes = "Patient has a history of allergies.";
        await _fixture.DbContext.SaveChangesAsync();

        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);
        var token = _fixture.GenerateTestToken(doctor);
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync($"/api/patients/{patient.Patient.Id}/notes");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await _fixture.DeserializeResponse<ApiResponse<string>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("Patient has a history of allergies.", result.Data);
    }

    [Fact]
    public async Task GetPatientNotes_AsPatient_ReturnsForbidden()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync($"/api/patients/{patient.Patient!.Id}/notes");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePatientNotes_AsDoctor_UpdatesNotes()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);
        var token = _fixture.GenerateTestToken(doctor);
        SetAuthHeader(token);

        var updateRequest = new UpdatePatientNotesRequest
        {
            Notes = "Updated: Patient responded well to treatment."
        };

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/patients/{patient.Patient!.Id}/notes",
            updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await _fixture.DeserializeResponse<ApiResponse<string>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("Updated: Patient responded well to treatment.", result.Data);

        // Verify in database
        _fixture.DbContext.Entry(patient.Patient).Reload();
        Assert.Equal("Updated: Patient responded well to treatment.", patient.Patient.DoctorNotes);
    }

    [Fact]
    public async Task UpdatePatientNotes_AsPatient_ReturnsForbidden()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);

        var updateRequest = new UpdatePatientNotesRequest
        {
            Notes = "Trying to update my own notes"
        };

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/patients/{patient.Patient!.Id}/notes",
            updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePatientNotes_AsClerk_ReturnsForbidden()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var clerk = await _fixture.CreateTestUserAsync("clerk@example.com", UserRole.Clerk);
        var token = _fixture.GenerateTestToken(clerk);
        SetAuthHeader(token);

        var updateRequest = new UpdatePatientNotesRequest
        {
            Notes = "Clerk trying to update notes"
        };

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/patients/{patient.Patient!.Id}/notes",
            updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
