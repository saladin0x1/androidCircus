using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models;

public class Appointment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [ForeignKey(nameof(Patient))]
    public Guid PatientId { get; set; }

    [Required]
    [ForeignKey(nameof(Doctor))]
    public Guid DoctorId { get; set; }

    [Required]
    public DateTime AppointmentDate { get; set; }

    public int DurationMinutes { get; set; } = 30;

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

    public string? Reason { get; set; }

    public string? Notes { get; set; }

    public string? DoctorNotes { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    // Navigation properties
    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
}
