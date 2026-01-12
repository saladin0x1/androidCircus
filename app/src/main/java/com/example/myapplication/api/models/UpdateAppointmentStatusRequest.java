package com.example.myapplication.api.models;

import com.google.gson.annotations.SerializedName;

public class UpdateAppointmentStatusRequest {
    @SerializedName("status")
    private String status;

    public UpdateAppointmentStatusRequest(String status) {
        this.status = status;
    }

    public String getStatus() {
        return status;
    }

    public void setStatus(String status) {
        this.status = status;
    }
}
