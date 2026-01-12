package com.example.myapplication.api.models;

import com.google.gson.annotations.SerializedName;

import java.util.Date;

public class RescheduleAppointmentRequest {
    @SerializedName("appointmentDate")
    private Date appointmentDate;

    public RescheduleAppointmentRequest(Date appointmentDate) {
        this.appointmentDate = appointmentDate;
    }

    public Date getAppointmentDate() {
        return appointmentDate;
    }

    public void setAppointmentDate(Date appointmentDate) {
        this.appointmentDate = appointmentDate;
    }
}
