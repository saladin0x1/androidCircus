package com.example.myapplication.api;

import com.example.myapplication.api.models.*;
import java.util.List;
import retrofit2.Call;
import retrofit2.http.*;

public interface ApiService {

    // Auth endpoints (no auth required)
    @POST("auth/login")
    Call<LoginResponse> login(@Body LoginRequest request);

    @POST("auth/register")
    Call<LoginResponse> register(@Body RegisterRequest request);

    @POST("auth/forgot-password")
    Call<ApiResponse<Object>> forgotPassword(@Body ForgotPasswordRequest request);

    @POST("auth/reset-password")
    Call<ApiResponse<Object>> resetPassword(@Body ResetPasswordRequest request);

    @POST("auth/refresh")
    Call<LoginResponse> refreshToken(@Body RefreshTokenRequest request);

    // Appointments (auth via interceptor)
    @GET("appointments")
    Call<ApiResponse<List<AppointmentDTO>>> getAppointments(@Query("status") String status);

    @GET("appointments/{id}")
    Call<ApiResponse<AppointmentDTO>> getAppointment(@Path("id") String id);

    @POST("appointments")
    Call<ApiResponse<AppointmentDTO>> createAppointment(@Body CreateAppointmentRequest request);

    @PUT("appointments/{id}")
    Call<ApiResponse<AppointmentDTO>> rescheduleAppointment(
        @Path("id") String id,
        @Body RescheduleAppointmentRequest request
    );

    @PUT("appointments/{id}/status")
    Call<ApiResponse<AppointmentDTO>> updateAppointmentStatus(
        @Path("id") String id,
        @Body UpdateAppointmentStatusRequest request
    );

    @GET("appointments/available-slots")
    Call<ApiResponse<List<TimeSlotDTO>>> getAvailableSlots(
        @Query("doctorId") String doctorId,
        @Query("date") String date
    );

    @PUT("appointments/{id}/complete")
    Call<ApiResponse<AppointmentDTO>> completeAppointment(
        @Path("id") String id,
        @Body CompleteAppointmentRequest request
    );

    @DELETE("appointments/{id}")
    Call<ApiResponse<Object>> cancelAppointment(@Path("id") String id);

    // Doctors (auth via interceptor)
    @GET("doctors")
    Call<ApiResponse<List<DoctorDTO>>> getDoctors();

    @GET("doctors/{id}")
    Call<ApiResponse<DoctorDTO>> getDoctor(@Path("id") String id);

    @GET("doctors/dashboard")
    Call<ApiResponse<DoctorDashboardDTO>> getDoctorDashboard();

    @GET("doctors/patients")
    Call<ApiResponse<List<PatientDTO>>> getDoctorPatients();

    @GET("doctors/agenda")
    Call<ApiResponse<List<AppointmentDTO>>> getDoctorAgenda(
        @Query("startDate") String startDate,
        @Query("endDate") String endDate
    );

    // Patients (auth via interceptor)
    @GET("patients")
    Call<ApiResponse<List<PatientDTO>>> getPatients(@Query("search") String search);

    @GET("patients/{id}")
    Call<ApiResponse<PatientDTO>> getPatient(@Path("id") String id);

    @GET("patients/{id}/notes")
    Call<ApiResponse<String>> getPatientNotes(@Path("id") String id);

    @PUT("patients/{id}/notes")
    Call<ApiResponse<String>> updatePatientNotes(
        @Path("id") String id,
        @Body UpdatePatientNotesRequest request
    );

    @GET("patients/{id}/medical-history")
    Call<ApiResponse<MedicalHistoryDTO>> getPatientMedicalHistory(@Path("id") String id);

    // Clerk (auth via interceptor)
    @GET("clerk/dashboard")
    Call<ApiResponse<DashboardDTO>> getDashboard();

    @GET("clerk/dashboard/today")
    Call<ApiResponse<List<AppointmentDTO>>> getTodayAppointments();

    @GET("clerk/dashboard/pending")
    Call<ApiResponse<List<AppointmentDTO>>> getPendingAppointments();

    // User Profile (auth via interceptor)
    @GET("users/me")
    Call<ApiResponse<UserProfile>> getProfile();

    @PUT("users/me")
    Call<ApiResponse<UserProfile>> updateProfile(@Body UpdateProfileRequest request);

    @PUT("users/me/password")
    Call<ApiResponse<Object>> updatePassword(@Body UpdatePasswordRequest request);

    // Pending Users (for Clerk approval, auth via interceptor)
    @GET("users/pending")
    Call<ApiResponse<List<PendingUser>>> getPendingUsers();

    @POST("users/{id}/approve")
    Call<ApiResponse<Object>> approveUser(@Path("id") String id);

    @POST("users/{id}/reject")
    Call<ApiResponse<Object>> rejectUser(@Path("id") String id);
}
