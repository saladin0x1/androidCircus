using System.Net;
using System.Net.Http.Json;
using API.DTOs;
using API.Models;
using API.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace API.Tests.Controllers;

public class AuthControllerTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly HttpClient _client;

    public AuthControllerTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsSuccessWithToken()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        await _fixture.CreateTestUserAsync("test@example.com", UserRole.Patient, "Password123!");

        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await _fixture.DeserializeResponse<LoginResponse>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data.Token);
        Assert.Equal("test@example.com", result.Data.Email);
        Assert.Equal("Patient", result.Data.Role);
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ReturnsUnauthorized()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var result = await _fixture.DeserializeResponse<LoginResponse>(response);
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("Invalid email or password", result.Error);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        await _fixture.CreateTestUserAsync("test@example.com", UserRole.Patient, "Password123!");

        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var result = await _fixture.DeserializeResponse<LoginResponse>(response);
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("Invalid email or password", result.Error);
    }

    [Fact]
    public async Task Login_WithInactiveAccount_ReturnsUnauthorized()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var user = await _fixture.CreateTestUserAsync("test@example.com", UserRole.Patient);
        user.IsActive = false;
        await _fixture.DbContext.SaveChangesAsync();

        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var result = await _fixture.DeserializeResponse<LoginResponse>(response);
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("Account is inactive", result.Error);
    }

    [Fact]
    public async Task Register_WithValidPatientData_ReturnsSuccess()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var registerRequest = new RegisterRequest
        {
            Email = "newpatient@example.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            Phone = "555-1234",
            Role = UserRole.Patient,
            DateOfBirth = DateTime.UtcNow.AddYears(-25)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await _fixture.DeserializeResponse<LoginResponse>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data.Token);
        Assert.Equal("newpatient@example.com", result.Data.Email);
        Assert.Equal("Patient", result.Data.Role);
        Assert.NotEmpty(result.Data.RoleSpecificId);
    }

    [Fact]
    public async Task Register_WithValidDoctorData_ReturnsSuccess()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var registerRequest = new RegisterRequest
        {
            Email = "newdoctor@example.com",
            Password = "Password123!",
            FirstName = "Jane",
            LastName = "Smith",
            Phone = "555-5678",
            Role = UserRole.Doctor,
            Specialization = "Cardiology"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await _fixture.DeserializeResponse<LoginResponse>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Doctor", result.Data.Role);
        Assert.NotEmpty(result.Data.RoleSpecificId);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ReturnsBadRequest()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        await _fixture.CreateTestUserAsync("existing@example.com", UserRole.Patient);

        var registerRequest = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            Role = UserRole.Patient
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await _fixture.DeserializeResponse<LoginResponse>(response);
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("Email already registered", result.Error);
    }

    [Fact]
    public async Task Register_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var registerRequest = new RegisterRequest
        {
            Email = "invalid-email", // Invalid email format
            Password = "123", // Too short
            FirstName = "",
            LastName = "",
            Role = UserRole.Patient
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Health_ReturnsHealthyStatus()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<object>();
        Assert.NotNull(result);
    }
}
