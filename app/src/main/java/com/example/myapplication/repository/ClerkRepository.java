package com.example.myapplication.repository;

import android.content.Context;
import com.example.myapplication.api.models.AppointmentDTO;
import com.example.myapplication.api.models.DashboardDTO;
import java.util.List;

public class ClerkRepository extends Repository {

    private static ClerkRepository instance;

    private ClerkRepository(Context context) {
        super(context);
    }

    public static synchronized ClerkRepository getInstance(Context context) {
        if (instance == null) {
            instance = new ClerkRepository(context);
        }
        return instance;
    }

    public void getDashboard(RepositoryCallback<DashboardDTO> callback) {
        executeCall(apiService.getDashboard(), callback);
    }

    public void getTodayAppointments(RepositoryCallback<List<AppointmentDTO>> callback) {
        executeCall(apiService.getTodayAppointments(), callback);
    }

    public void getPendingAppointments(RepositoryCallback<List<AppointmentDTO>> callback) {
        executeCall(apiService.getPendingAppointments(), callback);
    }
}
