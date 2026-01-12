using API.Models;

namespace API.DTOs;

public class AppointmentDTO
{
    public string? Id { get; set; }
    public string PatientId { get; set; } = string.Empty;
    public string DoctorId { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public string? DoctorNotes { get; set; }
    public string Status { get; set; } = "Scheduled";

    // Additional info for display
    public string? PatientName { get; set; }
    public string? DoctorName { get; set; }
    public string? DoctorSpecialization { get; set; }
}

public class CreateAppointmentRequest
{
    public string PatientId { get; set; } = string.Empty;
    public string DoctorId { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
}

public class CompleteAppointmentRequest
{
    public string? DoctorNotes { get; set; }
}
