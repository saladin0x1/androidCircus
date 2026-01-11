package com.example.myapplication.fragments;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.LinearLayout;
import android.widget.TextView;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.cardview.widget.CardView;
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
import com.example.myapplication.api.models.PatientDTO;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.Locale;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class PatientDossierFragment extends Fragment {

    private TextView patientAvatarLarge;
    private TextView patientNameText;
    private TextView patientEmailText;
    private TextView patientDobText;
    private TextView patientPhoneText;
    private RecyclerView appointmentsHistoryRecyclerView;
    private LinearLayout emptyStateLayout;
    private AppointmentsAdapter adapter;
    private SessionManager sessionManager;
    private ApiService apiService;

    private String selectedPatientId;
    private String selectedPatientName;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_patient_dossier, container, false);

        sessionManager = new SessionManager(requireContext());
        apiService = RetrofitClient.getApiService();

        // Initialize views
        patientAvatarLarge = view.findViewById(R.id.patientAvatarLarge);
        patientNameText = view.findViewById(R.id.patientNameText);
        patientEmailText = view.findViewById(R.id.patientEmailText);
        patientDobText = view.findViewById(R.id.patientDobText);
        patientPhoneText = view.findViewById(R.id.patientPhoneText);
        appointmentsHistoryRecyclerView = view.findViewById(R.id.appointmentsHistoryRecyclerView);
        emptyStateLayout = view.findViewById(R.id.emptyStateLayout);

        // Setup RecyclerView
        appointmentsHistoryRecyclerView.setLayoutManager(new LinearLayoutManager(requireContext()));
        adapter = new AppointmentsAdapter(new ArrayList<>());
        appointmentsHistoryRecyclerView.setAdapter(adapter);

        // Show empty state initially
        showEmptyState();

        // Check if patient data passed from PatientsFragment
        Bundle args = getArguments();
        if (args != null) {
            selectedPatientId = args.getString("patientId");
            selectedPatientName = args.getString("patientName");
            loadPatientDossier();
        }

        return view;
    }

    public void setPatient(String patientId, String patientName) {
        this.selectedPatientId = patientId;
        this.selectedPatientName = patientName;
        loadPatientDossier();
    }

    private void loadPatientDossier() {
        if (selectedPatientId == null) {
            showEmptyState();
            return;
        }

        String token = sessionManager.getAuthHeader();
        if (token == null) return;

        // Show loading state
        emptyStateLayout.setVisibility(View.GONE);
        patientNameText.setText(selectedPatientName);

        // Get initials for avatar
        String initials = getInitials(selectedPatientName);
        patientAvatarLarge.setText(initials);

        // Load patient details from API
        apiService.getPatient(token, selectedPatientId).enqueue(new Callback<ApiResponse<PatientDTO>>() {
            @Override
            public void onResponse(Call<ApiResponse<PatientDTO>> call, Response<ApiResponse<PatientDTO>> response) {
                if (response.isSuccessful() && response.body() != null) {
                    ApiResponse<PatientDTO> apiResponse = response.body();
                    if (apiResponse.isSuccess()) {
                        displayPatientInfo(apiResponse.getData());
                        // Also load appointments
                        loadPatientAppointments();
                    } else {
                        Toast.makeText(requireContext(), "Patient non trouvé", Toast.LENGTH_SHORT).show();
                        showEmptyState();
                    }
                } else {
                    Toast.makeText(requireContext(), "Erreur lors du chargement du patient", Toast.LENGTH_SHORT).show();
                    showEmptyState();
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<PatientDTO>> call, Throwable t) {
                Toast.makeText(requireContext(), "Erreur réseau", Toast.LENGTH_SHORT).show();
                showEmptyState();
            }
        });
    }

    private void loadPatientAppointments() {
        String token = sessionManager.getAuthHeader();
        if (token == null) return;

        apiService.getAppointments(token, "all").enqueue(new Callback<ApiResponse<List<AppointmentDTO>>>() {
            @Override
            public void onResponse(Call<ApiResponse<List<AppointmentDTO>>> call, Response<ApiResponse<List<AppointmentDTO>>> response) {
                if (response.isSuccessful() && response.body() != null) {
                    ApiResponse<List<AppointmentDTO>> apiResponse = response.body();
                    if (apiResponse.isSuccess()) {
                        displayAppointmentsData(apiResponse.getData());
                    }
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<List<AppointmentDTO>>> call, Throwable t) {
                // Keep patient info even if appointments fail to load
            }
        });
    }

    private void displayPatientInfo(PatientDTO patient) {
        patientNameText.setText(patient.getFirstName() + " " + patient.getLastName());
        patientEmailText.setText(patient.getEmail());
        patientPhoneText.setText(patient.getPhone() != null && !patient.getPhone().isEmpty() ? patient.getPhone() : "Non renseigné");

        // Format date of birth
        if (patient.getDateOfBirth() != null && !patient.getDateOfBirth().isEmpty()) {
            try {
                SimpleDateFormat inputFormat = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.getDefault());
                SimpleDateFormat outputFormat = new SimpleDateFormat("dd/MM/yyyy", Locale.getDefault());
                Date dob = inputFormat.parse(patient.getDateOfBirth());
                patientDobText.setText(outputFormat.format(dob));
            } catch (Exception e) {
                patientDobText.setText(patient.getDateOfBirth());
            }
        } else {
            patientDobText.setText("Non renseignée");
        }
    }

    private void displayAppointmentsData(List<AppointmentDTO> appointments) {
        // Filter appointments for this patient
        List<AppointmentDTO> patientAppointments = new ArrayList<>();
        for (AppointmentDTO appointment : appointments) {
            if (appointment.getPatientId().equals(selectedPatientId)) {
                patientAppointments.add(appointment);
            }
        }

        // Update appointments history
        if (!patientAppointments.isEmpty()) {
            adapter.updateData(patientAppointments);
        }
    }

    private void showEmptyState() {
        emptyStateLayout.setVisibility(View.VISIBLE);
        patientNameText.setText("Aucun patient sélectionné");
        patientEmailText.setText("");
        patientDobText.setText("");
        patientPhoneText.setText("");
    }

    private String getInitials(String name) {
        if (name == null || name.isEmpty()) return "??";

        String[] parts = name.trim().split("\\s+");
        StringBuilder initials = new StringBuilder();

        for (String part : parts) {
            if (!part.isEmpty()) {
                initials.append(part.substring(0, 1).toUpperCase(Locale.ROOT));
                if (initials.length() >= 2) break;
            }
        }

        return initials.toString();
    }
}
