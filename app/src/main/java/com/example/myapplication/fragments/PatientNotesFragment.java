package com.example.myapplication.fragments;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import com.example.myapplication.R;
import com.example.myapplication.api.ApiService;
import com.example.myapplication.api.RetrofitClient;
import com.example.myapplication.api.SessionManager;
import com.example.myapplication.api.models.ApiResponse;
import com.example.myapplication.api.models.UpdatePatientNotesRequest;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class PatientNotesFragment extends Fragment {

    private EditText notesEditText;
    private TextView saveButton;
    private SessionManager sessionManager;
    private ApiService apiService;
    private String patientId;
    private String patientName;
    private boolean isLoading = false;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_patient_notes, container, false);

        sessionManager = new SessionManager(requireContext());
        apiService = RetrofitClient.getApiService();

        // Get patient data from arguments
        Bundle args = getArguments();
        if (args != null) {
            patientId = args.getString("patientId");
            patientName = args.getString("patientName");
        }

        // Initialize views
        notesEditText = view.findViewById(R.id.notesEditText);
        saveButton = view.findViewById(R.id.saveButton);

        // Save button
        saveButton.setOnClickListener(v -> saveNotes());

        // Load existing notes
        if (patientId != null) {
            loadNotes();
        }

        return view;
    }

    private void loadNotes() {
        String token = sessionManager.getAuthHeader();
        if (token == null || patientId == null) return;

        apiService.getPatientNotes(token, patientId).enqueue(new Callback<ApiResponse<String>>() {
            @Override
            public void onResponse(Call<ApiResponse<String>> call, Response<ApiResponse<String>> response) {
                if (response.isSuccessful() && response.body() != null) {
                    ApiResponse<String> apiResponse = response.body();
                    if (apiResponse.isSuccess() && apiResponse.getData() != null) {
                        notesEditText.setText(apiResponse.getData());
                    }
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<String>> call, Throwable t) {
                // Keep empty on error, user can still add notes
            }
        });
    }

    private void saveNotes() {
        if (isLoading) return;

        String token = sessionManager.getAuthHeader();
        if (token == null || patientId == null) {
            Toast.makeText(requireContext(), "Erreur d'authentification", Toast.LENGTH_SHORT).show();
            return;
        }

        String notes = notesEditText.getText().toString();
        isLoading = true;
        saveButton.setEnabled(false);

        apiService.updatePatientNotes(token, patientId, new UpdatePatientNotesRequest(notes))
            .enqueue(new Callback<ApiResponse<String>>() {
            @Override
            public void onResponse(Call<ApiResponse<String>> call, Response<ApiResponse<String>> response) {
                isLoading = false;
                saveButton.setEnabled(true);

                if (response.isSuccessful() && response.body() != null) {
                    ApiResponse<String> apiResponse = response.body();
                    if (apiResponse.isSuccess()) {
                        Toast.makeText(requireContext(), "Notes enregistrées", Toast.LENGTH_SHORT).show();
                    } else {
                        Toast.makeText(requireContext(), "Erreur: " + apiResponse.getError().getMessage(), Toast.LENGTH_SHORT).show();
                    }
                } else {
                    Toast.makeText(requireContext(), "Erreur lors de l'enregistrement", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<String>> call, Throwable t) {
                isLoading = false;
                saveButton.setEnabled(true);
                Toast.makeText(requireContext(), "Erreur réseau", Toast.LENGTH_SHORT).show();
            }
        });
    }
}
