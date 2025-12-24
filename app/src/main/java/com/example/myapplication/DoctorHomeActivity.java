package com.example.myapplication;

import android.content.DialogInterface;
import android.content.Intent;
import android.os.Bundle;
import android.text.InputType;
import android.widget.EditText;
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
import com.example.myapplication.api.models.CompleteAppointmentRequest;
import com.example.myapplication.utils.NotificationHelper;
import java.util.ArrayList;
import java.util.List;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class DoctorHomeActivity extends AppCompatActivity {

    private TextView welcomeText, logoutButton;
    private RecyclerView recyclerView;
    private AppointmentsAdapter adapter;
    private SessionManager sessionManager;
    private ApiService apiService;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_doctor_home);

        sessionManager = new SessionManager(this);
        apiService = RetrofitClient.getApiService();

        welcomeText = findViewById(R.id.welcomeText);
        logoutButton = findViewById(R.id.logoutButton);
        recyclerView = findViewById(R.id.appointmentsRecyclerView);

        welcomeText.setText("Dr. " + sessionManager.getUserName());

        recyclerView.setLayoutManager(new LinearLayoutManager(this));
        
        adapter = new AppointmentsAdapter(new ArrayList<>(), this::showAppointmentOptions);
        recyclerView.setAdapter(adapter);

        logoutButton.setOnClickListener(v -> {
            sessionManager.logout();
            Intent intent = new Intent(DoctorHomeActivity.this, SignInActivity.class);
            startActivity(intent);
            finish();
        });

        loadAppointments();
    }

    private void showAppointmentOptions(AppointmentDTO appointment) {
        // Only allow actions on scheduled appointments
        if (!"Scheduled".equalsIgnoreCase(appointment.getStatus())) {
            Toast.makeText(this, "Ce rendez-vous est déjà " + appointment.getStatus().toLowerCase(), Toast.LENGTH_SHORT).show();
            return;
        }

        String[] options = {"Terminer le rendez-vous", "Annuler le rendez-vous"};

        new AlertDialog.Builder(this)
            .setTitle("Gestion du rendez-vous")
            .setItems(options, (dialog, which) -> {
                if (which == 0) {
                    showCompleteDialog(appointment.getId());
                } else {
                    confirmCancellation(appointment.getId());
                }
            })
            .show();
    }

    private void showCompleteDialog(String appointmentId) {
        AlertDialog.Builder builder = new AlertDialog.Builder(this);
        builder.setTitle("Terminer la consultation");

        final EditText input = new EditText(this);
        input.setInputType(InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_FLAG_MULTI_LINE);
        input.setHint("Notes du médecin / Prescription");
        input.setMinLines(3);
        builder.setView(input);

        builder.setPositiveButton("Terminer", (dialog, which) -> {
            String notes = input.getText().toString();
            completeAppointment(appointmentId, notes);
        });
        builder.setNegativeButton("Annuler", (dialog, which) -> dialog.cancel());

        builder.show();
    }

    private void confirmCancellation(String appointmentId) {
        new AlertDialog.Builder(this)
            .setTitle("Annuler le rendez-vous ?")
            .setMessage("Cette action est irréversible.")
            .setPositiveButton("Oui, annuler", (dialog, which) -> cancelAppointment(appointmentId))
            .setNegativeButton("Non", null)
            .show();
    }

    private void completeAppointment(String appointmentId, String notes) {
        String token = sessionManager.getAuthHeader();
        CompleteAppointmentRequest request = new CompleteAppointmentRequest(notes);

        apiService.completeAppointment(token, appointmentId, request).enqueue(new Callback<ApiResponse<AppointmentDTO>>() {
            @Override
            public void onResponse(Call<ApiResponse<AppointmentDTO>> call, Response<ApiResponse<AppointmentDTO>> response) {
                if (response.isSuccessful()) {
                    NotificationHelper.showNotification(DoctorHomeActivity.this, 
                        "Consultation terminée", 
                        "Le dossier du patient a été mis à jour avec succès.");
                    
                    Toast.makeText(DoctorHomeActivity.this, "Consultation terminée", Toast.LENGTH_SHORT).show();
                    loadAppointments(); // Refresh list
                } else {
                    Toast.makeText(DoctorHomeActivity.this, "Erreur lors de la validation", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<AppointmentDTO>> call, Throwable t) {
                Toast.makeText(DoctorHomeActivity.this, "Erreur réseau", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void cancelAppointment(String appointmentId) {
        String token = sessionManager.getAuthHeader();

        apiService.cancelAppointment(token, appointmentId).enqueue(new Callback<ApiResponse<Object>>() {
            @Override
            public void onResponse(Call<ApiResponse<Object>> call, Response<ApiResponse<Object>> response) {
                if (response.isSuccessful()) {
                    NotificationHelper.showNotification(DoctorHomeActivity.this, 
                        "Rendez-vous annulé", 
                        "Le rendez-vous a été retiré de votre agenda.");
                    
                    Toast.makeText(DoctorHomeActivity.this, "Rendez-vous annulé", Toast.LENGTH_SHORT).show();
                    loadAppointments(); // Refresh list
                } else {
                    Toast.makeText(DoctorHomeActivity.this, "Erreur lors de l'annulation", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<Object>> call, Throwable t) {
                Toast.makeText(DoctorHomeActivity.this, "Erreur réseau", Toast.LENGTH_SHORT).show();
            }
        });
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
                        adapter.updateData(apiResponse.getData());
                    }
                } else {
                    Toast.makeText(DoctorHomeActivity.this, "Erreur lors du chargement de l'agenda", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<List<AppointmentDTO>>> call, Throwable t) {
                Toast.makeText(DoctorHomeActivity.this, "Erreur réseau: " + t.getMessage(), Toast.LENGTH_SHORT).show();
            }
        });
    }

    @Override
    protected void onResume() {
        super.onResume();
        loadAppointments();
    }
}
