using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using API.DTOs;
using API.Models;
using API.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace API.Tests.Controllers;

public class DoctorsControllerTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly HttpClient _client;

    public DoctorsControllerTests(TestFixture fixture)
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
    public async Task GetDoctors_WithDoctorsInDb_ReturnsListOfDoctors()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        await _fixture.CreateTestUserAsync("user@example.com", UserRole.Patient);

        // Create 3 doctors
        await _fixture.CreateTestUserAsync("doctor1@example.com", UserRole.Doctor);
        await _fixture.CreateTestUserAsync("doctor2@example.com", UserRole.Doctor);
        await _fixture.CreateTestUserAsync("doctor3@example.com", UserRole.Doctor);

        var patient = await _fixture.DbContext.Users.FirstAsync(u => u.Email == "user@example.com");
        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync("/api/doctors");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<List<DoctorDTO>>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(3, result.Data.Count);
    }

    [Fact]
    public async Task GetDoctors_WhenNoDoctorsExist_ReturnsEmptyList()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        await _fixture.CreateTestUserAsync("user@example.com", UserRole.Patient);
        var token = _fixture.GenerateTestToken(await _fixture.DbContext.Users.FirstAsync());
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync("/api/doctors");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<List<DoctorDTO>>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Empty(result.Data);
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
    public async Task GetDoctor_WithValidId_ReturnsDoctor()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);
        var patient = await _fixture.CreateTestUserAsync("user@example.com", UserRole.Patient);
        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync($"/api/doctors/{doctor.Doctor!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<DoctorDTO>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("doctor@example.com", result.Data.Email);
    }

    [Fact]
    public async Task GetDoctor_WithInvalidId_ReturnsBadRequest()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("user@example.com", UserRole.Patient);
        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync("/api/doctors/invalid-guid");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<DoctorDTO>>(response);
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("INVALID_ID", result.Error?.Code);
    }

    [Fact]
    public async Task GetDoctor_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("user@example.com", UserRole.Patient);
        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/doctors/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<DoctorDTO>>(response);
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("NOT_FOUND", result.Error?.Code);
    }

    [Fact]
    public async Task CreateDoctor_AsClerk_ReturnsSuccess()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var clerk = await _fixture.CreateTestUserAsync("clerk@example.com", UserRole.Clerk);
        var token = _fixture.GenerateTestToken(clerk);
        SetAuthHeader(token);

        var request = new CreateDoctorRequest
        {
            Email = "newdoctor@example.com",
            Password = "Password123!",
            FirstName = "Jane",
            LastName = "Smith",
            Phone = "555-1234",
            Specialization = "Cardiology",
            LicenseNumber = "LIC12345",
            YearsOfExperience = 10
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/doctors", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<DoctorDTO>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("newdoctor@example.com", result.Data.Email);
        Assert.Equal("Jane", result.Data.FirstName);
        Assert.Equal("Cardiology", result.Data.Specialization);
        Assert.Equal("LIC12345", result.Data.LicenseNumber);
        Assert.Equal(10, result.Data.YearsOfExperience);
    }

    [Fact]
    public async Task CreateDoctor_WithExistingEmail_ReturnsBadRequest()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        await _fixture.CreateTestUserAsync("existing@example.com", UserRole.Patient);
        var clerk = await _fixture.CreateTestUserAsync("clerk@example.com", UserRole.Clerk);
        var token = _fixture.GenerateTestToken(clerk);
        SetAuthHeader(token);

        var request = new CreateDoctorRequest
        {
            Email = "existing@example.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "Doctor",
            Specialization = "General"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/doctors", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<DoctorDTO>>(response);
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("EMAIL_EXISTS", result.Error?.Code);
    }

    [Fact]
    public async Task CreateDoctor_AsPatient_ReturnsForbidden()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);

        var request = new CreateDoctorRequest
        {
            Email = "newdoctor@example.com",
            Password = "Password123!",
            FirstName = "Jane",
            LastName = "Smith",
            Specialization = "Cardiology"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/doctors", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateDoctor_AsClerk_ReturnsSuccess()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);
        var clerk = await _fixture.CreateTestUserAsync("clerk@example.com", UserRole.Clerk);
        var token = _fixture.GenerateTestToken(clerk);
        SetAuthHeader(token);

        var request = new UpdateDoctorRequest
        {
            FirstName = "Updated",
            LastName = "Name",
            Specialization = "Updated Specialization",
            YearsOfExperience = 15
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/doctors/{doctor.Doctor!.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<DoctorDTO>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("Updated", result.Data.FirstName);
        Assert.Equal("Name", result.Data.LastName);
        Assert.Equal("Updated Specialization", result.Data.Specialization);
        Assert.Equal(15, result.Data.YearsOfExperience);
    }

    [Fact]
    public async Task UpdateDoctor_AsPatient_ReturnsForbidden()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);

        var request = new UpdateDoctorRequest
        {
            FirstName = "Hacked"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/doctors/{doctor.Doctor!.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteDoctor_AsClerk_DeactivatesDoctor()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);
        var clerk = await _fixture.CreateTestUserAsync("clerk@example.com", UserRole.Clerk);
        var token = _fixture.GenerateTestToken(clerk);
        SetAuthHeader(token);

        // Act
        var response = await _client.DeleteAsync($"/api/doctors/{doctor.Doctor!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<object>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);

        // Verify doctor is deactivated
        _fixture.DbContext.Entry(doctor).Reload();
        Assert.False(doctor.IsActive);
    }

    [Fact]
    public async Task DeleteDoctor_AsPatient_ReturnsForbidden()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var doctor = await _fixture.CreateTestUserAsync("doctor@example.com", UserRole.Doctor);
        var patient = await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);
        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);

        // Act
        var response = await _client.DeleteAsync($"/api/doctors/{doctor.Doctor!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteDoctor_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var clerk = await _fixture.CreateTestUserAsync("clerk@example.com", UserRole.Clerk);
        var token = _fixture.GenerateTestToken(clerk);
        SetAuthHeader(token);
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/doctors/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<object>>(response);
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("NOT_FOUND", result.Error?.Code);
    }
}
