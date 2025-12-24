package com.example.myapplication;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;
import androidx.appcompat.app.AppCompatActivity;

import com.example.myapplication.api.ApiService;
import com.example.myapplication.api.RetrofitClient;
import com.example.myapplication.api.SessionManager;
import com.example.myapplication.api.models.LoginResponse;
import com.example.myapplication.api.models.RegisterRequest;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class SignUpActivity extends AppCompatActivity {

    private EditText nameInput, emailInput, passwordInput, phoneInput;
    private TextView signUpButton;
    private TextView signInLink;
    private ApiService apiService;
    private SessionManager sessionManager;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_signup);

        apiService = RetrofitClient.getApiService();
        sessionManager = new SessionManager(this);

        nameInput = findViewById(R.id.nameInput);
        emailInput = findViewById(R.id.emailInput);
        passwordInput = findViewById(R.id.passwordInput);
        phoneInput = findViewById(R.id.phoneInput);
        signUpButton = findViewById(R.id.signUpButton);
        signInLink = findViewById(R.id.signInLink);

        signUpButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                String fullName = nameInput.getText().toString().trim();
                String email = emailInput.getText().toString().trim();
                String password = passwordInput.getText().toString().trim();
                String phone = phoneInput.getText().toString().trim();

                if (fullName.isEmpty() || email.isEmpty() || password.isEmpty()) {
                    Toast.makeText(SignUpActivity.this, "Veuillez remplir tous les champs obligatoires", Toast.LENGTH_SHORT).show();
                    return;
                }

                if (password.length() < 6) {
                    Toast.makeText(SignUpActivity.this, "Le mot de passe doit contenir au moins 6 caractères", Toast.LENGTH_SHORT).show();
                    return;
                }

                // Split name into First and Last
                String firstName = fullName;
                String lastName = "."; // Default if no last name provided
                if (fullName.contains(" ")) {
                    int splitIndex = fullName.indexOf(" ");
                    firstName = fullName.substring(0, splitIndex);
                    lastName = fullName.substring(splitIndex + 1);
                }

                performRegistration(firstName, lastName, email, password, phone);
            }
        });

        signInLink.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                finish();
            }
        });
    }

    private void performRegistration(String firstName, String lastName, String email, String password, String phone) {
        Toast.makeText(SignUpActivity.this, "Inscription en cours...", Toast.LENGTH_SHORT).show();

        // Role 0 = Patient
        RegisterRequest request = new RegisterRequest(firstName, lastName, email, password, phone, 0);
        // Default DOB for now as it's not in the UI
        request.setDateOfBirth("2000-01-01"); 

        apiService.register(request).enqueue(new Callback<LoginResponse>() {
            @Override
            public void onResponse(Call<LoginResponse> call, Response<LoginResponse> response) {
                if (response.isSuccessful() && response.body() != null) {
                    LoginResponse loginResponse = response.body();
                    if (loginResponse.isSuccess()) {
                        LoginResponse.Data data = loginResponse.getData();
                        
                        // Auto-login
                        sessionManager.saveSession(
                            data.getToken(),
                            data.getUserId(),
                            data.getRole(),
                            data.getFirstName() + " " + data.getLastName(),
                            data.getEmail(),
                            data.getRoleSpecificId()
                        );

                        Toast.makeText(SignUpActivity.this, "Compte créé avec succès !", Toast.LENGTH_SHORT).show();
                        
                        // New users are always Patients (Role 0) in this flow
                        Intent intent = new Intent(SignUpActivity.this, HomeActivity.class);
                        startActivity(intent);
                        finishAffinity(); // Clear stack so back button exits app
                    } else {
                        Toast.makeText(SignUpActivity.this, "Erreur: " + loginResponse.getError(), Toast.LENGTH_SHORT).show();
                    }
                } else {
                    Toast.makeText(SignUpActivity.this, "Erreur lors de l'inscription", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<LoginResponse> call, Throwable t) {
                Toast.makeText(SignUpActivity.this, "Erreur réseau: " + t.getMessage(), Toast.LENGTH_SHORT).show();
            }
        });
    }
}
