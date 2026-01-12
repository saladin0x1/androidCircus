package com.example.myapplication.api.models;

public class DoctorDTO {
    private String id;
    private String name;
    private String specialization;

    public String getId() { return id; }
    public String getName() { return name; }
    public String getSpecialization() { return specialization; }
    
    // Override toString for simple Spinner display
    @Override
    public String toString() {
        return name + " (" + specialization + ")";
    }
}
