package com.example.myapplication.repository;

import android.content.Context;
import com.example.myapplication.api.models.AppointmentDTO;
import com.example.myapplication.api.models.DoctorDashboardDTO;
import com.example.myapplication.api.models.PatientDTO;
import java.util.List;

public class DoctorRepository extends Repository {

    private static DoctorRepository instance;

    private DoctorRepository(Context context) {
        super(context);
    }

    public static synchronized DoctorRepository getInstance(Context context) {
        if (instance == null) {
            instance = new DoctorRepository(context);
        }
        return instance;
    }

    public void getDashboard(RepositoryCallback<DoctorDashboardDTO> callback) {
        executeCall(apiService.getDoctorDashboard(), callback);
    }

    public void getPatients(RepositoryCallback<List<PatientDTO>> callback) {
        executeCall(apiService.getDoctorPatients(), callback);
    }

    public void getAgenda(String startDate, String endDate, RepositoryCallback<List<AppointmentDTO>> callback) {
        executeCall(apiService.getDoctorAgenda(startDate, endDate), callback);
    }
}
