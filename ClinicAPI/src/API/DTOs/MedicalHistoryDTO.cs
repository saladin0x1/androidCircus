namespace API.DTOs;

public class MedicalRecordDTO
{
    public string Id { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string? Reason { get; set; }
    public string? DoctorNotes { get; set; }
    public string? Diagnosis { get; set; }
    public string? Prescription { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string DoctorSpecialization { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class MedicalHistoryDTO
{
    public string PatientId { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public List<MedicalRecordDTO> Records { get; set; } = new();
    public int TotalRecords { get; set; }
}
