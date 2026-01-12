namespace API.DTOs;

public class LoginResponse
{
    public bool Success { get; set; }
    public UserData? Data { get; set; }
    public string? Error { get; set; }
    public string? Message { get; set; }
}

public class UserData
{
    public string UserId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string RoleSpecificId { get; set; } = string.Empty; // PatientId, DoctorId, or ClerkId
}
