package com.example.myapplication.api.models;

import com.google.gson.annotations.SerializedName;
import java.util.List;

public class MedicalHistoryDTO {
    @SerializedName("patientId")
    private String patientId;

    @SerializedName("patientName")
    private String patientName;

    @SerializedName("records")
    private List<MedicalRecord> records;

    @SerializedName("totalRecords")
    private int totalRecords;

    public String getPatientId() {
        return patientId;
    }

    public void setPatientId(String patientId) {
        this.patientId = patientId;
    }

    public String getPatientName() {
        return patientName;
    }

    public void setPatientName(String patientName) {
        this.patientName = patientName;
    }

    public List<MedicalRecord> getRecords() {
        return records;
    }

    public void setRecords(List<MedicalRecord> records) {
        this.records = records;
    }

    public int getTotalRecords() {
        return totalRecords;
    }

    public void setTotalRecords(int totalRecords) {
        this.totalRecords = totalRecords;
    }

    public static class MedicalRecord {
        @SerializedName("id")
        private String id;

        @SerializedName("appointmentDate")
        private String appointmentDate;

        @SerializedName("reason")
        private String reason;

        @SerializedName("doctorNotes")
        private String doctorNotes;

        @SerializedName("diagnosis")
        private String diagnosis;

        @SerializedName("prescription")
        private String prescription;

        @SerializedName("doctorName")
        private String doctorName;

        @SerializedName("doctorSpecialization")
        private String doctorSpecialization;

        @SerializedName("status")
        private String status;

        public String getId() {
            return id;
        }

        public void setId(String id) {
            this.id = id;
        }

        public String getAppointmentDate() {
            return appointmentDate;
        }

        public void setAppointmentDate(String appointmentDate) {
            this.appointmentDate = appointmentDate;
        }

        public String getReason() {
            return reason;
        }

        public void setReason(String reason) {
            this.reason = reason;
        }

        public String getDoctorNotes() {
            return doctorNotes;
        }

        public void setDoctorNotes(String doctorNotes) {
            this.doctorNotes = doctorNotes;
        }

        public String getDiagnosis() {
            return diagnosis;
        }

        public void setDiagnosis(String diagnosis) {
            this.diagnosis = diagnosis;
        }

        public String getPrescription() {
            return prescription;
        }

        public void setPrescription(String prescription) {
            this.prescription = prescription;
        }

        public String getDoctorName() {
            return doctorName;
        }

        public void setDoctorName(String doctorName) {
            this.doctorName = doctorName;
        }

        public String getDoctorSpecialization() {
            return doctorSpecialization;
        }

        public void setDoctorSpecialization(String doctorSpecialization) {
            this.doctorSpecialization = doctorSpecialization;
        }

        public String getStatus() {
            return status;
        }

        public void setStatus(String status) {
            this.status = status;
        }
    }
}
