package com.example.myapplication.api;

import com.example.myapplication.api.models.*;
import java.util.List;
import retrofit2.Call;
import retrofit2.http.*;

public interface ApiService {

    // Auth
    @POST("auth/login")
    Call<LoginResponse> login(@Body LoginRequest request);

    @POST("auth/register")
    Call<LoginResponse> register(@Body RegisterRequest request);

    // Appointments
    @GET("appointments")
    Call<ApiResponse<List<AppointmentDTO>>> getAppointments(
        @Header("Authorization") String token,
        @Query("status") String status
    );

    @GET("appointments/{id}")
    Call<ApiResponse<AppointmentDTO>> getAppointment(
        @Header("Authorization") String token,
        @Path("id") String id
    );

    @POST("appointments")
    Call<ApiResponse<AppointmentDTO>> createAppointment(
        @Header("Authorization") String token,
        @Body CreateAppointmentRequest request
    );

    @GET("clerk/dashboard")
    Call<ApiResponse<DashboardDTO>> getDashboard(
        @Header("Authorization") String token
    );

    @GET("doctors")
    Call<ApiResponse<List<DoctorDTO>>> getDoctors(
        @Header("Authorization") String token
    );

    @PUT("appointments/{id}/complete")
    Call<ApiResponse<AppointmentDTO>> completeAppointment(
        @Header("Authorization") String token,
        @Path("id") String id,
        @Body CompleteAppointmentRequest request
    );

    @DELETE("appointments/{id}")
    Call<ApiResponse<Object>> cancelAppointment(
        @Header("Authorization") String token,
        @Path("id") String id
    );

    @GET("patients/{id}")
    Call<ApiResponse<PatientDTO>> getPatient(
        @Header("Authorization") String token,
        @Path("id") String id
    );

    @GET("patients/{id}/notes")
    Call<ApiResponse<String>> getPatientNotes(
        @Header("Authorization") String token,
        @Path("id") String id
    );

    @PUT("patients/{id}/notes")
    Call<ApiResponse<String>> updatePatientNotes(
        @Header("Authorization") String token,
        @Path("id") String id,
        @Body UpdatePatientNotesRequest request
    );
}