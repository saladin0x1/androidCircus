using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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

    private static async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
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
        var result = await DeserializeResponse<LoginResponse>(response);
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
        var result = await DeserializeResponse<LoginResponse>(response);
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
        var result = await DeserializeResponse<LoginResponse>(response);
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
        var result = await DeserializeResponse<LoginResponse>(response);
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
        var result = await DeserializeResponse<LoginResponse>(response);
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
        var result = await DeserializeResponse<LoginResponse>(response);
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
        var result = await DeserializeResponse<LoginResponse>(response);
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

    [Fact]
    public async Task ForgotPassword_WithExistingEmail_ReturnsSuccess()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        await _fixture.CreateTestUserAsync("test@example.com", UserRole.Patient);

        var request = new ForgotPasswordRequest
        {
            Email = "test@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<object>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
    }

    [Fact]
    public async Task ForgotPassword_WithNonExistentEmail_ReturnsSuccess_PreventsEnumeration()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var request = new ForgotPasswordRequest
        {
            Email = "nonexistent@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", request);

        // Assert - Should still return success to prevent email enumeration
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<object>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
    }

    [Fact]
    public async Task ResetPassword_WithValidData_ReturnsSuccess()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        await _fixture.CreateTestUserAsync("test@example.com", UserRole.Patient, "OldPassword123!");

        var request = new ResetPasswordRequest
        {
            Email = "test@example.com",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/reset-password", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<object>>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);

        // Verify password was changed by trying to login with new password
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "NewPassword123!"
        };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WithMismatchedPasswords_ReturnsBadRequest()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        await _fixture.CreateTestUserAsync("test@example.com", UserRole.Patient);

        var request = new ResetPasswordRequest
        {
            Email = "test@example.com",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "DifferentPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/reset-password", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<object>>(response);
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("PASSWORD_MISMATCH", result.Error?.Code);
    }

    [Fact]
    public async Task ResetPassword_WithShortPassword_ReturnsBadRequest()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        await _fixture.CreateTestUserAsync("test@example.com", UserRole.Patient);

        var request = new ResetPasswordRequest
        {
            Email = "test@example.com",
            NewPassword = "12345",
            ConfirmPassword = "12345"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/reset-password", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<object>>(response);
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("PASSWORD_TOO_SHORT", result.Error?.Code);
    }

    [Fact]
    public async Task ResetPassword_WithNonExistentEmail_ReturnsNotFound()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var request = new ResetPasswordRequest
        {
            Email = "nonexistent@example.com",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/reset-password", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var result = await DeserializeResponse<ApiResponse<object>>(response);
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("USER_NOT_FOUND", result.Error?.Code);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsNewToken()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var user = await _fixture.CreateTestUserAsync("test@example.com", UserRole.Patient);

        // First login to get a token
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await DeserializeResponse<LoginResponse>(loginResponse);
        var originalToken = loginResult.Data.Token;

        // Set auth header for refresh
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {originalToken}");

        var refreshRequest = new RefreshTokenRequest();

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<LoginResponse>(response);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data.Token);
        // New token should be different from original
        Assert.NotEqual(originalToken, result.Data.Token);
    }

    [Fact]
    public async Task RefreshToken_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        _client.DefaultRequestHeaders.Clear();

        var refreshRequest = new RefreshTokenRequest();

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
