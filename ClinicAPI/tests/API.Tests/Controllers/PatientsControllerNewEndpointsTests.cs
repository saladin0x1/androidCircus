using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using API.DTOs;
using API.Models;
using API.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace API.Tests.Controllers;

public class PatientsControllerNewEndpointsTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly HttpClient _client;

    public PatientsControllerNewEndpointsTests(TestFixture fixture)
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
    public async Task GetPatients_AsDoctor_ReturnsPatientList()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient1 = await _fixture.CreateTestUserAsync("john.doe@example.com", UserRole.Patient);
        var patient2 = await _fixture.CreateTestUserAsync("jane.smith@example.com", UserRole.Patient);
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);

        var token = _fixture.GenerateTestToken(doctor);
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync("/api/patients");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<List<PatientDTO>>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(2, result.Data.Count);
    }

    [Fact]
    public async Task GetPatients_WithSearch_ReturnsFilteredResults()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient1 = await _fixture.CreateTestUserAsync("john.doe@example.com", UserRole.Patient);
        var patient2 = await _fixture.CreateTestUserAsync("jane.smith@example.com", UserRole.Patient);

        // Update names via DbContext for search test
        var user1 = await _fixture.DbContext.Users.FindAsync(patient1.Id);
        user1.FirstName = "John";
        user1.LastName = "Doe";

        var user2 = await _fixture.DbContext.Users.FindAsync(patient2.Id);
        user2.FirstName = "Jane";
        user2.LastName = "Smith";
        await _fixture.DbContext.SaveChangesAsync();

        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);
        var token = _fixture.GenerateTestToken(doctor);
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync("/api/patients?search=John");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<List<PatientDTO>>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Single(result.Data);
        Assert.Equal("John", result.Data[0].FirstName);
    }

    [Fact]
    public async Task GetPatients_AsPatient_ReturnsForbidden()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync("/api/patients");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePatient_AsPatient_OwnProfile_ReturnsSuccess()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);

        var updateRequest = new UpdatePatientRequest
        {
            FirstName = "Updated",
            LastName = "Name",
            Phone = "555-9999"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/patients/{patient.Patient!.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<PatientDTO>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("Updated", result.Data.FirstName);
        Assert.Equal("Name", result.Data.LastName);
        Assert.Equal("555-9999", result.Data.Phone);
    }

    [Fact]
    public async Task UpdatePatient_AsPatient_OtherProfile_ReturnsForbidden()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient1 = await _fixture.CreateTestUserAsync("patient1@example.com", UserRole.Patient);
        var patient2 = await _fixture.CreateTestUserAsync("patient2@example.com", UserRole.Patient);
        var token = _fixture.GenerateTestToken(patient1);
        SetAuthHeader(token);

        var updateRequest = new UpdatePatientRequest
        {
            FirstName = "Hacked"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/patients/{patient2.Patient!.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePatient_AsDoctor_ReturnsSuccess()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);
        var token = _fixture.GenerateTestToken(doctor);
        SetAuthHeader(token);

        var updateRequest = new UpdatePatientRequest
        {
            Phone = "555-1234"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/patients/{patient.Patient!.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<PatientDTO>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("555-1234", result.Data.Phone);
    }

    [Fact]
    public async Task UpdatePatientNotes_AsClerk_ReturnsSuccess()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var clerk = await _fixture.CreateTestUserAsync("clerk@example.com", UserRole.Clerk);
        var token = _fixture.GenerateTestToken(clerk);
        SetAuthHeader(token);

        var updateRequest = new UpdatePatientNotesRequest
        {
            Notes = "Clerk notes"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/patients/{patient.Patient!.Id}/notes", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<string>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("Clerk notes", result.Data);
    }

    [Fact]
    public async Task CreatePatient_AsClerk_ReturnsSuccess()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var clerk = await _fixture.CreateTestUserAsync("clerk@example.com", UserRole.Clerk);
        var token = _fixture.GenerateTestToken(clerk);
        SetAuthHeader(token);

        var request = new CreatePatientRequest
        {
            Email = "newpatient@example.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            Phone = "555-9999",
            DateOfBirth = DateTime.UtcNow.AddYears(-30),
            Address = "123 Main St",
            EmergencyContactName = "Jane Doe",
            EmergencyContactPhone = "555-8888"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/patients", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<PatientDTO>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("newpatient@example.com", result.Data.Email);
        Assert.Equal("John", result.Data.FirstName);
        Assert.Equal("Doe", result.Data.LastName);
        Assert.Equal("555-9999", result.Data.Phone);
        Assert.Equal("123 Main St", result.Data.Address);
    }

    [Fact]
    public async Task CreatePatient_WithExistingEmail_ReturnsBadRequest()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        await _fixture.CreateTestUserAsync("existing@example.com", UserRole.Patient);
        var clerk = await _fixture.CreateTestUserAsync("clerk@example.com", UserRole.Clerk);
        var token = _fixture.GenerateTestToken(clerk);
        SetAuthHeader(token);

        var request = new CreatePatientRequest
        {
            Email = "existing@example.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "Patient"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/patients", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<PatientDTO>>(response);
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("EMAIL_EXISTS", result.Error?.Code);
    }

    [Fact]
    public async Task CreatePatient_AsPatient_ReturnsForbidden()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);

        var request = new CreatePatientRequest
        {
            Email = "newpatient@example.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/patients", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeletePatient_AsClerk_DeactivatesPatient()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var clerk = await _fixture.CreateTestUserAsync("clerk@example.com", UserRole.Clerk);
        var token = _fixture.GenerateTestToken(clerk);
        SetAuthHeader(token);

        // Act
        var response = await _client.DeleteAsync($"/api/patients/{patient.Patient!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<object>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);

        // Verify patient is deactivated
        _fixture.DbContext.Entry(patient).Reload();
        Assert.False(patient.IsActive);
    }

    [Fact]
    public async Task DeletePatient_AsPatient_ReturnsForbidden()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);

        // Act
        var response = await _client.DeleteAsync($"/api/patients/{patient.Patient!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeletePatient_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var clerk = await _fixture.CreateTestUserAsync("clerk@example.com", UserRole.Clerk);
        var token = _fixture.GenerateTestToken(clerk);
        SetAuthHeader(token);
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/patients/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<object>>(response);
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("NOT_FOUND", result.Error?.Code);
    }
}
