package com.example.myapplication;

import android.content.Intent;
import android.os.Bundle;
import android.widget.TextView;
import android.widget.Toast;
import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.example.myapplication.api.ApiService;
import com.example.myapplication.api.RetrofitClient;
import com.example.myapplication.api.SessionManager;
import com.example.myapplication.api.models.ApiResponse;
import com.example.myapplication.api.models.AppointmentDTO;
import com.example.myapplication.api.models.DashboardDTO;
import java.util.ArrayList;
import java.util.List;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class ClerkHomeActivity extends AppCompatActivity {

    private TextView welcomeText, logoutButton;
    private TextView todayCountText, pendingCountText;
    private TextView approvalsButton;
    private RecyclerView recyclerView;
    private AppointmentsAdapter adapter;
    private SessionManager sessionManager;
    private ApiService apiService;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_clerk_home);

        sessionManager = new SessionManager(this);
        apiService = RetrofitClient.getApiService();

        welcomeText = findViewById(R.id.welcomeText);
        logoutButton = findViewById(R.id.logoutButton);
        todayCountText = findViewById(R.id.todayCountText);
        pendingCountText = findViewById(R.id.pendingCountText);
        approvalsButton = findViewById(R.id.approvalsButton);
        recyclerView = findViewById(R.id.appointmentsRecyclerView);

        welcomeText.setText(sessionManager.getUserName());

        recyclerView.setLayoutManager(new LinearLayoutManager(this));
        adapter = new AppointmentsAdapter(new ArrayList<>());
        recyclerView.setAdapter(adapter);

        logoutButton.setOnClickListener(v -> showLogoutConfirmation());
        approvalsButton.setOnClickListener(v -> {
            Intent intent = new Intent(ClerkHomeActivity.this, ClerkApprovalsActivity.class);
            startActivity(intent);
        });

        loadDashboard();
        loadAppointments();
    }

    private void loadDashboard() {
        apiService.getDashboard().enqueue(new Callback<ApiResponse<DashboardDTO>>() {
            @Override
            public void onResponse(Call<ApiResponse<DashboardDTO>> call, Response<ApiResponse<DashboardDTO>> response) {
                if (response.isSuccessful() && response.body() != null) {
                    ApiResponse<DashboardDTO> apiResponse = response.body();
                    if (apiResponse.isSuccess()) {
                        DashboardDTO data = apiResponse.getData();
                        todayCountText.setText(String.valueOf(data.getTodayAppointments()));
                        pendingCountText.setText(String.valueOf(data.getPendingAppointments()));
                    }
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<DashboardDTO>> call, Throwable t) {
                // Silently fail for dashboard stats
            }
        });
    }

    private void loadAppointments() {
        apiService.getAppointments("all").enqueue(new Callback<ApiResponse<List<AppointmentDTO>>>() {
            @Override
            public void onResponse(Call<ApiResponse<List<AppointmentDTO>>> call, Response<ApiResponse<List<AppointmentDTO>>> response) {
                if (response.isSuccessful() && response.body() != null) {
                    ApiResponse<List<AppointmentDTO>> apiResponse = response.body();
                    if (apiResponse.isSuccess()) {
                        adapter.updateData(apiResponse.getData());
                    }
                } else {
                    Toast.makeText(ClerkHomeActivity.this, "Erreur lors du chargement des données", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<List<AppointmentDTO>>> call, Throwable t) {
                Toast.makeText(ClerkHomeActivity.this, "Erreur réseau: " + t.getMessage(), Toast.LENGTH_SHORT).show();
            }
        });
    }

    @Override
    protected void onResume() {
        super.onResume();
        loadDashboard();
        loadAppointments();
    }

    private void showLogoutConfirmation() {
        new AlertDialog.Builder(this)
                .setTitle("Déconnexion")
                .setMessage("Voulez-vous vraiment vous déconnecter ?")
                .setPositiveButton("Oui", (dialog, which) -> {
                    sessionManager.logout();
                    Intent intent = new Intent(ClerkHomeActivity.this, SignInActivity.class);
                    startActivity(intent);
                    finish();
                })
                .setNegativeButton("Non", null)
                .show();
    }
}
