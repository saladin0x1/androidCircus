package com.example.myapplication;

import android.app.DatePickerDialog;
import android.os.Bundle;
import android.view.View;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.DatePicker;
import android.widget.EditText;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;
import androidx.appcompat.app.AppCompatActivity;
import com.example.myapplication.api.ApiService;
import com.example.myapplication.api.RetrofitClient;
import com.example.myapplication.api.SessionManager;
import com.example.myapplication.api.models.ApiResponse;
import com.example.myapplication.api.models.AppointmentDTO;
import com.example.myapplication.api.models.CreateAppointmentRequest;
import com.example.myapplication.api.models.DoctorDTO;
import com.example.myapplication.utils.NotificationHelper;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.List;
import java.util.Locale;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class CreateAppointmentActivity extends AppCompatActivity {

    private TextView backButton, dateInput;
    private Spinner doctorSpinner;
    private EditText reasonInput;
    private Button submitButton;
    
    private ApiService apiService;
    private SessionManager sessionManager;
    private List<DoctorDTO> doctorsList = new ArrayList<>();
    private String selectedDateIso = null; // Format: YYYY-MM-DDTHH:MM:SS

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_create_appointment);

        apiService = RetrofitClient.getApiService();
        sessionManager = new SessionManager(this);

        backButton = findViewById(R.id.backButton);
        dateInput = findViewById(R.id.dateInput);
        doctorSpinner = findViewById(R.id.doctorSpinner);
        reasonInput = findViewById(R.id.reasonInput);
        submitButton = findViewById(R.id.submitButton);

        backButton.setOnClickListener(v -> finish());

        // Date Picker logic
        dateInput.setOnClickListener(v -> showDatePicker());

        // Submit logic
        submitButton.setOnClickListener(v -> submitAppointment());

        // Load doctors
        loadDoctors();
    }

    private void showDatePicker() {
        final Calendar c = Calendar.getInstance();
        int year = c.get(Calendar.YEAR);
        int month = c.get(Calendar.MONTH);
        int day = c.get(Calendar.DAY_OF_MONTH);

        DatePickerDialog datePickerDialog = new DatePickerDialog(this,
            (view, year1, monthOfYear, dayOfMonth) -> {
                // Format for display
                String displayDate = dayOfMonth + "/" + (monthOfYear + 1) + "/" + year1;
                dateInput.setText(displayDate);

                // Format for API (ISO 8601) - defaulting to 09:00 AM
                // Note: Month is 0-indexed in Calendar
                selectedDateIso = String.format(Locale.US, "%04d-%02d-%02dT09:00:00", year1, monthOfYear + 1, dayOfMonth);
            }, year, month, day);
        
        // Prevent past dates
        datePickerDialog.getDatePicker().setMinDate(System.currentTimeMillis() - 1000);
        datePickerDialog.show();
    }

    private void loadDoctors() {
        String token = sessionManager.getAuthHeader();
        if (token == null) return;

        apiService.getDoctors(token).enqueue(new Callback<ApiResponse<List<DoctorDTO>>>() {
            @Override
            public void onResponse(Call<ApiResponse<List<DoctorDTO>>> call, Response<ApiResponse<List<DoctorDTO>>> response) {
                if (response.isSuccessful() && response.body() != null) {
                    ApiResponse<List<DoctorDTO>> apiResponse = response.body();
                    if (apiResponse.isSuccess()) {
                        doctorsList = apiResponse.getData();
                        setupSpinner();
                    }
                } else {
                    Toast.makeText(CreateAppointmentActivity.this, "Impossible de charger la liste des médecins", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<List<DoctorDTO>>> call, Throwable t) {
                Toast.makeText(CreateAppointmentActivity.this, "Erreur réseau: " + t.getMessage(), Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void setupSpinner() {
        ArrayAdapter<DoctorDTO> adapter = new ArrayAdapter<>(this, 
                android.R.layout.simple_spinner_item, doctorsList);
        adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
        doctorSpinner.setAdapter(adapter);
    }

    private void submitAppointment() {
        if (selectedDateIso == null) {
            Toast.makeText(this, "Veuillez sélectionner une date", Toast.LENGTH_SHORT).show();
            return;
        }

        if (doctorSpinner.getSelectedItem() == null) {
            Toast.makeText(this, "Veuillez sélectionner un médecin", Toast.LENGTH_SHORT).show();
            return;
        }

        String reason = reasonInput.getText().toString().trim();
        if (reason.isEmpty()) {
            Toast.makeText(this, "Veuillez indiquer un motif", Toast.LENGTH_SHORT).show();
            return;
        }

        DoctorDTO selectedDoctor = (DoctorDTO) doctorSpinner.getSelectedItem();
        String patientId = sessionManager.getRoleSpecificId(); // Note: We need to ensure SessionManager stores this!
        
        // Wait, SessionManager currently stores userId, but for appointment we need patientId?
        // Let's check AppointmentDTO/CreateAppointmentRequest.
        // The API takes "patientId" (GUID).
        // The LoginResponse.Data has "roleSpecificId". We need to store that in SessionManager.
        
        // I will use "userId" for now if "roleSpecificId" is not available, but let me check SessionManager.
        // SessionManager does NOT store roleSpecificId yet. I need to fix that first!
        // For now, let's assume I'll fix SessionManager.
        
        // Actually, the API might infer patientId from the token if not provided, but the DTO has it.
        // Let's proceed assuming I fix SessionManager in the next step.
        String storedRoleSpecificId = sessionManager.getRoleSpecificId();
        
        CreateAppointmentRequest request = new CreateAppointmentRequest(
            storedRoleSpecificId, // patientId
            selectedDoctor.getId(),
            selectedDateIso,
            reason,
            "" // notes
        );

        String token = sessionManager.getAuthHeader();
        apiService.createAppointment(token, request).enqueue(new Callback<ApiResponse<AppointmentDTO>>() {
            @Override
            public void onResponse(Call<ApiResponse<AppointmentDTO>> call, Response<ApiResponse<AppointmentDTO>> response) {
                if (response.isSuccessful() && response.body() != null) {
                    if (response.body().isSuccess()) {
                        NotificationHelper.showNotification(CreateAppointmentActivity.this, 
                            "Rendez-vous confirmé", 
                            "Votre consultation avec le Dr. " + selectedDoctor.getName() + " est enregistrée.");
                        
                        Toast.makeText(CreateAppointmentActivity.this, "Rendez-vous confirmé !", Toast.LENGTH_LONG).show();
                        finish(); // Go back to Home
                    } else {
                        Toast.makeText(CreateAppointmentActivity.this, "Erreur: " + response.body().getError().getMessage(), Toast.LENGTH_SHORT).show();
                    }
                } else {
                     Toast.makeText(CreateAppointmentActivity.this, "Erreur lors de la réservation", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<AppointmentDTO>> call, Throwable t) {
                Toast.makeText(CreateAppointmentActivity.this, "Erreur réseau: " + t.getMessage(), Toast.LENGTH_SHORT).show();
            }
        });
    }
}
