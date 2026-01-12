package com.example.myapplication.fragments;

import android.content.Intent;
import android.os.Bundle;
import android.text.Editable;
import android.text.TextWatcher;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.example.myapplication.PatientDossierActivity;
import com.example.myapplication.PatientsAdapter;
import com.example.myapplication.R;
import com.example.myapplication.api.ApiService;
import com.example.myapplication.api.RetrofitClient;
import com.example.myapplication.api.SessionManager;
import com.example.myapplication.api.models.ApiResponse;
import com.example.myapplication.api.models.AppointmentDTO;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.HashMap;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class PatientsFragment extends Fragment {

    private EditText searchEditText;
    private TextView patientsCountText;
    private RecyclerView patientsRecyclerView;
    private View emptyStateLayout;
    private PatientsAdapter adapter;
    private SessionManager sessionManager;
    private ApiService apiService;
    private List<PatientInfo> patientsList = new ArrayList<>();
    private List<PatientInfo> filteredList = new ArrayList<>();

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_patients, container, false);

        sessionManager = new SessionManager(requireContext());
        apiService = RetrofitClient.getApiService();

        // Initialize views
        searchEditText = view.findViewById(R.id.searchEditText);
        patientsCountText = view.findViewById(R.id.patientsCountText);
        patientsRecyclerView = view.findViewById(R.id.patientsRecyclerView);
        emptyStateLayout = view.findViewById(R.id.emptyStateLayout);

        // Setup RecyclerView
        patientsRecyclerView.setLayoutManager(new LinearLayoutManager(requireContext()));
        adapter = new PatientsAdapter(new ArrayList<>(), this::onPatientClick);
        patientsRecyclerView.setAdapter(adapter);

        // Search functionality
        searchEditText.addTextChangedListener(new TextWatcher() {
            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {}

            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count) {
                filterPatients(s.toString());
            }

            @Override
            public void afterTextChanged(Editable s) {}
        });

        // Load patients
        loadPatients();

        return view;
    }

    private void loadPatients() {
        apiService.getAppointments("all").enqueue(new Callback<ApiResponse<List<AppointmentDTO>>>() {
            @Override
            public void onResponse(Call<ApiResponse<List<AppointmentDTO>>> call, Response<ApiResponse<List<AppointmentDTO>>> response) {
                if (response.isSuccessful() && response.body() != null) {
                    ApiResponse<List<AppointmentDTO>> apiResponse = response.body();
                    if (apiResponse.isSuccess()) {
                        processPatientsData(apiResponse.getData());
                    }
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<List<AppointmentDTO>>> call, Throwable t) {
                Toast.makeText(requireContext(), "Erreur réseau", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void processPatientsData(List<AppointmentDTO> appointments) {
        // Group appointments by patient
        Map<String, PatientInfo> patientsMap = new HashMap<>();

        for (AppointmentDTO appointment : appointments) {
            String patientId = appointment.getPatientId();
            String patientName = appointment.getPatientName();

            if (!patientsMap.containsKey(patientId)) {
                patientsMap.put(patientId, new PatientInfo(patientId, patientName));
            }

            patientsMap.get(patientId).incrementAppointmentCount();
        }

        // Convert to list and sort by name
        patientsList = new ArrayList<>(patientsMap.values());
        Collections.sort(patientsList, Comparator.comparing(PatientInfo::getName));

        filteredList = new ArrayList<>(patientsList);
        updateUI();
    }

    private void filterPatients(String query) {
        filteredList.clear();

        if (query.isEmpty()) {
            filteredList.addAll(patientsList);
        } else {
            String lowerQuery = query.toLowerCase(Locale.ROOT);
            for (PatientInfo patient : patientsList) {
                if (patient.getName().toLowerCase(Locale.ROOT).contains(lowerQuery)) {
                    filteredList.add(patient);
                }
            }
        }

        updateUI();
    }

    private void updateUI() {
        if (filteredList.isEmpty()) {
            emptyStateLayout.setVisibility(View.VISIBLE);
            patientsRecyclerView.setVisibility(View.GONE);
        } else {
            emptyStateLayout.setVisibility(View.GONE);
            patientsRecyclerView.setVisibility(View.VISIBLE);
            adapter.updateData(filteredList);
        }

        int count = filteredList.size();
        patientsCountText.setText(count + " Patient" + (count > 1 ? "s" : ""));
    }

    private void onPatientClick(PatientInfo patient) {
        Intent intent = new Intent(requireContext(), PatientDossierActivity.class);
        intent.putExtra("patientId", patient.getId());
        intent.putExtra("patientName", patient.getName());
        startActivity(intent);
    }

    @Override
    public void onResume() {
        super.onResume();
        loadPatients();
    }

    // Patient info holder
    public static class PatientInfo {
        private final String id;
        private final String name;
        private int appointmentCount;

        public PatientInfo(String id, String name) {
            this.id = id;
            this.name = name;
            this.appointmentCount = 0;
        }

        public String getId() { return id; }
        public String getName() { return name; }
        public int getAppointmentCount() { return appointmentCount; }
        public void incrementAppointmentCount() { appointmentCount++; }
    }
}
