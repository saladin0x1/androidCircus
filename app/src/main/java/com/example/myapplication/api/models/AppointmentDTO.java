package com.example.myapplication.api.models;

public class AppointmentDTO {
    private String id;
    private String patientId;
    private String doctorId;
    private String appointmentDate;
    private String reason;
    private String notes;
    private String doctorNotes;
    private String status;
    private String patientName;
    private String doctorName;
    private String doctorSpecialization;

    public String getId() { return id; }
    public String getPatientId() { return patientId; }
    public String getDoctorId() { return doctorId; }
    public String getAppointmentDate() { return appointmentDate; }
    public String getReason() { return reason; }
    public String getNotes() { return notes; }
    public String getDoctorNotes() { return doctorNotes; }
    public String getStatus() { return status; }
    public String getPatientName() { return patientName; }
    public String getDoctorName() { return doctorName; }
    public String getDoctorSpecialization() { return doctorSpecialization; }
}
