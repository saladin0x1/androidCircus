package com.example.myapplication.api.models;

import com.google.gson.annotations.SerializedName;

public class PatientDTO {
    @SerializedName("id")
    private String id;

    @SerializedName("email")
    private String email;

    @SerializedName("firstName")
    private String firstName;

    @SerializedName("lastName")
    private String lastName;

    @SerializedName("phone")
    private String phone;

    @SerializedName("dateOfBirth")
    private String dateOfBirth;

    @SerializedName("address")
    private String address;

    @SerializedName("emergencyContactName")
    private String emergencyContactName;

    @SerializedName("emergencyContactPhone")
    private String emergencyContactPhone;

    @SerializedName("doctorNotes")
    private String doctorNotes;

    @SerializedName("registrationDate")
    private String registrationDate;

    // Getters
    public String getId() { return id; }
    public String getEmail() { return email; }
    public String getFirstName() { return firstName; }
    public String getLastName() { return lastName; }
    public String getPhone() { return phone; }
    public String getDateOfBirth() { return dateOfBirth; }
    public String getAddress() { return address; }
    public String getEmergencyContactName() { return emergencyContactName; }
    public String getEmergencyContactPhone() { return emergencyContactPhone; }
    public String getDoctorNotes() { return doctorNotes; }
    public String getRegistrationDate() { return registrationDate; }

    // Setters (if needed)
    public void setId(String id) { this.id = id; }
    public void setEmail(String email) { this.email = email; }
    public void setFirstName(String firstName) { this.firstName = firstName; }
    public void setLastName(String lastName) { this.lastName = lastName; }
    public void setPhone(String phone) { this.phone = phone; }
    public void setDateOfBirth(String dateOfBirth) { this.dateOfBirth = dateOfBirth; }
    public void setAddress(String address) { this.address = address; }
    public void setEmergencyContactName(String emergencyContactName) { this.emergencyContactName = emergencyContactName; }
    public void setEmergencyContactPhone(String emergencyContactPhone) { this.emergencyContactPhone = emergencyContactPhone; }
    public void setDoctorNotes(String doctorNotes) { this.doctorNotes = doctorNotes; }
    public void setRegistrationDate(String registrationDate) { this.registrationDate = registrationDate; }
}
