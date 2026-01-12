package com.example.myapplication.fragments;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.LinearLayout;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.example.myapplication.AppointmentsAdapter;
import com.example.myapplication.R;
import com.example.myapplication.api.ApiService;
import com.example.myapplication.api.RetrofitClient;
import com.example.myapplication.api.SessionManager;
import com.example.myapplication.api.models.ApiResponse;
import com.example.myapplication.api.models.AppointmentDTO;
import java.util.ArrayList;
import java.util.List;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class PatientHistoryFragment extends Fragment {

    private RecyclerView appointmentsRecyclerView;
    private LinearLayout emptyStateLayout;
    private AppointmentsAdapter adapter;
    private SessionManager sessionManager;
    private ApiService apiService;
    private String patientId;
    private String patientName;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_patient_history, container, false);

        sessionManager = new SessionManager(requireContext());
        apiService = RetrofitClient.getApiService();

        // Get patient data from arguments
        Bundle args = getArguments();
        if (args != null) {
            patientId = args.getString("patientId");
            patientName = args.getString("patientName");
        }

        // Initialize views
        appointmentsRecyclerView = view.findViewById(R.id.appointmentsRecyclerView);
        emptyStateLayout = view.findViewById(R.id.emptyStateLayout);

        // Setup RecyclerView
        appointmentsRecyclerView.setLayoutManager(new LinearLayoutManager(requireContext()));
        adapter = new AppointmentsAdapter(new ArrayList<>());
        appointmentsRecyclerView.setAdapter(adapter);

        // Load appointments
        if (patientId != null) {
            loadAppointments();
        }

        return view;
    }

    private void loadAppointments() {
        String token = sessionManager.getAuthHeader();
        if (token == null) return;

        apiService.getAppointments(token, "all").enqueue(new Callback<ApiResponse<List<AppointmentDTO>>>() {
            @Override
            public void onResponse(Call<ApiResponse<List<AppointmentDTO>>> call, Response<ApiResponse<List<AppointmentDTO>>> response) {
                if (response.isSuccessful() && response.body() != null) {
                    ApiResponse<List<AppointmentDTO>> apiResponse = response.body();
                    if (apiResponse.isSuccess()) {
                        displayAppointments(apiResponse.getData());
                    }
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<List<AppointmentDTO>>> call, Throwable t) {
                // Handle error
            }
        });
    }

    private void displayAppointments(List<AppointmentDTO> appointments) {
        // Filter appointments for this patient
        List<AppointmentDTO> patientAppointments = new ArrayList<>();
        for (AppointmentDTO appointment : appointments) {
            if (appointment.getPatientId().equals(patientId)) {
                patientAppointments.add(appointment);
            }
        }

        if (patientAppointments.isEmpty()) {
            emptyStateLayout.setVisibility(View.VISIBLE);
            appointmentsRecyclerView.setVisibility(View.GONE);
        } else {
            emptyStateLayout.setVisibility(View.GONE);
            appointmentsRecyclerView.setVisibility(View.VISIBLE);
            adapter.updateData(patientAppointments);
        }
    }
}
