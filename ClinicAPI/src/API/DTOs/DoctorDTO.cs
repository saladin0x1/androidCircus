using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class DoctorDTO
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string? LicenseNumber { get; set; }
    public int? YearsOfExperience { get; set; }
    public DateTime JoinedDate { get; set; }
    public bool IsActive { get; set; }
}

public class CreateDoctorRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    public string? Phone { get; set; }

    [Required]
    public string Specialization { get; set; } = string.Empty;

    public string? LicenseNumber { get; set; }

    public int? YearsOfExperience { get; set; }
}

public class UpdateDoctorRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Specialization { get; set; }
    public string? LicenseNumber { get; set; }
    public int? YearsOfExperience { get; set; }
    public bool? IsActive { get; set; }
}
