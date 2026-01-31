package com.example.myapplication.api.models;

import com.google.gson.annotations.SerializedName;

public class DoctorDTO {
    private String id;

    @SerializedName("firstName")
    private String firstName;

    @SerializedName("lastName")
    private String lastName;

    private String specialization;

    public String getId() { return id; }

    public String getFirstName() { return firstName; }

    public String getLastName() { return lastName; }

    public String getSpecialization() { return specialization; }

    // Composed name for backward compatibility
    public String getName() {
        if (firstName != null && lastName != null) {
            return firstName + " " + lastName;
        } else if (firstName != null) {
            return firstName;
        } else if (lastName != null) {
            return lastName;
        }
        return null;
    }

    // Override toString for simple Spinner display
    @Override
    public String toString() {
        String displayName = getName();
        if (displayName == null || displayName.isEmpty()) {
            displayName = "Médecin inconnu";
        }
        String displaySpec = (specialization != null) ? specialization : "Généraliste";
        return displayName + " (" + displaySpec + ")";
    }
}
