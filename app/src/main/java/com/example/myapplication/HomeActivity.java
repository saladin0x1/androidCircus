package com.example.myapplication;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
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
import com.example.myapplication.utils.ErrorMessageHelper;
import com.google.android.material.floatingactionbutton.FloatingActionButton;
import java.util.ArrayList;
import java.util.List;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class HomeActivity extends AppCompatActivity {

    private TextView welcomeText;
    private TextView profileButton, logoutButton;
    private RecyclerView recyclerView;
    private AppointmentsAdapter adapter;
    private FloatingActionButton addAppointmentFab;
    private SessionManager sessionManager;
    private ApiService apiService;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_home);

        sessionManager = new SessionManager(this);
        apiService = RetrofitClient.getApiService();

        welcomeText = findViewById(R.id.welcomeText);
        profileButton = findViewById(R.id.profileButton);
        logoutButton = findViewById(R.id.logoutButton);
        recyclerView = findViewById(R.id.appointmentsRecyclerView);
        addAppointmentFab = findViewById(R.id.addAppointmentFab);

        welcomeText.setText("Bienvenue, " + sessionManager.getUserName());

        recyclerView.setLayoutManager(new LinearLayoutManager(this));
        adapter = new AppointmentsAdapter(new ArrayList<>(), null, true);
        adapter.setOnCancelClickListener(this::showCancelConfirmation);
        recyclerView.setAdapter(adapter);

        profileButton.setOnClickListener(v -> {
            Intent intent = new Intent(HomeActivity.this, ProfileActivity.class);
            startActivity(intent);
        });

        logoutButton.setOnClickListener(v -> showLogoutConfirmation());

        addAppointmentFab.setOnClickListener(v -> {
            Intent intent = new Intent(HomeActivity.this, CreateAppointmentActivity.class);
            startActivity(intent);
        });

        loadAppointments();
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
                    String errorMsg = ErrorMessageHelper.getErrorMessage(HomeActivity.this, response.code(), "Erreur lors du chargement des rendez-vous");
                    Toast.makeText(HomeActivity.this, errorMsg, Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<List<AppointmentDTO>>> call, Throwable t) {
                Toast.makeText(HomeActivity.this, "Vérifiez votre connexion internet", Toast.LENGTH_SHORT).show();
            }
        });
    }

    @Override
    protected void onResume() {
        super.onResume();
        welcomeText.setText("Bienvenue, " + sessionManager.getUserName());
        loadAppointments();
    }

    private void showLogoutConfirmation() {
        new AlertDialog.Builder(this)
                .setTitle("Déconnexion")
                .setMessage("Voulez-vous vraiment vous déconnecter ?")
                .setPositiveButton("Oui", (dialog, which) -> {
                    sessionManager.logout();
                    Intent intent = new Intent(HomeActivity.this, SignInActivity.class);
                    startActivity(intent);
                    finish();
                })
                .setNegativeButton("Non", null)
                .show();
    }

    private void showCancelConfirmation(AppointmentDTO appointment) {
        new AlertDialog.Builder(this)
                .setTitle("Annuler le rendez-vous")
                .setMessage("Voulez-vous vraiment annuler ce rendez-vous ?")
                .setPositiveButton("Oui", (dialog, which) -> cancelAppointment(appointment))
                .setNegativeButton("Non", null)
                .show();
    }

    private void cancelAppointment(AppointmentDTO appointment) {
        apiService.cancelAppointment(appointment.getId()).enqueue(new Callback<ApiResponse<Object>>() {
            @Override
            public void onResponse(Call<ApiResponse<Object>> call, Response<ApiResponse<Object>> response) {
                if (response.isSuccessful()) {
                    Toast.makeText(HomeActivity.this, "Rendez-vous annulé", Toast.LENGTH_SHORT).show();
                    loadAppointments();
                } else {
                    String errorMsg = ErrorMessageHelper.getErrorMessage(HomeActivity.this, response.code(), "Erreur lors de l'annulation");
                    Toast.makeText(HomeActivity.this, errorMsg, Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<Object>> call, Throwable t) {
                Toast.makeText(HomeActivity.this, "Erreur réseau", Toast.LENGTH_SHORT).show();
            }
        });
    }
}
