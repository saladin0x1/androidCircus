package com.example.myapplication.repository;

import android.content.Context;
import com.example.myapplication.api.models.AppointmentDTO;
import com.example.myapplication.api.models.CreateAppointmentRequest;
import com.example.myapplication.api.models.CompleteAppointmentRequest;
import com.example.myapplication.api.models.RescheduleAppointmentRequest;
import com.example.myapplication.api.models.UpdateAppointmentStatusRequest;
import com.example.myapplication.api.models.TimeSlotDTO;
import java.util.List;

public class AppointmentRepository extends Repository {

    private static AppointmentRepository instance;

    private AppointmentRepository(Context context) {
        super(context);
    }

    public static synchronized AppointmentRepository getInstance(Context context) {
        if (instance == null) {
            instance = new AppointmentRepository(context);
        }
        return instance;
    }

    public void getAppointments(String status, RepositoryCallback<List<AppointmentDTO>> callback) {
        executeCall(apiService.getAppointments(status), callback);
    }

    public void getAppointment(String id, RepositoryCallback<AppointmentDTO> callback) {
        executeCall(apiService.getAppointment(id), callback);
    }

    public void createAppointment(CreateAppointmentRequest request, RepositoryCallback<AppointmentDTO> callback) {
        executeCall(apiService.createAppointment(request), callback);
    }

    public void rescheduleAppointment(String id, RescheduleAppointmentRequest request,
                                      RepositoryCallback<AppointmentDTO> callback) {
        executeCall(apiService.rescheduleAppointment(id, request), callback);
    }

    public void updateAppointmentStatus(String id, String status, RepositoryCallback<AppointmentDTO> callback) {
        UpdateAppointmentStatusRequest request = new UpdateAppointmentStatusRequest(status);
        executeCall(apiService.updateAppointmentStatus(id, request), callback);
    }

    public void completeAppointment(String id, CompleteAppointmentRequest request,
                                     RepositoryCallback<AppointmentDTO> callback) {
        executeCall(apiService.completeAppointment(id, request), callback);
    }

    public void cancelAppointment(String id, RepositoryCallback<Object> callback) {
        executeCall(apiService.cancelAppointment(id), callback);
    }

    public void getAvailableSlots(String doctorId, String date, RepositoryCallback<List<TimeSlotDTO>> callback) {
        executeCall(apiService.getAvailableSlots(doctorId, date), callback);
    }
}
