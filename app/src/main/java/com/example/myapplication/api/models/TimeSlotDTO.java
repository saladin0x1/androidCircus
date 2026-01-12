package com.example.myapplication.api.models;

import com.google.gson.annotations.SerializedName;

public class TimeSlotDTO {
    @SerializedName("time")
    private String time;

    @SerializedName("available")
    private boolean available;

    public TimeSlotDTO() {}

    public TimeSlotDTO(String time, boolean available) {
        this.time = time;
        this.available = available;
    }

    public String getTime() {
        return time;
    }

    public void setTime(String time) {
        this.time = time;
    }

    public boolean isAvailable() {
        return available;
    }

    public void setAvailable(boolean available) {
        this.available = available;
    }
}
