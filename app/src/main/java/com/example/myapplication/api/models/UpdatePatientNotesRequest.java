package com.example.myapplication.api.models;

import com.google.gson.annotations.SerializedName;

public class UpdatePatientNotesRequest {
    @SerializedName("notes")
    private String notes;

    public UpdatePatientNotesRequest(String notes) {
        this.notes = notes;
    }

    public String getNotes() {
        return notes;
    }

    public void setNotes(String notes) {
        this.notes = notes;
    }
}
