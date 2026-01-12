package com.example.myapplication.api.models;

public class CreateAppointmentRequest {
    private String patientId;
    private String doctorId;
    private String appointmentDate;
    private String reason;
    private String notes;

    public CreateAppointmentRequest(String patientId, String doctorId, String appointmentDate, String reason, String notes) {
        this.patientId = patientId;
        this.doctorId = doctorId;
        this.appointmentDate = appointmentDate;
        this.reason = reason;
        this.notes = notes;
    }
}
