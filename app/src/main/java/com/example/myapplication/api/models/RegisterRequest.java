package com.example.myapplication.api.models;

public class RegisterRequest {
    private String firstName;
    private String lastName;
    private String email;
    private String password;
    private String phone;
    private int role; // 0=Patient, 1=Doctor, 2=Clerk
    private String dateOfBirth; // For Patient
    private String specialization; // For Doctor

    public RegisterRequest(String firstName, String lastName, String email, String password, String phone, int role) {
        this.firstName = firstName;
        this.lastName = lastName;
        this.email = email;
        this.password = password;
        this.phone = phone;
        this.role = role;
    }

    // Setters for optional fields
    public void setDateOfBirth(String dateOfBirth) { this.dateOfBirth = dateOfBirth; }
    public void setSpecialization(String specialization) { this.specialization = specialization; }
}
