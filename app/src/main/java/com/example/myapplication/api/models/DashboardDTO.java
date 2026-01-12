package com.example.myapplication.api.models;

public class DashboardDTO {
    private int todayAppointments;
    private int pendingAppointments;
    private int totalPatients;
    private int totalDoctors;

    public int getTodayAppointments() { return todayAppointments; }
    public int getPendingAppointments() { return pendingAppointments; }
    public int getTotalPatients() { return totalPatients; }
    public int getTotalDoctors() { return totalDoctors; }
}
