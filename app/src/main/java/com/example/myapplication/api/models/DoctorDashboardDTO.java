package com.example.myapplication.api.models;

import com.google.gson.annotations.SerializedName;

public class DoctorDashboardDTO {
    @SerializedName("todayAppointments")
    private int todayAppointments;

    @SerializedName("pendingAppointments")
    private int pendingAppointments;

    @SerializedName("totalPatients")
    private int totalPatients;

    @SerializedName("completedAppointments")
    private int completedAppointments;

    public int getTodayAppointments() {
        return todayAppointments;
    }

    public void setTodayAppointments(int todayAppointments) {
        this.todayAppointments = todayAppointments;
    }

    public int getPendingAppointments() {
        return pendingAppointments;
    }

    public void setPendingAppointments(int pendingAppointments) {
        this.pendingAppointments = pendingAppointments;
    }

    public int getTotalPatients() {
        return totalPatients;
    }

    public void setTotalPatients(int totalPatients) {
        this.totalPatients = totalPatients;
    }

    public int getCompletedAppointments() {
        return completedAppointments;
    }

    public void setCompletedAppointments(int completedAppointments) {
        this.completedAppointments = completedAppointments;
    }
}
