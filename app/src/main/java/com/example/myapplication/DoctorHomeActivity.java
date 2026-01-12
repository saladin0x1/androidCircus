package com.example.myapplication;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.TextView;
import androidx.appcompat.app.AppCompatActivity;
import androidx.cardview.widget.CardView;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.example.myapplication.api.ApiService;
import com.example.myapplication.api.RetrofitClient;
import com.example.myapplication.api.SessionManager;
import com.example.myapplication.api.models.ApiResponse;
import com.example.myapplication.api.models.AppointmentDTO;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;
import java.util.HashSet;
import java.util.List;
import java.util.Locale;
import java.util.Set;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class DoctorHomeActivity extends AppCompatActivity {

    private TextView welcomeText, logoutButton;
    private TextView patientsCountText, appointmentsCountText;
    private CardView agendaCard, patientsCard;
    private RecyclerView todayAppointmentsRecyclerView;
    private View emptyStateLayout;
    private AppointmentsAdapter appointmentsAdapter;
    private SessionManager sessionManager;
    private ApiService apiService;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_doctor_home);

        sessionManager = new SessionManager(this);
        apiService = RetrofitClient.getApiService();

        // Initialize views
        welcomeText = findViewById(R.id.welcomeText);
        logoutButton = findViewById(R.id.logoutButton);
        patientsCountText = findViewById(R.id.patientsCountText);
        appointmentsCountText = findViewById(R.id.appointmentsCountText);
        agendaCard = findViewById(R.id.agendaCard);
        patientsCard = findViewById(R.id.patientsCard);
        todayAppointmentsRecyclerView = findViewById(R.id.todayAppointmentsRecyclerView);
        emptyStateLayout = findViewById(R.id.emptyStateLayout);

        // Set welcome text
        welcomeText.setText("Dr. " + sessionManager.getUserName());

        // Setup RecyclerView for today's appointments
        todayAppointmentsRecyclerView.setLayoutManager(new LinearLayoutManager(this));
        appointmentsAdapter = new AppointmentsAdapter(new ArrayList<>());
        todayAppointmentsRecyclerView.setAdapter(appointmentsAdapter);

        // Navigation cards
        agendaCard.setOnClickListener(v -> openAgenda());
        patientsCard.setOnClickListener(v -> openPatients());

        // Logout
        logoutButton.setOnClickListener(v -> {
            sessionManager.logout();
            Intent intent = new Intent(DoctorHomeActivity.this, SignInActivity.class);
            startActivity(intent);
            finish();
        });

        // Load data
        loadDashboardData();
    }

    private void openAgenda() {
        // Navigate to agenda screen (using AgendaFragment in a new activity or reuse existing)
        Intent intent = new Intent(this, DoctorAgendaActivity.class);
        startActivity(intent);
    }

    private void openPatients() {
        // Navigate to patients screen (using PatientsFragment in a new activity or reuse existing)
        Intent intent = new Intent(this, DoctorPatientsActivity.class);
        startActivity(intent);
    }

    private void loadDashboardData() {
        String token = sessionManager.getAuthHeader();
        if (token == null) return;

        apiService.getAppointments(token, "all").enqueue(new Callback<ApiResponse<List<AppointmentDTO>>>() {
            @Override
            public void onResponse(Call<ApiResponse<List<AppointmentDTO>>> call, Response<ApiResponse<List<AppointmentDTO>>> response) {
                if (response.isSuccessful() && response.body() != null) {
                    ApiResponse<List<AppointmentDTO>> apiResponse = response.body();
                    if (apiResponse.isSuccess()) {
                        List<AppointmentDTO> appointments = apiResponse.getData();
                        updateStatistics(appointments);
                        displayTodayAppointments(appointments);
                    }
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<List<AppointmentDTO>>> call, Throwable t) {
                // Silently fail
            }
        });
    }

    private void updateStatistics(List<AppointmentDTO> appointments) {
        // Count unique patients
        Set<String> uniquePatients = new HashSet<>();
        for (AppointmentDTO appointment : appointments) {
            uniquePatients.add(appointment.getPatientId());
        }
        patientsCountText.setText(String.valueOf(uniquePatients.size()));

        // Count total appointments
        appointmentsCountText.setText(String.valueOf(appointments.size()));
    }

    private void displayTodayAppointments(List<AppointmentDTO> appointments) {
        // Get today's date
        SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd", Locale.getDefault());
        String todayDate = sdf.format(new Date());

        // Filter appointments for today
        List<AppointmentDTO> todayAppointments = new ArrayList<>();
        for (AppointmentDTO appointment : appointments) {
            String appointmentDate = appointment.getAppointmentDate().substring(0, 10);
            if (appointmentDate.equals(todayDate)) {
                todayAppointments.add(appointment);
            }
        }

        if (todayAppointments.isEmpty()) {
            emptyStateLayout.setVisibility(View.VISIBLE);
            todayAppointmentsRecyclerView.setVisibility(View.GONE);
        } else {
            emptyStateLayout.setVisibility(View.GONE);
            todayAppointmentsRecyclerView.setVisibility(View.VISIBLE);
            appointmentsAdapter.updateData(todayAppointments);
        }
    }

    @Override
    protected void onResume() {
        super.onResume();
        loadDashboardData();
    }
}
