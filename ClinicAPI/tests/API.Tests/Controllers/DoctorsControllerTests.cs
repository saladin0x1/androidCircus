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
        await _fixture.CreateTestUserAsync("doctor1@example.com", UserRole.Doctor);
        await _fixture.CreateTestUserAsync("doctor2@example.com", UserRole.Doctor);
        await _fixture.CreateTestUserAsync("patient@example.com", UserRole.Patient);

        var patient = await _fixture.CreateTestUserAsync("user@example.com", UserRole.Patient);
        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync("/api/doctors");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        Assert.True(root.GetProperty("success").GetBoolean());
        var data = root.GetProperty("data");
        Assert.Equal(3, data.GetArrayLength());

        var firstDoctor = data[0];
        Assert.True(firstDoctor.TryGetProperty("id", out _));
        Assert.True(firstDoctor.TryGetProperty("name", out _));
        Assert.True(firstDoctor.TryGetProperty("specialization", out _));
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
        var content = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        Assert.True(root.GetProperty("success").GetBoolean());
        var data = root.GetProperty("data");
        Assert.Equal(0, data.GetArrayLength());
    }

    [Fact]
    public async Task GetDoctors_WithMultipleSpecializations_ReturnsAllDoctors()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var user1 = await _fixture.CreateTestUserAsync("cardio@example.com", UserRole.Doctor);
        user1.Doctor!.Specialization = "Cardiology";
        await _fixture.DbContext.SaveChangesAsync();

        var user2 = await _fixture.CreateTestUserAsync("neuro@example.com", UserRole.Doctor);
        user2.Doctor!.Specialization = "Neurology";
        await _fixture.DbContext.SaveChangesAsync();

        var user3 = await _fixture.CreateTestUserAsync("gp@example.com", UserRole.Doctor);
        user3.Doctor!.Specialization = "General Practice";
        await _fixture.DbContext.SaveChangesAsync();

        var patient = await _fixture.CreateTestUserAsync("user@example.com", UserRole.Patient);
        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync("/api/doctors");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        Assert.True(root.GetProperty("success").GetBoolean());
        var data = root.GetProperty("data");
        Assert.Equal(3, data.GetArrayLength());

        // Verify specializations are present
        var specializations = new HashSet<string>();
        foreach (var doctor in data.EnumerateArray())
        {
            specializations.Add(doctor.GetProperty("specialization").GetString() ?? "");
        }
        Assert.Contains("Cardiology", specializations);
        Assert.Contains("Neurology", specializations);
        Assert.Contains("General Practice", specializations);
    }

    [Fact]
    public async Task GetDoctors_ReturnsDoctorNamesCorrectly()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var user = await _fixture.CreateTestUserAsync("john.doe@example.com", UserRole.Doctor);
        user.FirstName = "John";
        user.LastName = "Doe";
        await _fixture.DbContext.SaveChangesAsync();

        var patient = await _fixture.CreateTestUserAsync("user@example.com", UserRole.Patient);
        var token = _fixture.GenerateTestToken(patient);
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync("/api/doctors");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;
        var data = root.GetProperty("data");

        var doctor = data[0];
        Assert.Equal("John Doe", doctor.GetProperty("name").GetString());
    }
}
